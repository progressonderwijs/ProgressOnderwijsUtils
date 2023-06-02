using System.Threading.Tasks;

namespace ProgressOnderwijsUtils.Tests.Data;

public sealed class ReadJsonTest : TransactedLocalConnection
{
    [Fact]
    public async Task ToDo()
    {
        SQL(
            $@"
                create table #ReadJsonTest (
                    ReadJsonTestId int not null
                    , BooleanColumn bit null
                    , NumberColumn int null
                    , StringColumn nvarchar(32) null
                );
            "
        ).ExecuteNonQuery(Connection);

        SQL(
            $@"
                insert into #ReadJsonTest values
                    (1, {true}, 17, 'iets'),
                    (2, null, null, null);
            "
        ).ExecuteNonQuery(Connection);

        using var stream = new MemoryStream();
        await SQL($"select t.* from #ReadJsonTest t").ReadJsonAsync(Connection, stream, CancellationToken.None);

        ApprovalTest.CreateHere().AssertUnchangedAndSave(Encoding.UTF8.GetString(stream.ToArray()));
    }
}
