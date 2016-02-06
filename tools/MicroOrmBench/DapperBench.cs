using System.Linq;
using Dapper;

namespace MicroOrmBench
{
    static class DapperExecutor
    {
        public static void RunQuery(Benchmarker benchmarker)
        {
            benchmarker.Bench("Dapper", (ctx, rows) =>
                ctx.Connection.Query<ExampleObject>(ExampleObject.RawQueryString, new { Arg = (int?)null, Top = rows, Num2 = 2, Hehe = "hehe" }).Count()
            );
        }

        public static void RunWideQuery(Benchmarker benchmarker)
        {
            benchmarker.Bench("Dapper (26-col)", (ctx, rows) =>
                ctx.Connection.Query<WideExampleObject>(WideExampleObject.RawQueryString, new { Top = rows, }).Count()
            );
        }
    }
}