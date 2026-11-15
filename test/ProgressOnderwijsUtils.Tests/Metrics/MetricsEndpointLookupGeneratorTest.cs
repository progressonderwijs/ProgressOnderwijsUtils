using ProgressOnderwijsUtils.Metrics;

namespace ProgressOnderwijsUtils.Tests.Metrics;

/// <summary>
/// The numbers of <see cref="MetricsEndpointLookupGenerator"/> end up in stored measurements, so these tests pin down
/// that a number never switches endpoint.
/// </summary>
public sealed class MetricsEndpointLookupGeneratorTest
{
    static readonly MetricsEndpointLookupGenerator generator = new("Some.Namespace", "EndpointLookup", "EndpointId");

    static MetricsEndpointLookupGenerator.NumberedEndpoint Active(string endpoint)
        => new(endpoint, false);

    static MetricsEndpointLookupGenerator.NumberedEndpoint Retired(string endpoint)
        => new(endpoint, true);

    static string Render(IEnumerable<MetricsEndpointLookupGenerator.NumberedEndpoint> endpoints)
        => endpoints.Select((entry, id) => $"{id}: {entry.Endpoint}{(entry.IsRetired ? " <retired>" : "")}").JoinStrings("\n");

    static string[] ManyRoutes(int count)
        => [.. Enumerable.Range(0, count).Select(index => $"GET {index}"),];

    static MetricsEndpointLookupGenerator.NumberedEndpoint[] ManyActiveRoutes(int count)
        => [.. ManyRoutes(count).Select(Active),];

    [Fact]
    public void A_whole_json_body_read_as_one_endpoint_is_refused()
    {
        var existing = ManyActiveRoutes(10);
        string[] current = ["""["GET 0","GET 1","GET 2"]""",];

        PAssert.That(() => Assert.Throws<InvalidOperationException>(() => MetricsEndpointLookupGenerator.AssertEndpointsAreUsable(existing, current)).Message.Contains("not read properly"));
    }

    [Fact]
    public void A_line_without_a_route_is_refused()
    {
        var existing = ManyActiveRoutes(10);
        string[] current = ["GET 0", "just some text",];

        PAssert.That(() => Assert.Throws<InvalidOperationException>(() => MetricsEndpointLookupGenerator.AssertEndpointsAreUsable(existing, current)).Message.Contains("not read properly"));
    }

    [Fact]
    public void A_duplicated_endpoint_is_refused()
    {
        var existing = ManyActiveRoutes(10);
        string[] current = ["GET 0", "GET 0",];

        PAssert.That(() => Assert.Throws<InvalidOperationException>(() => MetricsEndpointLookupGenerator.AssertEndpointsAreUsable(existing, current)).Message.Contains("duplicate endpoints"));
    }

    [Fact]
    public void Losing_almost_every_endpoint_at_once_is_refused()
    {
        var existing = ManyActiveRoutes(10);
        string[] current = ["GET 0",];

        PAssert.That(() => Assert.Throws<InvalidOperationException>(() => MetricsEndpointLookupGenerator.AssertEndpointsAreUsable(existing, current)).Message.Contains("would disappear"));
    }

    [Fact]
    public void Removing_a_single_endpoint_is_allowed()
    {
        var existing = ManyActiveRoutes(10);
        var current = ManyRoutes(10).Where(route => route != "GET 3").ToArray();

        MetricsEndpointLookupGenerator.AssertEndpointsAreUsable(existing, current);
    }

    [Fact]
    public void The_very_first_generation_is_allowed()
        => MetricsEndpointLookupGenerator.AssertEndpointsAreUsable([], ManyRoutes(10));

    [Fact]
    public void A_route_without_a_method_is_allowed()
        => MetricsEndpointLookupGenerator.AssertEndpointsAreUsable([], ["* without/method",]);

    /// <summary>
    /// The routes come in without leading slash; with a slash every number would shift and no stored measurement
    /// would find its endpoint anymore.
    /// </summary>
    [Fact]
    public void A_route_with_a_leading_slash_is_refused()
    {
        var existing = ManyActiveRoutes(10);
        string[] current = ["GET 0", "GET /api/routes",];

        PAssert.That(() => Assert.Throws<InvalidOperationException>(() => MetricsEndpointLookupGenerator.AssertEndpointsAreUsable(existing, current)).Message.Contains("not read properly"));
    }

    [Fact]
    public void An_unchanged_route_table_keeps_every_number()
    {
        var assigned = Render(MetricsEndpointLookupGenerator.AssignIds([Active("GET a"), Active("GET b"),], ["GET b", "GET a",]));
        var expected = Render([Active("GET a"), Active("GET b"),]);

        PAssert.That(() => assigned == expected);
    }

    [Fact]
    public void A_new_route_is_numbered_after_the_existing_ones()
    {
        var assigned = Render(MetricsEndpointLookupGenerator.AssignIds([Active("GET b"),], ["GET a", "GET b",]));
        var expected = Render([Active("GET b"), Active("GET a"),]);

        PAssert.That(() => assigned == expected);
    }

    /// <summary>
    /// Old measurements still refer to the number, so the method and route are kept to recognise them.
    /// </summary>
    [Fact]
    public void A_removed_route_keeps_its_number_and_endpoint_as_retired()
    {
        var assigned = Render(MetricsEndpointLookupGenerator.AssignIds([Active("GET a"), Active("GET b"), Active("GET c"),], ["GET a", "GET c",]));
        var expected = Render([Active("GET a"), Retired("GET b"), Active("GET c"),]);

        PAssert.That(() => assigned == expected);
    }

    [Fact]
    public void The_number_of_a_removed_route_is_not_handed_to_a_new_route()
    {
        var assigned = Render(MetricsEndpointLookupGenerator.AssignIds([Active("GET a"), Retired("GET b"), Active("GET c"),], ["GET a", "GET c", "GET d",]));
        var expected = Render([Active("GET a"), Retired("GET b"), Active("GET c"), Active("GET d"),]);

        PAssert.That(() => assigned == expected);
    }

    [Fact]
    public void A_route_that_returns_after_removal_gets_a_new_number()
    {
        var assigned = Render(MetricsEndpointLookupGenerator.AssignIds([Active("GET a"), Retired("GET b"),], ["GET a", "GET b",]));
        var expected = Render([Active("GET a"), Retired("GET b"), Active("GET b"),]);

        PAssert.That(() => assigned == expected);
    }

    [Fact]
    public void Numbers_survive_a_roundtrip_through_the_generated_file()
    {
        MetricsEndpointLookupGenerator.NumberedEndpoint[] existing = [Active("GET a"), Retired("GET x"), Active("POST b"),];
        var parsed = Render(MetricsEndpointLookupGenerator.ParseExisting(generator.GenerateCode(existing)));
        var expected = Render(existing);

        PAssert.That(() => parsed == expected);
    }

    /// <summary>
    /// The routes are in the file as C# strings; if they come out differently after a round, an unchanged route gets
    /// a new number when regenerating.
    /// </summary>
    [Theory]
    [InlineData(@"GET api/{id:regex(^\d+$)}")]
    [InlineData("GET api/{name:regex(^\"[a-z]+\"$)}")]
    [InlineData(@"GET api/{path:regex(^\\\""$)}")]
    public void A_route_with_escaped_characters_survives_a_roundtrip_through_the_generated_file(string endpoint)
    {
        MetricsEndpointLookupGenerator.NumberedEndpoint[] existing = [Active("GET a"), Active(endpoint), Retired(endpoint),];
        var parsed = Render(MetricsEndpointLookupGenerator.ParseExisting(generator.GenerateCode(existing)));
        var expected = Render(existing);

        PAssert.That(() => parsed == expected);
    }

    /// <summary>
    /// A line that is not read leaves a gap in the numbering; regenerating would then give numbers to another endpoint.
    /// </summary>
    [Fact]
    public void A_generated_file_with_an_unreadable_number_is_refused()
    {
        const string generated = """
                    ("GET", "a", false), // 0
                    null, // 1
                    ("GET", "b", false), // 2
            """;

        PAssert.That(() => Assert.Throws<InvalidOperationException>(() => MetricsEndpointLookupGenerator.ParseExisting(generated)).Message.Contains("not read properly"));
    }

    [Fact]
    public void Regenerating_uses_the_given_namespace_class_and_id_type()
    {
        var code = generator.Regenerate("", ["GET a", "* b",]);

        PAssert.That(() => code.Contains("namespace Some.Namespace;") && code.Contains("public static class EndpointLookup") && code.Contains("\"a\" => (EndpointId)1,"));
    }

    [Fact]
    public void Regenerating_its_own_output_changes_nothing()
    {
        string[] endpoints = ["GET a", "POST a", "* b",];
        var first = generator.Regenerate("", endpoints);

        PAssert.That(() => generator.Regenerate(first, endpoints) == first);
    }
}
