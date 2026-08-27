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

    [Fact]
    public void DateOnly_roundtrips_via_database()
    {
        SQL($"""
            create table #DateTest (
                Datum date not null,
                NullabeleDatum date null
            )
            """).ExecuteNonQuery(Connection);

        SQL($"insert into #DateTest values ({new DateOnly(2025, 6, 1)}, {new DateOnly(2000, 12, 31)})").ExecuteNonQuery(Connection);
        SQL($"insert into #DateTest values ({new DateOnly(1999, 1, 1)}, {(DateOnly?)null})").ExecuteNonQuery(Connection);

        var plainResults = SQL($"select Datum from #DateTest order by Datum").ReadPlain<DateOnly>(Connection);
        PAssert.That(() => Enumerable.SequenceEqual(plainResults, new[] { new DateOnly(1999, 1, 1), new DateOnly(2025, 6, 1), }));

        var pocoResults = SQL($"select Datum, NullabeleDatum from #DateTest order by Datum").ReadPocos<DateOnlyPoco>(Connection);
        PAssert.That(() => pocoResults[0] == new DateOnlyPoco { Datum = new(1999, 1, 1), NullabeleDatum = null, });
        PAssert.That(() => pocoResults[1] == new DateOnlyPoco { Datum = new(2025, 6, 1), NullabeleDatum = new(2000, 12, 31), });

        var filtered = SQL($"select Datum from #DateTest where 1=1 and Datum = {new DateOnly(2025, 6, 1)}").ReadPlain<DateOnly>(Connection);
        PAssert.That(() => filtered.Single() == new DateOnly(2025, 6, 1));
    }
}
