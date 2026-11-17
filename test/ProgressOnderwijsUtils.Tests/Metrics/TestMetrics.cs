using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using ProgressOnderwijsUtils.Metrics;

namespace ProgressOnderwijsUtils.Tests.Metrics;

public enum TestDatabase
{
    Alpha = 1, Beta = 2, Gamma = 3,
}

public enum TestEndpoint
{
    Home = 1,
    Login = 2,
    Search = 3,
    Status = 4,
}

public enum TestPool
{
    ReadOnly = 1,
    ReadWrite = 2,
    Sysadmin = 3,
    DebugLog = 4,
}

/// <summary>
/// Tracks every <see cref="TestDatabase"/>; a database is recognised by a catalog with the name of the enum value,
/// unless a test that runs against a real database maps an existing catalog itself.
/// </summary>
sealed class TestDatabaseDuration(Func<string, TestDatabase?>? classify = null)
    : MetricsDimension<TestDatabase>(MetricsHistogram.ExponentialLatencyBuckets(), Enum.GetValues<TestDatabase>())
{
    readonly Func<string, TestDatabase?> classify = classify ?? Classify;

    public static TestDatabase? Classify(string catalog)
        => Enum.TryParse<TestDatabase>(catalog, out var database) && Enum.IsDefined(database) ? database : null;

    protected override (TestDatabase? Value, bool Failed) ReadTags(ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        var database = default(TestDatabase?);
        var failed = false;
        foreach (var tag in tags) {
            switch (tag.Key) {
                case "db.namespace":
                    database = tag.Value is string dbNamespace ? SqlClientDbNamespace.Classify(dbNamespace, classify) : database;
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
                    isError = tag.Value is >= 400;
                    break;
            }
        }

        return (IdOf(method, route), isError);
    }
}

/// <summary>
/// A running collector sees the ASP.NET Core instruments of all hosts in the process and rejects those of every host
/// but its own, so the test classes that run collectors or create such instruments must not run alongside each other.
/// </summary>
[CollectionDefinition(nameof(MetricsCollectorCollection))]
public sealed class MetricsCollectorCollection;

static class TestMetrics
{
    public static MetricsDescription<TestEndpoint, TestDatabase, TestPool> Description(TestDatabaseDuration? databaseDuration = null)
        => new() {
            SmallestInterval = 1,
            LargestInterval = 128,
            DatabaseDurationHistogram = databaseDuration ?? new TestDatabaseDuration(),
            RequestDurationHistogram = new TestRequestDuration(),
        };

    /// <summary>
    /// A registry of its own per test, so that the pools of one test never show up in the samples of another.
    /// </summary>
    public static ConnectionPoolRegistry<TestDatabase, TestPool> Registry()
        => new(TestDatabaseDuration.Classify);

    public static MetricsCollector<TestEndpoint, TestDatabase, TestPool> Collector(TimeProvider clock)
        => new(Description(), Registry(), clock);

    /// <summary>
    /// The services of a host, with a meter factory of its own as ASP.NET Core creates its instruments with.
    /// </summary>
    public static ServiceProvider HostServices()
        => new ServiceCollection().AddMetrics().BuildServiceProvider();

    /// <summary>
    /// The meter factory of a host that never creates any instruments, for tests that do not measure requests.
    /// </summary>
    public static readonly IMeterFactory HostWithoutRequests = HostServices().GetRequiredService<IMeterFactory>();
}
