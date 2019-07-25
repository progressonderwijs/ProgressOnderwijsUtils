#nullable disable
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
            MicroOrmBenchmarkProgram.RunBenchmarks();
            //RunArrayBuilderBenchmarks();
        }

        [UsedImplicitly]
        static void RunArrayBuilderBenchmarks()
        {
            new BenchmarkSwitcher(new[] {
                typeof(IntArrayBuilderBenchmark),
                typeof(BigStructArrayBuilderBenchmark),
                typeof(ByteArrayBuilderBenchmark),
                typeof(ReferenceTypeArrayBuilderBenchmark),
                typeof(SmallStructArrayBuilderBenchmark),
            }).RunAllJoined();
        }
    }
}
