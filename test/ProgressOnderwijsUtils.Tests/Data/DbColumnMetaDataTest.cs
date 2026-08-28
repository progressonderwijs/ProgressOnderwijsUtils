namespace ProgressOnderwijsUtils.Tests.Data;

public sealed class DbColumnMetaDataTest : TransactedLocalConnection
{
    public sealed record SamplePoco : IWrittenImplicitly, IReadImplicitly
    {
        public DayOfWeek AnEnum { get; set; }
        public DateTime? ADateTime { get; set; }
        public string? SomeString { get; set; }
        public decimal? LotsOfMoney { get; set; }
        public double VagueNumber { get; set; }
        public DateTime DateTime { get; set; }
        public DateOnly DateOnly { get; set; }
        public TimeSpan TimeSpan { get; set; }
    }

    [Fact]
    public void CreatedTempTableMetaDataRoundTrips()
    {
        var columnsFromCode = PocoProperties<SamplePoco>.Instance.ArraySelect(prop => DbColumnMetaData.Create(prop.Name, prop.DataType, prop.IsKey, null, null));

        var tempTableName = SQL($"#test");
        columnsFromCode.CreateNewTableQuery(tempTableName).ExecuteNonQuery(Connection);

        var columnsFromCodeAsSql = columnsFromCode.ArraySelect(c => c.ToSqlColumnDefinition());
        var columnsFromDbAsSql = DbColumnMetaData.ColumnMetaDatas(Connection, tempTableName).ArraySelect(c => c.ToSqlColumnDefinition());

        PAssert.That(() => columnsFromCodeAsSql.AsEnumerable().SequenceEqual(columnsFromDbAsSql));
    }

    [Fact]
    public void Varbinary_ToSqlColumnDefinition_ExampleWorks()
        => PAssert.That(() => DbColumnMetaData.Create("test", typeof(byte[]), false, 42, null).ToSqlColumnDefinition() == "test VarBinary(42) null");

    [Fact]
    public void VarbinaryMax_ToSqlColumnDefinition_ExampleWorks()
        => PAssert.That(() => DbColumnMetaData.Create("test", typeof(byte[]), false, null, null).ToSqlColumnDefinition() == "test VarBinary(max) null");

    [Fact]
    public void NVarchar_ToSqlColumnDefinition_ExampleWorks()
        => PAssert.That(() => DbColumnMetaData.Create("test3", typeof(string), false, 42, null).ToSqlColumnDefinition() == $"test3 NVarChar(42) collate {DbColumnExtensions.DefaultDbCollation} null");

    [Fact]
    public void NVarchar_ToSqlColumnDefinitionWithCollation_ExampleWorks()
        => PAssert.That(() => DbColumnMetaData.Create("test3", typeof(string), false, 42, "Latin1_General_100_BIN2_UTF8").ToSqlColumnDefinition() == "test3 NVarChar(42) collate Latin1_General_100_BIN2_UTF8 null");

    [Fact]
    public void NChar_ToSqlColumnDefinition_ExampleWorks()
        => PAssert.That(() => DbColumnMetaData.Create("test", typeof(char), false, null, null).ToSqlColumnDefinition() == $"test NChar(1) collate {DbColumnExtensions.DefaultDbCollation} not null");

    [Fact]
    public void DateTime_ToSqlColumnDefinition_ExampleWorks()
        => PAssert.That(() => DbColumnMetaData.Create("test", typeof(DateTime), false, null, null).ToSqlColumnDefinition() == "test DateTime2 not null");

    [Fact]
    public void DateOnly_ToSqlColumnDefinition_ExampleWorks()
        => PAssert.That(() => DbColumnMetaData.Create("test", typeof(DateOnly), false, null, null).ToSqlColumnDefinition() == "test Date not null");

    public sealed record DateOnlyPoco : IWrittenImplicitly
    {
        public DateOnly Datum { get; init; }
        public DateOnly? NullabeleDatum { get; init; }
    }

    public sealed record DateTimePoco : IWrittenImplicitly
    {
        public DateTime Datum { get; init; }
        public DateTime? NullabeleDatum { get; init; }
    }

    public sealed record DateTimeDatumTijdPoco : IWrittenImplicitly
    {
        public DateTime DatumTijd { get; init; }
    }

    public sealed record DateOnlyVanDateTimePoco : IWrittenImplicitly
    {
        public DateOnly DatumTijd { get; init; }
    }

    [Fact]
    public void DateOnly_roundtrips_via_database()
    {
        SQL($"""
            create table #DateTest (
                Datum date not null,
                NullabeleDatum date null,
                DatumTijd datetime2 not null
            )
            """).ExecuteNonQuery(Connection);

        SQL($"insert into #DateTest values ({new DateOnly(2025, 6, 1)}, {new DateOnly(2000, 12, 31)}, {new DateTime(2026, 7, 1, 1, 1, 1)})").ExecuteNonQuery(Connection);
        SQL($"insert into #DateTest values ({new DateOnly(1999, 1, 1)}, {(DateOnly?)null}, {new DateTime(2005, 6, 9, 1, 1, 1)})").ExecuteNonQuery(Connection);

        // db: date -> code: DateOnly -> OK
        var dateToDateOnly = SQL($"select Datum from #DateTest order by Datum").ReadPlain<DateOnly>(Connection);
        PAssert.That(() => Enumerable.SequenceEqual(dateToDateOnly, new[] { new DateOnly(1999, 1, 1), new DateOnly(2025, 6, 1), }));

        var pocoResults = SQL($"select Datum, NullabeleDatum from #DateTest order by Datum").ReadPocos<DateOnlyPoco>(Connection);
        PAssert.That(() => pocoResults[0] == new DateOnlyPoco { Datum = new(1999, 1, 1), NullabeleDatum = null, });
        PAssert.That(() => pocoResults[1] == new DateOnlyPoco { Datum = new(2025, 6, 1), NullabeleDatum = new(2000, 12, 31), });

        // db: datetime2 -> code: DateOnly -> FAIL
        Assert.ThrowsAny<Exception>(() => SQL($"select DatumTijd from #DateTest order by DatumTijd").ReadPlain<DateOnly>(Connection));
        Assert.ThrowsAny<Exception>(() => SQL($"select DatumTijd from #DateTest order by DatumTijd").ReadPocos<DateOnlyVanDateTimePoco>(Connection));

        // db: date -> code: DateTime -> OK (for now)
        var dateToDateTime = SQL($"select Datum from #DateTest order by Datum").ReadPlain<DateTime>(Connection);
        PAssert.That(() => Enumerable.SequenceEqual(dateToDateTime, new[] { new DateTime(1999, 1, 1), new DateTime(2025, 6, 1), }));

        var dateTimePocoResults = SQL($"select Datum, NullabeleDatum from #DateTest order by Datum").ReadPocos<DateTimePoco>(Connection);
        PAssert.That(() => dateTimePocoResults[0] == new DateTimePoco { Datum = new(1999, 1, 1), NullabeleDatum = null, });
        PAssert.That(() => dateTimePocoResults[1] == new DateTimePoco { Datum = new(2025, 6, 1), NullabeleDatum = new(2000, 12, 31), });

        // db: datetime2 -> code: DateTime -> OK
        var datetime2ToDateTime = SQL($"select DatumTijd from #DateTest order by DatumTijd").ReadPlain<DateTime>(Connection);
        PAssert.That(() => Enumerable.SequenceEqual(datetime2ToDateTime, new[] { new DateTime(2005, 6, 9, 1, 1, 1), new DateTime(2026, 7, 1, 1, 1, 1), }));

        var datetime2PocoResults = SQL($"select DatumTijd from #DateTest order by DatumTijd").ReadPocos<DateTimeDatumTijdPoco>(Connection);
        PAssert.That(() => datetime2PocoResults[0] == new DateTimeDatumTijdPoco { DatumTijd = new(2005, 6, 9, 1, 1, 1), });
        PAssert.That(() => datetime2PocoResults[1] == new DateTimeDatumTijdPoco { DatumTijd = new(2026, 7, 1, 1, 1, 1), });

        // DateOnly as filter parameter
        var filtered = SQL($"select Datum from #DateTest where 1=1 and Datum = {new DateOnly(2025, 6, 1)}").ReadPlain<DateOnly>(Connection);
        PAssert.That(() => filtered.Single() == new DateOnly(2025, 6, 1));
    }
}
