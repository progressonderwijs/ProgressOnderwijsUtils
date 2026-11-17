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
    ConnectionPoolUsage<BenchmarkDatabase, BenchmarkPool>? connectionPoolUsage;
    ConnectionPoolCounters<BenchmarkDatabase, BenchmarkPool>[]? pools;

    [GlobalSetup]
    public void Setup()
    {
        var registry = BenchmarkMetrics.Registry();
        threadPoolUsage = new(new SecondsSinceStart(TimeProvider.System), BenchmarkMetrics.RingCapacity);
        connectionPoolUsage = new(new SecondsSinceStart(TimeProvider.System), BenchmarkMetrics.RingCapacity, new(new BenchmarkDatabaseDuration()), registry);
        (string connectionString, BenchmarkPool pool)[] connectionStrings = [
            .. BenchmarkMetrics.PoolsPerDatabase(),
            ("Initial Catalog=master", BenchmarkPool.Sysadmin),
            ("Initial Catalog=BenchBlob", BenchmarkPool.Sysadmin),
        ];
        pools = connectionStrings.Select(o => registry.For(o.connectionString, o.pool)).ToArray();
    }

    [Benchmark]
    public void ThreadPool_Sample()
        => threadPoolUsage?.Measurement();

    [Benchmark]
    public void ConnectionPools_Sample()
        => connectionPoolUsage?.Measurement(pools);
}
