namespace ProgressOnderwijsUtils;

public static class ParameterizedSqlExtensions
{
    [Pure]
    public static TSelf WithoutTimeout<TSelf>(this IWithTimeout<TSelf> sqlCommand)
        where TSelf : IWithTimeout<TSelf>
        => sqlCommand.WithTimeout(CommandTimeout.WithoutTimeout);

    [Pure]
    public static ParameterizedSql Append(this ParameterizedSql source, ParameterizedSql extra)
        => source + extra;

    /// <summary>
    /// Concatenate a sequence of sql expressions with a space separator.
    /// e.g.  concatenating 'a' and 'b' results in 'a b'
    /// </summary>
    public static ParameterizedSql ConcatenateSql(this IEnumerable<ParameterizedSql> sqlExpressions)
        => ConcatenateSql(sqlExpressions, EmptySql);

    /// <summary>
    /// Concatenate a sequence of sql expressions with a separator (surrounded by space).  A sequence of N items includes the separator N-1 times.
    /// e.g.  concatenating 'a' and 'b' with separator 'X' results in 'a X b'
    /// </summary>
    public static ParameterizedSql ConcatenateSql(this IEnumerable<ParameterizedSql> sqlExpressions, ParameterizedSql separator)
    {
        var builder = new ArrayBuilder<ISqlComponent>();
        var isBuilderEmpty = true;
        foreach (var expr in sqlExpressions) {
            if (expr is { impl: not null, IsEmpty: false, }) {
                if (isBuilderEmpty) {
                    isBuilderEmpty = false;
                } else if (separator is { impl: not null, IsEmpty: false, }) {
                    builder.Add(separator.impl);
                }
                builder.Add(expr.impl);
            }
        }
        return new SeveralSqlFragments(builder.ToArray()).BuildableToQuery();
    }

    [Pure]
    public static ParameterizedSql AsUnrolledSqlInExpression<T>(this IEnumerable<T> set)
        => set.Count() switch {
            0 => SQL($"(null)"),
            _ => SQL($"(").Append(set.Select(item => SQL($"{AsSqlParam(item)}")).ConcatenateSql(SQL($","))).Append(SQL($")")),
        };
}
