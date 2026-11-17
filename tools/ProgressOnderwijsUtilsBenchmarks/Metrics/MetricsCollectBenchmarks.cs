using ProgressOnderwijsUtils.Metrics;

namespace ProgressOnderwijsUtilsBenchmarks.Metrics;

/// <summary>
/// The cost of reading out the rings. Every viewer of the live chart collects once per second and the storage once
/// per twenty seconds, so this is not a hot path like measuring itself; it does add up with the number of viewers.
/// The rings are completely filled, so that the search for the level from just before the window has something to
/// find as well.
/// </summary>
[MemoryDiagnoser]
// ReSharper disable once ClassCanBeSealed.Global because a benchmark class may not be sealed as it is edited by the JIT
public class MetricsCollectBenchmarks
{
    [Params(1, 128)]
    public int Interval { get; set; }

    // These fields are filled by BenchmarkDotNet in GlobalSetup, before any benchmark runs.
    // ReSharper disable NullableWarningSuppressionIsUsed
    ActiveRequests activeRequests = null!;
    DurationHistogramRing<BenchmarkEndpoint> requestDuration = null!;
    ThreadPoolUsage threadPoolUsage = null!;
    ConnectionPoolUsage<BenchmarkDatabase, BenchmarkPool> connectionPoolUsage = null!;
    // ReSharper restore NullableWarningSuppressionIsUsed
    int firstSecond;
    int lastSecond;

    [GlobalSetup]
    public void Setup()
    {
        var description = BenchmarkMetrics.Description();
        var registry = BenchmarkMetrics.Registry();
        var ringLength = BenchmarkMetrics.RingCapacity;

        var clock = new SteppingClock();
        var seconds = new SecondsSinceStart(clock);
        activeRequests = new(new(seconds, ringLength));
        requestDuration = new(new(seconds, ringLength), description.RequestDurationHistogram);
        threadPoolUsage = new(new(seconds, ringLength));
        connectionPoolUsage = new(new(seconds, ringLength), description.ConnectionPools, registry);

        var trackedRoute = description.RequestDurationHistogram.Values[1] is { } trackedEndpoint
            ? BenchmarkEndpointLookup.EndpointOf(trackedEndpoint)?.Route
            : null;
        KeyValuePair<string, object?>[] tags = [new("http.request.method", "GET"), new("http.route", trackedRoute), new("http.response.status_code", 200),];
        var pools = BenchmarkMetrics.PoolsPerDatabase()
            .Select(dbPool => registry.For(dbPool.connectionString, dbPool.pool))
            .ToArray();

        // Well over the length of the ring in seconds, so that every slot is filled and has been reused once already.
        for (var second = 0; second < ringLength * 2; second++) {
            clock.Advance();
            activeRequests.Measurement(1);
            requestDuration.Measurement(0.02, tags);
            activeRequests.Measurement(-1);
            threadPoolUsage.Measurement();
            connectionPoolUsage.Measurement(pools);
        }

        lastSecond = seconds.Second - BenchmarkMetrics.DefaultSettleSeconds;
        firstSecond = lastSecond - Interval + 1;
    }

    [Benchmark]
    public long ActiveRequests_Collect()
        => activeRequests.Collect(firstSecond, lastSecond);

    [Benchmark]
    public int[] RequestDuration_Collect()
        => requestDuration.Collect(firstSecond, lastSecond);

    [Benchmark]
    public object? ThreadPool_Collect()
        => threadPoolUsage.Collect(firstSecond, lastSecond);

    [Benchmark]
    public object? ConnectionPools_Collect()
        => connectionPoolUsage.Collect(firstSecond, lastSecond);

    /// <summary>
    /// A clock that only moves forward when the setup asks for it, so that the ring can be filled per second without
    /// waiting and stands still while measuring.
    /// </summary>
    sealed class SteppingClock : TimeProvider
    {
        DateTimeOffset now = DateTimeOffset.UnixEpoch.AddDays(20000);

        public void Advance()
            => now = now.AddSeconds(1);

        public override DateTimeOffset GetUtcNow()
            => now;

        public override long GetTimestamp()
            => now.UtcTicks;

        public override long TimestampFrequency
            => TimeSpan.TicksPerSecond;
    }
}
