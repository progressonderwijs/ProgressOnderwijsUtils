using System.Data.Common;
using ProgressOnderwijsUtils.SchemaReflection;

namespace ProgressOnderwijsUtils;

public sealed record BulkInsertTarget
{
    public const SqlBulkCopyOptions DefaultOptionsCorrespondingToInsertIntoBehavior = SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.KeepNulls;
    public string TableName { get; init; }
    public ColumnDefinition[] Columns { get; init; }
    public BulkCopyFieldMappingMode Mode { get; init; }
    public SqlBulkCopyOptions Options { get; init; }
    public bool SilentlySkipReadonlyTargetColumns { get; init; }

    public ParameterizedSql TableNameSql
        => ParameterizedSql.RawSql_PotentialForSqlInjection(TableName);

    public BulkInsertTarget(string tableName, ColumnDefinition[] columnDefinition)
        : this(tableName, columnDefinition, BulkCopyFieldMappingMode.ExactMatch, DefaultOptionsCorrespondingToInsertIntoBehavior) { }

    BulkInsertTarget(string tableName, ColumnDefinition[] columnDefinition, BulkCopyFieldMappingMode mode, SqlBulkCopyOptions options)
        => (TableName, Columns, Mode, Options, SilentlySkipReadonlyTargetColumns) = (tableName, columnDefinition, mode, options, false);

    public static BulkInsertTarget FromDatabaseDescription(DatabaseDescription.Table table)
        => new(table.QualifiedName, table.Columns.ArraySelect(ColumnDefinition.FromDbColumnMetaData));

    public static BulkInsertTarget LoadFromTable(SqlConnection conn, ParameterizedSql tableName)
        => LoadFromTable(conn, tableName.CommandText());

    public static BulkInsertTarget LoadFromTable(SqlConnection conn, string tableName)
        => FromCompleteSetOfColumns(tableName, DbColumnMetaData.ColumnMetaDatas(conn, tableName));

    public static BulkInsertTarget FromCompleteSetOfColumns(string tableName, IDbColumn[] columns)
        => new(tableName, columns.ArraySelect(ColumnDefinition.FromDbColumnMetaData));

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
            SilentlySkipReadonlyTargetColumns = SilentlySkipReadonlyTargetColumns,
        }.ValidateAndFilter(BulkInsertFieldMapping.Create(sourceFields, Columns));
}
