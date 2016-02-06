﻿namespace MicroOrmBench
{
    static class Program
    {
        public static void Main()
        {
            var benchmarker = new Benchmarker {IterationsPerTry = 1000, Tries = 50};
            benchmarker.ReportInitialDistribution();
            RunCurrentBenchmarks(new Benchmarker {IterationsPerTry = 8, Tries = 2, Output = _ => { }}); //warm-up
            RunCurrentBenchmarks(benchmarker);
        }

        static void RunCurrentBenchmarks(Benchmarker benchmarker)
        {
            HandrolledAdoNetExecutor.RunQuery(benchmarker);
            DapperExecutor.RunQuery(benchmarker);
            ParameterizedSqlExecutor.RunQuery(benchmarker);
            //HandrolledAdoNetExecutor.RunWideQuery(benchmarker);
            //DapperExecutor.RunWideQuery(benchmarker);
            //ParameterizedSqlExecutor.RunWideQuery(benchmarker);

            //ParameterizedSqlExecutor.ConstructWithoutExecuting(benchmarker);
        }
    }
}