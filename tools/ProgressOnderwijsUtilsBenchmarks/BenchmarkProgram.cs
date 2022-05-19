using BenchmarkDotNet.Running;
using ProgressOnderwijsUtilsBenchmarks.MicroOrmBench;

namespace ProgressOnderwijsUtilsBenchmarks;

public static class BenchmarkProgram
{
    static void Main()
        // => RunTreeBenchmarks();
        //=> BenchmarkRunner.Run<HtmlFragmentBenchmark>();
        //=> MicroOrmBenchmarkProgram.RunBenchmarks();
        //=> RunArrayBuilderBenchmarks();
        => BenchmarkRunner.Run<SmallBatchInsertBench>();

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
