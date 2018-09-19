using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;
using static ProgressOnderwijsUtils.SafeSql;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace ProgressOnderwijsUtils
{
    public static class CascadedDelete
    {
        /// <summary>
        /// Recursively deletes records from a database table, including all its foreign-key dependents.
        /// 
        /// WARNING: this is still fairly rough code, and although it appears correct, it may fail with confusing error messages when it encounters any unsupported situation.
        /// 
        /// In particularly, this code cannot break cyclical dependencies, and also cannot detect them: when a dependency chain reaches 500 long, it will crash.
        /// </summary>
        [NotNull]
        public static DeletionReport[] RecursivelyDelete(
            [NotNull] SqlCommandCreationContext conn,
            ParameterizedSql initialTableAsEntered,
            bool OutputAllDeletedRows,
            [CanBeNull] Action<string> logger,
            [NotNull] DataTable pksToDelete
            )
        {
            void log(string message) => logger?.Invoke(message);

            DataTable ExecuteDeletion(ParameterizedSql deletionCommand)
            {
                if (OutputAllDeletedRows) {
                    return deletionCommand.ReadDataTable(conn, MissingSchemaAction.Add);
                } else {
                    deletionCommand.ExecuteNonQuery(conn);
                    return null;
                }
            }

            var outputClause = OutputAllDeletedRows ? SQL($"output deleted.*") : default;

            var initialTableName = initialTableAsEntered.CommandText();
            var initialTable =
                ParameterizedSql.CreateDynamic(SQL($"select object_schema_name(object_id({initialTableName})) + '.' + object_name(object_id({initialTableName}))")
                    .ReadScalar<string>(conn));

            var fksByParentTable = SQL($@"
                select 
                    fk_id = fk.object_id,
                    pk_table=object_schema_name(fk.referenced_object_id) + '.' +  object_name(fk.referenced_object_id),
                    fk_table=object_schema_name(fk.parent_object_id) + '.' + object_name(fk.parent_object_id),
                    pk_column=COL_NAME( fkc.referenced_object_id , fkc.referenced_column_id),
                    fk_column=COL_NAME( fkc.parent_object_id , fkc.parent_column_id)
                from sys.foreign_keys fk
                join sys.foreign_key_columns fkc on fkc.constraint_object_id = fk.object_id
                order by fk.object_id, fkc.constraint_column_id
            ").ReadMetaObjects<FkCol>(conn)
                .GroupBy(row => row.Fk_id)
                .Select(rowGroup => new ForeignKey { ParentTable = rowGroup.First().Pk_table, DependantTable = rowGroup.First().FkTableSql, Columns = rowGroup.ToArray(), })
                .ToLookup(rowGroup => rowGroup.ParentTable, StringComparer.OrdinalIgnoreCase);

            var pkColumnsByTable = SQL($@"
                select 
                    pk_table=object_schema_name(pk.parent_object_id) + '.' + object_name(pk.parent_object_id),
                    pk_column=col_name(pk.parent_object_id, ic.column_id)
                from sys.key_constraints pk
                join sys.objects o_pk on pk.parent_object_id = o_pk.object_id
                join sys.index_columns as ic on ic.object_id = pk.parent_object_id  and ic.index_id = pk.unique_index_id
                where pk.type = 'PK'  and o_pk.type='U'
                order by pk.parent_object_id, ic.column_id
            ").ReadMetaObjects<PkCol>(conn)
                .ToLookup(
                    row => row.Pk_table,
                    row => ParameterizedSql.CreateDynamic(row.Pk_column),
                    StringComparer.OrdinalIgnoreCase
                );

            var initialPrimaryKeyColumns = pkColumnsByTable[initialTable.CommandText()].ToArray();
            var initialPrimaryKeyColumnNames = initialPrimaryKeyColumns.Select(col => col.CommandText()).ToArray();
            var providedIdColumns = pksToDelete.Columns.Cast<DataColumn>().Select(dc => dc.ColumnName).ToArray();
            if (!providedIdColumns.SetEqual(initialPrimaryKeyColumnNames, StringComparer.OrdinalIgnoreCase)) {
                throw new InvalidOperationException("Expected primary key columns: " + initialPrimaryKeyColumnNames.JoinStrings(", ") + "; provided columns: " + providedIdColumns.JoinStrings(", "));
            }

            var delTable = SQL($"[#del_init]");
            //union all is a nasty hack to enforce that the identity property is not propagated to the temp table
            SQL($@"
                select {initialPrimaryKeyColumns.ConcatenateSql(SQL($", "))} 
                into {delTable}
                from {initialTable}
                where 1=0
                union all
                select {initialPrimaryKeyColumns.ConcatenateSql(SQL($", "))} 
                from {initialTable}
                where 1=0
            ").ExecuteNonQuery(conn);
            using (var bulkCopy = new SqlBulkCopy(conn.Connection)) {
                bulkCopy.BulkCopyTimeout = conn.CommandTimeoutInS;
                bulkCopy.DestinationTableName = delTable.CommandText();
                bulkCopy.WriteToServer(pksToDelete);
            }

            var initialRowCountToDelete = SQL($@"
                delete dt
                from {delTable} dt
                left join {initialTable} initT on {initialPrimaryKeyColumns.Select(col => SQL($"dt.{col}=initT.{col}")).ConcatenateSql(SQL($" and "))}
                where {initialPrimaryKeyColumns.Select(col => SQL($"initT.{col} is null")).ConcatenateSql(SQL($" and "))}
                ;
                select count(*) from {delTable}
            ").ReadScalar<int>(conn);

            log($"Recursively deleting {initialRowCountToDelete} rows (of {pksToDelete.Rows.Count} ids) from {initialTable.CommandText()})");

            int delBatch = 0;

            var deletionStack = new Stack<Action>();
            var perflog = new List<DeletionReport>();
            long totalDeletes = 0;

            void DeleteKids(ParameterizedSql tableName, ParameterizedSql tempTableName, SList<string> logStack, int depth)
            {
                if (depth > 500) {
                    throw new InvalidOperationException("A dependency chain of over 500 long was encountered; possible cycle: aborting.");
                }

                var pkeysOfCurrentTable = pkColumnsByTable[tableName.CommandText()].ToArray();
                if (pkeysOfCurrentTable.None()) {
                    throw new InvalidOperationException($"Table {tableName.CommandText()} is missing a primary key");
                }
                var ttJoin = pkeysOfCurrentTable.Select(col => SQL($"pk.{col}=tt.{col}")).ConcatenateSql(SQL($" and "));

                deletionStack.Push(() => {
                    var nrRowsToDelete = SQL($"select count(*) from {tempTableName}").ReadScalar<int>(conn);
                    log($"Delete {nrRowsToDelete} from {tableName.CommandText()}...");
                    var sw = Stopwatch.StartNew();
                    var deletedRows = ExecuteDeletion(SQL($@"
                        delete pk
                        {outputClause}
                        from {tableName} pk
                        join {tempTableName} tt on {ttJoin};
                    
                        drop table {tempTableName};
                    "));
                    sw.Stop();
                    log($"...took {sw.Elapsed}");
                    perflog.Add(new DeletionReport { Table = tableName.CommandText(), DeletedAtMostRowCount = nrRowsToDelete, DeletionDuration = sw.Elapsed, DeletedRows = deletedRows });
                });

                var fks = fksByParentTable[tableName.CommandText()];

                foreach (var fk in fks) {
                    var pkeysOfReferencingTable = pkColumnsByTable[fk.DependantTable.CommandText()];
                    var pkJoin = fk.Columns.Select(col => SQL($"fk.{col.FkColumnSql}=pk.{col.PkColumnSql}")).ConcatenateSql(SQL($" and "));
                    var newDelTable = ParameterizedSql.CreateDynamic("[#del_" + delBatch + "]");
                    if (pkeysOfReferencingTable.None()) {
                        log($"Warning: table {fk.DependantTable.CommandText()}->{logStack.JoinStrings("->")} is missing a primary key");
                        deletionStack.Push(() => {
                            var nrRowsToDelete = SQL($@"
                                select count(*)
                                from {fk.DependantTable} fk
                                join {tableName} as pk on {pkJoin}
                                join {tempTableName} as tt on {ttJoin}
                            ").ReadScalar<int>(conn);
                            log($"Delete {nrRowsToDelete} from {fk.DependantTable.CommandText()}...");
                            var sw = Stopwatch.StartNew();
                            var deletedRows = ExecuteDeletion(SQL($@"
                                delete fk
                                {outputClause}
                                from {fk.DependantTable} fk
                                join {tableName} as pk on {pkJoin}
                                join {tempTableName} as tt on {ttJoin}
                            "));
                            sw.Stop();
                            log($"...took {sw.Elapsed}");
                            perflog.Add(new DeletionReport { Table = fk.DependantTable.CommandText(), DeletedAtMostRowCount = nrRowsToDelete, DeletionDuration = sw.Elapsed, DeletedRows = deletedRows });
                        });
                        continue;
                    }

                    var whereClause = !tableName.CommandText().EqualsOrdinalCaseInsensitive(fk.DependantTable.CommandText())
                        ? SQL($"where 1=1")
                        : SQL($"where {pkeysOfCurrentTable.Select(col => SQL($"pk.{col}<>fk.{col}")).ConcatenateSql(SQL($" or "))}");
                    var referencingPkCols = pkeysOfReferencingTable.Select(col => SQL($"fk.{col}")).ConcatenateSql(SQL($", "));
                    var statement = SQL($@"
                        select {referencingPkCols} 
                        into {newDelTable}
                        from {fk.DependantTable} as fk
                        join {tableName} as pk on {pkJoin}
                        join {tempTableName} as tt on {ttJoin}
                        {whereClause}
                        ;
                        
                        select count(*) from {newDelTable}
                    ");

                    var kidRowsCount = statement.ReadScalar<int>(conn);

                    totalDeletes += kidRowsCount;

                    log($"{delBatch,6}: Found {kidRowsCount} in {fk.DependantTable.CommandText()}->{logStack.JoinStrings("->")}");

                    if (kidRowsCount == 0) {
                        SQL($"drop table {newDelTable}").ExecuteNonQuery(conn);
                    } else {
                        delBatch++;
                        DeleteKids(fk.DependantTable, newDelTable, logStack.Prepend(fk.DependantTable.CommandText()), depth + 1);
                    }
                }
            }

            DeleteKids(initialTable, delTable, SList.SingleElement(initialTable.CommandText()), 0);
            while (deletionStack.Count > 0) {
                deletionStack.Pop()();
            }

            log($"{totalDeletes} rows Deleted");
            return perflog.ToArray();
        }

        public struct DeletionReport
        {
            public string Table;
            public TimeSpan DeletionDuration;
            public int DeletedAtMostRowCount;
            public DataTable DeletedRows;
        }

        [UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.Members)]
        sealed class FkCol : IMetaObject
        {
            public int Fk_id { get; set; }
            public string Pk_table { get; set; }
            public string Fk_table { get; set; }
            public string Pk_column { get; set; }
            public string Fk_column { get; set; }
            public ParameterizedSql FkTableSql => ParameterizedSql.CreateDynamic(Fk_table);
            public ParameterizedSql PkColumnSql => ParameterizedSql.CreateDynamic(Pk_column);
            public ParameterizedSql FkColumnSql => ParameterizedSql.CreateDynamic(Fk_column);
        }

        [UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.Members)]
        sealed class PkCol : IMetaObject
        {
            public string Pk_table { get; set; }
            public string Pk_column { get; set; }
        }

        struct ForeignKey
        {
            public string ParentTable;
            public ParameterizedSql DependantTable;
            public FkCol[] Columns;
        }
    }
}
