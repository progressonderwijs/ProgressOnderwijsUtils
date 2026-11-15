using ProgressOnderwijsUtils.Metrics;

namespace ProgressOnderwijsUtils.Tests.Metrics;

public enum TestDatabase
{
    Alpha = 1,
    Beta = 2,
    Gamma = 3,
}

public enum TestEndpoint
{
    Home = 1,
    Login = 2,
    Search = 3,
    Status = 4,
}

public enum TestPurpose
{
    ReadOnly = 1,
    ReadWrite = 2,
    Sysadmin = 3,
    DebugLog = 4,
}

/// <summary>
/// Tracks every <see cref="TestDatabase"/>; a database is recognised by a catalog with the name of the enum value.
/// </summary>
sealed class TestDatabaseDuration() : MetricsDimension<TestDatabase>(MetricsHistogram.ExponentialLatencyBuckets(), Enum.GetValues<TestDatabase>())
{
    public static TestDatabase? Classify(string catalog)
        => Enum.TryParse<TestDatabase>(catalog, out var database) && Enum.IsDefined(database) ? database : null;

    protected override (TestDatabase? Value, bool Failed) ReadTags(ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        var database = default(TestDatabase?);
        var failed = false;
        foreach (var tag in tags) {
            switch (tag.Key) {
                case "db.namespace":
                    database = tag.Value is string catalog ? Classify(catalog) : database;
                    break;
                case "error.type":
                    failed = tag.Value is not null;
                    break;
            }
        }

        return (database, failed);
    }
}

/// <summary>
/// Tracks <see cref="TrackedEndpoints"/>; <see cref="TestEndpoint.Status"/> is a known endpoint that is not tracked
/// and so shares the other slot with unknown routes.
/// </summary>
sealed class TestRequestDuration() : MetricsDimension<TestEndpoint>(MetricsHistogram.Log2Buckets(), TrackedEndpoints)
{
    public static readonly TestEndpoint[] TrackedEndpoints = [TestEndpoint.Home, TestEndpoint.Login, TestEndpoint.Search,];

    public static (string Method, string Route) EndpointOf(TestEndpoint endpoint)
        => endpoint switch {
            TestEndpoint.Home => ("GET", "home"),
            TestEndpoint.Login => ("POST", "login"),
            TestEndpoint.Search => ("GET", "search/{query}"),
            TestEndpoint.Status => ("GET", "status"),
            _ => throw new ArgumentOutOfRangeException(nameof(endpoint), endpoint, "Expected a defined test endpoint."),
        };

    static TestEndpoint? IdOf(string? method, string? route)
    {
        foreach (var endpoint in Enum.GetValues<TestEndpoint>()) {
            if (EndpointOf(endpoint) == (method, route)) {
                return endpoint;
            }
        }

        return null;
    }

    protected override (TestEndpoint? Value, bool Failed) ReadTags(ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        string? method = null;
        string? route = null;
        var isError = false;
        foreach (var tag in tags) {
            switch (tag.Key) {
                case "http.request.method":
                    method = tag.Value as string;
                    break;
                case "http.route":
                    route = tag.Value as string;
                    break;
                case "http.response.status_code":
                    isError = tag.Value is int statusCode && statusCode >= 400;
                    break;
            }
        }

        return (IdOf(method, route), isError);
    }
}

static class TestMetrics
{
    public static MetricsDescription<TestEndpoint, TestDatabase, TestPurpose> Description()
        => new() {
            SmallestInterval = 1,
            LargestInterval = 128,
            DatabaseDurationHistogram = new TestDatabaseDuration(),
            RequestDurationHistogram = new TestRequestDuration(),
        };

    /// <summary>
    /// A registry of its own per test, so that the pools of one test never show up in the samples of another.
    /// </summary>
    public static ConnectionPoolRegistry<TestDatabase, TestPurpose> Registry()
        => new(TestDatabaseDuration.Classify);

    public static MetricsCollector<TestEndpoint, TestDatabase, TestPurpose> Collector(TimeProvider clock)
        => new(Description(), Registry(), clock);
}
