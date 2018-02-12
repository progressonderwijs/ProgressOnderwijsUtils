using System.Linq;
using Dapper;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtilsBenchmarks.MicroOrm
{
    static class DapperExecutor
    {
        public static void RunQuery([NotNull] Benchmarker benchmarker)
        {
            benchmarker.BenchSQLite("Dapper (sqlite)", (ctx, rows) =>
                ctx.Query<ExampleObject>(ExampleObject.RawSqliteQueryString, new { Arg = ExampleObject.someInt64Value, Top = rows, Num2 = 2, Hehe = "hehe" }).Count()
                );

            benchmarker.BenchSqlServer("Dapper", (ctx, rows) =>
                ctx.Connection.Query<ExampleObject>(ExampleObject.RawQueryString, new { Arg = ExampleObject.someInt64Value, Top = rows, Num2 = 2, Hehe = "hehe" }).Count()
            );
        }

        public static void RunWideQuery([NotNull] Benchmarker benchmarker)
        {
            benchmarker.BenchSqlServer("Dapper (26-col)", (ctx, rows) =>
                ctx.Connection.Query<WideExampleObject>(WideExampleObject.RawQueryString, new { Top = rows, }).Count()
            );
        }
    }
}