namespace ProgressOnderwijsUtils.SchemaReflection;

public enum SqlIndexType : byte
{
    //https://docs.microsoft.com/en-us/sql/relational-databases/system-catalog-views/sys-indexes-transact-sql
    // ReSharper disable once UnusedMember.Global
    Heap = 0,
    ClusteredIndex = 1,
    NonClusteredIndex = 2,
    Xml = 3,
    Spatial = 4,
    ClusteredColumnStore = 5,
    NonClusteredColumnStore = 6,
    MemoryOptimizedNonClusteredHash = 7,
}

public static class SqlIndexTypeExtensions
{
    public static bool IsClusteredIndex(this SqlIndexType indexType)
        => indexType is SqlIndexType.ClusteredIndex or SqlIndexType.ClusteredColumnStore;

    public static bool IsColumnStore(this SqlIndexType indexType)
        => indexType is SqlIndexType.ClusteredColumnStore or SqlIndexType.NonClusteredColumnStore;

    public static ParameterizedSql ToSqlName(this SqlIndexType indexType)
        => indexType switch {
            SqlIndexType.Heap => SQL($"heap"),
            SqlIndexType.ClusteredIndex => SQL($"clustered"),
            SqlIndexType.NonClusteredIndex => SQL($"nonclustered"),
            SqlIndexType.Xml => SQL($"xml"),
            SqlIndexType.Spatial => SQL($"spatial"),
            SqlIndexType.ClusteredColumnStore => SQL($"clustered columnstore"),
            SqlIndexType.NonClusteredColumnStore => SQL($"nonclustered columnstore"),
            SqlIndexType.MemoryOptimizedNonClusteredHash => SQL($"nonclustered hash"),
            _ => throw new ArgumentOutOfRangeException(nameof(indexType), indexType, null),
        };
}
