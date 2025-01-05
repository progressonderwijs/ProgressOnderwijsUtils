//#define SINGLETHREADED

using System.Data.SQLite;
using Dapper;
using IncrementalMeanVarianceAccumulator;

namespace ProgressOnderwijsUtilsBenchmarks.MicroOrmBench;

sealed class Benchmarker
{
    public int IterationsPerTry;
    public int Tries;
    public Action<string> Output = Console.WriteLine;

    int ReshuffledIndex(int i)
        => (int)(((ulong)i + 654321ul) * 17547989ul % (uint)IterationsPerTry);

    int ReshuffledIndexToRowCount(int i)
        => (int)(Math.Exp((double)i / IterationsPerTry * i / IterationsPerTry * 8) - 0.1);

    int IndexToRowCount(int i)
        => ReshuffledIndexToRowCount(ReshuffledIndex(i));

    void ReportShufflingErrors()
    {
        var shuffledDistinctCount = Enumerable.Range(0, IterationsPerTry).Select(ReshuffledIndex).Distinct().Count();
        if (shuffledDistinctCount != IterationsPerTry) {
            Output($"Shuffling of indexes is INVALID: shuffling {IterationsPerTry} indices resulted in just {shuffledDistinctCount} indices");
        }
    }

    public void ReportInitialDistribution()
    {
        ReportShufflingErrors();

        var rowCounts = Enumerable.Range(0, IterationsPerTry).Select(IndexToRowCount).ToArray();

        Output($"Testing {Tries} groups of {IterationsPerTry} queries with {rowCounts.Min()}-{rowCounts.Max()} rows");
        Output(
            $"Row-count median: {rowCounts.OrderBy(x => x).Skip((IterationsPerTry - 1) / 2).Take(2).Average()}; "
            + $"mean: {rowCounts.Average():f1}; "
            + $"0 rows: {rowCounts.Count(x => x == 0) * 100.0 / IterationsPerTry:f2}%; "
            + $"1 row: {rowCounts.Count(x => x == 1) * 100.0 / IterationsPerTry:f2}%; "
            + $"<50 rows: {rowCounts.Count(x => x < 50) * 100.0 / IterationsPerTry:f2}% <50"
        );
        Output($"Reporting time per query execution; parallel executions use {ThreadCount} threads");
    }

    public void BenchSqlServer(string name, Func<SqlConnection, int, int> action)
    {
        using (var sqlConn = CreateSqlConnection()) {
            ParameterizedSql.TableValuedTypeDefinitionScripts.ExecuteNonQuery(sqlConn);
        }

        Bench(name, CreateSqlConnection, action);
    }

    public void BenchEFSqlServer(string name, Func<EntityFrameworkBench, int, int> action)
    {
        using (var sqlConn = CreateSqlConnection()) {
            ParameterizedSql.TableValuedTypeDefinitionScripts.ExecuteNonQuery(sqlConn);
        }

        Bench(name, () => new(CreateSqlConnection()), action);
    }

    public void BenchSQLite(string name, Func<SQLiteConnection, int, int> action)
        => Bench(name, CreateSqliteConnection, action);

    public static SqlConnection CreateSqlConnection()
    {
        var sqlConn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB");
        try {
            sqlConn.Open();
        } catch {
            sqlConn.Dispose();
            throw;
        }
        return sqlConn;
    }

    public static SQLiteConnection CreateSqliteConnection()
    {
        var sqliteConn = new SQLiteConnection(
            new SQLiteConnectionStringBuilder {
                DataSource = @":memory:", //benchmark.db
                JournalMode = SQLiteJournalModeEnum.Wal,
                FailIfMissing = false,
                DateTimeFormat = SQLiteDateFormats.Ticks,
            }.ToString()
        );
        var ok = false;
        try {
            sqliteConn.Open();
            ExampleObject.PrefillExampleTable(sqliteConn);
            ok = true;
            return sqliteConn;
        } finally {
            if (!ok) {
                sqliteConn.Dispose();
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
            var ExecuteBenchLoop = () => {
                using var conn = connect();
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
            };
            var sw = Stopwatch.StartNew();
            var tasks = Enumerable.Range(0, ThreadCount).Select(_ => Task.Factory.StartNew(ExecuteBenchLoop, TaskCreationOptions.LongRunning)).ToArray();
            Task.WaitAll(tasks);
            elapsed.Add(sw.Elapsed.TotalMilliseconds * 1000.0);
            foreach (var task in tasks) {
                latencyDistribution = latencyDistribution.Add(task.GetAwaiter().GetResult());
            }
        }

        var gen0 = GC.CollectionCount(0) - initialGen0;
        var gen1 = GC.CollectionCount(1) - initialGen1;
        var gen2 = GC.CollectionCount(2) - initialGen2;
        elapsed.Sort();
        var mean = elapsed.Average();
        var variance = elapsed.Select(t => (t - mean) * (t - mean)).Average() / (elapsed.Count - 1);
        var stddev = Math.Sqrt(variance);
        var scale = 1000.0 / (Tries * IterationsPerTry);
        Output(
            $"{mean / IterationsPerTry:f1}μs ~ {stddev / IterationsPerTry:f2}μs parallel;" +
            $"{latencyDistribution.Mean * 1000:f0}μs ~ {latencyDistribution.SampleStandardDeviation * 1000:f1}μs latency;" +
            $" {gen0 * scale:f2}/{gen1 * scale:f2}/{gen2 * scale:f2} milliGC;  {name}  {ignore}"
        );
    }

    public static int ThreadCount
#if SINGLETHREADED
        => 1;
#else
        => Environment.ProcessorCount + 1 >> 1;
#endif
}
