//#define SINGLETHREADED

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using IncrementalMeanVarianceAccumulator;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsBenchmarks.MicroOrm
{
    sealed class Benchmarker
    {
        public int IterationsPerTry;
        public int Tries;
        public Action<string> Output = Console.WriteLine;
        int ReshuffledIndex(int i) => (int)(((ulong)i + 654321ul) * 17547989ul % (uint)IterationsPerTry);
        int ReshuffledIndexToRowCount(int i) => (int)(Math.Exp((double)i / IterationsPerTry * i / IterationsPerTry * 8) - 0.1);
        int IndexToRowCount(int i) => ReshuffledIndexToRowCount(ReshuffledIndex(i));

        void ReportShufflingErrors()
        {
            var shuffledDistinctCount = Enumerable.Range(0, IterationsPerTry).Select(ReshuffledIndex).Distinct().Count();
            if (shuffledDistinctCount != IterationsPerTry) {
                Output(
                    $"Shuffling of indexes is INVALID: shuffling {IterationsPerTry} indices resulted in just {shuffledDistinctCount} indices");
            }
        }

        public void ReportInitialDistribution()
        {
            ReportShufflingErrors();

            var rowCounts = Enumerable.Range(0, IterationsPerTry).Select(IndexToRowCount).ToArray();

            Output($"Testing {Tries} groups of {IterationsPerTry} queries with {rowCounts.Min()}-{rowCounts.Max()} rows");
            Output(
                $"median: {rowCounts.OrderBy(x => x).Skip((IterationsPerTry - 1) / 2).Take(2).Average()}; "
                    + $"mean: {rowCounts.Average():f2}; "
                    + $"{rowCounts.Count(x => x == 0) * 100.0 / IterationsPerTry:f2}% 0; "
                    + $"{rowCounts.Count(x => x == 1) * 100.0 / IterationsPerTry:f2}% 1; "
                    + $"{rowCounts.Count(x => x < 50) * 100.0 / IterationsPerTry:f2}% <50; "
                );
        }

        public void BenchSqlServer(string name, Func<SqlCommandCreationContext, int, int> action)
        {
            using (var ctx = CreateSqlConnection())
                ParameterizedSql.TableValuedTypeDefinitionScripts.ExecuteNonQuery(ctx);

            Bench(name, CreateSqlConnection, action);
        }

        public void BenchSQLite(string name, Func<SQLiteConnection, int, int> action)
        {
            Bench(name, CreateSqliteConnection, action);
        }

        public static SqlCommandCreationContext CreateSqlConnection()
        {
            var conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB");
            bool ok = false;
            try {
                conn.Open();

                var sqlCommandCreationContext = new SqlCommandCreationContext(conn, 0, null);
                ok = true;
                return sqlCommandCreationContext;
            } finally {
                if (!ok) {
                    conn.Dispose();
                }
            }
        }

        public static SQLiteConnection CreateSqliteConnection()
        {
            var conn = new SQLiteConnection(new SQLiteConnectionStringBuilder {
                DataSource = @":memory:", //benchmark.db
                JournalMode = SQLiteJournalModeEnum.Wal,
                FailIfMissing = false,
                DateTimeFormat = SQLiteDateFormats.Ticks,
            }.ToString());
            bool ok = false;
            try {
                conn.Open();

                conn.Query<ExampleObject>(@"
                    create table example (key INTEGER PRIMARY KEY, a int null, b int not null, c TEXT, d BOOLEAN null, e int not null);

                    insert into example (a,b,c,d,e)
                    select
                        a.x as a
                        , b.x as b
                        , c.x as c
                        , d.x as d
                        , e.x as e
                    from       (select 0 as x union all select 1 union all select null) a
                    cross join (select 0 as x union all select 1 union all select 2) b
                    cross join (select 'abracadabra fee fi fo fum' as x union all select null union all select 'quick brown fox') c
                    cross join(select cast(1 as bit) as x union all select cast(0 as bit) union all select null) d
                    cross join(select 0 as x union all select 1 union all select 2) e
                    cross join(select 0 as x union all select 1 union all select 2 union all select 3) f
                    cross join(select 0 as x union all select 1 union all select 2 union all select 3) g
                ");
                ok = true;
                return conn;
            } finally {
                if (!ok) {
                    conn.Dispose();
                }
            }
        }

        public void Bench<TConn>(string name, Func<TConn> connect, Func<TConn, int, int> action)
            where TConn : IDisposable
        {
            GC.Collect();
            var initialGen0 = GC.CollectionCount(0);
            var initialGen1 = GC.CollectionCount(1);
            var initialGen2 = GC.CollectionCount(2);
            var elapsed = new List<double>();
            long ignore = 0;
            var latencyDistribution = MeanVarianceAccumulator.Empty;
            for (var k = 0L; k < Tries; k++) {
                var i = 0;
                Func<MeanVarianceAccumulator> ExecuteBenchLoop = () => {
                    using (var conn = connect()) {
                        var latencies = MeanVarianceAccumulator.Empty;
                        var swInner = new Stopwatch();
                        while (true) {
                            var localI = Interlocked.Increment(ref i) - 1;
                            if (localI >= IterationsPerTry) {
                                break;
                            }
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
                elapsed.Add(sw.Elapsed.TotalMilliseconds * 1000.0);
                foreach (var task in tasks) {
                    latencyDistribution = latencyDistribution.Add(task.Result);
                }
#endif
            }

            var gen0 = GC.CollectionCount(0) - initialGen0;
            var gen1 = GC.CollectionCount(1) - initialGen1;
            var gen2 = GC.CollectionCount(2) - initialGen2;
            elapsed.Sort();
            var mean = elapsed.Average();
            var variance = elapsed.Select(t => (t - mean) * (t - mean)).Average() / (elapsed.Count - 1);
            var stddev = Math.Sqrt(variance);
            var scale = 1000.0 / (Tries * IterationsPerTry);
            Output($"{mean / IterationsPerTry:f2}μs ~ {stddev / IterationsPerTry:f2}μs overall;" +
                $"{latencyDistribution.Mean * 1000:f2}μs ~ {latencyDistribution.SampleStandardDeviation * 1000:f2}μs latency;" +
                $" {gen0 * scale:f4}/{gen1 * scale:f4}/{gen2 * scale:f4} milliGC;  {name}  {ignore}");
        }
    }
}
