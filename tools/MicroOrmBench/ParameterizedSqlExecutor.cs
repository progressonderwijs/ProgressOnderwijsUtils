using System.Linq;
using Dapper;
using ProgressOnderwijsUtils;

namespace MicroOrmBench
{
    static class ParameterizedSqlExecutor
    {
        public static void RunQuery(Benchmarker benchmarker)
        {
            benchmarker.Bench("ParameterizedSql",
                (ctx, rows) => ExampleObject.ParameterizedSqlForRows(rows)
                    .ReadMetaObjects<ExampleObject>(ctx)
                    .Length)
                ;
        }

        public static void RunWideQuery(Benchmarker benchmarker)
        {
            benchmarker.Bench("ParameterizedSql (26-col)",
                (ctx, rows) => WideExampleObject.ParameterizedSqlForRows(rows)
                    .ReadMetaObjects<WideExampleObject>(ctx)
                    .Length)
                ;
        }

        public static void ConstructWithoutExecuting(Benchmarker benchmarker)
        {
            benchmarker.Bench("ParameterizedSql noexec",
                (ctx, rows) =>
                {
                    ExampleObject.ParameterizedSqlForRows(rows).CreateSqlCommand(ctx).Dispose();
                    return 0;
                });
        }
    }
}