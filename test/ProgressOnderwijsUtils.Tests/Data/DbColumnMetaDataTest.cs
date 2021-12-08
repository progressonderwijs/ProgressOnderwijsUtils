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
        public TimeSpan TimeSpan { get; set; }
    }

    [Fact]
    public void CreatedTempTableMetaDataRoundTrips()
    {
        var columnsFromCode = PocoProperties<SamplePoco>.Instance.ArraySelect(prop => DbColumnMetaData.Create(prop.Name, prop.DataType, prop.IsKey, null));

        var tempTableName = SQL($"#test");
        columnsFromCode.CreateNewTableQuery(tempTableName).ExecuteNonQuery(Connection);

        var columnsFromCodeAsSql = columnsFromCode.ArraySelect(c => c.ToSqlColumnDefinition());
        var columnsFromDbAsSql = DbColumnMetaData.ColumnMetaDatas(Connection, tempTableName).ArraySelect(c => c.ToSqlColumnDefinition());

        PAssert.That(() => columnsFromCodeAsSql.SequenceEqual(columnsFromDbAsSql));
    }

    [Fact]
    public void Varbinary_ToSqlColumnDefinition_ExampleWorks()
        => PAssert.That(() => DbColumnMetaData.Create("test", typeof(byte[]), false, 42).ToSqlColumnDefinition() == "test VarBinary(42) null");

    [Fact]
    public void VarbinaryMax_ToSqlColumnDefinition_ExampleWorks()
        => PAssert.That(() => DbColumnMetaData.Create("test", typeof(byte[]), false, null).ToSqlColumnDefinition() == "test VarBinary(max) null");

    [Fact]
    public void NVarchar_ToSqlColumnDefinition_ExampleWorks()
        => PAssert.That(() => DbColumnMetaData.Create("test3", typeof(string), false, 42).ToSqlColumnDefinition() == "test3 NVarChar(42) null");

    [Fact]
    public void NChar_ToSqlColumnDefinition_ExampleWorks()
        => PAssert.That(() => DbColumnMetaData.Create("test", typeof(char), false, null).ToSqlColumnDefinition() == "test NChar(1) not null");
}
