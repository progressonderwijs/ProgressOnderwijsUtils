using JetBrains.Annotations;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsBenchmarks.MicroOrm
{
    static class ParameterizedSqlExecutor
    {
        public static void RunQuery([NotNull] Benchmarker benchmarker)
        {
            benchmarker.BenchSqlServer("ParameterizedSql",
                (ctx, rows) => ExampleObject.ParameterizedSqlForRows(rows)
                    .ReadMetaObjects<ExampleObject>(ctx)
                    .Length)
                ;
        }

        public static void RunWideQuery([NotNull] Benchmarker benchmarker)
        {
            benchmarker.BenchSqlServer("ParameterizedSql (26-col)",
                (ctx, rows) => WideExampleObject.ParameterizedSqlForRows(rows)
                    .ReadMetaObjects<WideExampleObject>(ctx)
                    .Length)
                ;
        }

        public static void ConstructWithoutExecuting([NotNull] Benchmarker benchmarker)
        {
            benchmarker.BenchSqlServer("ParameterizedSql noexec",
                (ctx, rows) =>
                {
                    ExampleObject.ParameterizedSqlForRows(rows).CreateSqlCommand(ctx).Dispose();
                    return 0;
                });
        }
    }
}