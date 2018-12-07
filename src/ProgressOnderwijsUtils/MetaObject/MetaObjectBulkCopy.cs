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

    public sealed class BulkInsertTarget
    {
        public string TableName;
        public ColumnDefinition[] Columns;
        public BulkCopyFieldMappingMode Mode;
        public SqlBulkCopyOptions Options = SqlBulkCopyOptions.CheckConstraints;
        BulkInsertTarget() { }

        public BulkInsertTarget(string tableName, ColumnDefinition[] columnDefinition)
        {
            TableName = tableName;
            Columns = columnDefinition;
        }

        public static BulkInsertTarget FromDatabaseDescription([NotNull] DatabaseDescription.Table table)
            => new BulkInsertTarget(table.QualifiedName, table.Columns.ArraySelect((col, colIdx) => ColumnDefinition.FromDbColumnMetaData(col.ColumnMetaData, colIdx)));

        public static BulkInsertTarget LoadFromTable(SqlCommandCreationContext conn, ParameterizedSql tableName)
            => LoadFromTable(conn, tableName.CommandText());

        public static BulkInsertTarget LoadFromTable(SqlCommandCreationContext conn, string tableName)
            => FromCompleteSetOfColumns(tableName, DbColumnMetaData.ColumnMetaDatas(conn, tableName));

        public static BulkInsertTarget FromCompleteSetOfColumns(string tableName, DbColumnMetaData[] columns)
            => new BulkInsertTarget(tableName, columns.ArraySelect(ColumnDefinition.FromDbColumnMetaData));

        public BulkInsertTarget With(BulkCopyFieldMappingMode mode)
            => new BulkInsertTarget { TableName = TableName, Columns = Columns, Mode = mode, Options = Options };

        public BulkInsertTarget With(SqlBulkCopyOptions options)
            => new BulkInsertTarget { TableName = TableName, Columns = Columns, Mode = Mode, Options = options };

        public void BulkInsert<T>(SqlCommandCreationContext sqlContext, IEnumerable<T> metaObjects, CancellationToken cancellationToken = default)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
        {
            MetaObjectBulkInsertOperation.Execute(sqlContext, TableName, Columns, Mode, Options, metaObjects, cancellationToken);
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
        /// <param name="sqlContext">The Sql connection to write to</param>
        /// <param name="table">The table, including schema information, to write the entities into.</param>
        public static void BulkCopyToSqlServer<T>([NotNull] this IEnumerable<T> metaObjects, [NotNull] SqlCommandCreationContext sqlContext, [NotNull] DatabaseDescription.Table table)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
            => BulkInsertTarget.FromDatabaseDescription(table).BulkInsert(sqlContext, metaObjects);

        /// <summary>
        /// Performs a bulk insert.  Maps columns based on name, not order (unlike SqlBulkCopy by default); uses a 1 hour timeout, and options CheckConstraints | UseInternalTransaction.
        /// For more fine-grained control, create an SqlBulkCopy instance manually, and call bulkCopy.WriteMetaObjectsToServer(objs, sqlConnection, tableName)
        /// </summary>
        public static void BulkCopyToSqlServer<T>(this IEnumerable<T> metaObjects, SqlCommandCreationContext sqlContext, BulkInsertTarget target)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
            => target.BulkInsert(sqlContext, metaObjects);
    }
}
