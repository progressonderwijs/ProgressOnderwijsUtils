using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Threading;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.SchemaReflection;
using System.Linq;
using System;

namespace ProgressOnderwijsUtils
{
    public sealed class BulkInsertTarget
    {
        public const SqlBulkCopyOptions defaultBulkCopyOptions = SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.KeepNulls;
        public readonly string TableName;
        public readonly ColumnDefinition[] Columns;
        public readonly BulkCopyFieldMappingMode Mode;
        public readonly SqlBulkCopyOptions Options;

        public BulkInsertTarget(string tableName, ColumnDefinition[] columnDefinition)
            : this(tableName, columnDefinition, BulkCopyFieldMappingMode.ExactMatch, defaultBulkCopyOptions) { }

        BulkInsertTarget(string tableName, ColumnDefinition[] columnDefinition, BulkCopyFieldMappingMode mode, SqlBulkCopyOptions options)
            => (TableName, Columns, Mode, Options) = (tableName, columnDefinition, mode, options);

        public static BulkInsertTarget FromDatabaseDescription(DatabaseDescription.Table table)
            => new BulkInsertTarget(table.QualifiedName, table.Columns.ArraySelect((col, colIdx) => ColumnDefinition.FromDbColumnMetaData(col.ColumnMetaData, colIdx)));

        public static BulkInsertTarget LoadFromTable(SqlConnection conn, ParameterizedSql tableName)
            => LoadFromTable(conn, tableName.CommandText());

        public static BulkInsertTarget LoadFromTable(SqlConnection conn, string tableName)
            => FromCompleteSetOfColumns(tableName, DbColumnMetaData.ColumnMetaDatas(conn, tableName));

        public static BulkInsertTarget FromCompleteSetOfColumns(string tableName, DbColumnMetaData[] columns)
            => new BulkInsertTarget(tableName, columns.ArraySelect(ColumnDefinition.FromDbColumnMetaData));

        public BulkInsertTarget With(BulkCopyFieldMappingMode mode)
            => new BulkInsertTarget(TableName, Columns, mode, Options);

        public BulkInsertTarget With(SqlBulkCopyOptions options)
            => new BulkInsertTarget(TableName, Columns, Mode, options);

        public void BulkInsert<[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
            T>(SqlConnection sqlConn, IEnumerable<T> pocos, CommandTimeout timeout = default, CancellationToken cancellationToken = default)
            where T : IReadImplicitly
        {
            if (SmallBatchInsertImplementation<T>.TrySmallBatchInsertOptimization(sqlConn, this, pocos, timeout) is {} toInsertViaSqlBulkCopy) {
                using var dbDataReader = new PocoDataReader<T>(toInsertViaSqlBulkCopy, cancellationToken.CreateLinkedTokenWith(timeout.ToCancellationToken(sqlConn)));
                BulkInsert(sqlConn, dbDataReader, typeof(T).ToCSharpFriendlyTypeName(), timeout);
            }
        }

        public void BulkInsert(SqlConnection sqlConn, DataTable dataTable, CommandTimeout timeout = default)
        {
            using var dbDataReader = dataTable.CreateDataReader();
            BulkInsert(sqlConn, dbDataReader, $"DataTable({dataTable.TableName})", timeout);
        }

        public void BulkInsert(SqlConnection sqlConn, DbDataReader dbDataReader, string sourceNameForTracing, CommandTimeout timeout = default)
            => BulkInsertImplementation.Execute(sqlConn, TableName, Columns, Mode, Options, timeout, dbDataReader, sourceNameForTracing);
    }
}
