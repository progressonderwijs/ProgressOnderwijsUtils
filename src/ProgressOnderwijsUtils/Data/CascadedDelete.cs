using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;
using ProgressOnderwijsUtils.SchemaReflection;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using static ProgressOnderwijsUtils.SafeSql;

// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace ProgressOnderwijsUtils
{
    public static class CascadedDelete
    {
        public static DeletionReport[] RecursivelyDelete<TId>(SqlConnection conn, DatabaseDescription.Table initialTableAsEntered, bool outputAllDeletedRows, Action<string>? logger, Func<string, bool>? stopCascading, string pkColumn, params TId[] pksToDelete)
            where TId : Enum
        {
            var pkColumnSql = ParameterizedSql.CreateDynamic(pkColumn);
            return RecursivelyDelete(conn, initialTableAsEntered, outputAllDeletedRows, logger, stopCascading, new[] { pkColumn }, SQL($@"
                select {pkColumnSql} = q.QueryTableValue 
                from {pksToDelete} q
            "));
        }

        public static DeletionReport[] RecursivelyDelete<[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
            TId>(
            SqlConnection conn,
            DatabaseDescription.Table initialTableAsEntered,
            bool outputAllDeletedRows,
            Action<string>? logger,
            Func<string, bool>? stopCascading,
            params TId[] pksToDelete
        )
            where TId : IReadImplicitly
        {
            var pksTable = SQL($"#pksTable");
            var pkColumns = PocoUtils.GetProperties<TId>().Select(pocoProperty => pocoProperty.Name).ToArray();
            var pkColumnsSql = pkColumns.ArraySelect(ParameterizedSql.CreateDynamic);

            var pkColumnsMetaData = initialTableAsEntered.Columns.Select(col => col.ColumnMetaData).Where(col => pkColumns.Contains(col.ColumnName, StringComparer.OrdinalIgnoreCase)).ToArray();
            pkColumnsMetaData.CreateNewTableQuery(pksTable).ExecuteNonQuery(conn);

            var target = new BulkInsertTarget(
                pksTable.CommandText(),
                PocoUtils.GetProperties<TId>().ArraySelect((pocoProperty, index) => new ColumnDefinition(pocoProperty.DataType, pocoProperty.Name, index, ColumnAccessibility.Normal))
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

        public static DeletionReport[] RecursivelyDelete(SqlConnection conn, DatabaseDescription.Table initialTableAsEntered, bool outputAllDeletedRows, Action<string>? logger, Func<string, bool>? stopCascading, DataTable pksToDelete)
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
        public static DeletionReport[] RecursivelyDelete(SqlConnection conn, DatabaseDescription.Table initialTableAsEntered, bool outputAllDeletedRows, Action<string>? logger, Func<string, bool>? stopCascading, string[] pkColumns, ParameterizedSql pksTVParameter)
        {
            void log(string message)
                => logger?.Invoke(message);

            bool StopCascading(ParameterizedSql tableName)
                => stopCascading?.Invoke(tableName.CommandText()) ?? false;

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
            var stackOfEnqueuedDeletions = new Dictionary<(DbObjectId objectId, ParameterizedSql keyList), Stack<ParameterizedSql>>();

            var deletionStack = new Stack<Action>();
            var perflog = new List<DeletionReport>();
            long totalDeletes = 0;

            DeleteKids(initialTableAsEntered, delTable, initialKeyColumns, SList.SingleElement(initialTableAsEntered.QualifiedName));
            while (deletionStack.Count > 0) {
                deletionStack.Pop()();
            }

            log($"{totalDeletes} rows Deleted");
            return perflog.ToArray();

            void DeleteKids(DatabaseDescription.Table table, ParameterizedSql tempTableName, ParameterizedSql[] keyColumns, SList<string> logStack)
            {
                if (StopCascading(table.QualifiedNameSql)) {
                    return;
                }
                var onStackDeletionTables = stackOfEnqueuedDeletions.GetOrAdd((table.ObjectId, keyColumns.ConcatenateSql(SQL($","))), new Stack<ParameterizedSql>());
                if (onStackDeletionTables.Any()) {
                    var unionOfProblems = onStackDeletionTables.Select(onStackTmpTable => SQL($"(select * from {onStackTmpTable} intersect select * from {tempTableName})")).ConcatenateSql(SQL($" union all "));
                    var cycleDetected = SQL($"select iif(exists({unionOfProblems}), {true}, {false})").ReadScalar<bool>(conn);

                    if (cycleDetected) {
                        throw new InvalidOperationException($"A cycle was detected in the current dependency chain: {logStack.JoinStrings("->")}");
                    }
                }
                onStackDeletionTables.Push(tempTableName);

                var ttJoin = keyColumns.Select(col => SQL($"pk.{col}=tt.{col}")).ConcatenateSql(SQL($" and "));

                deletionStack.Push(
                    () => {
                        var nrRowsToDelete = SQL($"select count(*) from {tempTableName}").ReadScalar<int>(conn);
                        log($"Delete {nrRowsToDelete} from {table.QualifiedName}...");

                        ParameterizedSql DeletionQuery(ParameterizedSql outputClause)
                            => SQL($@"
                                delete pk
                                {outputClause}
                                from {table.QualifiedNameSql} pk
                                join {tempTableName} tt on {ttJoin};
                            ");

                        DataTable? DeletionExecution()
                        {
                            if (!outputAllDeletedRows) {
                                DeletionQuery(default(ParameterizedSql)).ExecuteNonQuery(conn);
                                return null;
                            }

                            if (table.Triggers.None()) {
                                return DeletionQuery(SQL($"output deleted.*")).OfDataTable().Execute(conn);
                            }

                            return SQL($@"
                                declare @output_deleted table(
                                    {table.Columns.Select(col => col.ColumnMetaData.AsStaticRowVersion().ToSqlColumnDefinitionSql()).ConcatenateSql(SQL($", "))}
                                );
                                {DeletionQuery(SQL($"output deleted.* into @output_deleted"))}
                                select * from @output_deleted;
                            ").OfDataTable().Execute(conn);
                        }

                        var sw = Stopwatch.StartNew();
                        var deletedRows = DeletionExecution();
                        SQL($"drop table {tempTableName};").ExecuteNonQuery(conn);
                        sw.Stop();
                        log($"...took {sw.Elapsed}");
                        perflog.Add(new() { Table = table.QualifiedName, DeletedAtMostRowCount = nrRowsToDelete, DeletionDuration = sw.Elapsed, DeletedRows = deletedRows });
                    }
                );

                foreach (var fk in table.KeysFromReferencingChildren) {
                    var childTable = fk.ReferencingChildTable;
                    var pkJoin = fk.Columns.Select(col => SQL($"fk.{col.ReferencingChildColumn.SqlColumnName()}=pk.{col.ReferencedParentColumn.SqlColumnName()}")).ConcatenateSql(SQL($" and "));
                    var newDelTable = ParameterizedSql.CreateDynamic("[#del_" + delBatch + "]");
                    var whereClause = !table.QualifiedName.EqualsOrdinalCaseInsensitive(childTable.QualifiedName)
                        ? SQL($"where 1=1")
                        : SQL($"where {keyColumns.Select(col => SQL($"pk.{col}<>fk.{col} or fk.{col} is null")).ConcatenateSql(SQL($" or "))}");
                    var referencingCols = fk.Columns.ArraySelect(col => col.ReferencingChildColumn.SqlColumnName());
                    var selectClause = referencingCols.Select(col => SQL($"fk.{col}")).ConcatenateSql(SQL($", "));
                    var statement = SQL(
                        $@"
                        select {selectClause} 
                        into {newDelTable}
                        from {childTable.QualifiedNameSql} as fk
                        join {table.QualifiedNameSql} as pk on {pkJoin}
                        join {tempTableName} as tt on {ttJoin}
                        {whereClause}
                        ;
                        
                        select count(*) from {newDelTable}
                    "
                    );

                    var kidRowsCount = statement.ReadScalar<int>(conn);

                    totalDeletes += kidRowsCount;

                    var newChain = logStack.Prepend(childTable.QualifiedName);
                    log($"{delBatch,6}: Found {kidRowsCount} in {newChain.JoinStrings("->")}");

                    if (kidRowsCount == 0) {
                        SQL($"drop table {newDelTable}").ExecuteNonQuery(conn);
                    } else {
                        delBatch++;
                        DeleteKids(childTable, newDelTable, referencingCols, newChain);
                    }
                }
                onStackDeletionTables.Pop();
            }
        }

        public struct DeletionReport
        {
            public string Table;
            public TimeSpan DeletionDuration;
            public int DeletedAtMostRowCount;
            public DataTable? DeletedRows;
        }
    }
}
