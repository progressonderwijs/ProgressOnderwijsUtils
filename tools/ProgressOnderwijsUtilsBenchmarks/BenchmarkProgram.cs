using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using ProgressOnderwijsUtilsBenchmarks.Metrics;
using ProgressOnderwijsUtilsBenchmarks.MicroOrmBench;

namespace ProgressOnderwijsUtilsBenchmarks;

public static class BenchmarkProgram
{
    static void Main()
        //=> RunTreeBenchmarks();
    //=> MicroOrmBenchmarkProgram.RunBenchmarks();
    //=> RunArrayBuilderBenchmarks();
    => RunMetricsBenchmarks();
    //=> BenchmarkRunner.Run<SmallBatchInsertBench>();
    //=> BenchmarkRunner.Run<NullabilityBenchmark>();
    //=> BenchmarkRunner.Run<RandomHelperBenchmark>();

    [UsedImplicitly]
    static void RunArrayBuilderBenchmarks()
    {
        IntArrayBuilderBenchmark.SanityCheck(10000);
        _ = new BenchmarkSwitcher(
            [
                typeof(IntArrayBuilderBenchmark),
                typeof(BigStructArrayBuilderBenchmark),
                typeof(ByteArrayBuilderBenchmark),
                typeof(ReferenceTypeArrayBuilderBenchmark),
                typeof(SmallStructArrayBuilderBenchmark),
            ]
        ).RunAllJoined();
    }

    [UsedImplicitly]
    static void RunMetricsBenchmarks()
        // BenchmarkDotNet 0.15.8 does not recognise .NET 11 and so cannot build a benchmark process for it; the
        // in-process toolchain runs the benchmarks in this process instead.
        => _ = new BenchmarkSwitcher(
            [
                typeof(MetricsCollectBenchmarks),
                typeof(MetricsCollectorBenchmarks),
                typeof(MetricsFootprintBenchmarks),
                typeof(MetricsHistogramBenchmarks),
                typeof(MetricsSamplerBenchmarks),
            ]
        ).RunAll(DefaultConfig.Instance.AddJob(Job.Default.WithToolchain(InProcessEmitToolchain.Instance)));

    [UsedImplicitly]
    static void RunTreeBenchmarks()
        => BenchmarkRunner.Run<TreeBenchmark>();
}
