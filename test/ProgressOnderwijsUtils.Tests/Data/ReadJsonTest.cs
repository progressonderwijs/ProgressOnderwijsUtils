using System.IO.Pipelines;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ProgressOnderwijsUtils.Tests.Data;

public sealed class ReadJsonTest : TransactedLocalConnection
{
    [Fact]
    public void Utf8JosonWriter_writes_invalid_json_when_aborted()
    {
        var pipe = new Pipe();
        using var writer = new Utf8JsonWriter(pipe.Writer);
        writer.WriteStartArray();
        writer.WriteStartObject();
        writer.WriteString("property", "testje");
        pipe.Writer.Complete();

        var json = Maybe.Try(() => JsonNode.Parse(Encoding.UTF8.GetString(pipe.Reader.ReadAsync().GetAwaiter().GetResult().Buffer))).Catch<Exception>();
        PAssert.That(() => json.AssertError().Message.Contains("json", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Utf8JosonWriter_writes_invalid_json_upon_exception()
    {
        var pipe = new Pipe();
        try {
            using var writer = new Utf8JsonWriter(pipe.Writer);
            writer.WriteStartArray();
            writer.WriteStartObject();
            writer.WriteString("property", "testje");
            throw new NotSupportedException();
        } catch (NotSupportedException) { }
        pipe.Writer.Complete();

        var json = Maybe.Try(() => JsonNode.Parse(Encoding.UTF8.GetString(pipe.Reader.ReadAsync().GetAwaiter().GetResult().Buffer))).Catch<Exception>();
        PAssert.That(() => json.AssertError().Message.Contains("json", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ReadJson_can_read_all_known_used_column_types_from_the_db()
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

        var pipe = new Pipe();
        SQL($"select t.* from #ReadJsonTest t").ReadJson(Connection, pipe.Writer, new() { Indented = true, });
        pipe.Writer.Complete();

        ApprovalTest.CreateHere().AssertUnchangedAndSave(Encoding.UTF8.GetString(pipe.Reader.ReadAsync().GetAwaiter().GetResult().Buffer));
    }

    enum ReadJsonPocoTestId { }

    sealed record ReadJsonPocoTest : IWrittenImplicitly
    {
        public ReadJsonPocoTestId ReadJsonPocoTestId { get; init; }
        public bool BooleanColumn { get; init; }
        public int? NumberColumn { get; init; }
        public string? StringColumn { get; init; }
    }

    [Fact]
    public void Deserialize_ReadJson_gives_the_same_result_as_ReadPocos()
    {
        SQL(
            $@"
                create table #ReadJsonPocoTest (
                    ReadJsonPocoTestId int not null
                    , BooleanColumn bit not null
                    , NumberColumn int null
                    , StringColumn nvarchar(32) null
                );
            "
        ).ExecuteNonQuery(Connection);

        SQL(
            $@"
                insert into #ReadJsonPocoTest (
                    ReadJsonPocoTestId
                    , BooleanColumn
                    , NumberColumn
                    , StringColumn
                ) values
                    (1, {true}, {17}, {"iets"})
                    , (2, {false}, null, null);
            "
        ).ExecuteNonQuery(Connection);

        var query = SQL($"select t.* from #ReadJsonPocoTest t");
        var pocos = query.ReadPocos<ReadJsonPocoTest>(Connection);

        var pipe = new Pipe();
        query.ReadJson(Connection, pipe.Writer, new() { Indented = true, });
        pipe.Writer.Complete();
        var json = Encoding.UTF8.GetString(pipe.Reader.ReadAsync().GetAwaiter().GetResult().Buffer);
        var jsonPocos = JsonSerializer.Deserialize<ReadJsonPocoTest[]>(json).AssertNotNull();

        PAssert.That(() => jsonPocos.SequenceEqual(pocos));
    }
}
