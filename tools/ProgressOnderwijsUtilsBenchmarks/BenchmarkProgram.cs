using BenchmarkDotNet.Running;
using ProgressOnderwijsUtilsBenchmarks.OrderingBenchmarks;

namespace ProgressOnderwijsUtilsBenchmarks
{
    public class BenchmarkProgram
    {
        static void Main()
        {
            BenchmarkRunner.Run<SortedSetBench>();
            //BenchmarkRunner.Run<HtmlFragmentBenchmark>();
            //MicroOrm.MicroOrmBenchmarkProgram.RunBenchmarks();
        }
    }
}
