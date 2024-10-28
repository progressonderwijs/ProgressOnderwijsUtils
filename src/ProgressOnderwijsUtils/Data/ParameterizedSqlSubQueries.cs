namespace ProgressOnderwijsUtils;

public static class ParameterizedSqlSubQueries
{
    //TODO: dit aanzetten voor datasource tests
    // ReSharper disable once UnusedMember.Global
    public static void AssertNoVariableColumns(ParameterizedSql parameterizedSql)
    {
        var commandText = parameterizedSql.CommandText();
        var commandTextWithoutComments = Regex.Replace(
            commandText,
            @"/\*.*?\*/|--.*?$",
            "",
            RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Multiline
        );
        if (Regex.IsMatch(commandTextWithoutComments, @"(?<!count\()\*")) {
            throw new InvalidOperationException($"{parameterizedSql.GetType().FullName}: Query may not use * as that might cause runtime exceptions in productie when DB changes:\n{commandText}");
        }
    }

    [Pure]
    public static ParameterizedSql CreateOrderByClause(OrderByColumns sortOrder)
        => sortOrder.Columns.None()
            ? EmptySql
            : ParameterizedSql.RawSql_PotentialForSqlInjection($"order by {sortOrder.Columns.Select(sc => sc.SqlSortString()).JoinStrings(", ")}");

    /// <summary>
    /// Generate a non-conflicting alias for a given query.
    /// </summary>
    [Pure]
    public static ParameterizedSql GenerateUniqueQueryAlias(ParameterizedSql query)
        => GenerateUniqueQueryAlias(query.CommandText().GetHashCode(), query.CommandText());

    /// <summary>
    /// Generate a non-conflicting alias for a given query and seed.
    /// </summary>
    /// <param name="seed">Typically from a database sequence id.</param>
    /// <param name="query"></param>
    [Pure]
    public static ParameterizedSql GenerateUniqueQueryAlias(int seed, string query)
    {
        var r = RandomHelper.Insecure(seed);
        var queryAliasString = r.GetStringOfLatinLower(10);
        while (query.Contains(queryAliasString, StringComparison.OrdinalIgnoreCase)) {
            queryAliasString = r.GetStringOfLatinLower(queryAliasString.Length + 1);
        }
        return ParameterizedSql.UnescapedSqlIdentifier("_" + queryAliasString);
    }
}
