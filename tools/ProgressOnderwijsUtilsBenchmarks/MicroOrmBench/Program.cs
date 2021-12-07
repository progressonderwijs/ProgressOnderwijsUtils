namespace ProgressOnderwijsUtilsBenchmarks.MicroOrmBench;

static class MicroOrmBenchmarkProgram
{
    public static void RunBenchmarks()
    {
        var benchmarker = new Benchmarker { IterationsPerTry = 2000, Tries = 100, };
        benchmarker.ReportInitialDistribution();
        RunCurrentBenchmarks(new() { IterationsPerTry = 8, Tries = 2, Output = _ => { }, }); //warm-up
        RunCurrentBenchmarks(benchmarker);
    }

    static void RunCurrentBenchmarks(Benchmarker benchmarker)
    {
        ParameterizedSqlExecutor.ConstructWithoutExecuting(benchmarker);
        ParameterizedSqlExecutor.RunQuery(benchmarker);
        ParameterizedSqlExecutor.RunWideQuery(benchmarker);
        ParameterizedSqlExecutor.RunTvpQuery(benchmarker);
        DapperExecutor.RunQuery(benchmarker);
        HandrolledAdoNetExecutor.RunQuery(benchmarker);
        HandrolledAdoNetExecutor.RunWideQuery(benchmarker);
        DapperExecutor.RunWideQuery(benchmarker);
    }
}
