namespace ProgressOnderwijsUtils.Tests.Data;

public sealed class AsSqlInExpressionTest : TransactedLocalConnection
{
    [Fact]
    public void In_helper_generates_for_empty_set()
        => PAssert.That(() => SQL($"x.y in {Array.Empty<int>().AsUnrolledSqlInExpression()}").CommandText() == "x.y in (null)");

    [Fact]
    public void In_helper_generates_for_single_item()
        => PAssert.That(() => SQL($"x.y in {new[] { 7, }.AsUnrolledSqlInExpression()}").CommandText() == "x.y in ( @par0 )");

    [Fact]
    public void In_helper_generates_for_multiple_items()
        => PAssert.That(() => SQL($"x.y in {new[] { 7, 11, 13, }.AsUnrolledSqlInExpression()}").CommandText() == "x.y in ( @par0 , @par1 , @par2 )");

    [Fact]
    public void In_helper_works_on_actual_query()
    {
        SQL(
            $@"
                create table Tin (x int not null, y int null);
                insert into Tin (x, y) values (3, 7), (11, null);
            "
        ).ExecuteNonQuery(Connection);

        PAssert.That(() => SQL($"select t.x from Tin t where t.x in {Array.Empty<int>().AsUnrolledSqlInExpression()}").ReadPlain<int?>(Connection).None());
        PAssert.That(() => SQL($"select t.y from Tin t where t.y in {Array.Empty<int>().AsUnrolledSqlInExpression()}").ReadPlain<int?>(Connection).None());

        PAssert.That(() => SQL($"select t.x from Tin t where t.x in {new[] { 3, }.AsUnrolledSqlInExpression()}").ReadPlain<int?>(Connection).Single() == 3);
        PAssert.That(() => SQL($"select t.x from Tin t where t.x in {new[] { 999, }.AsUnrolledSqlInExpression()}").ReadPlain<int?>(Connection).None());

        PAssert.That(() => SQL($"select t.y from Tin t where t.y in {new[] { 7, 999, }.AsUnrolledSqlInExpression()}").ReadPlain<int?>(Connection).Single() == 7);
    }
}
