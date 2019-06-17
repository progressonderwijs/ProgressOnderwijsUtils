using System;
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

        sealed class SampleRow : ValueBase<SampleRow>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public DayOfWeek AnEnum { get; set; }
            public DateTime? ADateTime { get; set; }
            public string SomeString { get; set; }
            public decimal? LotsOfMoney { get; set; }
            public double VagueNumber { get; set; }
            public TrivialConvertibleValue<string> CustomBla { get; set; }
            public TrivialConvertibleValue<string>? CustomBlaThanCanBeNull { get; set; }
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
            var fromDb = SQL($"select * from #test").ReadMetaObjects<SampleRow>(Connection);
            AssertCollectionIsEquivalentToSampleData(fromDb);
        }

        static void AssertCollectionIsEquivalentToSampleData(SampleRow[] fromDb)
        {
            var missingInDb = SampleData.Except(fromDb);
            var extraInDb = fromDb.Except(SampleData);
            PAssert.That(() => missingInDb.None());
            PAssert.That(() => extraInDb.None());
            PAssert.That(() => fromDb.Length == SampleData.Length);
        }

        [Fact]
        public void EmptyBulkInsertAndReadRoundTrips()
        {
            var target = CreateTable(SQL($"#test"));
            SampleData.Take(0).BulkCopyToSqlServer(Connection, target);
            var fromDb = SQL($"select * from #test").ReadMetaObjects<SampleRow>(Connection);
            PAssert.That(() => fromDb.None());
        }

        [Fact]
        public void CanCreateDbColumnMetaData()
        {
            var metaProps = MetaInfo<SampleRow>.Instance;
            var dbProps = metaProps.Select(property => DbColumnMetaData.Create(
                property.Name,
                property.DataType,
                property.IsKey,
                null));
            PAssert.That(() => metaProps.Count == dbProps.Count());
        }
    }
}
