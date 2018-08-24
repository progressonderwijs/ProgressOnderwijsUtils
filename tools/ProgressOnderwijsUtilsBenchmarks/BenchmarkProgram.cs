using BenchmarkDotNet.Running;

namespace ProgressOnderwijsUtilsBenchmarks
{
    public static class BenchmarkProgram
    {
        static void Main()
        {
            //RunArrayBuilderBenchmarks();
            BenchmarkRunner.Run<HtmlFragmentBenchmark>();
            //MicroOrm.MicroOrmBenchmarkProgram.RunBenchmarks();
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
