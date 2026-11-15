using System.Collections.Frozen;
using ProgressOnderwijsUtils.Metrics;

namespace ProgressOnderwijsUtilsBenchmarks.Metrics;

/// <summary>
/// The tracked databases. The values lie far apart on purpose, like the ids of a real application, so that the
/// lookup table of the dimension has the same sparse shape.
/// </summary>
enum BenchmarkDatabase
{
    Alpha = 1,
    Beta = 1173,
    Gamma = 1208,
    Delta = 1242,
    Epsilon = 1300,
    Zeta = 1311,
    Eta = 1501,
    Theta = 1600,
    Iota = 1800,
    Kappa = 1900,
}

/// <summary>
/// The number of an endpoint in <see cref="BenchmarkEndpointLookup"/>; like a generated numbering it has no named
/// values.
/// </summary>
enum BenchmarkEndpoint { }

enum BenchmarkPoolPurpose
{
    ReadOnly = 1,
    ReadWrite = 2,
    Sysadmin = 3,
    DebugLog = 4,
    HttpLog = 5,
    Background = 6,
    Migration = 7,
}

/// <summary>
/// Tracks every <see cref="BenchmarkDatabase"/>; a database is recognised by its catalog name.
/// </summary>
sealed class BenchmarkDatabaseDuration() : MetricsDimension<BenchmarkDatabase>(MetricsHistogram.ExponentialLatencyBuckets(), Enum.GetValues<BenchmarkDatabase>())
{
    static readonly FrozenDictionary<string, BenchmarkDatabase> databaseByCatalog = Enum.GetValues<BenchmarkDatabase>()
        .ToFrozenDictionary(CatalogOf, database => database, StringComparer.OrdinalIgnoreCase);

    public static string CatalogOf(BenchmarkDatabase database)
        => $"Bench{database}";

    public static BenchmarkDatabase? Classify(string catalog)
        => databaseByCatalog.TryGetValue(catalog, out var database) ? database : null;

    protected override (BenchmarkDatabase? Value, bool Failed) ReadTags(ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        var database = default(BenchmarkDatabase?);
        var failed = false;
        for (var i = 0; i < tags.Length; i++) {
            ref readonly var tag = ref tags[i];
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
/// Tracks seven of the endpoints in <see cref="BenchmarkEndpointLookup"/>; the other known endpoints share the other
/// slot with unknown routes.
/// </summary>
sealed class BenchmarkRequestDuration() : MetricsDimension<BenchmarkEndpoint>(
    MetricsHistogram.Log2Buckets(),
    [
        BenchmarkEndpointLookup.IdOf("GET", "{tenant:Tenant}/{*menuPath}"),
        BenchmarkEndpointLookup.IdOf("POST", "api/pjax"),
        BenchmarkEndpointLookup.IdOf("GET", "{tenant}/login"),
        BenchmarkEndpointLookup.IdOf("GET", "api/{tenant}/export/{version}/{objectName}"),
        BenchmarkEndpointLookup.IdOf("GET", "api/{tenant}/export/{version}/{objectName}/gt/{rowRevision}"),
        BenchmarkEndpointLookup.IdOf("POST", "api/{tenant}/export/{version}/{objectName}"),
        BenchmarkEndpointLookup.IdOf("GET", "status"),
    ]
)
{
    const int firstErrorStatusCode = 400;

    /// <summary>
    /// Takes everything a measurement needs out of the tags in one pass: searching separately would walk the same
    /// short list three times.
    /// </summary>
    protected override (BenchmarkEndpoint? Value, bool Failed) ReadTags(ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        string? method = null;
        string? route = null;
        var isError = false;
        for (var i = 0; i < tags.Length; i++) {
            ref readonly var tag = ref tags[i];
            switch (tag.Key) {
                case "http.request.method":
                    method = tag.Value as string;
                    break;
                case "http.route":
                    route = tag.Value as string;
                    break;
                case "http.response.status_code":
                    isError = tag.Value switch {
                        int code => code >= firstErrorStatusCode,
                        long longCode => longCode >= firstErrorStatusCode,
                        string str => str is ['4' or '5', _, _,] && char.IsAsciiDigit(str[1]) && char.IsAsciiDigit(str[2]),
                        _ => false,
                    };
                    break;
            }
        }

        return (BenchmarkEndpointLookup.IdOf(method, route), isError);
    }
}

/// <summary>
/// The numbering of the known endpoints, written out in the shape MetricsEndpointLookupGenerator generates: a switch
/// on the method and then on the route, so that looking up an endpoint costs a measurement what it costs an
/// application.
/// </summary>
static class BenchmarkEndpointLookup
{
    public static readonly BenchmarkEndpoint UnknownEndpoint = (BenchmarkEndpoint)(-2);

    public static BenchmarkEndpoint IdOf(string? method, string? route)
        => route is null || method is null
            ? UnknownEndpoint
            : method switch {
                "DELETE" => route switch {
                    "api/{tenant}/documents/{id}" => (BenchmarkEndpoint)2,
                    "api/{tenant}/sessions/{id}" => (BenchmarkEndpoint)3,
                    _ => IdOfAnyMethod(route),
                },
                "GET" => route switch {
                    "{tenant:Tenant}/{*menuPath}" => (BenchmarkEndpoint)4,
                    "{tenant}/login" => (BenchmarkEndpoint)5,
                    "{tenant}/logout" => (BenchmarkEndpoint)6,
                    "api/{tenant}/courses" => (BenchmarkEndpoint)7,
                    "api/{tenant}/courses/{id}" => (BenchmarkEndpoint)8,
                    "api/{tenant}/documents/{id}" => (BenchmarkEndpoint)9,
                    "api/{tenant}/enrolments" => (BenchmarkEndpoint)10,
                    "api/{tenant}/enrolments/{id}" => (BenchmarkEndpoint)11,
                    "api/{tenant}/export/{version}/{objectName}" => (BenchmarkEndpoint)12,
                    "api/{tenant}/export/{version}/{objectName}/gt/{rowRevision}" => (BenchmarkEndpoint)13,
                    "api/{tenant}/grades" => (BenchmarkEndpoint)14,
                    "api/{tenant}/grades/{id}" => (BenchmarkEndpoint)15,
                    "api/{tenant}/invoices" => (BenchmarkEndpoint)16,
                    "api/{tenant}/invoices/{id}" => (BenchmarkEndpoint)17,
                    "api/{tenant}/students" => (BenchmarkEndpoint)18,
                    "api/{tenant}/students/{id}" => (BenchmarkEndpoint)19,
                    "api/{tenant}/students/{id}/photo" => (BenchmarkEndpoint)20,
                    "api/metrics/live" => (BenchmarkEndpoint)21,
                    "api/translations/{language}" => (BenchmarkEndpoint)22,
                    "favicon.ico" => (BenchmarkEndpoint)23,
                    "robots.txt" => (BenchmarkEndpoint)24,
                    "status" => (BenchmarkEndpoint)25,
                    _ => IdOfAnyMethod(route),
                },
                "PATCH" => route switch {
                    "api/{tenant}/students/{id}" => (BenchmarkEndpoint)26,
                    _ => IdOfAnyMethod(route),
                },
                "POST" => route switch {
                    "{tenant}/login" => (BenchmarkEndpoint)27,
                    "api/{tenant}/courses" => (BenchmarkEndpoint)28,
                    "api/{tenant}/documents" => (BenchmarkEndpoint)29,
                    "api/{tenant}/enrolments" => (BenchmarkEndpoint)30,
                    "api/{tenant}/export/{version}/{objectName}" => (BenchmarkEndpoint)31,
                    "api/{tenant}/grades" => (BenchmarkEndpoint)32,
                    "api/{tenant}/invoices" => (BenchmarkEndpoint)33,
                    "api/{tenant}/students" => (BenchmarkEndpoint)34,
                    "api/pjax" => (BenchmarkEndpoint)35,
                    "api/upload" => (BenchmarkEndpoint)36,
                    _ => IdOfAnyMethod(route),
                },
                "PUT" => route switch {
                    "api/{tenant}/courses/{id}" => (BenchmarkEndpoint)37,
                    "api/{tenant}/enrolments/{id}" => (BenchmarkEndpoint)38,
                    "api/{tenant}/grades/{id}" => (BenchmarkEndpoint)39,
                    _ => IdOfAnyMethod(route),
                },
                _ => IdOfAnyMethod(route),
            };

    /// <summary>
    /// The id of a route that applies to every method. A measurement carries the real method, so without this
    /// fallback such a route would never be found.
    /// </summary>
    static BenchmarkEndpoint IdOfAnyMethod(string route)
        => route switch {
            "{*catchAll}" => 0,
            "health" => (BenchmarkEndpoint)1,
            _ => UnknownEndpoint,
        };

    /// <summary>
    /// The endpoint of an id, or null when the id does not exist. The ids are contiguous, so they are at the same
    /// time the position in the table.
    /// </summary>
    public static (string Method, string Route)? EndpointOf(BenchmarkEndpoint id)
        => (uint)id < (uint)endpoints.Length ? endpoints[(int)id] : null;

    static readonly (string Method, string Route)[] endpoints = [
        ("*", "{*catchAll}"), // 0
        ("*", "health"), // 1
        ("DELETE", "api/{tenant}/documents/{id}"), // 2
        ("DELETE", "api/{tenant}/sessions/{id}"), // 3
        ("GET", "{tenant:Tenant}/{*menuPath}"), // 4
        ("GET", "{tenant}/login"), // 5
        ("GET", "{tenant}/logout"), // 6
        ("GET", "api/{tenant}/courses"), // 7
        ("GET", "api/{tenant}/courses/{id}"), // 8
        ("GET", "api/{tenant}/documents/{id}"), // 9
        ("GET", "api/{tenant}/enrolments"), // 10
        ("GET", "api/{tenant}/enrolments/{id}"), // 11
        ("GET", "api/{tenant}/export/{version}/{objectName}"), // 12
        ("GET", "api/{tenant}/export/{version}/{objectName}/gt/{rowRevision}"), // 13
        ("GET", "api/{tenant}/grades"), // 14
        ("GET", "api/{tenant}/grades/{id}"), // 15
        ("GET", "api/{tenant}/invoices"), // 16
        ("GET", "api/{tenant}/invoices/{id}"), // 17
        ("GET", "api/{tenant}/students"), // 18
        ("GET", "api/{tenant}/students/{id}"), // 19
        ("GET", "api/{tenant}/students/{id}/photo"), // 20
        ("GET", "api/metrics/live"), // 21
        ("GET", "api/translations/{language}"), // 22
        ("GET", "favicon.ico"), // 23
        ("GET", "robots.txt"), // 24
        ("GET", "status"), // 25
        ("PATCH", "api/{tenant}/students/{id}"), // 26
        ("POST", "{tenant}/login"), // 27
        ("POST", "api/{tenant}/courses"), // 28
        ("POST", "api/{tenant}/documents"), // 29
        ("POST", "api/{tenant}/enrolments"), // 30
        ("POST", "api/{tenant}/export/{version}/{objectName}"), // 31
        ("POST", "api/{tenant}/grades"), // 32
        ("POST", "api/{tenant}/invoices"), // 33
        ("POST", "api/{tenant}/students"), // 34
        ("POST", "api/pjax"), // 35
        ("POST", "api/upload"), // 36
        ("PUT", "api/{tenant}/courses/{id}"), // 37
        ("PUT", "api/{tenant}/enrolments/{id}"), // 38
        ("PUT", "api/{tenant}/grades/{id}"), // 39
    ];
}

static class BenchmarkMetrics
{
    public const int RingCapacity = MetricsCollector<BenchmarkDatabase, BenchmarkEndpoint, BenchmarkPoolPurpose>.RingCapacity;
    public const int DefaultSettleSeconds = MetricsCollector<BenchmarkDatabase, BenchmarkEndpoint, BenchmarkPoolPurpose>.DefaultSettleSeconds;
    public const byte LargestInterval = 128;

    public static MetricsDescription<BenchmarkDatabase, BenchmarkEndpoint, BenchmarkPoolPurpose> Description()
        => new() {
            SmallestInterval = 1,
            LargestInterval = LargestInterval,
            DatabaseDurationHistogram = new BenchmarkDatabaseDuration(),
            RequestDurationHistogram = new BenchmarkRequestDuration(),
        };

    /// <summary>
    /// A registry of its own per benchmark, so that the pools of one benchmark never show up in the samples of another.
    /// </summary>
    public static ConnectionPoolRegistry<BenchmarkDatabase, BenchmarkPoolPurpose> Registry()
        => new(BenchmarkDatabaseDuration.Classify);

    /// <summary>
    /// Three pools per tracked database (readonly, readwrite and sysadmin), roughly as an application has them.
    /// </summary>
    public static IEnumerable<(string ConnectionString, BenchmarkPoolPurpose Purpose)> PoolsPerDatabase()
        => Enum.GetValues<BenchmarkDatabase>()
            .SelectMany(
                database => new[] { BenchmarkPoolPurpose.ReadOnly, BenchmarkPoolPurpose.ReadWrite, BenchmarkPoolPurpose.Sysadmin, }
                    .Select(purpose => ($"Initial Catalog={BenchmarkDatabaseDuration.CatalogOf(database)};Application Name={purpose}", purpose))
            );
}
