using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;
using ProgressOnderwijsUtils.SchemaReflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using static ProgressOnderwijsUtils.SafeSql;

// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace ProgressOnderwijsUtils
{
    public static class CascadedDelete
    {
        [NotNull]
        public static DeletionReport[] RecursivelyDelete<TId>(
            [NotNull] SqlConnection conn,
            [NotNull] DatabaseDescription.Table initialTableAsEntered,
            bool outputAllDeletedRows,
            [CanBeNull] Action<string> logger,
            [CanBeNull] Func<string, bool> stopCascading,
            [NotNull] string pkColumn,
            [NotNull] params TId[] pksToDelete
        )
            where TId : Enum
        {
            var pkColumnSql = ParameterizedSql.CreateDynamic(pkColumn);
            return RecursivelyDelete(conn, initialTableAsEntered, outputAllDeletedRows, logger, stopCascading, new[] { pkColumn }, SQL($@"
                select {pkColumnSql} = q.QueryTableValue 
                from {pksToDelete} q
            "));
        }

        [NotNull]
        public static DeletionReport[] RecursivelyDelete<[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
            TId>(
            [NotNull] SqlConnection conn,
            [NotNull] DatabaseDescription.Table initialTableAsEntered,
            bool outputAllDeletedRows,
            [CanBeNull] Action<string> logger,
            [CanBeNull] Func<string, bool> stopCascading,
            [NotNull] params TId[] pksToDelete
        )
            where TId : IPropertiesAreUsedImplicitly, IMetaObject
        {
            var pksTable = SQL($"#pksTable");
            var pkColumns = MetaObject.GetMetaProperties<TId>().Select(mp => mp.Name).ToArray();
            var pkColumnsSql = pkColumns.ArraySelect(ParameterizedSql.CreateDynamic);

            var pkColumnsMetaData = initialTableAsEntered.Columns.Select(col => col.ColumnMetaData).Where(col => pkColumns.Contains(col.ColumnName, StringComparer.OrdinalIgnoreCase)).ToArray();
            pkColumnsMetaData.CreateNewTableQuery(pksTable).ExecuteNonQuery(conn);

            var target = new BulkInsertTarget(
                pksTable.CommandText(),
                MetaObject.GetMetaProperties<TId>().ArraySelect((mp, index) => new ColumnDefinition(mp.DataType, mp.Name, index, ColumnAccessibility.Normal))
            );
            pksToDelete.BulkCopyToSqlServer(conn, target);
            var report = RecursivelyDelete(conn, initialTableAsEntered, outputAllDeletedRows, logger, stopCascading, pkColumns, SQL($@"
                select {pkColumnsSql.ConcatenateSql(SQL($", "))}
                from {pksTable}
            "));

            SQL($@"
                drop table {pksTable}
            ").ExecuteNonQuery(conn);

            return report;
        }

        [NotNull]
        public static DeletionReport[] RecursivelyDelete(
            [NotNull] SqlConnection conn,
            DatabaseDescription.Table initialTableAsEntered,
            bool outputAllDeletedRows,
            [CanBeNull] Action<string> logger,
            [CanBeNull] Func<string, bool> stopCascading,
            [NotNull] DataTable pksToDelete
        )
        {
            var pksTable = SQL($"#pksTable");
            var pkColumns = pksToDelete.Columns.Cast<DataColumn>().Select(dc => dc.ColumnName).ToArray();
            var pkColumnsSql = pkColumns.ArraySelect(ParameterizedSql.CreateDynamic);

            var pkColumnsMetaData = initialTableAsEntered.Columns.Select(col => col.ColumnMetaData).Where(col => pkColumns.Contains(col.ColumnName, StringComparer.OrdinalIgnoreCase)).ToArray();
            pkColumnsMetaData.CreateNewTableQuery(pksTable).ExecuteNonQuery(conn);
            BulkInsertTarget.FromCompleteSetOfColumns(pksTable.CommandText(), pkColumnsMetaData).BulkInsert(conn, pksToDelete);

            var report = RecursivelyDelete(conn, initialTableAsEntered, outputAllDeletedRows, logger, stopCascading, pkColumns, SQL($@"
                select {pkColumnsSql.ConcatenateSql(SQL($", "))}
                from {pksTable}
            "));

            SQL($@"
                drop table {pksTable}
            ").ExecuteNonQuery(conn);

            return report;
        }

        /// <summary>
        /// Recursively deletes records from a database table, including all its foreign-key dependents.
        /// 
        /// WARNING: this is still fairly rough code, and although it appears correct, it may fail with confusing error messages when it encounters any unsupported situation.
        /// 
        /// In particularly, this code cannot break cyclical dependencies, and also cannot detect them: when a dependency chain reaches 500 long, it will crash.
        /// </summary>
        [NotNull]
        [UsefulToKeep("Library function")]
        public static DeletionReport[] RecursivelyDelete(
            [NotNull] SqlConnection conn,
            [NotNull] DatabaseDescription.Table initialTableAsEntered,
            bool outputAllDeletedRows,
            [CanBeNull] Action<string> logger,
            [CanBeNull] Func<string, bool> stopCascading,
            [NotNull] string[] pkColumns,
            ParameterizedSql pksTVParameter
        )
        {
            void log(string message)
                => logger?.Invoke(message);

            bool StopCascading(ParameterizedSql tableName)
                => stopCascading?.Invoke(tableName.CommandText()) ?? false;

            DataTable ExecuteDeletion(ParameterizedSql deletionCommand)
            {
                if (outputAllDeletedRows) {
                    return deletionCommand.ReadDataTable(conn, MissingSchemaAction.Add);
                } else {
                    deletionCommand.ExecuteNonQuery(conn);
                    return null;
                }
            }

            var outputClause = outputAllDeletedRows ? SQL($"output deleted.*") : default;
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
                join sys.objects o_pk on pk.parent_object_id = o_pk.object_id and o_pk.type='U'
                join sys.index_columns as ic on ic.object_id = pk.parent_object_id  and ic.index_id = pk.unique_index_id
                where 1=0
                    or pk.type = 'PK' 
                    or pk.type='UQ' 
                        and not exists(
                            select * from  sys.key_constraints pk2 
                            where pk2.parent_object_id=pk.parent_object_id 
                            and (pk2.type ='PK' or pk2.type ='UQ' and pk2.object_id<pk.object_id)
                        )
                order by pk.parent_object_id, ic.column_id
            ").ReadMetaObjects<PkCol>(conn)
                .ToLookup(
                    row => row.Pk_table,
                    row => ParameterizedSql.CreateDynamic(row.Pk_column),
                    StringComparer.OrdinalIgnoreCase
                );

            var initialKeyColumns = pkColumns.Select(name => initialTableAsEntered.Columns.Single(col => col.ColumnName.EqualsOrdinalCaseInsensitive(name)).SqlColumnName()).ToArray();

            var delTable = SQL($"#del_init");

            var pkColumnsMetaData = initialTableAsEntered.Columns.Select(col => col.ColumnMetaData).Where(col => pkColumns.Contains(col.ColumnName, StringComparer.OrdinalIgnoreCase)).ToArray();
            pkColumnsMetaData.CreateNewTableQuery(delTable).ExecuteNonQuery(conn);

            var idsToDelete = SQL($@"
                insert into {delTable} ({initialKeyColumns.ConcatenateSql(SQL($", "))})
                {pksTVParameter};

                select count(*) from {delTable};
            ").ReadScalar<int>(conn);

            var initialRowCountToDelete = SQL($@"
                delete dt
                from {delTable} dt
                left join {initialTableAsEntered.QualifiedNameSql} initT on {initialKeyColumns.Select(col => SQL($"dt.{col}=initT.{col}")).ConcatenateSql(SQL($" and "))}
                where {initialKeyColumns.Select(col => SQL($"initT.{col} is null")).ConcatenateSql(SQL($" and "))}
                ;
                select count(*) from {delTable}
            ").ReadScalar<int>(conn);

            log($"Recursively deleting {initialRowCountToDelete} rows (of {idsToDelete} ids) from {initialTableAsEntered.QualifiedName})");

            var delBatch = 0;

            var deletionStack = new Stack<Action>();
            var perflog = new List<DeletionReport>();
            long totalDeletes = 0;

            void DeleteKids(ParameterizedSql tableName, ParameterizedSql tempTableName, SList<string> logStack, int depth)
            {
                if (StopCascading(tableName)) {
                    return;
                }

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

            DeleteKids(initialTableAsEntered.QualifiedNameSql, delTable, SList.SingleElement(initialTableAsEntered.QualifiedName), 0);
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

            public ParameterizedSql FkTableSql
                => ParameterizedSql.CreateDynamic(Fk_table);

            public ParameterizedSql PkColumnSql
                => ParameterizedSql.CreateDynamic(Pk_column);

            public ParameterizedSql FkColumnSql
                => ParameterizedSql.CreateDynamic(Fk_column);
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
