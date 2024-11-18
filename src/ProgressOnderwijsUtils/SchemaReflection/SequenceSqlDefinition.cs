namespace ProgressOnderwijsUtils.SchemaReflection;

public sealed record SequenceSqlDefinition(
    string QualifiedName,
    SqlSystemTypeId Type,
    bool IsCycling,
    bool IsCached,
    long? CacheSize,
    long Increment,
    long MinimumValue,
    long MaximumValue) : IWrittenImplicitly
{
    public void AppendCreationScript(StringBuilder sb)
    {
        if (Type != SqlSystemTypeId.BigInt && Type != SqlSystemTypeId.Int) {
            throw new NotImplementedException("Alleen int en bigint zijn ondersteunt");
        }

        _ = sb.Append($"create sequence {QualifiedName} as {Type}");
        if (Increment != 1) {
            _ = sb.Append($" increment by {Increment}");
        }
        if (Type switch {
                SqlSystemTypeId.Int => MinimumValue != int.MinValue,
                SqlSystemTypeId.BigInt => MinimumValue != long.MinValue,
                _ => true,
            }) {
            _ = sb.Append($" minvalue {MinimumValue}");
        }
        if (Type switch {
                SqlSystemTypeId.Int => MaximumValue != int.MaxValue,
                SqlSystemTypeId.BigInt => MaximumValue != long.MaxValue,
                _ => true,
            }) {
            _ = sb.Append($" maxvalue {MaximumValue}");
        }
        _ = sb.Append(IsCycling ? " cycle" : "");

        if (!IsCached) {
            _ = sb.Append(" no cache;");
        } else if (CacheSize.HasValue) {
            _ = sb.Append($" cache{CacheSize}");
        }
        _ = sb.Append("\n");
    }

    public static SequenceSqlDefinition[] LoadAll(SqlConnection conn)
        => SQL(
            $"""
            select
                QualifiedName = object_schema_name(s.object_id) + '.' + object_name(s.object_id)
                , Type = cast(s.system_type_id as int)
                , IsCycling = s.is_cycling
                , IsCached = s.is_cached
                , CacheSize = s.cache_size
                , Increment = cast(s.increment as bigint)
                , MinimumValue = cast(s.minimum_value as bigint)
                , MaximumValue = cast(s.maximum_value as bigint)
            from sys.sequences s
            """
        ).ReadPocos<SequenceSqlDefinition>(conn);
}
