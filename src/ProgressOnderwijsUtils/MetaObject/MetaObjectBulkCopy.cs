using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.SchemaReflection;

namespace ProgressOnderwijsUtils
{
    public enum BulkCopyFieldMappingMode
    {
        ExactMatch,
        AllowExtraMetaObjectProperties,
        AllowExtraDatabaseColumns,
    }

    struct BulkInsertTargetTable
    {
        public string TableName;
        public ColumnDefinition[] Columns;

        public static BulkInsertTargetTable FromDatabaseDescription([NotNull] DatabaseDescription.Table table)
            => new BulkInsertTargetTable {
                TableName = table.QualifiedName,
                Columns = table.Columns
                    .Select((col, colIdx) => col.Is_Computed || col.Is_RowVersion ? null : ColumnDefinition.FromSqlXType(colIdx, col.ColumnName, col.User_Type_Id))
                    .Where(c => c != null)
                    .ToArray(),
            };

        public static BulkInsertTargetTable FromCompleteSetOfColumns(string tableName, DbColumnMetaData[] columns)
            => new BulkInsertTargetTable {
                TableName = tableName,
                Columns = InsertableColumnsFromCompleteSet(columns),
            };

        public static ColumnDefinition[] InsertableColumnsFromCompleteSet(DbColumnMetaData[] columns)
            => columns
                .Select((col, colIdx) => col.Is_Computed || col.Is_RowVersion ? null : ColumnDefinition.FromSqlXType(colIdx, col.ColumnName, col.User_Type_Id))
                .Where(c => c != null)
                .ToArray();
    }

    /// <summary>
    /// Contains extension methods to insert metaobjects (POCOs) into database tables using SqlBulkCopy.
    /// </summary>
    public static class MetaObjectBulkCopy
    {
        /// <summary>
        /// Performs a bulk insert.  Maps columns based on name, not order (unlike SqlBulkCopy by default); uses a 1 hour timeout, and options CheckConstraints | UseInternalTransaction.
        /// For more fine-grained control, create an SqlBulkCopy instance manually, and call bulkCopy.WriteMetaObjectsToServer(objs, sqlConnection, tableName)
        /// </summary>
        /// <typeparam name="T">The type of metaobject to be inserted</typeparam>
        /// <param name="metaObjects">The list of entities to insert</param>
        /// <param name="sqlconn">The Sql connection to write to</param>
        /// <param name="table">The table, including schema information, to write the entities into.</param>
        public static void BulkCopyToSqlServer<T>([NotNull] this IEnumerable<T> metaObjects, [NotNull] SqlCommandCreationContext sqlconn, [NotNull] DatabaseDescription.Table table)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
        {
            metaObjects.BulkCopyToSqlServer(sqlconn, table, BulkCopyFieldMappingMode.ExactMatch);
        }

        public static void BulkCopyToSqlServer<T>([NotNull] this IEnumerable<T> metaObjects, [NotNull] SqlCommandCreationContext sqlconn, [NotNull] DatabaseDescription.Table table, BulkCopyFieldMappingMode mode)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
        {
            metaObjects.BulkCopyToSqlServer(sqlconn, table.QualifiedName, table.Columns.ArraySelect(column => column.ColumnMetaData), mode);
        }

        /// <summary>
        /// Performs a bulk insert.  Maps columns based on name, not order (unlike SqlBulkCopy by default); uses a 1 hour timeout, and options CheckConstraints | UseInternalTransaction.
        /// For more fine-grained control, create an SqlBulkCopy instance manually, and call bulkCopy.WriteMetaObjectsToServer(objs, sqlConnection, tableName)
        /// </summary>
        /// <typeparam name="T">The type of metaobject to be inserted</typeparam>
        /// <param name="metaObjects">The list of entities to insert</param>
        /// <param name="sqlconn">The Sql connection to write to</param>
        /// <param name="tableName">The name of the table to insert into.</param>
        /// <param name="columns">The schema of the table as it currently exists in the database.</param>
        public static void BulkCopyToSqlServer<T>([NotNull] this IEnumerable<T> metaObjects, [NotNull] SqlCommandCreationContext sqlconn, [NotNull] string tableName, [NotNull] DbColumnMetaData[] columns)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
        {
            metaObjects.BulkCopyToSqlServer(sqlconn, tableName, columns, BulkCopyFieldMappingMode.ExactMatch);
        }

        public static void BulkCopyToSqlServer<T>([NotNull] this IEnumerable<T> metaObjects, [NotNull] SqlCommandCreationContext sqlconn, [NotNull] string tableName, [NotNull] DbColumnMetaData[] columns, BulkCopyFieldMappingMode mode)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
        {
            using (var bulkCopy = new SqlBulkCopy(sqlconn.Connection, SqlBulkCopyOptions.CheckConstraints, null)) {
                bulkCopy.BulkCopyTimeout = sqlconn.CommandTimeoutInS;
                var token = sqlconn.CommandTimeoutInS == 0
                    ? CancellationToken.None
                    : new CancellationTokenSource(TimeSpan.FromSeconds(sqlconn.CommandTimeoutInS)).Token;
                new MetaObjectBulkInsertOperation<T> {
                    bulkCopy = bulkCopy,
                    cancellationToken = token,
                    context = sqlconn,
                    metaObjects = metaObjects,
                    mode = mode,
                    targetTable = BulkInsertTargetTable.FromCompleteSetOfColumns(tableName,columns),
                }.Execute(); //.Wait(token);
            }
        }

        /// <summary>
        /// Writes meta-objects to the server.  If you use this method, it must be the only "WriteToServer" method you call on this bulk-copy instance because it sets the column mapping.
        /// </summary>
        public static void WriteMetaObjectsToServer<T>(
            [NotNull] this SqlBulkCopy bulkCopy,
            [NotNull] IEnumerable<T> metaObjects,
            [NotNull] SqlCommandCreationContext context,
            [NotNull] DatabaseDescription.Table table,
            CancellationToken cancellationToken)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
        {
            bulkCopy.WriteMetaObjectsToServer(metaObjects, context, table, BulkCopyFieldMappingMode.ExactMatch, cancellationToken);
        }

        public static void WriteMetaObjectsToServer<T>(
            [NotNull] this SqlBulkCopy bulkCopy,
            [NotNull] IEnumerable<T> metaObjects,
            [NotNull] SqlCommandCreationContext context,
            [NotNull] DatabaseDescription.Table table,
            BulkCopyFieldMappingMode mode,
            CancellationToken cancellationToken)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
        {
            new MetaObjectBulkInsertOperation<T> {
                bulkCopy = bulkCopy,
                cancellationToken = cancellationToken,
                context = context,
                metaObjects = metaObjects,
                mode = mode,
                targetTable = BulkInsertTargetTable.FromDatabaseDescription(table),
            }.Execute();
        }

        /// <summary>
        /// Writes meta-objects to the server.  If you use this method, it must be the only "WriteToServer" method you call on this bulk-copy instance because it sets the column mapping.
        /// </summary>
        public static void WriteMetaObjectsToServer<T>(
            [NotNull] this SqlBulkCopy bulkCopy,
            [NotNull] IEnumerable<T> metaObjects,
            [NotNull] SqlCommandCreationContext context,
            [NotNull] string tableName,
            [NotNull] DbColumnMetaData[] columns,
            CancellationToken cancellationToken)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
        {
            new MetaObjectBulkInsertOperation<T> {
                bulkCopy = bulkCopy,
                cancellationToken = cancellationToken,
                context = context,
                metaObjects = metaObjects,
                mode = BulkCopyFieldMappingMode.ExactMatch,
                targetTable = BulkInsertTargetTable.FromCompleteSetOfColumns(tableName, columns),
            }.Execute();
        }
    }
}
