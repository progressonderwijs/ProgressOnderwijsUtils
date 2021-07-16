using System.Linq;
using Dapper;

namespace ProgressOnderwijsUtilsBenchmarks.MicroOrmBench
{
    static class DapperExecutor
    {
        public static void RunQuery(Benchmarker benchmarker)
        {
            benchmarker.BenchSQLite("Dapper (sqlite)", (sqlConn, rows) =>
                sqlConn.Query<ExampleObject>(ExampleObject.RawSqliteQueryString, new { Arg = ExampleObject.someInt64Value, Top = rows, Num2 = 2, Hehe = "hehe" }).Count()
            );

            benchmarker.BenchSqlServer("Dapper", (sqlConn, rows) =>
                sqlConn.Query<ExampleObject>(ExampleObject.RawQueryString, new { Arg = ExampleObject.someInt64Value, Top = rows, Num2 = 2, Hehe = "hehe" }).Count()
            );
        }

        public static void RunWideQuery(Benchmarker benchmarker)
        {
            benchmarker.BenchSqlServer("Dapper (26-col)", (sqlConn, rows) =>
                sqlConn.Query<WideExampleObject>(WideExampleObject.RawQueryString, new { Top = rows, }).Count()
            );
        }
    }
}
