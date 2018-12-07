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
        ExactMatch, //must be default(BulkCopyFieldMappingMode)
        AllowExtraMetaObjectProperties,
        AllowExtraDatabaseColumns,
    }

    public struct BulkInsertTarget
    {
        public string TableName;
        public ColumnDefinition[] Columns;
        public BulkCopyFieldMappingMode Mode;

        public static BulkInsertTarget FromDatabaseDescription([NotNull] DatabaseDescription.Table table)
            => new BulkInsertTarget {
                TableName = table.QualifiedName,
                Columns = table.Columns
                    .Select((col, colIdx) => col.Is_Computed || col.Is_RowVersion ? null : ColumnDefinition.FromSqlXType(colIdx, col.ColumnName, col.User_Type_Id))
                    .Where(c => c != null)
                    .ToArray(),
            };

        public static BulkInsertTarget FromCompleteSetOfColumns(string tableName, DbColumnMetaData[] columns)
            => new BulkInsertTarget {
                TableName = tableName,
                Columns = InsertableColumnsFromCompleteSet(columns),
            };

        public static ColumnDefinition[] InsertableColumnsFromCompleteSet(DbColumnMetaData[] columns)
            => columns
                .Select((col, colIdx) => col.Is_Computed || col.Is_RowVersion ? null : ColumnDefinition.FromSqlXType(colIdx, col.ColumnName, col.User_Type_Id))
                .Where(c => c != null)
                .ToArray();

        public BulkInsertTarget WithMode(BulkCopyFieldMappingMode mode)
            => new BulkInsertTarget { TableName = TableName, Columns = Columns, Mode = mode };

        /// <summary>
        /// Writes meta-objects to the server.  If you use this method, it must be the only "WriteToServer" method you call on this bulk-copy instance because it sets the column mapping.
        /// </summary>
        public void WriteObjectsToServer<T>(SqlBulkCopy bulkCopy, IEnumerable<T> metaObjects, SqlCommandCreationContext context, CancellationToken cancellationToken)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
        {
            new MetaObjectBulkInsertOperation<T> {
                bulkCopy = bulkCopy,
                cancellationToken = cancellationToken,
                context = context,
                metaObjects = metaObjects,
                Target = this,
            }.Execute();
        }
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
                BulkInsertTarget.FromCompleteSetOfColumns(tableName, columns).WithMode(mode)
                    .WriteObjectsToServer(bulkCopy, metaObjects, sqlconn, default);
            }
        }
    }
}
