using System.Globalization;
using System.Text.RegularExpressions;

namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// Generates the numbering of the endpoints that can be measured on, as source code of a lookup class. The numbers
/// end up in stored measurements, so they have to stay the same across builds: existing endpoints keep their number
/// and a disappeared endpoint keeps its number as retired instead of releasing it. Were a number reused, then old
/// measurements would silently be about another endpoint. A retired endpoint also keeps its method and route, so that
/// old measurements can still be recognised.
/// </summary>
/// <param name="Namespace">The namespace of the generated class.</param>
/// <param name="ClassName">The name of the generated class.</param>
/// <param name="IdTypeName">The int-based enum the numbers are cast to, as written in the generated code.</param>
public sealed partial record MetricsEndpointLookupGenerator(string Namespace, string ClassName, string IdTypeName)
{
    /// <summary>
    /// An endpoint with the number it has in the table; the number is the position in the table.
    /// </summary>
    internal sealed record NumberedEndpoint(string Endpoint, bool IsRetired);

    /// <summary>
    /// The method in the endpoint table of a route that applies to every method.
    /// </summary>
    const string WildcardMethod = "*";

    /// <summary>
    /// How many of the known endpoints may disappear at once at most. Removing endpoints is allowed, but when almost
    /// everything disappears at the same time the input is more likely broken than the endpoint table changed.
    /// </summary>
    const double MaxRemovedFraction = 0.2;

    /// <summary>
    /// The source code of the lookup class for the current endpoints, keeping the numbers of the previously generated
    /// code.
    /// </summary>
    /// <param name="existingCode">The previously generated code, or an empty string when there is none yet.</param>
    /// <param name="currentEndpoints">
    /// Every endpoint as 'METHOD route', with <c>*</c> as the method of a route that applies to every method and the
    /// route without leading slash, as ASP.NET passes it in the <c>http.route</c> tag.
    /// </param>
    public string Regenerate(string existingCode, string[] currentEndpoints)
    {
        var existing = ParseExisting(existingCode);
        AssertEndpointsAreUsable(existing, currentEndpoints);
        return GenerateCode(AssignIds(existing, currentEndpoints));
    }

    /// <summary>
    /// The generated file is overwritten and lives in source control, so broken input may never end up in it: the
    /// numbers are only worth something as long as they keep belonging to the same endpoint.
    /// Both checks come from practice: a stale build once delivered the whole json body as a single endpoint, after
    /// which all existing numbers became vacant in one go.
    /// </summary>
    internal static void AssertEndpointsAreUsable(NumberedEndpoint[] existing, string[] current)
    {
        if (current.FirstOrNull(line => !EndpointLine().IsMatch(line)) is { } malformed) {
            throw new InvalidOperationException($"'{Shorten(malformed)}' is not a 'METHOD route'; the endpoint table was not read properly and would break the generated file.");
        }

        if (current.Distinct(StringComparer.Ordinal).Count() != current.Length) {
            throw new InvalidOperationException("The endpoint table contains duplicate endpoints; it then cannot be determined which number belongs to which endpoint.");
        }

        var known = existing.Where(entry => !entry.IsRetired).Select(entry => entry.Endpoint).ToArray();
        var removed = known.Where(endpoint => !current.Contains(endpoint, StringComparer.Ordinal)).ToArray();
        if (known.Length > 0 && removed.Length > known.Length * MaxRemovedFraction) {
            throw new InvalidOperationException(
                $"{removed.Length} of the {known.Length} endpoints would disappear, for example '{Shorten(removed[0])}'. "
                + "Only run this when the application runs the current code; retiring numbers cannot be undone."
            );
        }
    }

    static string Shorten(string value)
        => value.Length > 60 ? $"{value[..60]}..." : value;

    /// <summary>
    /// Reads the numbers that are already fixed from the previously generated file, including the numbers of retired
    /// endpoints. A number that is not read could get another endpoint when regenerating, so the numbers have to be
    /// read contiguously from 0.
    /// </summary>
    internal static NumberedEndpoint[] ParseExisting(string generated)
    {
        var parsed = EntryLine().Matches(generated)
            .Select(match => (
                    Id: int.Parse(match.Groups["id"].Value, CultureInfo.InvariantCulture),
                    Entry: new NumberedEndpoint(
                        $"{Unescape(match.Groups["method"].Value)} {Unescape(match.Groups["route"].Value)}",
                        match.Groups["retired"].Value is "true"
                    )
                )
            )
            .ToArray();

        for (var position = 0; position < parsed.Length; position++) {
            if (parsed[position].Id != position) {
                throw new InvalidOperationException(
                    $"The existing file has number {parsed[position].Id} at position {position}; the file was not read properly "
                    + "and regenerating would give numbers to another endpoint. Restore the missing lines from the git history."
                );
            }
        }

        var countMatch = CountLine().Match(generated);
        if (generated.Length > 0
            && (!countMatch.Success || int.Parse(countMatch.Groups["count"].Value, CultureInfo.InvariantCulture) != parsed.Length)) {
            throw new InvalidOperationException("The existing file was not read properly: the number of endpoints read does not match Count. Restore the missing lines from the git history.");
        }

        return [.. parsed.Select(entry => entry.Entry),];
    }

    /// <summary>
    /// Existing endpoints keep their number, new ones get one at the end in sort order and disappeared endpoints keep
    /// their number as retired endpoint.
    /// </summary>
    internal static NumberedEndpoint[] AssignIds(NumberedEndpoint[] existing, string[] current)
    {
        var active = existing.Where(entry => !entry.IsRetired).Select(entry => entry.Endpoint).ToHashSet(StringComparer.Ordinal);
        var kept = existing.Select(entry => entry.IsRetired || current.Contains(entry.Endpoint, StringComparer.Ordinal) ? entry : entry with { IsRetired = true, });
        var added = current.Where(endpoint => !active.Contains(endpoint)).Order(StringComparer.Ordinal).Select(endpoint => new NumberedEndpoint(endpoint, false));
        return [.. kept, .. added,];
    }

    internal string GenerateCode(NumberedEndpoint[] endpoints)
    {
        var known = endpoints
            .Select((entry, id) => (entry, id))
            .Where(numbered => !numbered.entry.IsRetired)
            .Select(numbered => (Method: MethodOf(numbered.entry), Route: RouteOf(numbered.entry), numbered.id))
            .ToArray();

        // A route that applies to every method has a * in the endpoint table, but a measurement always carries the
        // real method. Without a fallback such a route would never be found; the exact method goes first, because
        // that one is more specific.
        var wildcardRoutes = known
            .Where(entry => entry.Method is WildcardMethod)
            .OrderBy(entry => entry.Route, StringComparer.Ordinal)
            .Select(entry => $"            {Literal(entry.Route)} => ({IdTypeName}){entry.id},")
            .JoinStrings("\n");

        var byMethod = known
            .Where(entry => entry.Method is not WildcardMethod)
            .GroupBy(entry => entry.Method, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => $"                {Literal(group.Key)} => route switch {{\n" + group
                .OrderBy(entry => entry.Route, StringComparer.Ordinal)
                .Select(entry => $"                    {Literal(entry.Route)} => ({IdTypeName}){entry.id},")
                .JoinStrings("\n") + "\n                    _ => IdOfAnyMethod(route),\n                },"
            )
            .JoinStrings("\n");

        var table = endpoints
            .Select((entry, id) => $"        ({Literal(MethodOf(entry))}, {Literal(RouteOf(entry))}, {(entry.IsRetired ? "true" : "false")}), // {id}")
            .JoinStrings("\n");

        return $$"""
            // <auto-generated />
            // Generated by MetricsEndpointLookupGenerator; do not edit by hand.
            // The numbers are fixed because they end up in stored measurements: an existing endpoint
            // keeps its number and a disappeared endpoint keeps its number as retired endpoint.
            #nullable enable
            using System.Collections.Generic;

            namespace {{Namespace}};

            public static class {{ClassName}}
            {
                /// <summary>
                /// A request that cannot be traced back to an endpoint: the router found no route,
                /// or the route is not in this numbering. Neither case can be named and
                /// so they cannot be told apart in a measurement.
                /// </summary>
                public static readonly {{IdTypeName}} UnknownEndpoint = ({{IdTypeName}})(-2);

                /// <summary>
                /// The number of ids that are fixed, including the ids of retired endpoints.
                /// </summary>
                public const int Count = {{endpoints.Length}};

                /// <summary>
                /// The id of an endpoint, or <see cref="UnknownEndpoint"/> when the request cannot be
                /// traced back to an endpoint in this numbering.
                /// The method is on the outside so that no composite key per request is needed.
                /// </summary>
                public static {{IdTypeName}} IdOf(string? method, string? route)
                    => route is null || method is null
                        ? UnknownEndpoint
                        : method switch {
            {{byMethod}}
                            _ => IdOfAnyMethod(route),
                        };

                /// <summary>
                /// The id of a route that applies to every method. A measurement carries the real
                /// method, so without this fallback such a route would never be found.
                /// </summary>
                static {{IdTypeName}} IdOfAnyMethod(string route)
                    => route switch {
            {{wildcardRoutes}}
                        _ => UnknownEndpoint,
                    };

                /// <summary>
                /// The existing endpoint of an id, or null when the endpoint is retired or the id does not
                /// exist. The ids are contiguous, so here they are at the same time the position in the table
                /// and this is not a search.
                /// </summary>
                public static (string Method, string Route)? EndpointOf({{IdTypeName}} id)
                    => (uint)id < (uint)endpoints.Length && endpoints[(int)id] is (var method, var route, false) ? (method, route) : null;

                /// <summary>
                /// The retired endpoint of an id, or null when the endpoint still exists or the id does not
                /// exist. Old measurements still refer to such an id and stay recognisable this way.
                /// </summary>
                public static (string Method, string Route)? RetiredEndpointOf({{IdTypeName}} id)
                    => (uint)id < (uint)endpoints.Length && endpoints[(int)id] is (var method, var route, true) ? (method, route) : null;

                /// <summary>
                /// All ids that point at an existing endpoint, without the retired ones. With this it can be
                /// calculated which endpoints are not tracked separately and so count on the shared slot.
                /// </summary>
                public static IEnumerable<{{IdTypeName}}> AllEndpoints()
                {
                    for (var id = 0; id < endpoints.Length; id++) {
                        if (!endpoints[id].IsRetired) {
                            yield return ({{IdTypeName}})id;
                        }
                    }
                }

                static readonly (string Method, string Route, bool IsRetired)[] endpoints = [
            {{table}}
                ];
            }

            """;
    }

    static string MethodOf(NumberedEndpoint entry)
        => entry.Endpoint.Split(' ', 2)[0];

    static string RouteOf(NumberedEndpoint entry)
        => entry.Endpoint.Split(' ', 2)[1];

    static string Literal(string value)
        => $"\"{value.Replace("\\", @"\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    /// <summary>
    /// The inverse of <see cref="Literal"/>. Without this a route with a backslash or quote (like a regex constraint)
    /// gets an extra escape every round, and with that a new number while the route did not change.
    /// </summary>
    static string Unescape(string literalContent)
        => EscapedCharacter().Replace(literalContent, "${character}");

    [GeneratedRegex("""\\(?<character>.)""", RegexOptions.Singleline | RegexOptions.ExplicitCapture)]
    private static partial Regex EscapedCharacter();

    [GeneratedRegex(
        """^\s+\("(?<method>(?:[^"\\]|\\.)*)", "(?<route>(?:[^"\\]|\\.)*)", (?<retired>true|false)\), // (?<id>\d+)\r?$""",
        RegexOptions.Multiline | RegexOptions.ExplicitCapture
    )]
    private static partial Regex EntryLine();

    [GeneratedRegex("""^\s*public const int Count = (?<count>\d+);\r?$""", RegexOptions.Multiline | RegexOptions.ExplicitCapture)]
    private static partial Regex CountLine();

    /// <summary>
    /// A usable line is a method (or <c>*</c>) followed by a route that starts with the route itself.
    /// The route is in it without leading slash, as ASP.NET passes it in the <c>http.route</c> tag.
    /// Were that slash in it, then no measurement would find its endpoint anymore and every number would shift when
    /// regenerating, without anything complaining about it.
    /// </summary>
    [GeneratedRegex("""^(?:\*|[A-Z]+) (?!/)\S*\z""")]
    private static partial Regex EndpointLine();
}
