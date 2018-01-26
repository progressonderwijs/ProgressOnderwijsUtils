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
        [NotNull]
        public static DeletionPerformance[] Bla<TId>(
            [NotNull] SqlCommandCreationContext conn,
            ParameterizedSql initialTable,
            ParameterizedSql initialPrimaryKeyColumn,
            [NotNull] TId[] idsToDelete,
            [NotNull] Action<string> log
            )
            where TId : struct, IConvertible, IComparable
        {
            Func<string, ParameterizedSql> dyn = ParameterizedSql.CreateDynamic;
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
                        rowGroup.First().Fk_table,
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
                    row => row.Pk_column,
                    StringComparer.OrdinalIgnoreCase
                );

            var tableImpact =
                keys.Select(g => g.Key).ToDictionary(
                    pk_table => pk_table,
                    pk_table =>
                        Utils.TransitiveClosure(
                            new[] { pk_table },
                            table => keys[table].Select(fk => fk.Fk_table)
                            )
                            .Count(),
                    StringComparer.OrdinalIgnoreCase
                    );

            int delBatch = 0;

            var deletionStack = new Stack<Action>();
            var perflog = new List<DeletionPerformance>();
            long totalDeletes = 0;

            void DeleteKids(ParameterizedSql tableName, ParameterizedSql tempTableName, SList<string> stack)
            {
                var ttJoin = dyn(pkeys[tableName.CommandText()].Select(col => "pk." + col + "=tt." + col).JoinStrings(" and "));

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

                foreach (var fk in fks) { //keys.OrderByDescending(g => tableImpact.GetOrDefault(g.Key));
                    var referencingPkCols = dyn(pkeys[fk.Fk_table].Select(col => "fk." + col).JoinStrings(", "));
                    var pkJoin = dyn(fk.columns.Select(col => "fk." + col.Fk_column + "=pk." + col.Pk_column).JoinStrings(" and "));

                    var newDelTable = dyn("[##del_" + delBatch + "]");
                    var whereClause = !tableName.CommandText().EqualsOrdinalCaseInsensitive(fk.Fk_table) 
                        ? SQL($"where 1=1") 
                        : SQL($"where ").Append(dyn(pkeys[tableName.CommandText()].Select(col => "pk." + col + "<>fk." + col).JoinStrings(" or ")));

                    var fkTableSql = dyn(fk.Fk_table);

                    var statement = SQL($@"
                        select {referencingPkCols} 
                        into {newDelTable}
                        from {fkTableSql} as fk
                        join {tableName} as pk on {pkJoin}
                        join {tempTableName} as tt on {ttJoin}
                        {whereClause}
                        ;
                        
                        select count(*) from {newDelTable}
                    ");

                    var kidRowsCount = statement.ReadScalar<int>(conn);

                    totalDeletes += kidRowsCount;

                    log($"{delBatch,6}: Found {kidRowsCount} in {fk.Fk_table}->{stack.JoinStrings("->")}");

                    if (kidRowsCount == 0) {
                        SQL($"drop table {newDelTable}").ExecuteNonQuery(conn);
                    } else {
                        delBatch++;
                        DeleteKids(fkTableSql, newDelTable, stack.Prepend(fk.Fk_table));
                    }
                }
            }

            DeleteKids(initialTable, delTable, SList.SingleElement(initialTable.CommandText()));
            while (deletionStack.Count > 0) {
                deletionStack.Pop()();
            }

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
        }

        sealed class PkCol : IMetaObject
        {
            public string Pk_table { get; set; }
            public string Pk_column { get; set; }
        }
    }
}
