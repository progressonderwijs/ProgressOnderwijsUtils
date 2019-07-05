using System.Linq;
using JetBrains.Annotations;
using ProgressOnderwijsUtils;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtilsBenchmarks.MicroOrmBench
{
    static class ParameterizedSqlExecutor
    {
        public static void RunQuery([NotNull] Benchmarker benchmarker)
            => benchmarker.BenchSqlServer("ParameterizedSql",
                (sqlConn, rows) => ExampleObject.ParameterizedSqlForRows(rows)
                    .ReadPocos<ExampleObject>(sqlConn)
                    .Length);

        public static void RunTvpQuery([NotNull] Benchmarker benchmarker)
            => benchmarker.BenchSqlServer("ParameterizedSql-TVP",
                (sqlConn, rows) =>
                    SQL($"select QueryTableValue from ({Enumerable.Range(0, rows).ToArray()}) x")
                        .ReadPlain<int>(sqlConn)
                        .Length);

        public static void RunWideQuery([NotNull] Benchmarker benchmarker)
            => benchmarker.BenchSqlServer("ParameterizedSql (26-col)",
                (sqlConn, rows) => WideExampleObject.ParameterizedSqlForRows(rows)
                    .ReadPocos<WideExampleObject>(sqlConn)
                    .Length);

        public static void ConstructWithoutExecuting([NotNull] Benchmarker benchmarker)
            => benchmarker.BenchSqlServer("ParameterizedSql noexec",
                (sqlConn, rows) => {
                    ExampleObject.ParameterizedSqlForRows(rows).CreateSqlCommand(sqlConn, CommandTimeout.WithoutTimeout).Dispose();
                    return 0;
                });
    }
}
