#nullable disable
using System;
using System.Data.SqlClient;
using System.Linq;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.SchemaReflection;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests.Data
{
    public sealed class BulkInsertTest : TransactedLocalConnection
    {
        BulkInsertTarget CreateTable(ParameterizedSql tempTableName)
        {
            SQL($@"
                create table {tempTableName} (
                    AnEnum int not null
                    , ADateTime datetime2
                    , SomeString nvarchar(max)
                    , LotsOfMoney decimal(19, 5)
                    , VagueNumber float not null
                    , CustomBla nvarchar(max) not null
                    , CustomBlaThanCanBeNull nvarchar(max) null
                )
            ").ExecuteNonQuery(Connection);

            return BulkInsertTarget.LoadFromTable(Connection, tempTableName.CommandText());
        }

        sealed class SampleRow : ValueBase<SampleRow>, IWrittenImplicitly, IReadImplicitly
        {
            public DayOfWeek AnEnum { get; set; }
            public DateTime? ADateTime { get; set; }
            public string SomeString { get; set; }
            public decimal? LotsOfMoney { get; set; }
            public double VagueNumber { get; set; }
            public TrivialValue<string> CustomBla { get; set; }
            public TrivialValue<string>? CustomBlaThanCanBeNull { get; set; }
        }

        static readonly SampleRow[] SampleData = {
            new SampleRow {
                ADateTime = new DateTime(2003, 4, 5).AddHours(17.345),
                AnEnum = DayOfWeek.Saturday,
                LotsOfMoney = -12.34m,
                VagueNumber = 123.456,
                SomeString = "sdf",
                CustomBla = TrivialConvertibleValue.Create("aap"),
            },
            new SampleRow {
                ADateTime = new DateTime(2013, 8, 7),
                AnEnum = DayOfWeek.Monday,
                LotsOfMoney = null,
                //VagueNumer = double.NaN,
                SomeString = null,
                CustomBla = TrivialConvertibleValue.Create("aap"),
            },
            new SampleRow {
                ADateTime = null,
                AnEnum = (DayOfWeek)12345,
                LotsOfMoney = 6543,
                VagueNumber = 1 / 3.0,
                SomeString = "Hello world!",
                CustomBla = TrivialConvertibleValue.Create("aap"),
            },
            new SampleRow {
                ADateTime = DateTime.MaxValue,
                AnEnum = DayOfWeek.Friday,
                LotsOfMoney = 1000_000_000.00m,
                VagueNumber = Math.E,
                SomeString = "annual income",
                CustomBla = TrivialConvertibleValue.Create("aap"),
                CustomBlaThanCanBeNull = TrivialConvertibleValue.Create("noot"),
            }
        };

        [Fact]
        public void BulkCopysWithConcurrentQueriesCrash()
        {
            var target = CreateTable(SQL($"#test"));
            var evilEnumerable = SampleData.Where(o => SQL($"select 1").ReadScalar<int>(Connection) == 1);
            Assert.ThrowsAny<Exception>(() => evilEnumerable.BulkCopyToSqlServer(Connection, target));
        }

        [Fact]
        public void BulkInsertAndReadRoundTrips()
        {
            var target = CreateTable(SQL($"#test"));
            SampleData.BulkCopyToSqlServer(Connection, target);
            var fromDb = SQL($"select * from #test").ReadPocos<SampleRow>(Connection);
            AssertCollectionsEquivalent(SampleData, fromDb);
        }

        [Fact]
        public void CanInsertDatatable()
        {
            var target = CreateTable(SQL($"#test"));
            var target2 = CreateTable(SQL($"#test2"));
            target.BulkInsert(Connection, SampleData);

            var dataTable = SQL($"select * from #test").OfDataTable().Execute(Connection);
            target2.BulkInsert(Connection, dataTable);

            var fromDb = SQL($"select * from #test2").ReadPocos<SampleRow>(Connection);
            AssertCollectionsEquivalent(SampleData, fromDb);
        }

        sealed class SampleRow2 : ValueBase<SampleRow2>, IWrittenImplicitly, IReadImplicitly
        {
            public int intNonNull { get; set; }
            public int? intNull { get; set; }
            public string stringNull { get; set; }
            public string stringNonNull { get; set; }
        }

        [Fact]
        public void CanInsertDatareader()
        {
            using (var conn2 = new SqlConnection(ConnectionString)) {
                conn2.Open();
                var query = SQL($@"
                    select *
                    from (
                        values (1, null, 'test', 'test2')
                        , (2, 1, null, 'test3')
                    ) x(intNonNull, intNull, stringNull, stringNonNull)
                ").OfPocos<SampleRow2>();
                var expectedData = new[] {
                    new SampleRow2 { intNonNull = 1, intNull = null, stringNull = "test", stringNonNull = "test2" },
                    new SampleRow2 { intNonNull = 2, intNull = 1, stringNull = null, stringNonNull = "test3" },
                };
                AssertCollectionsEquivalent(expectedData, query.Execute(conn2)); //sanity check that we're testing consistent data

                SQL($@"
                    create table #tmp (
                        intNonNull int not null
                        , intNull int null
                        , stringNull nvarchar(max) null
                        , stringNonNull nvarchar(max) not null
                    )
                ").ExecuteNonQuery(Connection);
                var target = BulkInsertTarget.LoadFromTable(Connection, "#tmp");

                using (var cmd = query.Sql.CreateSqlCommand(conn2, default))
                using (var reader = cmd.Command.ExecuteReader()) {
                    target.BulkInsert(Connection, reader, "from query");
                }

                AssertCollectionsEquivalent(expectedData, SQL($"select * from #tmp").OfPocos<SampleRow2>().Execute(Connection));
            }
        }

        static void AssertCollectionsEquivalent<T>(T[] sampleData, T[] fromDb)
            where T : IEquatable<T>
        {
            var missingInDb = sampleData.Except(fromDb);
            var extraInDb = fromDb.Except(sampleData);
            PAssert.That(() => missingInDb.None());
            PAssert.That(() => extraInDb.None());
            PAssert.That(() => fromDb.Length == sampleData.Length);
        }

        [Fact]
        public void EmptyBulkInsertAndReadRoundTrips()
        {
            var target = CreateTable(SQL($"#test"));
            SampleData.Take(0).BulkCopyToSqlServer(Connection, target);
            var fromDb = SQL($"select * from #test").ReadPocos<SampleRow>(Connection);
            PAssert.That(() => fromDb.None());
        }

        [Fact]
        public void CanCreateDbColumnMetaData()
        {
            var pocoProperties = PocoProperties<SampleRow>.Instance;
            var dbProps = pocoProperties.Select(property => DbColumnMetaData.Create(
                property.Name,
                property.DataType,
                property.IsKey,
                null));
            PAssert.That(() => pocoProperties.Count == dbProps.Count());
        }
    }
}
