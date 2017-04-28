using System;
using System.Linq;
using Dapper;

namespace MicroOrmBench
{
    static class DapperExecutor
    {
        public static void RunQuery(Benchmarker benchmarker)
        {
            benchmarker.BenchSQLite("Dapper (sqlite)", (ctx, rows) =>
                ctx.Query<ExampleObject>(ExampleObject.RawSqliteQueryString, new { Arg = ExampleObject.someInt64Value, Top = rows, Num2 = 2, Hehe = "hehe" }).Count()
                );

            benchmarker.BenchSqlServer("Dapper", (ctx, rows) =>
                ctx.Connection.Query<ExampleObject>(ExampleObject.RawQueryString, new { Arg = ExampleObject.someInt64Value, Top = rows, Num2 = 2, Hehe = "hehe" }).Count()
            );
        }

        public static void RunWideQuery(Benchmarker benchmarker)
        {
            benchmarker.BenchSqlServer("Dapper (26-col)", (ctx, rows) =>
                ctx.Connection.Query<WideExampleObject>(WideExampleObject.RawQueryString, new { Top = rows, }).Count()
            );
        }
    }
}