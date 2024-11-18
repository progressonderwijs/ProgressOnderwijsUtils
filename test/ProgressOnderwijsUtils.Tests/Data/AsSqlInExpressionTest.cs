namespace ProgressOnderwijsUtils.Tests.Data;

public sealed class AsSqlInExpressionTest : TransactedLocalConnection
{
    [Fact]
    public void In_helper_generates_for_empty_set()
    {
        var xyInEmptySql = SQL($"x.y in {Array.Empty<int>().AsUnrolledSqlInExpression()}");
        PAssert.That(() => xyInEmptySql.CommandText() == "x.y in (null)");
    }

    [Fact]
    public void In_helper_generates_for_single_item()
    {
        var sql = SQL($"x.y in {new[] { 7, }.AsUnrolledSqlInExpression()}");
        PAssert.That(() => sql.CommandText() == "x.y in ( @par0 )");
    }

    [Fact]
    public void In_helper_generates_for_multiple_items()
    {
        var sql = SQL($"x.y in {new[] { 7, 11, 13, }.AsUnrolledSqlInExpression()}");
        PAssert.That(() => sql.CommandText() == "x.y in ( @par0 , @par1 , @par2 )");
    }

    [Fact]
    public void In_helper_works_on_actual_query()
    {
        SQL(
            $"""
            create table Tin (x int not null, y int null);
            insert into Tin (x, y) values (3, 7), (11, null);
            """
        ).ExecuteNonQuery(Connection);

        var xQueriedInEmpty = SQL($"select t.x from Tin t where t.x in {Array.Empty<int>().AsUnrolledSqlInExpression()}").ReadPlain<int?>(Connection);
        PAssert.That(() => xQueriedInEmpty.None());

        var yQueriedInEmpty = SQL($"select t.y from Tin t where t.y in {Array.Empty<int>().AsUnrolledSqlInExpression()}").ReadPlain<int?>(Connection);
        PAssert.That(() => yQueriedInEmpty.None());

        var xQueriedInExistingSet = SQL($"select t.x from Tin t where t.x in {new[] { 3, }.AsUnrolledSqlInExpression()}").ReadPlain<int?>(Connection);
        PAssert.That(() => xQueriedInExistingSet.Single() == 3);

        var xQueriedInNonExistingSet = SQL($"select t.x from Tin t where t.x in {new[] { 999, }.AsUnrolledSqlInExpression()}").ReadPlain<int?>(Connection);
        PAssert.That(() => xQueriedInNonExistingSet.None());

        var yQueriedPartialOverlap = SQL($"select t.y from Tin t where t.y in {new[] { 7, 999, }.AsUnrolledSqlInExpression()}").ReadPlain<int?>(Connection);
        PAssert.That(() => yQueriedPartialOverlap.Single() == 7);
    }
}
