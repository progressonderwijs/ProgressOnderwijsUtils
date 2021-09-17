using BenchmarkDotNet.Running;
using JetBrains.Annotations;
using ProgressOnderwijsUtilsBenchmarks.MicroOrmBench;

namespace ProgressOnderwijsUtilsBenchmarks
{
    public static class BenchmarkProgram
    {
        static void Main()
        {
            //BenchmarkRunner.Run<HtmlFragmentBenchmark>();
            //MicroOrmBenchmarkProgram.RunBenchmarks();
            //RunTreeBenchmarks();
            //RunArrayBuilderBenchmarks();
            _ = BenchmarkRunner.Run<SmallBatchInsertBench>();
        }

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
}
