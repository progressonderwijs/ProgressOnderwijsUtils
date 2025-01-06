namespace ProgressOnderwijsUtilsBenchmarks.MicroOrmBench;

static class MicroOrmBenchmarkProgram
{
    public static void RunBenchmarks()
    {
        using var proc = Process.GetCurrentProcess();
        proc.PriorityClass = ProcessPriorityClass.AboveNormal;
        var benchmarker = new Benchmarker { IterationsPerTry = 2000, Tries = 100, };
        benchmarker.ReportInitialDistribution();
        Console.WriteLine("Warming up...");
        RunCurrentBenchmarks(new() { IterationsPerTry = 16, Tries = 3, Output = _ => { }, }); //warm-up
        Console.WriteLine("Benchmarking...");
        RunCurrentBenchmarks(benchmarker);
    }

    static void RunCurrentBenchmarks(Benchmarker benchmarker)
    {
        benchmarker.Output("Running 6-column queries ");

        EntityFrameworkBench.RunQuery(benchmarker);
        DapperExecutor.RunQuery(benchmarker);
        ParameterizedSqlExecutor.RunQuery(benchmarker);
        HandrolledAdoNetExecutor.RunQuery(benchmarker);

        benchmarker.Output("Running 26-column queries (shape of AdventureWorks SalesOrderHeader)");

        EntityFrameworkBench.RunWideQuery(benchmarker);
        DapperExecutor.RunWideQuery(benchmarker);
        ParameterizedSqlExecutor.RunWideQuery(benchmarker);
        HandrolledAdoNetExecutor.RunWideQuery(benchmarker);

        benchmarker.Output("Running special-case benchmarks");

        ParameterizedSqlExecutor.RunTvpQuery(benchmarker);
        ParameterizedSqlExecutor.ConstructWithoutExecuting(benchmarker);
    }
}
