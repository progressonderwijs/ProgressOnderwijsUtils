namespace ProgressOnderwijsUtils.SchemaReflection;

[DbIdEnum]
public enum DbIndexId { }

public sealed record DbObjectIndex : IWrittenImplicitly
{
    public DbObjectId ObjectId { get; init; }
    public DbIndexId IndexId { get; init; }
    public string? IndexNaam { get; init; }
    public byte IndexType { get; init; }
    public bool IsPrimaryKey { get; init; }
    public bool IsUniqueConstraint { get; init; }
    public bool IsUnique { get; init; }
    public string? Filter { get; init; }
    public byte DataCompressionType { get; init; }

    public static DbObjectIndex[] LoadAll(SqlConnection conn)
        => SQL(
            $"""
            select
                ObjectId = i.object_id
                , IndexId = i.index_id
                , IndexNaam = i.name
                , IndexType =  i.type
                , IsUniqueConstraint = i.is_unique_constraint
                , IsUnique =  i.is_unique
                , IsPrimaryKey =  i.is_primary_key
                , Filter = i.filter_definition
                , DataCompressionType = p.data_compression
            from sys.indexes i
            join sys.partitions p on p.object_id = i.object_id and p.index_id = i.index_id
            where i.object_id not in (select o.object_id from sys.objects o where o.is_ms_shipped = 1)
            """
        ).ReadPocos<DbObjectIndex>(conn);
}

public sealed record DbObjectIndexColumn : IWrittenImplicitly
{
    public DbObjectId ObjectId { get; init; }
    public DbIndexId IndexId { get; init; }
    public DbColumnId ColumnId { get; init; }
    public int KeyOrdinal { get; init; }
    public bool IsDescending { get; init; }
    public bool IsIncluded { get; init; }

    public static DbObjectIndexColumn[] LoadAll(SqlConnection conn)
        => SQL(
            $"""
            select
                ObjectId = sic.object_id
                , IndexId = sic.index_id
                , ColumnId = sic.column_id
                , KeyOrdinal = sic.key_ordinal
                , IsDescending = sic.is_descending_key
                , IsIncluded = sic.is_included_column
            from sys.index_columns sic
            where i.object_id not in (select o.object_id from sys.objects o where o.is_ms_shipped = 1)
            """
        ).ReadPocos<DbObjectIndexColumn>(conn);
}
