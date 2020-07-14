using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.SchemaReflection;

namespace ProgressOnderwijsUtils
{
    public sealed class BulkInsertTarget
    {
        readonly string TableName;
        readonly ColumnDefinition[] Columns;
        readonly BulkCopyFieldMappingMode Mode;
        readonly SqlBulkCopyOptions Options;

        public BulkInsertTarget(string tableName, ColumnDefinition[] columnDefinition)
            : this(tableName, columnDefinition, BulkCopyFieldMappingMode.ExactMatch, SqlBulkCopyOptions.CheckConstraints) { }

        BulkInsertTarget(string tableName, ColumnDefinition[] columnDefinition, BulkCopyFieldMappingMode mode, SqlBulkCopyOptions options)
            => (TableName, Columns, Mode, Options) = (tableName, columnDefinition, mode, options);

        [NotNull]
        public static BulkInsertTarget FromDatabaseDescription([NotNull] DatabaseDescription.Table table)
            => new BulkInsertTarget(table.QualifiedName, table.Columns.ArraySelect((col, colIdx) => ColumnDefinition.FromDbColumnMetaData(col.ColumnMetaData, colIdx)));

        [NotNull]
        public static BulkInsertTarget LoadFromTable([NotNull] SqlCommandCreationContext conn, ParameterizedSql tableName)
            => LoadFromTable(conn, tableName.CommandText());

        [NotNull]
        public static BulkInsertTarget LoadFromTable([NotNull] SqlCommandCreationContext conn, [NotNull] string tableName)
            => FromCompleteSetOfColumns(tableName, DbColumnMetaData.ColumnMetaDatas(conn, tableName));

        [NotNull]
        public static BulkInsertTarget FromCompleteSetOfColumns(string tableName, [NotNull] DbColumnMetaData[] columns)
            => new BulkInsertTarget(tableName, columns.ArraySelect(ColumnDefinition.FromDbColumnMetaData));

        [NotNull]
        public BulkInsertTarget With(BulkCopyFieldMappingMode mode)
            => new BulkInsertTarget(TableName, Columns, mode, Options);

        [NotNull]
        public BulkInsertTarget With(SqlBulkCopyOptions options)
            => new BulkInsertTarget(TableName, Columns, Mode, options);

        public void BulkInsert<[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
            T>([NotNull] SqlCommandCreationContext sqlContext, [NotNull] IEnumerable<T> metaObjects, CancellationToken cancellationToken = default)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
            => MetaObjectBulkInsertOperation.Execute(sqlContext, TableName, Columns, Mode, Options, metaObjects, cancellationToken);
    }
}
