using BenchmarkDotNet.Running;
using ProgressOnderwijsUtilsBenchmarks.NullabilityVerifierBenchmark;

namespace ProgressOnderwijsUtilsBenchmarks;

public static class BenchmarkProgram
{
    static void Main()
        // => RunTreeBenchmarks();
        //=> MicroOrmBenchmarkProgram.RunBenchmarks();
        //=> RunArrayBuilderBenchmarks();
        //=> BenchmarkRunner.Run<SmallBatchInsertBench>();
        //=> BenchmarkRunner.Run<NullabilityBenchmark>();
        => BenchmarkRunner.Run<RandomHelperBenchmark>();

    [UsedImplicitly]
    static void RunArrayBuilderBenchmarks()
    {
        IntArrayBuilderBenchmark.SanityCheck(10000);
        _ = new BenchmarkSwitcher(
            new[] {
                typeof(IntArrayBuilderBenchmark),
                typeof(BigStructArrayBuilderBenchmark),
                typeof(ByteArrayBuilderBenchmark),
                typeof(ReferenceTypeArrayBuilderBenchmark),
                typeof(SmallStructArrayBuilderBenchmark),
            }
        ).RunAllJoined();
    }

    [UsedImplicitly]
    static void RunTreeBenchmarks()
        => BenchmarkRunner.Run<TreeBenchmark>();
}
