using BenchmarkDotNet.Running;
using ProgressOnderwijsUtilsBenchmarks.OrderingBenchmarks;

namespace ProgressOnderwijsUtilsBenchmarks
{
    public class BenchmarkProgram
    {
        static void Main()
        {
            //SortedSetBench.ReportDistributionAndRun();
            SortedSetULongBench.RunBenchmarks();
            //BenchmarkRunner.Run<HtmlFragmentBenchmark>();
            //MicroOrm.MicroOrmBenchmarkProgram.RunBenchmarks();
        }
    }
}
