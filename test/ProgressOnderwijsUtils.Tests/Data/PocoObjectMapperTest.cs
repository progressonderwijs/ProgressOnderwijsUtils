using System;
using System.Linq;
using System.Runtime.CompilerServices;
using ExpressionToCodeLib;
using Microsoft.Data.SqlClient;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests.Data
{
    public sealed class PocoObjectMapperTest : TransactedLocalConnection
    {
        public static ParameterizedSql ParameterizedSqlForRows(int rows)
            => SQL($@"
                SELECT top ({rows})
                    SalesOrderID,DueDate,ShipDate,Status,OnlineOrderFlag,AccountNumber,SalesPersonID,TotalDue,Comment,rowGuid, SomeBlob, SomeNullableBlob
                from (select SalesOrderID = 13 union all select 14) a01
                cross join(select AccountNumber = N'abracadabra fee fi fo fum' union all select N'abcdef') a02
                cross join(select Comment = N'abracadabra fee fi fo fum' union all select null) a04
                cross join(select DueDate = cast('2014-01-02' as datetime2) union all select cast('2014-01-03' as datetime2)) a09
                cross join(select OnlineOrderFlag = cast(1 as bit) union all select cast(0 as bit)) a12
                cross join(select Rowguid = NEWID ( )) a16
                cross join(select SalesPersonId = 37 union all select null ) a18
                cross join(select ShipDate = cast('2014-01-02' as datetime2) union all select null) a19
                cross join(select Status = cast(1 as tinyint) union all select cast(10 as tinyint)) a22
                cross join(select TotalDue = cast(1.1 as decimal(18,2))) a26
                cross join(select SomeBlob = cast('deadbeef' as varbinary(max))) a27
                cross join(select SomeNullableBlob = cast('deadbeef' as varbinary(max)) union all select null) a28
            "
            );

        public sealed class ExampleWithJustSetters : IWrittenImplicitly
        {
            public int SalesOrderId { get; set; }
            public string AccountNumber { get; set; } = null!;
            public string? Comment { get; set; }
            public DateTime DueDate { get; set; }
            public bool OnlineOrderFlag { get; set; }
            public Guid Rowguid { get; set; }
            public int? SalesPersonId { get; set; }
            public DateTime? ShipDate { get; set; }
            public byte Status { get; set; }
            public decimal TotalDue { get; set; }
            public byte[] SomeBlob { get; set; } = null!;
            public byte[]? SomeNullableBlob { get; set; }
        }

        [Fact]
        public void ReadWithJustSetters()
        {
            // ReSharper disable once UnusedVariable
            var bla = ParameterizedSqlForRows(512).OfPocos<ExampleWithJustSetters>().Execute(Connection);
            PAssert.That(() => bla.Length == 512);
            PAssert.That(() => bla.Select(o => o.AccountNumber).Distinct().SetEqual(new[] { "abracadabra fee fi fo fum", "abcdef" }));
        }

        public sealed class ExampleWithJustSettersWithExtraProperties : IWrittenImplicitly
        {
            public int ExtraExtra { get; set; }
            public int SalesOrderId { get; set; }
            public string AccountNumber { get; set; } = null!;
            public string? Comment { get; set; }
            public DateTime DueDate { get; set; }
            public bool OnlineOrderFlag { get; set; }
            public Guid Rowguid { get; set; }
            public int? SalesPersonId { get; set; }
            public DateTime? ShipDate { get; set; }
            public byte Status { get; set; }
            public decimal TotalDue { get; set; }
            public byte[] SomeBlob { get; set; } = null!;
            public byte[]? SomeNullableBlob { get; set; }
        }

        [Fact]
        public void ReadWithMissingMappingsDoesNotIgnoreExtraProperties()
        {
            // ReSharper disable once UnusedVariable
            var query = ParameterizedSqlForRows(512).OfPocos<ExampleWithJustSettersWithMissingProperties>();
            Assert.ThrowsAny<Exception>(() => query.Execute(Connection));
            Assert.ThrowsAny<Exception>(() => query.WithFieldMappingMode(FieldMappingMode.IgnoreExtraPocoProperties).Execute(Connection));
            var retval = SQL($"select AccountNumber, SalesOrderId from ({query.Sql}) x").ReadPocos<ExampleWithJustSettersWithMissingProperties>(Connection); //implicitly assert does not throw

            PAssert.That(() => retval.Length == 512);
            PAssert.That(() => retval.Select(o => o.AccountNumber).Distinct().SetEqual(new[] { "abracadabra fee fi fo fum", "abcdef" }));
        }

        public sealed class ExampleWithJustSettersWithMissingProperties : IWrittenImplicitly
        {
            public string AccountNumber { get; set; } = null!;
            public int SalesOrderId { get; set; }
        }

        [Fact]
        public void ReadWithExtraMappings()
        {
            // ReSharper disable once UnusedVariable
            var query = ParameterizedSqlForRows(512).OfPocos<ExampleWithJustSettersWithExtraProperties>();
            Assert.ThrowsAny<Exception>(() => query.Execute(Connection));
            var retval = query.WithFieldMappingMode(FieldMappingMode.IgnoreExtraPocoProperties).Execute(Connection); // implicitly assert: does not throw.

            PAssert.That(() => retval.Length == 512);
            PAssert.That(() => retval.Select(o => o.AccountNumber).Distinct().SetEqual(new[] { "abracadabra fee fi fo fum", "abcdef" }));
        }

        public sealed record ExampleWithConstructor(string AccountNumber, byte[] SomeBlob) : IWrittenImplicitly
        {
            public int SalesOrderId { get; set; }
            public string? Comment { get; set; }
            public DateTime DueDate { get; set; }
            public bool OnlineOrderFlag { get; set; }
            public Guid Rowguid { get; set; }
            public int? SalesPersonId { get; set; }
            public DateTime? ShipDate { get; set; }
            public byte Status { get; set; }
            public decimal TotalDue { get; set; }
            public byte[]? SomeNullableBlob { get; set; }
        }

        [Fact]
        public void ReadWithSomeConstructorArgs()
        {
            // ReSharper disable once UnusedVariable
            var bla = ParameterizedSqlForRows(512).OfPocos<ExampleWithConstructor>().Execute(Connection);
            PAssert.That(() => bla.Length == 512);
            PAssert.That(() => bla.Select(o => o.AccountNumber).Distinct().SetEqual(new[] { "abracadabra fee fi fo fum", "abcdef" }));
        }

        public sealed class ExampleWithMoreConstructor : IWrittenImplicitly
        {
            public ExampleWithMoreConstructor(string accountNumber, byte[] someBlob)
            {
                AccountNumber = accountNumber;
                SomeBlob = someBlob;
                throw new Exception("This constructor should never be selected, the poco orm should choose the longest constructor");
            }

            public ExampleWithMoreConstructor(int salesOrderId, string accountNumber, DateTime dueDate, bool onlineOrderFlag, byte status, decimal totalDue, byte[] someBlob)
            {
                SalesOrderId = salesOrderId;
                AccountNumber = accountNumber;
                DueDate = dueDate;
                OnlineOrderFlag = onlineOrderFlag;
                Status = status;
                TotalDue = totalDue;
                SomeBlob = someBlob;
            }
            // ReSharper disable UnusedParameter.Local
#pragma warning disable IDE0060 // Remove unused parameter
            public ExampleWithMoreConstructor(int salesOrderId, string accountNumber, DateTime dueDate, bool onlineOrderFlag, byte status, decimal totalDue, byte[] someBlob, int unmatchable)
#pragma warning restore IDE0060 // Remove unused parameter
                // ReSharper restore UnusedParameter.Local
            {
                throw new Exception("This constructor should never be selected, the poco orm should not choose a constructor with an unmatchable arg");
            }

            public int SalesOrderId { get; }
            public string AccountNumber { get; }
            public string? Comment { get; set; }
            public DateTime DueDate { get; }
            public bool OnlineOrderFlag { get; }
            public Guid Rowguid { get; set; }
            public int? SalesPersonId { get; set; }
            public DateTime? ShipDate { get; set; }
            public byte Status { get; }
            public decimal TotalDue { get; }
            public byte[] SomeBlob { get; }
            public byte[]? SomeNullableBlob { get; set; }
        }

        [Fact]
        public void ReadWithManyConstructorArgs()
        {
            // ReSharper disable once UnusedVariable
            var bla = ParameterizedSqlForRows(512).OfPocos<ExampleWithMoreConstructor>().Execute(Connection);
            PAssert.That(() => bla.Length == 512);
            PAssert.That(() => bla.Select(o => o.AccountNumber).Distinct().SetEqual(new[] { "abracadabra fee fi fo fum", "abcdef" }));
            PAssert.That(() => bla.Select(o => o.Status).Distinct().SetEqual(new byte[] { 1, 10 }));
            PAssert.That(() => bla.None(o => o.SomeBlob.PretendNullable() == null));
            PAssert.That(() => bla.Any(o => o.SomeNullableBlob == null) && bla.Any(o => o.SomeNullableBlob != null));
        }

        public sealed class ExampleWithConstructorWithExtraProperties : IWrittenImplicitly
        {
            public ExampleWithConstructorWithExtraProperties(string accountNumber, byte[] someBlob)
            {
                AccountNumber = accountNumber;
                SomeBlob = someBlob;
            }

            public int ExtraExtra { get; set; }
            public int SalesOrderId { get; set; }
            public string AccountNumber { get; }
            public string? Comment { get; set; }
            public DateTime DueDate { get; set; }
            public bool OnlineOrderFlag { get; set; }
            public Guid Rowguid { get; set; }
            public int? SalesPersonId { get; set; }
            public DateTime? ShipDate { get; set; }
            public byte Status { get; set; }
            public decimal TotalDue { get; set; }
            public byte[] SomeBlob { get; }
            public byte[]? SomeNullableBlob { get; set; }
        }

        [Fact]
        public void ReadByConstructorWithExtraMappings()
        {
            // ReSharper disable once UnusedVariable
            var query = ParameterizedSqlForRows(512).OfPocos<ExampleWithConstructorWithExtraProperties>();
            Assert.ThrowsAny<Exception>(() => query.Execute(Connection));
            var retval = query.WithFieldMappingMode(FieldMappingMode.IgnoreExtraPocoProperties).Execute(Connection); // implicitly assert: does not throw.

            PAssert.That(() => retval.Length == 512);
            PAssert.That(() => retval.Select(o => o.AccountNumber).Distinct().SetEqual(new[] { "abracadabra fee fi fo fum", "abcdef" }));
        }

        public enum Enum64Bit : ulong { }

        public sealed record PocoWithRowVersions(ulong Version) : IWrittenImplicitly, IReadImplicitly
        {
            public ulong AnotherVersion { get; init; }
            public uint? AshorterVersion { get; init; }
            public Enum64Bit AFinalVersion { get; init; }
            public int Counter { get; init; }

            public static ParameterizedSql CreateTableWithSampleData(SqlConnection sqlConnection)
            {
                var tableName = SQL($"#rowversions");
                SQL($@"
                create table {tableName} (
                    AShorterVersion binary(4)
                    , AnotherVersion binary(8) not null
                    , Version rowversion
                    , AFinalVersion varbinary(max)
                    , Counter int identity not null
                );
            ").ExecuteNonQuery(sqlConnection);

                SQL($@"
                insert into {tableName} (AShorterVersion, AnotherVersion, AFinalVersion) values (cast(1 as int), cast(2 as bigint), cast(3 as bigint));
                insert into {tableName} (AShorterVersion, AnotherVersion, AFinalVersion) values (cast(100 as int), cast(20000 as bigint), cast(30000 as bigint));
                insert into {tableName} (AShorterVersion, AnotherVersion, AFinalVersion) values (cast(10000 as int), cast(200000000 as bigint), cast(300000000 as bigint));
                insert into {tableName} (AShorterVersion, AnotherVersion, AFinalVersion) values (cast(1000000 as int), cast(2000000000000 as bigint), cast(3000000000000 as bigint));
                insert into {tableName} (AShorterVersion, AnotherVersion, AFinalVersion) values (cast(100000000 as int), cast(20000000000000000 as bigint), cast(30000000000000000 as bigint));
            ").ExecuteNonQuery(sqlConnection);
                return tableName;
            }
        }

        [Fact]
        public void CanReadULong()
        {
            var tableName = PocoWithRowVersions.CreateTableWithSampleData(Connection);

            var pocos = SQL($"select * from {tableName} order by Counter").ReadPocos<PocoWithRowVersions>(Connection);

            PAssert.That(() => pocos.Length == 5);

            var expected = Enumerable.Range(0, 5)
                .Select(power => new PocoWithRowVersions(0) { Counter = power + 1, AshorterVersion = 1 * (uint)Math.Pow(100, power), AnotherVersion = 2 * (ulong)Math.Pow(10000, power), AFinalVersion = (Enum64Bit)(3 * (ulong)Math.Pow(10000, power)) })
                .ToArray();

            var actualWithoutRowversion = pocos.Select(rec => rec with { Version = 0 }); //can't predict roversion, just its ordering

            PAssert.That(() => actualWithoutRowversion.SequenceEqual(expected));

            PAssert.That(() => pocos.SequenceEqual(pocos.OrderBy(o => o.Counter)));
            PAssert.That(() => pocos.SequenceEqual(pocos.OrderBy(o => o.Version)));
            PAssert.That(() => pocos.SequenceEqual(pocos.OrderBy(o => o.AshorterVersion)));
            PAssert.That(() => pocos.SequenceEqual(pocos.OrderBy(o => o.AFinalVersion)));
            PAssert.That(() => pocos.SequenceEqual(pocos.OrderBy(o => o.AnotherVersion)));
        }

        [Fact]
        public void CanParameterizeULong()
        {
            var tableName = PocoWithRowVersions.CreateTableWithSampleData(Connection);

            var pocos = SQL($"select * from {tableName} order by Counter").ReadPocos<PocoWithRowVersions>(Connection);

            var middle = pocos[2];
            var uints = SQL($"select AshorterVersion from {tableName} where Version > {middle.Version} and AnotherVersion >= {pocos[3].AnotherVersion} order by Version").ReadPlain<uint>(Connection);
            PAssert.That(() => uints.SequenceEqual(new[] { 1000000u, 100000000u }));
        }

        [Fact]
        public void UInt32_round_trips()
        {
            for (var val = uint.MaxValue; val > 0; val = val / 4 + val / 3) {
                var fromDb = SQL($"select {val}").ReadPlain<uint>(Connection).Single();
                PAssert.That(() => fromDb == val);
                var scalarFromDb = SQL($"select {val}").ReadScalar<uint>(Connection);
                PAssert.That(() => scalarFromDb == val);
            }
        }

        [Fact]
        public void ULongPoco_BulkCopyRoundTrips()
        {
            var tableName = PocoWithRowVersions.CreateTableWithSampleData(Connection);
            var initialPocos = SQL($"select * from {tableName} order by Counter").ReadPocos<PocoWithRowVersions>(Connection);
            PAssert.That(() => initialPocos.Length == 5);
            SQL($"delete from {tableName}").ExecuteNonQuery(Connection);
            var rowsAfterDelete = SQL($"select * from {tableName} order by Counter").ReadPocos<PocoWithRowVersions>(Connection);
            PAssert.That(() => rowsAfterDelete.None());

            initialPocos.BulkCopyToSqlServer(Connection, BulkInsertTarget.LoadFromTable(Connection, tableName).With(BulkCopyFieldMappingMode.AllowExtraPocoProperties));

            var rowsAfterBulkInsert = SQL($"select * from {tableName} order by Counter").ReadPocos<PocoWithRowVersions>(Connection);

            var expected = Enumerable.Range(0, initialPocos.Length)
                .Select(power => new PocoWithRowVersions(0) {
                    Counter = power + 1 + initialPocos.Length,
                    AshorterVersion = 1 * (uint)Math.Pow(100, power),
                    AnotherVersion = 2 * (ulong)Math.Pow(10000, power),
                    AFinalVersion = (Enum64Bit)(3 * (ulong)Math.Pow(10000, power))
                })
                .ToArray();

            var actualWithoutRowversion = rowsAfterBulkInsert.Select(rec => rec with { Version = 0 }); //can't predict roversion, just its ordering

            PAssert.That(() => actualWithoutRowversion.SequenceEqual(expected));
            PAssert.That(() => !rowsAfterBulkInsert.SequenceEqual(expected), "this should differ because the DB should have assigned rowversions");
        }
    }
}
