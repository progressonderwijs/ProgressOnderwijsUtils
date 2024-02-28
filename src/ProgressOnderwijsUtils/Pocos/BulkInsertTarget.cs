using System.Data.Common;
using ProgressOnderwijsUtils.SchemaReflection;

namespace ProgressOnderwijsUtils;

public sealed class BulkInsertTarget
{
    public enum ReadOnlyTargetError { Given, Suppressed, }

    public const SqlBulkCopyOptions DefaultOptionsCorrespondingToInsertIntoBehavior = SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.KeepNulls;
    public readonly string TableName;
    public readonly ColumnDefinition[] Columns;
    public readonly BulkCopyFieldMappingMode Mode;
    public readonly SqlBulkCopyOptions Options;
    public readonly ReadOnlyTargetError ReadOnlyTarget;

    public ParameterizedSql TableNameSql
        => ParameterizedSql.RawSql_PotentialForSqlInjection(TableName);

    public BulkInsertTarget(string tableName, ColumnDefinition[] columnDefinition)
        : this(tableName, columnDefinition, BulkCopyFieldMappingMode.ExactMatch, DefaultOptionsCorrespondingToInsertIntoBehavior, ReadOnlyTargetError.Given) { }

    BulkInsertTarget(string tableName, ColumnDefinition[] columnDefinition, BulkCopyFieldMappingMode mode, SqlBulkCopyOptions options, ReadOnlyTargetError readOnlyTarget)
        => (TableName, Columns, Mode, Options, ReadOnlyTarget) = (tableName, columnDefinition, mode, options, readOnlyTarget);

    public static BulkInsertTarget FromDatabaseDescription(DatabaseDescription.Table table)
        => new(table.QualifiedName, table.Columns.ArraySelect(ColumnDefinition.FromDbColumnMetaData));

    public static BulkInsertTarget LoadFromTable(SqlConnection conn, ParameterizedSql tableName)
        => LoadFromTable(conn, tableName.CommandText());

    public static BulkInsertTarget LoadFromTable(SqlConnection conn, string tableName)
        => FromCompleteSetOfColumns(tableName, DbColumnMetaData.ColumnMetaDatas(conn, tableName));

    public static BulkInsertTarget FromCompleteSetOfColumns(string tableName, IDbColumn[] columns)
        => new(tableName, columns.ArraySelect(ColumnDefinition.FromDbColumnMetaData));

    public BulkInsertTarget With(BulkCopyFieldMappingMode mode)
        => new(TableName, Columns, mode, Options, ReadOnlyTarget);

    public BulkInsertTarget With(SqlBulkCopyOptions options)
        => new(TableName, Columns, Mode, options, ReadOnlyTarget);

    public BulkInsertTarget With(ReadOnlyTargetError error)
        => new(TableName, Columns, Mode, Options, error);

    public void BulkInsert<[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)] T>(SqlConnection sqlConn, IEnumerable<T> pocos, CommandTimeout timeout = new(), CancellationToken cancellationToken = new())
        where T : IReadImplicitly
    {
        if (SmallBatchInsertImplementation.TrySmallBatchInsertOptimization(sqlConn, this, pocos, timeout) is { } toInsertViaSqlBulkCopy) {
            using var dbDataReader = new PocoDataReader<T>(toInsertViaSqlBulkCopy, cancellationToken.CreateLinkedTokenWith(timeout.ToCancellationToken(sqlConn)));
            BulkInsert(sqlConn, dbDataReader, typeof(T).ToCSharpFriendlyTypeName(), timeout);
        }
    }

    public void BulkInsert(SqlConnection sqlConn, DataTable dataTable, CommandTimeout timeout = new())
    {
        using var dbDataReader = dataTable.CreateDataReader();
        BulkInsert(sqlConn, dbDataReader, $"DataTable({dataTable.TableName})", timeout);
    }

    public void BulkInsert(SqlConnection sqlConn, DbDataReader dbDataReader, string sourceNameForTracing, CommandTimeout timeout = new())
        => BulkInsertImplementation.Execute(sqlConn, dbDataReader, this, sourceNameForTracing, timeout);

    public Maybe<BulkInsertFieldMapping[], string> CreateValidatedMapping(ColumnDefinition[] sourceFields)
        => new FieldMappingValidation {
            AllowExtraSourceColumns = Mode == BulkCopyFieldMappingMode.AllowExtraPocoProperties,
            AllowExtraTargetColumns = Mode == BulkCopyFieldMappingMode.AllowExtraDatabaseColumns,
            OverwriteAutoIncrement = Options.HasFlag(SqlBulkCopyOptions.KeepIdentity),
            SilentlySkipReadonlyTargetColumns = ReadOnlyTarget == ReadOnlyTargetError.Suppressed,
        }.ValidateAndFilter(BulkInsertFieldMapping.Create(sourceFields, Columns));
}
