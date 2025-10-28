using System.IO.Pipelines;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

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
            $"""
            create table #ReadJsonTest (
                ReadJsonTestId int not null
            
                -- exact numerics
                , BitColumn bit
                , IntColumn int
                , BigIntColumn bigint
                , DecimalColumn decimal(4,2)
            
                -- Approximate numerics
                , FloatColumn float
            
                -- Date and time
                , DateColumn date
                , DateTimeOffsetColumn datetimeoffset
            
                -- Character strings
                , CharColumn char
                , VarCharColumn varchar(32)
            
                -- Unicode character strings
                , NCharColumn nchar
                , NVarCharColumn nvarchar(32)
            
                -- Binary strings (equiv. to rowversion)
                , BinaryColumn binary(8)
            
                -- Other data types
                , UniqueIdentifierColumn uniqueidentifier
            );
            """
        ).ExecuteNonQuery(Connection);

        SQL(
            $"""
            insert into #ReadJsonTest (
                ReadJsonTestId
                , BitColumn
                , IntColumn
                , BigIntColumn
                , DecimalColumn
                , FloatColumn
                , DateColumn
                , DateTimeOffsetColumn
                , CharColumn
                , VarCharColumn
                , NCharColumn
                , NVarCharColumn
                , BinaryColumn
                , UniqueIdentifierColumn
            ) values
                (1, {true}, {int.MaxValue}, {long.MaxValue}, {0.99m}, {1.234}, {new DateTime(2008, 4, 1)}, {new DateTime(2023, 11, 9, 8, 25, 01, DateTimeKind.Utc)}, 'x', 'xyz', N'p', N'pqr', {new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }}, {"82DBEE37-3AF8-46F2-A403-AE0A1950BC6E"} )
                , (2, null, null, null, null, null, null, null, null, null, null, null, null, null);
            """
        ).ExecuteNonQuery(Connection);

        var pipe = new Pipe();
        SQL($"select t.* from #ReadJsonTest t order by t.ReadJsonTestId").ReadJson(Connection, pipe.Writer, new() { Indented = true, }, JsonIgnoreCondition.Never);
        pipe.Writer.Complete();

        ApprovalTest.CreateHere().AssertUnchangedAndSave(Encoding.UTF8.GetString(pipe.Reader.ReadAsync().GetAwaiter().GetResult().Buffer));
    }

    enum ReadJsonPocoTestId { }

    [Fact]
    public void ReadJson_datetime_with_timezone_information()
    {
        SQL(
            $"""
            create table #ReadJsonTest (
                DateTimeColumn datetime
                , DateTime2Column datetime2
                , DateTime2Column_Utc datetime2
                , DateTimeOffsetColumn datetimeoffset
            );
            """
        ).ExecuteNonQuery(Connection);

        var dateTime = new DateTime(1, 2, 3, 4, 5, 6, 7);
        SQL(
            $"""
            insert into #ReadJsonTest (
                DateTimeColumn
                , DateTime2Column
                , DateTime2Column_Utc
                , DateTimeOffsetColumn
            ) values
                ({new DateTime(2023, 5, 6, 16, 13, 55)}, {dateTime}, {dateTime.ToUniversalTime()}, {new DateTime(2023, 11, 9, 8, 19, 27, DateTimeKind.Utc)})
            """
        ).ExecuteNonQuery(Connection);

        var pipe = new Pipe();
        SQL($"select t.* from #ReadJsonTest t").ReadJson(Connection, pipe.Writer, new() { Indented = true, });
        pipe.Writer.Complete();

        var json = Encoding.UTF8.GetString(pipe.Reader.ReadAsync().GetAwaiter().GetResult().Buffer);
        foreach (var line in json.Split("\r\n").Where(l => l.Contains(":"))) {
            PAssert.That(() => line.Split(": ", StringSplitOptions.None)[1].Contains("+"));
        }
    }

    sealed record ReadJsonPocoTest : IWrittenImplicitly
    {
        public ReadJsonPocoTestId ReadJsonPocoTestId { get; init; }
        public bool BooleanColumn { get; init; }
        public int? NumberColumn { get; init; }
        public long? LongColumn { get; init; }
        public decimal? DecimalColumn { get; init; }
        public double? DoubleColumn { get; init; }
        public string? StringColumn { get; init; }
        public DateTime? DateTimeColumn { get; init; }
        public byte[]? BinaryColumn { get; init; }
        public ulong RijRevisie { get; init; }
    }

    [Fact]
    public void Deserialize_ReadJson_gives_the_same_result_as_ReadPocos()
    {
        SQL(
            $"""
            create table #ReadJsonPocoTest (
                ReadJsonPocoTestId int not null
                , BooleanColumn bit not null
                , NumberColumn int
                , LongColumn bigint
                , DecimalColumn decimal(10, 2)
                , DoubleColumn float(53)
                , StringColumn nvarchar(32)
                , DateTimeColumn datetime2
                , BinaryColumn varbinary(32)
                , RijRevisie rowversion
            );
            """
        ).ExecuteNonQuery(Connection);

        SQL(
            $"""
            insert into #ReadJsonPocoTest (
                ReadJsonPocoTestId
                , BooleanColumn
                , NumberColumn
                , LongColumn
                , DoubleColumn
                , DecimalColumn
                , StringColumn
                , DateTimeColumn
                , BinaryColumn
            ) values
                (1, {true}, {17}, {long.MaxValue}, {12.99m}, {1.23456789}, {"iets"}, {new DateTime(2000, 4, 1, 9, 32, 55)}, {new byte[] { 255, 254, 253, 252, 251, 250, 249, 248, 247, 246, 245 }})
                , (2, {false}, null, null, null, null, null, null, null);
            """
        ).ExecuteNonQuery(Connection);

        var query = SQL($"select t.* from #ReadJsonPocoTest t order by t.ReadJsonPocoTestId");
        var pocos = query.ReadPocos<ReadJsonPocoTest>(Connection);

        var pipe = new Pipe();
        query.ReadJson(Connection, pipe.Writer, new() { Indented = true, }, JsonIgnoreCondition.WhenWritingNull, true);
        pipe.Writer.Complete();
        var json = Encoding.UTF8.GetString(pipe.Reader.ReadAsync().GetAwaiter().GetResult().Buffer);
        var jsonPocos = JsonSerializer.Deserialize<ReadJsonPocoTest[]>(json).AssertNotNull();

        PAssert.That(() => jsonPocos.Length == pocos.Length);
        for (var i = 0; i < jsonPocos.Length; i++) {
            var structered = jsonPocos[i] with {
                BinaryColumn = pocos[i].BinaryColumn,
            };
            PAssert.That(() => structered == pocos[i]);

            if (jsonPocos[i].BinaryColumn is { } buf) {
                PAssert.That(() => buf.SequenceEqual(pocos[i].BinaryColumn.AssertNotNull()));
            } else {
                PAssert.That(() => pocos[i].BinaryColumn == null);
            }
        }
    }

    [Fact]
    public void Null_properties_can_be_serialized_by_configuration()
    {
        SQL(
            $"""
            create table #ReadJsonNullsTest (
                I int null
                , S nvarchar(12) null
            );
            """
        ).ExecuteNonQuery(Connection);

        SQL(
            $"""
            insert into #ReadJsonNullsTest (I, S) values
                (1, 'een')
                , (null, 'twee')
                , (2, null)
                , (null, null)
            """
        ).ExecuteNonQuery(Connection);

        var pipe = new Pipe();
        SQL($"select t.* from #ReadJsonNullsTest t").ReadJson(Connection, pipe.Writer, new() { Indented = true, });
        SQL($"select t.* from #ReadJsonNullsTest t").ReadJson(Connection, pipe.Writer, new() { Indented = true, }, JsonIgnoreCondition.Never);
        pipe.Writer.Complete();

        var json = Encoding.UTF8.GetString(pipe.Reader.ReadAsync().GetAwaiter().GetResult().Buffer);
        ApprovalTest.CreateHere().AssertUnchangedAndSave(json);
    }
}
