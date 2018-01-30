using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;
using static ProgressOnderwijsUtils.SafeSql;

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
        public static DeletionPerformance[] RecursivelyDelete<TId>(
            [NotNull] SqlCommandCreationContext conn,
            ParameterizedSql initialTableAsEntered,
            [NotNull] TId[] idsToDelete,
            [CanBeNull] Action<string> logger
            )
            where TId : struct, IConvertible, IComparable
        {
            void log(string message) => logger?.Invoke(message);

            var initialTableName = initialTableAsEntered.CommandText();
            var initialTable =
                ParameterizedSql.CreateDynamic(SQL($"select object_schema_name(object_id({initialTableName})) + '.' + object_name(object_id({initialTableName}))")
                    .ReadScalar<string>(conn));

            var keys = SQL($@"
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
                .ToLookup(
                    rowGroup => rowGroup.First().Pk_table,
                    rowGroup => new {
                        rowGroup.First().FkTableSql,
                        columns = rowGroup.ToArray(),
                    },
                    StringComparer.OrdinalIgnoreCase
                );

            var pkeys = SQL($@"
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

            var initialPrimaryKeyColumn = pkeys[initialTable.CommandText()].Single();

            var delTable = SQL($"[##del_init]");
            var initialRowCountToDelete = SQL($@"
                select {initialPrimaryKeyColumn} 
                into {delTable}
                from {initialTable}
                where {initialPrimaryKeyColumn} in {idsToDelete}
                ;
                select count(*) from {delTable}
            ").ReadScalar<int>(conn);

            log($"Recursively deleting {initialRowCountToDelete} rows (of {idsToDelete.Length} ids) from {initialTable.CommandText()})");

            int delBatch = 0;

            var deletionStack = new Stack<Action>();
            var perflog = new List<DeletionPerformance>();
            long totalDeletes = 0;

            void DeleteKids(ParameterizedSql tableName, ParameterizedSql tempTableName, SList<string> logStack, int depth)
            {
                if (depth > 500) {
                    throw new InvalidOperationException("A dependency chain of over 500 long was encountered; possible cycle: aborting.");
                }

                var pkeysOfCurrentTable = pkeys[tableName.CommandText()].ToArray();
                if (pkeysOfCurrentTable.None()) {
                    throw new InvalidOperationException($"Table {tableName.CommandText()} is missing a primary key");
                }
                var ttJoin = pkeysOfCurrentTable.Select(col => SQL($"pk.{col}=tt.{col}")).ConcatenateSql(SQL($" and "));

                deletionStack.Push(() => {
                    var nrRowsToDelete = SQL($"select count(*) from {tempTableName}").ReadScalar<int>(conn);
                    log($"Delete {nrRowsToDelete} from {tableName.CommandText()}...");
                    var sw = Stopwatch.StartNew();
                    SQL($@"
                    delete pk
                    from {tableName} pk
                    join {tempTableName} tt on {ttJoin};
                    
                    drop table {tempTableName};
                ").ExecuteNonQuery(conn);
                    sw.Stop();
                    log("...took {sw.Elapsed}");
                    perflog.Add(new DeletionPerformance { Table = tableName.CommandText(), RowCount = nrRowsToDelete, DeletionDuration = sw.Elapsed });
                });

                var fks = keys[tableName.CommandText()];

                foreach (var fk in fks) {
                    var referencingPkCols = pkeys[fk.FkTableSql.CommandText()].Select(col => SQL($"fk.{col}")).ConcatenateSql(SQL($", "));
                    var pkJoin = fk.columns.Select(col => SQL($"fk.{col.FkColumnSql}=pk.{col.PkColumnSql}")).ConcatenateSql(SQL($" and "));

                    var newDelTable = ParameterizedSql.CreateDynamic("[##del_" + delBatch + "]");
                    var whereClause = !tableName.CommandText().EqualsOrdinalCaseInsensitive(fk.FkTableSql.CommandText())
                        ? SQL($"where 1=1")
                        : SQL($"where {pkeysOfCurrentTable.Select(col => SQL($"pk.{col}<>fk.{col}")).ConcatenateSql(SQL($" or "))}");

                    var statement = SQL($@"
                        select {referencingPkCols} 
                        into {newDelTable}
                        from {fk.FkTableSql} as fk
                        join {tableName} as pk on {pkJoin}
                        join {tempTableName} as tt on {ttJoin}
                        {whereClause}
                        ;
                        
                        select count(*) from {newDelTable}
                    ");

                    var kidRowsCount = statement.ReadScalar<int>(conn);

                    totalDeletes += kidRowsCount;

                    log($"{delBatch,6}: Found {kidRowsCount} in {fk.FkTableSql.CommandText()}->{logStack.JoinStrings("->")}");

                    if (kidRowsCount == 0) {
                        SQL($"drop table {newDelTable}").ExecuteNonQuery(conn);
                    } else {
                        delBatch++;
                        DeleteKids(fk.FkTableSql, newDelTable, logStack.Prepend(fk.FkTableSql.CommandText()), depth + 1);
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

        public struct DeletionPerformance
        {
            public string Table;
            public TimeSpan DeletionDuration;
            public int RowCount;
        }

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

        sealed class PkCol : IMetaObject
        {
            public string Pk_table { get; set; }
            public string Pk_column { get; set; }
        }
    }
}
