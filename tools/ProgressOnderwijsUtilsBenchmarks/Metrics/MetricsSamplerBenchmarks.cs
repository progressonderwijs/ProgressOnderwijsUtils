using ProgressOnderwijsUtils.Metrics;

namespace ProgressOnderwijsUtilsBenchmarks.Metrics;

/// <summary>
/// The cost of one sampling by the sampler thread: it runs every 250 ms, so speed hardly matters, but it must not
/// allocate. There are three pools per tracked database plus a couple of untracked databases, roughly as in
/// production (readonly, readwrite and sysadmin per database).
/// </summary>
[MemoryDiagnoser]
// ReSharper disable once ClassCanBeSealed.Global because a benchmark class may not be sealed as it is edited by the JIT
public class MetricsSamplerBenchmarks
{
    ThreadPoolUsage? threadPoolUsage;
    ConnectionPoolUsage<BenchmarkDatabase, BenchmarkPoolPurpose>? connectionPoolUsage;
    ConnectionPoolCounters<BenchmarkDatabase, BenchmarkPoolPurpose>[]? pools;

    [GlobalSetup]
    public void Setup()
    {
        var registry = BenchmarkMetrics.Registry();
        threadPoolUsage = new(new(SecondsClock.Ticking(TimeProvider.System), BenchmarkMetrics.RingCapacity));
        connectionPoolUsage = new(new(SecondsClock.Ticking(TimeProvider.System), BenchmarkMetrics.RingCapacity), new(new BenchmarkDatabaseDuration()), registry);
        (string ConnectionString, BenchmarkPoolPurpose Purpose)[] connectionStrings = [
            .. BenchmarkMetrics.PoolsPerDatabase(),
            ("Initial Catalog=master", BenchmarkPoolPurpose.Sysadmin),
            ("Initial Catalog=BenchBlob", BenchmarkPoolPurpose.Sysadmin),
        ];
        pools = connectionStrings.Select(o => registry.For(o.ConnectionString, o.Purpose)).ToArray();
    }

    [Benchmark]
    public void ThreadPool_Sample()
        => threadPoolUsage?.Measurement();

    [Benchmark]
    public void ConnectionPools_Sample()
        => connectionPoolUsage?.Measurement(pools);
}
