namespace ProgressOnderwijsUtils;

sealed class QueryTableValuedParameterComponent<TIn, TOut> : IQueryParameter, ISqlComponent
    where TOut : IReadImplicitly
{
    static readonly string columnListClause =
        PocoUtils.GetProperties<TOut>()
            .Select(pocoProperty => $"TVP.{pocoProperty.Name}")
            .JoinStrings(", ");

    readonly string DbTypeName;
    readonly IEnumerable<TIn> values;
    readonly Func<IEnumerable<TIn>, TOut[]> projection;
    int cachedLength = -1;

    public object EquatableValue
        => new TvpEqualityToken<TIn>(values, DbTypeName);

    internal QueryTableValuedParameterComponent(string dbTypeName, IEnumerable<TIn> values, Func<IEnumerable<TIn>, TOut[]> projection)
    {
        this.values = values;
        this.projection = projection;
        DbTypeName = dbTypeName;
        if (values is IReadOnlyList<TIn> arrayLike) {
            cachedLength = arrayLike.Count; //if you pass in something with a length, we can reliably determine its size
        }
    }

    public void AppendTo<TCommandFactory>(ref TCommandFactory factory)
        where TCommandFactory : struct, ICommandFactory
    {
        var paramName = factory.RegisterParameterAndGetName(this);
        ParameterizedSqlFactory.AppendSql(ref factory, "(select ");
        if (cachedLength >= 0) {
            var roundedUpToNearestPowerOfFourOrZeroOrMinusOne = cachedLength == 0
                ? 0
                : cachedLength > 1 << 20
                    ? -1
                    : 1 << (Utils.LogBase2RoundedUp((uint)cachedLength) + 1 >> 1 << 1);
            if (roundedUpToNearestPowerOfFourOrZeroOrMinusOne >= 0) {
                if (cachedLength > roundedUpToNearestPowerOfFourOrZeroOrMinusOne) {
                    throw new($"Internal error: {cachedLength} > {roundedUpToNearestPowerOfFourOrZeroOrMinusOne}");
                }
                ParameterizedSqlFactory.AppendSql(ref factory, "top(");
                ParameterizedSqlFactory.AppendSql(ref factory, roundedUpToNearestPowerOfFourOrZeroOrMinusOne.ToString());
                ParameterizedSqlFactory.AppendSql(ref factory, ") ");
            }
        }

        ParameterizedSqlFactory.AppendSql(ref factory, columnListClause);
        ParameterizedSqlFactory.AppendSql(ref factory, " from ");
        ParameterizedSqlFactory.AppendSql(ref factory, paramName);
        ParameterizedSqlFactory.AppendSql(ref factory, " TVP)");
    }

    public void ToSqlParameter(ref SqlParamArgs paramArgs)
    {
        var objs = projection(values);
        cachedLength = objs.Length;
        //if you pass in something without a length, then only the first consumer gets a size;
        paramArgs.Value = new PocoDataReader<TOut>(objs, CancellationToken.None);
        paramArgs.TypeName = DbTypeName;
    }
}

sealed class TvpEqualityToken<TIn>(IEnumerable<TIn> Values, string DbTypeName) : IEquatable<TvpEqualityToken<TIn>>
{
    readonly IEnumerable<TIn> values = Values;
    readonly string dbTypeName = DbTypeName;

    static readonly SequenceEqualityComparer<TIn> sequenceEqualityComparer = new(
        new EqualsEqualityComparer<TIn>(
            (a, b) => StructuralComparisons.StructuralEqualityComparer.Equals(a, b),
            a => a is null ? 0 : StructuralComparisons.StructuralEqualityComparer.GetHashCode(a)
        ),
        false
    );

    public bool Equals(TvpEqualityToken<TIn>? other)
        => other is not null
            && dbTypeName == other.dbTypeName
            && sequenceEqualityComparer.Equals(values, other.values);

    public override bool Equals(object? obj)
        => Equals(obj as TvpEqualityToken<TIn>);

    public override int GetHashCode()
        => dbTypeName.GetHashCode() ^ sequenceEqualityComparer.GetHashCode(values);
}

sealed class SingletonQueryTableValuedParameterComponent<TOut> : ISqlComponent
    where TOut : IReadImplicitly
{
    readonly TOut row;

    internal SingletonQueryTableValuedParameterComponent(TOut row)
        => this.row = row;

    public void AppendTo<TCommandFactory>(ref TCommandFactory factory)
        where TCommandFactory : struct, ICommandFactory
    {
        var isFirst = true;
        ParameterizedSqlFactory.AppendSql(ref factory, "(select ");
        foreach (var property in PocoUtils.GetProperties<TOut>()) {
            if (property.Getter is { } getter) {
                if (!isFirst) {
                    ParameterizedSqlFactory.AppendSql(ref factory, ", ");
                } else {
                    isFirst = false;
                }

                ParameterizedSqlFactory.AppendSql(ref factory, property.Name);
                ParameterizedSqlFactory.AppendSql(ref factory, " = ");
                QueryScalarParameterComponent.AppendScalarParameter(ref factory, getter(row));
            }
        }
        ParameterizedSqlFactory.AppendSql(ref factory, ")");
    }
}
