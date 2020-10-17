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
        [UsefulToKeep("Library function")]
        public static DeletionReport[] RecursivelyDelete(SqlConnection conn, DatabaseDescription.Table initialTableAsEntered, bool outputAllDeletedRows, Action<string>? logger, Func<string, bool>? stopCascading, string[] pkColumns, ParameterizedSql pksTVParameter)
        {
            void log(string message)
                => logger?.Invoke(message);

            bool StopCascading(ParameterizedSql tableName)
                => stopCascading?.Invoke(tableName.CommandText()) ?? false;

            DataTable? ExecuteDeletion(ParameterizedSql deletionCommand)
            {
                if (outputAllDeletedRows) {
                    return deletionCommand.OfDataTable().Execute(conn);
                } else {
                    deletionCommand.ExecuteNonQuery(conn);
                    return null;
                }
            }

            var outputClause = outputAllDeletedRows ? SQL($"output deleted.*") : default;

            var databaseDescription = initialTableAsEntered.db;

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

                var pkeysOfCurrentTable = databaseDescription.GetTableByName(tableName.CommandText()).PrimaryKey.ToArray();
                if (pkeysOfCurrentTable.None()) {
                    throw new InvalidOperationException($"Table {tableName.CommandText()} is missing a primary key");
                }
                var ttJoin = pkeysOfCurrentTable.Select(col => SQL($"pk.{col.SqlColumnName()}=tt.{col.SqlColumnName()}")).ConcatenateSql(SQL($" and "));

                deletionStack.Push(
                    () => {
                        var nrRowsToDelete = SQL($"select count(*) from {tempTableName}").ReadScalar<int>(conn);
                        log($"Delete {nrRowsToDelete} from {tableName.CommandText()}...");
                        var sw = Stopwatch.StartNew();
                        var deletedRows = ExecuteDeletion(
                            SQL(
                                $@"
                        delete pk
                        {outputClause}
                        from {tableName} pk
                        join {tempTableName} tt on {ttJoin};
                    
                        drop table {tempTableName};
                    "
                            )
                        );
                        sw.Stop();
                        log($"...took {sw.Elapsed}");
                        perflog.Add(new DeletionReport { Table = tableName.CommandText(), DeletedAtMostRowCount = nrRowsToDelete, DeletionDuration = sw.Elapsed, DeletedRows = deletedRows });
                    }
                );

                var fks = databaseDescription.GetTableByName(tableName.CommandText()).KeysFromReferencingChildren;

                foreach (var fk in fks) {
                    var pkeysOfReferencingTable = fk.ReferencingChildTable.PrimaryKey.ToArray();
                    var pkJoin = fk.Columns.Select(col => SQL($"fk.{col.ReferencingChildColumn.SqlColumnName()}=pk.{col.ReferencedParentColumn.SqlColumnName()}")).ConcatenateSql(SQL($" and "));
                    var newDelTable = ParameterizedSql.CreateDynamic("[#del_" + delBatch + "]");
                    if (pkeysOfReferencingTable.None()) {
                        log($"Warning: table {fk.ReferencingChildTable.QualifiedName}->{logStack.JoinStrings("->")} is missing a primary key");
                        deletionStack.Push(
                            () => {
                                var nrRowsToDelete = SQL(
                                    $@"
                                select count(*)
                                from {fk.ReferencingChildTable.QualifiedNameSql} fk
                                join {tableName} as pk on {pkJoin}
                                join {tempTableName} as tt on {ttJoin}
                            "
                                ).ReadScalar<int>(conn);
                                log($"Delete {nrRowsToDelete} from {fk.ReferencingChildTable.QualifiedName}...");
                                var sw = Stopwatch.StartNew();
                                var deletedRows = ExecuteDeletion(
                                    SQL(
                                        $@"
                                delete fk
                                {outputClause}
                                from {fk.ReferencingChildTable.QualifiedNameSql} fk
                                join {tableName} as pk on {pkJoin}
                                join {tempTableName} as tt on {ttJoin}
                            "
                                    )
                                );
                                sw.Stop();
                                log($"...took {sw.Elapsed}");
                                perflog.Add(new DeletionReport { Table = fk.ReferencingChildTable.QualifiedName, DeletedAtMostRowCount = nrRowsToDelete, DeletionDuration = sw.Elapsed, DeletedRows = deletedRows });
                            }
                        );
                        continue;
                    }

                    var whereClause = !tableName.CommandText().EqualsOrdinalCaseInsensitive(fk.ReferencingChildTable.QualifiedName)
                        ? SQL($"where 1=1")
                        : SQL($"where {pkeysOfCurrentTable.Select(col => SQL($"pk.{col.SqlColumnName()}<>fk.{col.SqlColumnName()}")).ConcatenateSql(SQL($" or "))}");
                    var referencingPkCols = pkeysOfReferencingTable.Select(col => SQL($"fk.{col.SqlColumnName()}")).ConcatenateSql(SQL($", "));
                    var statement = SQL(
                        $@"
                        select {referencingPkCols} 
                        into {newDelTable}
                        from {fk.ReferencingChildTable.QualifiedNameSql} as fk
                        join {tableName} as pk on {pkJoin}
                        join {tempTableName} as tt on {ttJoin}
                        {whereClause}
                        ;
                        
                        select count(*) from {newDelTable}
                    "
                    );

                    var kidRowsCount = statement.ReadScalar<int>(conn);

                    totalDeletes += kidRowsCount;

                    log($"{delBatch,6}: Found {kidRowsCount} in {fk.ReferencingChildTable.QualifiedName}->{logStack.JoinStrings("->")}");

                    if (kidRowsCount == 0) {
                        SQL($"drop table {newDelTable}").ExecuteNonQuery(conn);
                    } else {
                        delBatch++;
                        DeleteKids(fk.ReferencingChildTable.QualifiedNameSql, newDelTable, logStack.Prepend(fk.ReferencingChildTable.QualifiedName), depth + 1);
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
            public DataTable? DeletedRows;
        }
    }
}
