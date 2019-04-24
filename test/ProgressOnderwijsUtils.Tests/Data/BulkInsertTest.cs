using System;
using System.Linq;
using System.Runtime.Serialization;
using ExpressionToCodeLib;
using JetBrains.Annotations;
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
                    , CustomBla nvarchar(max)
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
        }

        [Serializable]
        [MetaObjectPropertyConvertible]
        public struct CustomBlaStruct : IConvertible
        {
            CustomBlaStruct(string value)
            {
                CustomBlaString = value;
            }

            public string CustomBlaString { get; }

            [MetaObjectPropertyLoader]
            public static CustomBlaStruct MethodWithIrrelevantName(string value)
                => new CustomBlaStruct(value);

            public TypeCode GetTypeCode()
                => TypeCode.String;

            public bool ToBoolean(IFormatProvider provider)
                => throw new NotImplementedException("ToBoolean");

            public byte ToByte(IFormatProvider provider)
                => throw new NotImplementedException("ToByte");

            public char ToChar(IFormatProvider provider)
                => throw new NotImplementedException("ToChar");

            public DateTime ToDateTime(IFormatProvider provider)
                => throw new NotImplementedException("ToDateTime");

            public decimal ToDecimal(IFormatProvider provider)
                => throw new NotImplementedException("ToDecimal");

            public double ToDouble(IFormatProvider provider)
                => throw new NotImplementedException("ToDouble");

            public short ToInt16(IFormatProvider provider)
                => throw new NotImplementedException("ToInt16");

            public int ToInt32(IFormatProvider provider)
                => throw new NotImplementedException("ToInt32");

            public long ToInt64(IFormatProvider provider)
                => throw new NotImplementedException("ToInt64");

            public sbyte ToSByte(IFormatProvider provider)
                => throw new NotImplementedException("ToSByte");

            public float ToSingle(IFormatProvider provider)
                => throw new NotImplementedException("ToSingle");

            public string ToString(IFormatProvider provider)
                => CustomBlaString;

            public object ToType(Type conversionType, IFormatProvider provider)
                => throw new NotImplementedException();

            public ushort ToUInt16(IFormatProvider provider)
                => throw new NotImplementedException();

            public uint ToUInt32(IFormatProvider provider)
                => throw new NotImplementedException();

            public ulong ToUInt64(IFormatProvider provider)
                => throw new NotImplementedException();
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
    }
}
