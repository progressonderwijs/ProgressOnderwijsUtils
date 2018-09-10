using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Internal;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests
{
    public class TableValuedParameterTest : TransactedLocalConnection
    {
        public enum SomeEnum
        { }

        [Fact]
        public void DatabaseCanProcessTableValuedParameters()
        {
            var q = SQL($@"select sum(x.querytablevalue) from ") + ParameterizedSql.TableParamDynamic(Enumerable.Range(1, 100).ToArray()) + SQL($" x");
            var sum = q.ReadScalar<int>(Context);
            PAssert.That(() => sum == (100 * 100 + 100) / 2);
        }

        [Fact]
        public void ParameterizedSqlCanIncludeTvps()
        {
            var q = SQL($@"select sum(x.querytablevalue) from {Enumerable.Range(1, 100)} x");
            var sum = q.ReadScalar<int>(Context);
            PAssert.That(() => sum == (100 * 100 + 100) / 2);
        }

        [Fact]
        public void ParameterizedSqlCanIncludeEnumTvps()
        {
            var q = SQL($@"select sum(x.querytablevalue) from {Enumerable.Range(1, 100).Select(i => (SomeEnum)i)} x");
            var sum = (int)q.ReadScalar<SomeEnum>(Context);
            PAssert.That(() => sum == (100 * 100 + 100) / 2);
        }

        [Fact]
        public void ParameterizedSqlTvpsCanCountDaysOfWeek()
        {
            var q = SQL($@"select count(x.querytablevalue) from {new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }} x");
            var dayCount = q.ReadScalar<int>(Context);
            PAssert.That(() => dayCount == 5);
        }

        [Fact]
        public void ParameterizedSqlTvpsCanCountStrings()
        {
            var q = SQL($@"select count(distinct x.querytablevalue) from {new[] { "foo", "bar", "foo" }} x");
            var dayCount = q.ReadScalar<int>(Context);
            PAssert.That(() => dayCount == 2);
        }

        [Fact]
        public void MetaObjectReadersCanIncludeNull()
        {
            var stringsWithNull = new[] { "foo", "bar", null, "fizzbuzz" };
            var metaObjects = stringsWithNull.ArraySelect(s => new TableValuedParameterWrapper<string> { QueryTableValue = s });

            SQL($@"create table #strings (querytablevalue nvarchar(max))").ExecuteNonQuery(Context);
            //manual bulk insert because our default TVP types explicitly forbid null
            metaObjects.BulkCopyToSqlServer(Context.Connection, "#strings");

            var output = SQL($@"select x.querytablevalue from #strings x").ReadPlain<string>(Context);
            SQL($@"drop table #strings").ExecuteNonQuery(Context);
            PAssert.That(() => stringsWithNull.SetEqual(output));
        }

        [Fact]
        public void Binary_columns_can_be_used_in_tvps()
        {
            PAssert.That(() => SQL($@"
                select sum(datalength(hashes.QueryTableValue))
                from {new[] { Encoding.ASCII.GetBytes("0123456789"), Encoding.ASCII.GetBytes("abcdef") }} hashes
            ").ReadPlain<long>(Context).Single() == 16);
        }

        public sealed class TestDataMetaObject : IMetaObject
        {
            public byte[] Data { get; set; }
        }

        static readonly byte[] testData = Enumerable.Range(0, 100).Select(i => (byte)i).ToArray();

        [Fact]
        public void Test_DbDataReaderBase_GetBytes_works_the_same_as_in_SqlDataReader()
        {
            var testen = new[] { new TestDataMetaObject { Data = testData } };
            var reader = new MetaObjectDataReader<TestDataMetaObject>(testen, CancellationToken.None);
            Assert_DataReader_GetBytes_works(reader);
        }

        [Fact]
        public void Test_SqlDataReader_GetBytes_for_its_spec()
        {
            SQL($@"
                create table get_bytes_test
                (
                    data varbinary(max) not null
                );

                insert into get_bytes_test
                values ({testData});
            ").ExecuteNonQuery(Context);

            using (var cmd = SQL($@"select data from get_bytes_test").CreateSqlCommand(Context))
            using (var reader = cmd.Command.ExecuteReader(CommandBehavior.Default))
                Assert_DataReader_GetBytes_works(reader);
        }

        static void Assert_DataReader_GetBytes_works([NotNull] DbDataReader reader)
        {
            PAssert.That(() => reader.Read());

            long nofRead;
            var buffer = new byte[10];

            // TODO: willen we dit ook zo ondersteunen?
            //nofRead = reader.GetBytes(0, 0, null, 0, 1);
            //PAssert.That(() => nofRead == 100);

            nofRead = reader.GetBytes(0, 0, buffer, 0, 1);
            PAssert.That(() => buffer[0] == 0);
            PAssert.That(() => nofRead == 1);

            nofRead = reader.GetBytes(0, 20, buffer, 0, 1);
            PAssert.That(() => buffer[0] == 20);
            PAssert.That(() => nofRead == 1);

            nofRead = reader.GetBytes(0, 30, buffer, 0, 2);
            PAssert.That(() => buffer[1] == 31);
            PAssert.That(() => nofRead == 2);

            nofRead = reader.GetBytes(0, 80, buffer, 5, 2);
            PAssert.That(() => buffer[6] == 81);
            PAssert.That(() => nofRead == 2);

            nofRead = reader.GetBytes(0, 99, buffer, 0, 10);
            PAssert.That(() => buffer[0] == 99);
            PAssert.That(() => buffer[1] == 31);
            PAssert.That(() => nofRead == 1);

            nofRead = reader.GetBytes(0, 100, buffer, 0, 1);
            PAssert.That(() => nofRead == 0);

            nofRead = reader.GetBytes(0, 110, buffer, 0, 1);
            PAssert.That(() => nofRead == 0);

            PAssert.That(() => !reader.Read());
        }

        [Fact]
        public void WrapSupportsEnumerableOfInt()
        {
            var internalArray = TableValuedParameterWrapperHelper.WrapPlainValueInMetaObject(Enumerable.Range(7, 7));
            PAssert.That(() => internalArray.Select(o => o.QueryTableValue).SequenceEqual(Enumerable.Range(7, 7)));
        }

        [Fact]
        public void WrapSupportsEnumerableOfString()
        {
            var internalArray = TableValuedParameterWrapperHelper.WrapPlainValueInMetaObject(Enumerable.Range(7, 7).Select(n => n.ToString()));
            PAssert.That(() => internalArray.Select(o => o.QueryTableValue).SequenceEqual(Enumerable.Range(7, 7).Select(n => n.ToString())));
        }

        [Fact]
        public void WrapSupportsReadonlyListOfInt()
        {
            var internalArray = TableValuedParameterWrapperHelper.WrapPlainValueInMetaObject(Enumerable.Range(7, 7).ToList());
            PAssert.That(() => internalArray.Select(o => o.QueryTableValue).SequenceEqual(Enumerable.Range(7, 7)));
        }

        [Fact]
        public void WrapSupportsArrayOfInt()
        {
            var internalArray = TableValuedParameterWrapperHelper.WrapPlainValueInMetaObject(Enumerable.Range(7, 7).ToArray());
            PAssert.That(() => internalArray.Select(o => o.QueryTableValue).SequenceEqual(Enumerable.Range(7, 7)));
        }
    }
}
