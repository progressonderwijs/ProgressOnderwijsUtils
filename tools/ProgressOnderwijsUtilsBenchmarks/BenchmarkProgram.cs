using BenchmarkDotNet.Running;

namespace ProgressOnderwijsUtilsBenchmarks
{
    public class BenchmarkProgram
    {
        static void Main()
        {
            BenchmarkRunner.Run<HtmlFragmentBenchmark>();
            //MicroOrm.MicroOrmBenchmarkProgram.RunBenchmarks();
        }
    }
}
