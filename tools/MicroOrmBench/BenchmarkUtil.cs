﻿//#define SINGLETHREADED
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IncrementalMeanVarianceAccumulator;
using ProgressOnderwijsUtils;

namespace MicroOrmBench
{
    class Benchmarker
    {
        public int IterationsPerTry;
        public int Tries;
        public Action<string> Output = Console.WriteLine;

        int ReshuffledIndex(int i) => (int) (((ulong) i + 654321ul)*17547989ul%(uint) IterationsPerTry);
        int ReshuffledIndexToRowCount(int i) => (int) (Math.Exp((double) i/IterationsPerTry*i/IterationsPerTry*8) - 0.1);
        int IndexToRowCount(int i) => ReshuffledIndexToRowCount(ReshuffledIndex(i));

        void ReportShufflingErrors()
        {
            var shuffledDistinctCount = Enumerable.Range(0, IterationsPerTry).Select(ReshuffledIndex).Distinct().Count();
            if (shuffledDistinctCount != IterationsPerTry)
                Output(
                    $"Shuffling of indexes is INVALID: shuffling {IterationsPerTry} indices resulted in just {shuffledDistinctCount} indices");
        }

        public void ReportInitialDistribution()
        {
            ReportShufflingErrors();

            var rowCounts = Enumerable.Range(0, IterationsPerTry).Select(IndexToRowCount).ToArray();

            Output($"Testing {Tries} groups of {IterationsPerTry} queries with {rowCounts.Min()}-{rowCounts.Max()} rows");
            Output(
                $"median: {rowCounts.OrderBy(x => x).Skip((IterationsPerTry - 1)/2).Take(2).Average()}; "
                + $"mean: {rowCounts.Average():f2}; "
                + $"{rowCounts.Count(x => x == 0)*100.0/IterationsPerTry:f2}% 0; "
                + $"{rowCounts.Count(x => x == 1)*100.0/IterationsPerTry:f2}% 1; "
                + $"{rowCounts.Count(x => x < 50)*100.0/IterationsPerTry:f2}% <50; "
                );
        }

        public void Bench(string name, Func<SqlCommandCreationContext, int, int> action)
        {
            GC.Collect();
            var initialGen0 = GC.CollectionCount(0);
            var initialGen1 = GC.CollectionCount(1);
            var initialGen2 = GC.CollectionCount(2);
            var elapsed = new List<double>();
            long ignore = 0;
            var latencyDistribution = MeanVarianceAccumulator.Empty;
            for (var k = 0L; k < Tries; k++)
            {
                int i = 0;
                Func<MeanVarianceAccumulator> ExecuteBenchLoop = () => {
                    using (
                        var conn = (SqlCommandCreationContext)new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB")) {
                        conn.Connection.Open();
                        var latencies = MeanVarianceAccumulator.Empty;
                        var swInner = new Stopwatch();
                        while (true) {
                            var localI = Interlocked.Increment(ref i) - 1;
                            if (localI >= IterationsPerTry)
                                break;
                            swInner.Restart();
                            var val = action(conn, IndexToRowCount(localI));
                            latencies = latencies.Add(swInner.Elapsed.TotalMilliseconds);
                            Interlocked.Add(ref ignore, val);
                        }
                        return latencies;
                    }
                };
                var sw = Stopwatch.StartNew();
#if SINGLETHREADED
                var innerLatencyDistribution = ExecuteBenchLoop();
                elapsed.Add(sw.Elapsed.TotalMilliseconds * 1000.0);
                latencyDistribution = latencyDistribution.Add(innerLatencyDistribution);
#else
                var tasks = Enumerable.Range(0, Environment.ProcessorCount).Select(tI => Task.Factory.StartNew(ExecuteBenchLoop, TaskCreationOptions.LongRunning)).ToArray();
                Task.WaitAll(tasks);
                elapsed.Add(sw.Elapsed.TotalMilliseconds*1000.0);
                foreach (var task in tasks)
                    latencyDistribution = latencyDistribution.Add(task.Result);
#endif

}

            var gen0 = GC.CollectionCount(0) - initialGen0;
            var gen1 = GC.CollectionCount(1) - initialGen1;
            var gen2 = GC.CollectionCount(2) - initialGen2;
            elapsed.Sort();
            var mean = elapsed.Average();
            var variance = elapsed.Select(t => (t - mean)*(t - mean)).Average()/(elapsed.Count - 1);
            var stddev = Math.Sqrt(variance);
            var scale = 1000.0/(Tries*IterationsPerTry);
                       Output($"{mean/IterationsPerTry:f2}μs ~ {stddev/IterationsPerTry:f2}μs overall;" +
                    $"{latencyDistribution.Mean * 1000:f2}μs ~ {latencyDistribution.SampleStandardDeviation * 1000:f2}μs latency;" +
                   $" {gen0*scale:f4}/{gen1*scale:f4}/{gen2*scale:f4} milliGC;  {name}  {ignore}");

        }
    }
}