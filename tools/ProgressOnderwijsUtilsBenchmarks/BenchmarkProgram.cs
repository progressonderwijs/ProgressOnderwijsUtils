using BenchmarkDotNet.Running;

namespace ProgressOnderwijsUtilsBenchmarks
{
    public static class BenchmarkProgram
    {
        static void Main()
        {
            MicroOrm.MicroOrmBenchmarkProgram.RunBenchmarks();
            return;
            RunArrayBuilderBenchmarks();
            BenchmarkRunner.Run<HtmlFragmentBenchmark>();
        }

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
