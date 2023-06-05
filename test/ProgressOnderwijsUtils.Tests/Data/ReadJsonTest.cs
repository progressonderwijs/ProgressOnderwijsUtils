using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ProgressOnderwijsUtils.Tests.Data;

public sealed class ReadJsonTest : TransactedLocalConnection
{
    [Fact]
    public void Utf8JosonWriter_writes_invalid_json_when_aborted()
    {
        using var stream = new MemoryStream();
        {
            using var writer = new Utf8JsonWriter(stream);
            writer.WriteStartArray();
            writer.WriteStartObject();
            writer.WriteString("property", "testje");
        }

        var json = Maybe.Try(() => JsonNode.Parse(Encoding.UTF8.GetString(stream.ToArray()))).Catch<Exception>();
        PAssert.That(() => json.AssertError().Message.Contains("json", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Utf8JosonWriter_writes_invalid_json_upon_exception()
    {
        using var stream = new MemoryStream();
        try
        {
            using var writer = new Utf8JsonWriter(stream);
            writer.WriteStartArray();
            writer.WriteStartObject();
            writer.WriteString("property", "testje");
            throw new NotSupportedException();
        } catch (NotSupportedException) {}

        var json = Maybe.Try(() => JsonNode.Parse(Encoding.UTF8.GetString(stream.ToArray()))).Catch<Exception>();
        PAssert.That(() => json.AssertError().Message.Contains("json", StringComparison.OrdinalIgnoreCase));
    }

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
