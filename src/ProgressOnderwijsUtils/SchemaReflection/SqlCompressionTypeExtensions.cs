namespace ProgressOnderwijsUtils.SchemaReflection;

public enum SqlCompressionType : byte
{
    //https://docs.microsoft.com/en-us/sql/relational-databases/system-catalog-views/sys-partitions-transact-sql
    None = 0,
    Row = 1,
    Page = 2,
    ColumnStore = 3,
    ColumnStoreArchive = 4,
}

public static class SqlCompressionTypeExtensions
{
    public static bool IsColumnStore(this SqlCompressionType compression)
        => compression is SqlCompressionType.ColumnStore or SqlCompressionType.ColumnStoreArchive;

    public static ParameterizedSql ToSqlName(this SqlCompressionType compression)
        => compression switch {
            SqlCompressionType.None => SQL($"none"),
            SqlCompressionType.Row => SQL($"row"),
            SqlCompressionType.Page => SQL($"page"),
            SqlCompressionType.ColumnStore => SQL($"columnstore"),
            SqlCompressionType.ColumnStoreArchive => SQL($"columnstore_archive"),
            _ => throw new ArgumentOutOfRangeException(nameof(compression), compression, null),
        };
}
