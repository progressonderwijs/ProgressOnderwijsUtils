#nullable disable
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using ExpressionToCodeLib;
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
            : this(tableName, columnDefinition, BulkCopyFieldMappingMode.ExactMatch, SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.FireTriggers) { }

        BulkInsertTarget(string tableName, ColumnDefinition[] columnDefinition, BulkCopyFieldMappingMode mode, SqlBulkCopyOptions options)
            => (TableName, Columns, Mode, Options) = (tableName, columnDefinition, mode, options);

        [NotNull]
        public static BulkInsertTarget FromDatabaseDescription([NotNull] DatabaseDescription.Table table)
            => new BulkInsertTarget(table.QualifiedName, table.Columns.ArraySelect((col, colIdx) => ColumnDefinition.FromDbColumnMetaData(col.ColumnMetaData, colIdx)));

        [NotNull]
        public static BulkInsertTarget LoadFromTable([NotNull] SqlConnection conn, ParameterizedSql tableName)
            => LoadFromTable(conn, tableName.CommandText());

        [NotNull]
        public static BulkInsertTarget LoadFromTable([NotNull] SqlConnection conn, [NotNull] string tableName)
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
            T>([NotNull] SqlConnection sqlConn, [NotNull] IEnumerable<T> pocos, CommandTimeout timeout = default, CancellationToken cancellationToken = default)
            where T : IReadImplicitly
        {
            using (var dbDataReader = new PocoDataReader<T>(pocos, cancellationToken.CreateLinkedTokenWith(timeout.ToCancellationToken(sqlConn)))) {
                BulkInsert(sqlConn, dbDataReader, pocos.GetType().ToCSharpFriendlyTypeName(), timeout);
            }
        }

        public void BulkInsert([NotNull] SqlConnection sqlConn, [NotNull] DataTable dataTable, CommandTimeout timeout = default)
        {
            using (var dbDataReader = dataTable.CreateDataReader()) {
                BulkInsert(sqlConn, dbDataReader, $"DataTable({dataTable.TableName})", timeout);
            }
        }

        public void BulkInsert(SqlConnection sqlConn, DbDataReader dbDataReader, string sourceNameForTracing, CommandTimeout timeout = default)
            => BulkInsertImplementation.Execute(sqlConn, TableName, Columns, Mode, Options, timeout, dbDataReader, sourceNameForTracing);
    }
}
