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
        try {
            using var writer = new Utf8JsonWriter(stream);
            writer.WriteStartArray();
            writer.WriteStartObject();
            writer.WriteString("property", "testje");
            throw new NotSupportedException();
        } catch (NotSupportedException) { }

        var json = Maybe.Try(() => JsonNode.Parse(Encoding.UTF8.GetString(stream.ToArray()))).Catch<Exception>();
        PAssert.That(() => json.AssertError().Message.Contains("json", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ToDo()
    {
        SQL(
            $@"
                create table #ReadJsonTest (
                    ReadJsonTestId int not null

                    -- exact numerics
                    , BitColumn bit null
                    , IntColumn int null
                    , BigIntColumn bigint null
                    , DecimalColumn decimal(4,2) null

                    -- Approximate numerics
                    , FloatColumn float null

                    -- Date and time
                    , DateColumn date null
                    , DateTimeColumn datetime null
                    , DateTime2Column datetime2 null

                    -- Character strings
                    , CharColumn char null
                    , VarCharColumn varchar(32) null

                    -- Unicode character strings
                    , NCharColumn nchar null
                    , NVarCharColumn nvarchar(32) null

                    -- Binary strings
                    , BinaryColumn binary(8) null

                    -- Other data types
                    , UniqueIdentifierColumn uniqueidentifier null
                    , RowVersionColumn rowversion not null
                );
            "
        ).ExecuteNonQuery(Connection);

        SQL(
            $@"
                insert into #ReadJsonTest (
                    ReadJsonTestId
                    , BitColumn
                    , IntColumn
                    , BigIntColumn
                    , DecimalColumn
                    , FloatColumn
                    , DateColumn
                    , DateTimeColumn
                    , DateTime2Column
                    , CharColumn
                    , VarCharColumn
                    , NCharColumn
                    , NVarCharColumn
                    , BinaryColumn
                    , UniqueIdentifierColumn
                ) values
                    (1, {true}, {int.MaxValue}, {long.MaxValue}, {0.99m}, {1.234}, {new DateTime(2008, 4, 1)}, {new DateTime(2023, 5, 6, 16, 13, 55)}, {new DateTime(1, 2, 3, 4, 5, 6, 7)}, 'x', 'xyz', N'p', N'pqr', {new byte[] { 1, 2, 3 }}, {"82DBEE37-3AF8-46F2-A403-AE0A1950BC6E"} )
                    , (2, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
            "
        ).ExecuteNonQuery(Connection);

        using var stream = new MemoryStream();
        SQL($"select t.* from #ReadJsonTest t").ReadJson(Connection, stream);

        ApprovalTest.CreateHere().AssertUnchangedAndSave(Encoding.UTF8.GetString(stream.ToArray()));
    }
}
