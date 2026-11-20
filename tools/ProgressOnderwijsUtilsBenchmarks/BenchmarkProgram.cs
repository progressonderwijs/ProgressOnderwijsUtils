using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using ProgressOnderwijsUtils.Benchmarks.Metrics;
using ProgressOnderwijsUtils.Benchmarks.MicroOrmBench;
using ProgressOnderwijsUtils.Benchmarks.NullabilityVerifierBenchmark;

namespace ProgressOnderwijsUtils.Benchmarks;

public static class BenchmarkProgram
{
    /// <summary>
    /// Usage: dotnet run -- &lt;benchmark&gt;
    /// </summary>
    /// <example>
    /// cd tools\ProgressOnderwijsUtilsBenchmarks
    /// dotnet run -c Release -- metrics
    /// </example>
    static void Main(string[] args)
    {
        var benchmarks = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase) {
            ["metrics"] = RunMetricsBenchmarks,
            ["tree"] = RunTreeBenchmarks,
            ["arraybuilder"] = RunArrayBuilderBenchmarks,
            ["microorm"] = MicroOrmBenchmarkProgram.RunBenchmarks,
            ["smallbatchinsert"] = () => BenchmarkRunner.Run<SmallBatchInsertBench>(),
            ["nullability"] = () => BenchmarkRunner.Run<NullabilityBenchmark>(),
            ["randomhelper"] = () => BenchmarkRunner.Run<RandomHelperBenchmark>(),
        };

        if (args is { Length: 1, } && benchmarks.TryGetValue(args[0], out var selected)) {
            selected();
        } else {
            Console.Error.WriteLine($"Unknown option(s) '{args.JoinStrings(", ")}'. Choose one of: {benchmarks.Keys.JoinStrings(", ")}");
            Environment.ExitCode = 1;
        }
    }

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
