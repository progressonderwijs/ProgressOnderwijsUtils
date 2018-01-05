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
        public static DeletionPerformance[] Bla<TId>(
            [NotNull] SqlCommandCreationContext conn,
            [NotNull] string initialTable,
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
                from {dyn(initialTable)}
                where {initialPrimaryKeyColumn} in {idsToDelete}
                ;
                select count(*) from {delTable}
            ").ReadScalar<int>(conn);

            log($"Recursively deleting {initialRowCountToDelete} rows (of {idsToDelete.Length} ids) from {initialTable})");

            var keys = SQL($@"
            select 
                fk_id=fk.object_id,
                fk_column_id=fkc.constraint_column_id,
                pk_table=SCHEMA_NAME(o_pk.schema_id) + '.' + o_pk.name,
                fk_table=SCHEMA_NAME(o_fk.schema_id) + '.' + o_fk.name,
                pk_column=c_pk.name,
                fk_column=c_fk.name
            from sys.foreign_keys fk
            join sys.foreign_key_columns fkc on fkc.constraint_object_id = fk.object_id
            join sys.columns c_pk on c_pk.object_id = fkc.referenced_object_id and c_pk.column_id = fkc.referenced_column_id
            join sys.columns c_fk on c_fk.object_id = fkc.parent_object_id and c_fk.column_id = fkc.parent_column_id
            join sys.objects o_pk on fk.referenced_object_id = o_pk.object_id
            join sys.objects o_fk on fk.parent_object_id = o_fk.object_id
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
                pk_table=SCHEMA_NAME(o_pk.schema_id) + '.' + o_pk.name,
                pk_column=c_pk.name
            from sys.key_constraints pk
            join sys.objects o_pk on pk.parent_object_id = o_pk.object_id
            join sys.index_columns as ic on ic.object_id = o_pk.object_id  and ic.index_id = pk.unique_index_id
            join sys.columns c_pk on c_pk.object_id = pk.parent_object_id  and ic.column_id=c_pk.column_id 
            where pk.type = 'PK'  and o_pk.type='U'
            order by o_pk.object_id, ic.column_id
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

            Stack<Action> deletionStack = new Stack<Action>();
            List<DeletionPerformance> perflog = new List<DeletionPerformance>();
            long totalDeletes = 0;
            Action<string, ParameterizedSql, SList<string>> deleteKids = null;
            deleteKids = (tableName, tempTableName, stack) => {
                var ttJoin = dyn(pkeys[tableName].Select(col => "pk." + col + "=tt." + col).JoinStrings(" and "));

                deletionStack.Push(() => {
                    var nrRowsToDelete = SQL($"select count(*) from {tempTableName}").ReadScalar<int>(conn);
                    log($"Delete {nrRowsToDelete} from {tableName}...");
                    var sw = Stopwatch.StartNew();
                    SQL($@"
                    delete pk
                    from {dyn(tableName)} pk
                    join {tempTableName} tt on {ttJoin};
                    
                    drop table {tempTableName};
                ").ExecuteNonQuery(conn);
                    sw.Stop();
                    log("...took {sw.Elapsed}");
                    perflog.Add(new DeletionPerformance { Table = tableName, RowCount = nrRowsToDelete, DeletionDuration = sw.Elapsed });
                });

                var fks = keys[tableName];

                foreach (var fk in fks) { //keys.OrderByDescending(g => tableImpact.GetOrDefault(g.Key));
                    var referencingPkCols = dyn(pkeys[fk.Fk_table].Select(col => "fk." + col).JoinStrings(", "));
                    var pkJoin = dyn(fk.columns.Select(col => "fk." + col.Fk_column + "=pk." + col.Pk_column).JoinStrings(" and "));

                    var newDelTable = dyn("[##del_" + delBatch + "]");
                    var whereClause = !tableName.EqualsOrdinalCaseInsensitive(fk.Fk_table)
                        ? SQL($"where 1=1")
                        : SQL($"where ").Append(
                            dyn(pkeys[tableName].Select(col => "pk." + col + "<>fk." + col).JoinStrings(" or "))
                            );

                    var statement = SQL($@"
                        select {referencingPkCols} 
                        into {newDelTable}
                        from {dyn(fk.Fk_table)} as fk
                        join {dyn(tableName)} as pk on {pkJoin}
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
                        deleteKids(fk.Fk_table, newDelTable, stack.Prepend(fk.Fk_table));
                    }
                }
            };
            deleteKids(initialTable, delTable, SList.SingleElement(initialTable));
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
