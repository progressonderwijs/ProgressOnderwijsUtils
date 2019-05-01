using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProgressOnderwijsUtils.SchemaReflection;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests.Data
{
    public sealed class BulkInsertTest : TransactedLocalConnection
    {
        BulkInsertTarget CreateTable()
        {
            var tableName = SQL($"#test");
            SQL($@"
                create table {tableName} (
                    AnEnum int not null
                    , ADateTime datetime2
                    , SomeString nvarchar(max)
                    , LotsOfMoney decimal(19, 5)
                    , VagueNumber float not null
                    , CustomBla nvarchar(max) not null
                    , CustomBlaThanCanBeNull nvarchar(max) null
                )
            ").ExecuteNonQuery(Context);

            return BulkInsertTarget.LoadFromTable(Context, tableName.CommandText());
        }

        sealed class SampleRow : ValueBase<SampleRow>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public DayOfWeek AnEnum { get; set; }
            public DateTime? ADateTime { get; set; }
            public string SomeString { get; set; }
            public decimal? LotsOfMoney { get; set; }
            public double VagueNumber { get; set; }
            public CustomBlaStruct CustomBla { get; set; }
            public CustomBlaStruct? CustomBlaThanCanBeNull { get; set; }
        }

        public struct CustomBlaStruct : IMetaObjectPropertyConvertible<CustomBlaStruct, string, CustomBlaStruct.CustomBlaStructConverter>
        {
            public struct CustomBlaStructConverter : IConverterSource<CustomBlaStruct, string>
            {
                public ValueConverter<CustomBlaStruct, string> GetValueConverter()
                    => this.DefineConverter(v => v.CustomBlaString, v => new CustomBlaStruct(v));
            }

            CustomBlaStruct(string value)
                => CustomBlaString = value;

            public string CustomBlaString { get; }

            [MetaObjectPropertyLoader]
            public static CustomBlaStruct MethodWithIrrelevantName(string value)
                => new CustomBlaStruct(value);
        }

        static readonly SampleRow[] SampleData = {
            new SampleRow {
                ADateTime = new DateTime(2003, 4, 5).AddHours(17.345),
                AnEnum = DayOfWeek.Saturday,
                LotsOfMoney = -12.34m,
                VagueNumber = 123.456,
                SomeString = "sdf",
                CustomBla = CustomBlaStruct.MethodWithIrrelevantName("aap"),
            },
            new SampleRow {
                ADateTime = new DateTime(2013, 8, 7),
                AnEnum = DayOfWeek.Monday,
                LotsOfMoney = null,
                //VagueNumer = double.NaN,
                SomeString = null,
                CustomBla = CustomBlaStruct.MethodWithIrrelevantName("aap"),
            },
            new SampleRow {
                ADateTime = null,
                AnEnum = (DayOfWeek)12345,
                LotsOfMoney = 6543,
                VagueNumber = 1 / 3.0,
                SomeString = "Hello world!",
                CustomBla = CustomBlaStruct.MethodWithIrrelevantName("aap"),
            },
            new SampleRow {
                ADateTime = DateTime.MaxValue,
                AnEnum = DayOfWeek.Friday,
                LotsOfMoney = 1000_000_000.00m,
                VagueNumber = Math.E,
                SomeString = "annual income",
                CustomBla = CustomBlaStruct.MethodWithIrrelevantName("aap"),
                CustomBlaThanCanBeNull = CustomBlaStruct.MethodWithIrrelevantName("noot"),
            }
        };

        [Fact]
        public void BulkCopysWithConcurrentQueriesCrash()
        {
            var target = CreateTable();
            var evilEnumerable = SampleData.Where(o => SQL($"select 1").ReadScalar<int>(Context) == 1);
            Assert.ThrowsAny<Exception>(() => evilEnumerable.BulkCopyToSqlServer(Context, target));
        }

        [Fact]
        public void BulkInsertAndReadRoundTrips()
        {
            var target = CreateTable();
            SampleData.BulkCopyToSqlServer(Context, target);
            var fromDb = SQL($"select * from #test").ReadMetaObjects<SampleRow>(Context);
            var missingInDb = SampleData.Except(fromDb);
            var extraInDb = fromDb.Except(SampleData);
            PAssert.That(() => missingInDb.None());
            PAssert.That(() => extraInDb.None());
            PAssert.That(() => fromDb.Length == SampleData.Length);
        }

        [Fact]
        public void EmptyBulkInsertAndReadRoundTrips()
        {
            var target = CreateTable();
            SampleData.Take(0).BulkCopyToSqlServer(Context, target);
            var fromDb = SQL($"select * from #test").ReadMetaObjects<SampleRow>(Context);
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
