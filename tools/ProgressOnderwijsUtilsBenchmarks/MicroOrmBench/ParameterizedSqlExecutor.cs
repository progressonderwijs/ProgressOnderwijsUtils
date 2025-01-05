namespace ProgressOnderwijsUtilsBenchmarks.MicroOrmBench;

static class ParameterizedSqlExecutor
{
    public static void RunQuery(Benchmarker benchmarker)
        => benchmarker.BenchSqlServer(
            "ParameterizedSql",
            (sqlConn, rows) => ExampleObject.ParameterizedSqlForRows(rows)
                .ReadPocos<ExampleObject>(sqlConn)
                .Length
        );

    public static void RunWideQuery(Benchmarker benchmarker)
        => benchmarker.BenchSqlServer(
            "ParameterizedSql (26-col)",
            (sqlConn, rows) => WideExampleObject.ParameterizedSqlForRows(rows)
                .ReadPocos<WideExampleObject>(sqlConn)
                .Length
        );

    public static void RunTvpQuery(Benchmarker benchmarker)
        => benchmarker.BenchSqlServer(
            "ParameterizedSql table valued parameter",
            (sqlConn, rows) =>
                SQL($"select QueryTableValue from ({Enumerable.Range(0, rows).ToArray()}) x")
                    .ReadPlain<int>(sqlConn)
                    .Length
        );

    public static void ConstructWithoutExecuting(Benchmarker benchmarker)
        => benchmarker.BenchSqlServer(
            "ParameterizedSql noexec",
            (sqlConn, rows) => {
                ExampleObject.ParameterizedSqlForRows(rows).CreateSqlCommand(sqlConn, CommandTimeout.WithoutTimeout).Dispose();
                return 0;
            }
        );
}
