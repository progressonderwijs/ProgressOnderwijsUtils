﻿using System;
using System.Linq;
using ExpressionToCodeLib;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests.Data
{
    public class BulkInsertTest : TransactedLocalConnection
    {
        sealed class SampleRow : ValueBase<SampleRow>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public DayOfWeek AnEnum { get; set; }
            public DateTime? ADateTime { get; set; }
            public string SomeString { get; set; }
            public decimal? LotsOfMoney { get; set; }
            public double VagueNumber { get; set; }
        }

        static readonly SampleRow[] SampleData = new[] {
            new SampleRow {
                ADateTime = new DateTime(2003, 4, 5).AddHours(17.345),
                AnEnum = DayOfWeek.Saturday,
                LotsOfMoney = -12.34m,
                VagueNumber = 123.456,
                SomeString = "sdf",
            },
            new SampleRow {
                ADateTime = new DateTime(2013, 8, 7),
                AnEnum = DayOfWeek.Monday,
                LotsOfMoney = null,
                //VagueNumer = double.NaN,
                SomeString = null,
            },
            new SampleRow {
                ADateTime = null,
                AnEnum = (DayOfWeek)12345,
                LotsOfMoney = 6543,
                VagueNumber = 1 / 3.0,
                SomeString = "Hello world!",
            },
            new SampleRow {
                ADateTime = DateTime.MaxValue,
                AnEnum = DayOfWeek.Friday,
                LotsOfMoney = 1000_000_000.00m,
                VagueNumber = Math.E,
                SomeString = "annual income",
            },
        };

        void CreateTable()
        {
            SQL($@"create table #test (
                AnEnum int not null,
                ADateTime datetime2,
                SomeString nvarchar(max),
                LotsOfMoney decimal(19,5),
                VagueNumber float not null,
            )").ExecuteNonQuery(Context);
        }

        [Fact]
        public void BulkInsertAndReadRoundTrips()
        {
            CreateTable();
            SampleData.BulkCopyToSqlServer(Context, "#test");
            var fromDb = SQL($"select * from #test").ReadMetaObjects<SampleRow>(Context);
            var missingInDb = SampleData.Except(fromDb);
            var extraInDb = fromDb.Except(SampleData);
            PAssert.That(() => missingInDb.None());
            PAssert.That(() => extraInDb.None());
            PAssert.That(() => fromDb.Length == SampleData.Length);
        }

        [Fact]
        public void BulkCopysWithConcurrentQueriesCrash()
        {
            CreateTable();
            var evilEnumerable = SampleData.Where(o => SQL($"select 1").ReadScalar<int>(Context) == 1);
            Assert.ThrowsAny<Exception>(() => evilEnumerable.BulkCopyToSqlServer(Context, "#test"));
        }
    }
}
