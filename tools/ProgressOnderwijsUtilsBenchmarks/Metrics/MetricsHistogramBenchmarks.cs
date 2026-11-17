using ProgressOnderwijsUtils.Metrics;

namespace ProgressOnderwijsUtilsBenchmarks.Metrics;

/// <summary>
/// The cost of looking up a bucket, apart from the ring around it: this happens on every measured request and
/// database operation. The durations run across all buckets, the edges included, so that a branch predictor cannot
/// keep guessing the same way.
/// </summary>
[MemoryDiagnoser]
public class MetricsHistogramBenchmarks
{
    const int DurationCount = 1024;

    readonly MetricsHistogram requestHistogram = MetricsHistogram.Log2Buckets();
    readonly MetricsHistogram databaseHistogram = MetricsHistogram.ExponentialLatencyBuckets();
    readonly double[] durations = new double[DurationCount];

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        for (var i = 0; i < durations.Length; i++) {
            // Logarithmically distributed between 0.1 ms and 60 s, so that every bucket gets its turn.
            durations[i] = Math.Pow(10, random.NextDouble() * 5.8 - 4);
        }
    }

    [Benchmark(OperationsPerInvoke = DurationCount)]
    public int RequestHistogram_GetBucketIndex()
    {
        var sum = 0;
        foreach (var duration in durations) {
            sum += requestHistogram.GetBucketIndex(duration);
        }

        return sum;
    }

    [Benchmark(OperationsPerInvoke = DurationCount)]
    public int DatabaseHistogram_GetBucketIndex()
    {
        var sum = 0;
        foreach (var duration in durations) {
            sum += databaseHistogram.GetBucketIndex(duration);
        }

        return sum;
    }
}
