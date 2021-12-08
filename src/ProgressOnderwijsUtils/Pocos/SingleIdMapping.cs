namespace ProgressOnderwijsUtils;

public readonly struct SingleIdMapping<TId>
    where TId : struct, IConvertible
{
    public SingleIdMapping(TId? fromCode, TId intoDatabase)
    {
        FromCode = fromCode;
        IntoDatabase = intoDatabase;
    }

    public TId? FromCode { get; }
    public TId IntoDatabase { get; }
}

public static class SingleIdMappingExtensions
{
    public static DistinctArray<TId> DatabaseIds<TId>(this SingleIdMapping<TId>[] mappings)
        where TId : struct, IConvertible
        => mappings.Select(mapping => mapping.IntoDatabase).ToDistinctArrayFromDistinct();

    public static Dictionary<TId, TId> CodeToDatabaseIds<TId>(this SingleIdMapping<TId>[] mappings)
        where TId : struct, IConvertible
        => mappings.ToDictionary(m => m.FromCode.AssertNotNull(), m => m.IntoDatabase);
}
