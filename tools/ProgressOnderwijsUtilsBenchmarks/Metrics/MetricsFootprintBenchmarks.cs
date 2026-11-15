using ProgressOnderwijsUtils.Metrics;

namespace ProgressOnderwijsUtilsBenchmarks.Metrics;

/// <summary>
/// Measures what a collector costs in memory for an increasing number of tracked endpoints. Every tracked endpoint
/// gets a slot of its own in every second of the ring, so the ring grows by BucketCount * OutcomeCount * RingCapacity
/// cells per extra endpoint.
/// This is separate from <see cref="MetricsCollectorBenchmarks"/>: that one is about the cost per request, this one
/// about what stays allocated for as long as the process runs.
/// </summary>
[MemoryDiagnoser]
// ReSharper disable once ClassCanBeSealed.Global because a benchmark class may not be sealed as it is edited by the JIT
public class MetricsFootprintBenchmarks
{
    /// <summary>
    /// A dimension with a configurable number of tracked endpoints; <see cref="BenchmarkRequestDuration"/> is fixed
    /// to the endpoints an application really tracks, and growing along is exactly what is measured here.
    /// </summary>
    sealed class TrackedEndpointsDimension : MetricsDimension<BenchmarkEndpoint>
    {
        TrackedEndpointsDimension(MetricsHistogram histogram, IEnumerable<BenchmarkEndpoint> tracked)
            : base(histogram, tracked) { }

        public static TrackedEndpointsDimension Create(int trackedEndpointCount)
            => new(MetricsHistogram.Log2Buckets(), Enumerable.Range(0, trackedEndpointCount).Select(id => (BenchmarkEndpoint)id));

        protected override (BenchmarkEndpoint? Value, bool Failed) ReadTags(ReadOnlySpan<KeyValuePair<string, object?>> tags)
            => throw new NotSupportedException("This benchmark only measures the ring, not reading the tags.");
    }

    /// <summary>
    /// 7 is what an application tracks now; the higher values show how the ring grows along.
    /// </summary>
    [Params(0, 7, 32, 128)]
    public int TrackedEndpointCount { get; set; }

    // Filled in GlobalSetup before any benchmark runs.
    // ReSharper disable once NullableWarningSuppressionIsUsed
    MetricsDescription<BenchmarkEndpoint, BenchmarkDatabase, BenchmarkPoolPurpose> description = null!;

    [GlobalSetup]
    public void Setup()
        => description = new() {
            SmallestInterval = 1,
            LargestInterval = BenchmarkMetrics.LargestInterval,
            DatabaseDurationHistogram = new BenchmarkDatabaseDuration(),
            RequestDurationHistogram = TrackedEndpointsDimension.Create(TrackedEndpointCount),
        };

    /// <summary>
    /// The cells a single second takes up; this is the size that grows along with every tracked endpoint.
    /// </summary>
    [Benchmark]
    public int CellsPerSecond()
        => description.RequestDurationHistogram.Size;

    /// <summary>
    /// Creating the ring itself, so that MemoryDiagnoser shows how much memory a collector holds on to for as long
    /// as it exists.
    /// </summary>
    [Benchmark]
    public object AllocateRequestDurationRing()
        => new DurationHistogramRing<BenchmarkEndpoint>(
            new(new SecondsSinceStart(TimeProvider.System), BenchmarkMetrics.RingCapacity),
            description.RequestDurationHistogram
        );
}
