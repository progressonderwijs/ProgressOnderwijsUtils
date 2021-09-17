using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.SchemaReflection;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests.Data
{
    public sealed record BulkInsertTestSampleRow : IWrittenImplicitly, IReadImplicitly
    {
        public DayOfWeek AnEnum { get; set; }
        public DateTime? ADateTime { get; set; }
        public string? SomeString { get; set; }
        public decimal? LotsOfMoney { get; set; }
        public double VagueNumber { get; set; }
        public TrivialValue<string> CustomBla { get; set; }
        public TrivialValue<string>? CustomBlaThanCanBeNull { get; set; }

        public static BulkInsertTarget CreateTable(SqlConnection sqlConnection, ParameterizedSql tempTableName)
        {
            SQL(
                $@"
                create table {tempTableName} (
                    AnEnum int not null
                    , ADateTime datetime2
                    , SomeString nvarchar(max)
                    , LotsOfMoney decimal(19, 5)
                    , VagueNumber float not null
                    , CustomBla nvarchar(max) not null
                    , CustomBlaThanCanBeNull nvarchar(max) null
                )
            "
            ).ExecuteNonQuery(sqlConnection);

            return BulkInsertTarget.LoadFromTable(sqlConnection, tempTableName.CommandText());
        }

        static readonly BulkInsertTestSampleRow[] FourSampleRows = {
            new BulkInsertTestSampleRow {
                ADateTime = new DateTime(2003, 4, 5).AddHours(17.345),
                AnEnum = DayOfWeek.Saturday,
                LotsOfMoney = -12.34m,
                VagueNumber = 123.456,
                SomeString = "sdf",
                CustomBla = TrivialConvertibleValue.Create("aap"),
            },
            new BulkInsertTestSampleRow {
                ADateTime = new DateTime(2013, 8, 7),
                AnEnum = DayOfWeek.Monday,
                LotsOfMoney = null,
                //VagueNumer = double.NaN,
                SomeString = null,
                CustomBla = TrivialConvertibleValue.Create("aap"),
            },
            new BulkInsertTestSampleRow {
                ADateTime = null,
                AnEnum = (DayOfWeek)12345,
                LotsOfMoney = 6543,
                VagueNumber = 1 / 3.0,
                SomeString = "Hello world!",
                CustomBla = TrivialConvertibleValue.Create("aap"),
            },
            new BulkInsertTestSampleRow {
                ADateTime = DateTime.MaxValue,
                AnEnum = DayOfWeek.Friday,
                LotsOfMoney = 1000_000_000.00m,
                VagueNumber = Math.E,
                SomeString = "annual income",
                CustomBla = TrivialConvertibleValue.Create("aap"),
                CustomBlaThanCanBeNull = TrivialConvertibleValue.Create("noot"),
            }
        };

        public static BulkInsertTestSampleRow[] SampleRows(int n)
            => Enumerable.Range(0, (n + 3) / 4).SelectMany(_ => FourSampleRows).ToArray();
    }

    public sealed class BulkInsertTest : TransactedLocalConnection
    {
        [Fact]
        public void BulkCopysWithConcurrentQueriesCrash()
        {
            var target = BulkInsertTestSampleRow.CreateTable(Connection, SQL($"#test"));
            var evilEnumerable = BulkInsertTestSampleRow.SampleRows(16).Where(_ => SQL($"select 1").ReadScalar<int>(Connection) == 1);
            Assert.ThrowsAny<Exception>(() => evilEnumerable.BulkCopyToSqlServer(Connection, target));
        }

        [Fact]
        public void BulkInsertAndReadRoundTrips()
        {
            var target = BulkInsertTestSampleRow.CreateTable(Connection, SQL($"#test"));
            BulkInsertTestSampleRow.SampleRows(4).BulkCopyToSqlServer(Connection, target);
            var fromDb = SQL($"select * from #test").ReadPocos<BulkInsertTestSampleRow>(Connection);
            AssertCollectionsEquivalent(BulkInsertTestSampleRow.SampleRows(4), fromDb);
        }

        [Fact]
        public void BulkInsertAndReadRoundTrips_ManyRows()
        {
            var manyRows = BulkInsertTestSampleRow.SampleRows(400);
            for (var index = 0; index < manyRows.Length; index++) {
                manyRows[index].VagueNumber = index / 16.0; //make sure all rows are distinct for this test.
            }
            var target = BulkInsertTestSampleRow.CreateTable(Connection, SQL($"#test"));
            manyRows.BulkCopyToSqlServer(Connection, target);
            var fromDb = SQL($"select * from #test").ReadPocos<BulkInsertTestSampleRow>(Connection);
            AssertCollectionsEquivalent(manyRows, fromDb);
            var suspciousObjectsThatRoundTrippedFromDbAndAreReferenceEqualsToSource = manyRows.Intersect(fromDb, new ReferenceEqualityComparer<BulkInsertTestSampleRow>()).ToArray();
            PAssert.That(() => suspciousObjectsThatRoundTrippedFromDbAndAreReferenceEqualsToSource.None(), "just to make sure bulk insert actually isn't somehow staying in memory");
        }

        [Fact]
        public void CanInsertDatatable()
        {
            var target = BulkInsertTestSampleRow.CreateTable(Connection, SQL($"#test"));
            var target2 = BulkInsertTestSampleRow.CreateTable(Connection, SQL($"#test2"));
            target.BulkInsert(Connection, BulkInsertTestSampleRow.SampleRows(4));

            var dataTable = SQL($"select * from #test").OfDataTable().Execute(Connection);
            target2.BulkInsert(Connection, dataTable);

            var fromDb = SQL($"select * from #test2").ReadPocos<BulkInsertTestSampleRow>(Connection);
            AssertCollectionsEquivalent(BulkInsertTestSampleRow.SampleRows(4), fromDb);
        }

        sealed record SampleRow2 : IWrittenImplicitly, IReadImplicitly
        {
            public int intNonNull { get; set; }
            public int? intNull { get; set; }
            public string? stringNull { get; set; }
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
            public string stringNonNull { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
        }

        [Fact]
        public void CanInsertDatareader()
        {
            using var conn2 = new SqlConnection(ConnectionString);
            conn2.Open();
            var query = SQL(
                $@"
                    select *
                    from (
                        values (1, null, 'test', 'test2')
                        , (2, 1, null, 'test3')
                    ) x(intNonNull, intNull, stringNull, stringNonNull)
                "
            ).OfPocos<SampleRow2>();
            var expectedData = new[] {
                new SampleRow2 { intNonNull = 1, intNull = null, stringNull = "test", stringNonNull = "test2" },
                new SampleRow2 { intNonNull = 2, intNull = 1, stringNull = null, stringNonNull = "test3" },
            };
            AssertCollectionsEquivalent(expectedData, query.Execute(conn2)); //sanity check that we're testing consistent data

            SQL(
                $@"
                    create table #tmp (
                        intNonNull int not null
                        , intNull int null
                        , stringNull nvarchar(max) null
                        , stringNonNull nvarchar(max) not null
                    )
                "
            ).ExecuteNonQuery(Connection);
            var target = BulkInsertTarget.LoadFromTable(Connection, "#tmp");

            using (var cmd = query.Sql.CreateSqlCommand(conn2, new()))
            using (var reader = cmd.Command.ExecuteReader()) {
                target.BulkInsert(Connection, reader, "from query");
            }

            AssertCollectionsEquivalent(expectedData, SQL($"select * from #tmp").OfPocos<SampleRow2>().Execute(Connection));
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
        public void BulkInsertSmallBatchesRespectsKeepNul()
        {
            SQL(
                $@"
                    create table #tmp (
                        intNonNull int not null
                        , intNull int null default 37 
                        , stringNull nvarchar(max) null
                        , stringNonNull nvarchar(max) not null
                    )
                "
            ).ExecuteNonQuery(Connection);
            var target = BulkInsertTarget.LoadFromTable(Connection, "#tmp");
            new[] { new SampleRow2 { intNonNull = 1, intNull = null, stringNull = "test", stringNonNull = "test" }, }
                .BulkCopyToSqlServer(Connection, target);
            new[] { new SampleRow2 { intNonNull = 2, intNull = null, stringNull = "test", stringNonNull = "test" }, }
                .BulkCopyToSqlServer(Connection, target.With(target.Options ^ SqlBulkCopyOptions.KeepNulls));

            var fromDb = SQL($"select * from #tmp").ReadPocos<SampleRow2>(Connection);

            var expected =
                new[] {
                    new SampleRow2 { intNonNull = 1, intNull = null, stringNull = "test", stringNonNull = "test" },
                    new SampleRow2 { intNonNull = 2, intNull = 37, stringNull = "test", stringNonNull = "test" },
                };

            AssertCollectionsEquivalent(expected, fromDb);
        }

        [Fact]
        public void SmallBatchInsertImplementationPrefixSanityCheck()
        {
            CheckPostConditions(Enumerable.Range(1, 4), 2);
            CheckPostConditions(Enumerable.Range(1, 4), 5);
            CheckPostConditions(Enumerable.Range(1, 4).ToArray(), 5); //there's a special case for collections such as IReadOnlyList
            CheckPostConditions(Enumerable.Range(1, 4).ToArray(), 2);

            void CheckPostConditions(IEnumerable<int> enumerable, int count)
            {
                var output = SmallBatchInsertImplementation.PeekAtPrefix(enumerable, count);
                PAssert.That(() => output.head.SequenceEqual(enumerable.Take(count)));
                PAssert.That(() => output.fullSequence.SequenceEqual(enumerable));
            }
        }

        [Fact]
        public void EmptyBulkInsertAndReadRoundTrips()
        {
            var target = BulkInsertTestSampleRow.CreateTable(Connection, SQL($"#test"));
            BulkInsertTestSampleRow.SampleRows(4).Take(0).BulkCopyToSqlServer(Connection, target);
            var fromDb = SQL($"select * from #test").ReadPocos<BulkInsertTestSampleRow>(Connection);
            PAssert.That(() => fromDb.None());
        }

        [Fact]
        public void CanCreateDbColumnMetaData()
        {
            var pocoProperties = PocoProperties<BulkInsertTestSampleRow>.Instance;
            var dbProps = pocoProperties.Select(
                property => DbColumnMetaData.Create(
                    property.Name,
                    property.DataType,
                    property.IsKey,
                    null
                )
            );
            PAssert.That(() => pocoProperties.Count == dbProps.Count());
        }
    }
}
