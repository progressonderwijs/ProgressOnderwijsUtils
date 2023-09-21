namespace ProgressOnderwijsUtils.SchemaReflection;

public interface IDbColumn
{
    DbColumnMetaData ColumnMetaData { get; }
    DbColumnId ColumnId { get; }
    string ColumnName { get; }
    bool IsPrimaryKey { get; }
    bool IsRowVersion { get; }
    bool IsComputed { get; }
    bool IsNullable { get; }
    SqlSystemTypeId UserTypeId { get; }
    bool IsString { get; }
    bool IsUnicode { get; }
    short MaxLength { get; }
    byte Precision { get; }
    byte Scale { get; }
    bool HasAutoIncrementIdentity { get; }
}

public static class DbColumnExtensions
{
    static readonly Regex isSafeForSql = new("^[a-zA-Z0-9_]+$", RegexOptions.ECMAScript | RegexOptions.Compiled);
    public static readonly string DefaultDbCollation = "SQL_Latin1_General_CP1_CI_AS";

    [Pure]
    public static ParameterizedSql SqlColumnName(this IDbColumn column)
        => ParameterizedSql.CreateDynamic(isSafeForSql.IsMatch(column.ColumnName) ? column.ColumnName : throw new NotSupportedException("this isn't safe!"));

    public static string ToSqlColumnDefinition(this IDbColumn column)
        => $"{column.ColumnName} {column.ToSqlTypeName()}";

    public static ParameterizedSql ToSqlColumnDefinitionSql(this IDbColumn column)
        => ParameterizedSql.CreateDynamic($"{column.ColumnName} {column.ToSqlTypeName()}");

    public static string ToSqlTypeName(this IDbColumn column)
        => column.ToSqlTypeNameWithoutNullability() + CollationForStringColumn(column) + column.NullabilityAnnotation();

    static string CollationForStringColumn(IDbColumn column)
        => column is { IsString: true, UserTypeId: not SqlSystemTypeId.Xml, } ? $" collate {column.ColumnMetaData.CollationName ?? DefaultDbCollation}" : "";

    public static string ToSqlTypeNameWithoutNullability(this IDbColumn column)
        => column.UserTypeId.SqlUnderlyingTypeInfo().SqlTypeName + column.ColumnPrecisionSpecifier();

    static string NullabilityAnnotation(this IDbColumn column)
        => column.IsNullable ? " null" : " not null";

    static string ColumnPrecisionSpecifier(this IDbColumn column)
        => column.UserTypeId switch {
            _ when column.SemanticMaxLength(out var supportMaxLen, out var hasMaxLen) is var maxLen && supportMaxLen => hasMaxLen ? $"({maxLen})" : "(max)",
            SqlSystemTypeId.Decimal or SqlSystemTypeId.Numeric => $"({column.Precision},{column.Scale})",
            SqlSystemTypeId.DateTime2 or SqlSystemTypeId.DateTimeOffset or SqlSystemTypeId.Time when column.Scale != 7 => $"({column.Scale})",
            _ => "",
        };

    public static DataColumn ToDataColumn(this IDbColumn column)
        => new(column.ColumnName, column.UserTypeId.SqlUnderlyingTypeInfo().ClrType);

    public static IDbColumn AsStaticRowVersion(this IDbColumn column)
    {
        if (column.IsRowVersion) {
            return column.ColumnMetaData with {
                UserTypeId = SqlSystemTypeId.Binary,
                MaxLength = 8,
            };
        } else {
            return column;
        }
    }

    public static bool IsReadOnly(this IDbColumn sqlColumn)
        => sqlColumn.HasAutoIncrementIdentity || sqlColumn.IsComputed || sqlColumn.IsRowVersion;

    public static short SemanticMaxLength(this IDbColumn column, out bool typeSupportsMaxLength, out bool hasMaxLength)
    {
        if (column.UserTypeId is SqlSystemTypeId.NVarChar or SqlSystemTypeId.NChar) {
            typeSupportsMaxLength = true;
            hasMaxLength = column.MaxLength > 0;
            return (short)(column.MaxLength >> 1);
        } else if (column.UserTypeId is SqlSystemTypeId.VarChar or SqlSystemTypeId.Char or SqlSystemTypeId.VarBinary or SqlSystemTypeId.Binary) {
            typeSupportsMaxLength = true;
            hasMaxLength = column.MaxLength > 0;
            return column.MaxLength;
        } else {
            typeSupportsMaxLength = false;
            hasMaxLength = false;
            return 0;
        }
    }
}
