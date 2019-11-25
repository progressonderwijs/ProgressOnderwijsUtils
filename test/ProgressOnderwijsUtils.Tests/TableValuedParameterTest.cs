using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Internal;
using ProgressOnderwijsUtils.Tests.Data;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class TableValuedParameterTest : TransactedLocalConnection
    {
        public enum SomeEnum { }

        [Fact]
        public void ConvertibleProperty()
        {
            var value = SQL($@"select {TrivialConvertibleValue.Create("aap")}").ReadScalar<TrivialValue<string>>(Connection);
            PAssert.That(() => value.Value == "aap");
        }

        [Fact]
        public void ConvertibleNullablePropertyWitValue()
        {
            var value = SQL($@"select {TrivialConvertibleValue.Create("aap")}").ReadScalar<TrivialValue<string>?>(Connection);
            PAssert.That(() => value!.Value.Value == "aap");
        }

        [Fact]
        public void ConvertibleNullablePropertyWithoutValue()
        {
            var value = SQL($@"select {default(TrivialValue<string>?)}").ReadScalar<TrivialValue<string>?>(Connection);
            PAssert.That(() => value == null);
        }

        [Fact]
        public void ConvertibleNonNullablePropertyWithoutValueShouldThrow()
        {
            var nullStringReturningQuery = SQL($@"select cast(null as nvarchar(max))");

            var unused = nullStringReturningQuery.ReadScalar<string>(Connection); //assert query OK.

            Assert.ThrowsAny<Exception>(() => nullStringReturningQuery.ReadScalar<TrivialValue<string>>(Connection));
        }

        [Fact]
        public void DatabaseCanProcessTableValuedParameters()
        {
            var q = SQL($@"select sum(x.querytablevalue) from ") + ParameterizedSql.TableParamDynamic(Enumerable.Range(1, 100).ToArray()) + SQL($" x");
            var sum = q.ReadScalar<int>(Connection);
            PAssert.That(() => sum == (100 * 100 + 100) / 2);
        }

        [Fact]
        public void ParameterizedSqlCanIncludeTvps()
        {
            var q = SQL($@"select sum(x.querytablevalue) from {Enumerable.Range(1, 100)} x");
            var sum = q.ReadScalar<int>(Connection);
            PAssert.That(() => sum == (100 * 100 + 100) / 2);
        }

        [Fact]
        public void SingletonTvPsCanBeExecuted()
        {
            var q = SQL($"select sum(x.querytablevalue) from {Enumerable.Range(1, 1).ToArray()} x");
            using (var cmd = q.CreateSqlCommand(Connection, CommandTimeout.WithoutTimeout)) {
                //make sure I'm actually testing that exceptional single-value case, not the general Strucutured case.
                PAssert.That(() => cmd.Command.Parameters.Cast<SqlParameter>().Select(p => p.SqlDbType).SequenceEqual(new[] { SqlDbType.Int }));
            }
            var sum = q.ReadScalar<int>(Connection);
            PAssert.That(() => sum == 1);
        }

        [Fact]
        public void ParameterizedSqlCanIncludeEnumTvps()
        {
            var q = SQL($@"select sum(x.querytablevalue) from {Enumerable.Range(1, 100).Select(i => (SomeEnum)i)} x");
            var sum = (int)q.ReadScalar<SomeEnum>(Connection);
            PAssert.That(() => sum == (100 * 100 + 100) / 2);
        }

        [Fact]
        public void ParameterizedSqlTvpsCanCountDaysOfWeek()
        {
            var q = SQL($@"select count(x.querytablevalue) from {new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }} x");
            var dayCount = q.ReadScalar<int>(Connection);
            PAssert.That(() => dayCount == 5);
        }

        [Fact]
        public void ParameterizedSqlTvpsCanCountStrings()
        {
            var q = SQL($@"select count(distinct x.querytablevalue) from {new[] { "foo", "bar", "foo" }} x");
            var dayCount = q.ReadScalar<int>(Connection);
            PAssert.That(() => dayCount == 2);
        }

        [Fact]
        public void PocoReadersCanIncludeNull()
        {
            var stringsWithNull = new[] { "foo", "bar", null, "fizzbuzz" };
            var pocos = stringsWithNull.ArraySelect(s => new TableValuedParameterWrapper<string?> { QueryTableValue = s });

            var tableName = SQL($"#strings");
            SQL($@"create table {tableName} (querytablevalue nvarchar(max))").ExecuteNonQuery(Connection);
            //manual bulk insert because our default TVP types explicitly forbid null
            pocos.BulkCopyToSqlServer(Connection, BulkInsertTarget.LoadFromTable(Connection, tableName));

            var output = SQL($@"select x.querytablevalue from #strings x").ReadPlain<string>(Connection);
            SQL($@"drop table #strings").ExecuteNonQuery(Connection);
            PAssert.That(() => stringsWithNull.SetEqual(output));
        }

        [Fact]
        public void Binary_columns_can_be_used_in_tvps()
            => PAssert.That(() => SQL($@"
                select sum(datalength(hashes.QueryTableValue))
                from {new[] { Encoding.ASCII.GetBytes("0123456789"), Encoding.ASCII.GetBytes("abcdef") }} hashes
            ").ReadPlain<long>(Connection).Single() == 16);

        public sealed class TestDataPoco : IReadImplicitly
        {
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
            public byte[] Data { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
        }

        static readonly byte[] testData = Enumerable.Range(0, 100).Select(i => (byte)i).ToArray();

        [Fact]
        public void Test_DbDataReaderBase_GetBytes_works_the_same_as_in_SqlDataReader()
        {
            var testen = new[] { new TestDataPoco { Data = testData } };
            using (var reader = new PocoDataReader<TestDataPoco>(testen, CancellationToken.None)) {
                Assert_DataReader_GetBytes_works(reader);
            }
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
            ").ExecuteNonQuery(Connection);

            using (var cmd = SQL($@"select data from get_bytes_test").CreateSqlCommand(Connection, CommandTimeout.DeferToConnectionDefault))
            using (var reader = cmd.Command.ExecuteReader(CommandBehavior.Default)) {
                Assert_DataReader_GetBytes_works(reader);
            }
        }

        static void Assert_DataReader_GetBytes_works([NotNull] DbDataReader reader)
        {
            PAssert.That(() => reader.Read());

            var buffer = new byte[10];

            // TODO: willen we dit ook zo ondersteunen?
            //nofRead = reader.GetBytes(0, 0, null, 0, 1);
            //PAssert.That(() => nofRead == 100);

            var nofRead = reader.GetBytes(0, 0, buffer, 0, 1);
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
            var internalArray = TableValuedParameterWrapperHelper.WrapPlainValueInSinglePropertyPoco(Enumerable.Range(7, 7));
            PAssert.That(() => internalArray.Select(o => o.QueryTableValue).SequenceEqual(Enumerable.Range(7, 7)));
        }

        [Fact]
        public void WrapSupportsEnumerableOfString()
        {
            var internalArray = TableValuedParameterWrapperHelper.WrapPlainValueInSinglePropertyPoco(Enumerable.Range(7, 7).Select(n => n.ToString()));
            PAssert.That(() => internalArray.Select(o => o.QueryTableValue).SequenceEqual(Enumerable.Range(7, 7).Select(n => n.ToString())));
        }

        [Fact]
        public void WrapSupportsReadonlyListOfInt()
        {
            var internalArray = TableValuedParameterWrapperHelper.WrapPlainValueInSinglePropertyPoco(Enumerable.Range(7, 7).ToList());
            PAssert.That(() => internalArray.Select(o => o.QueryTableValue).SequenceEqual(Enumerable.Range(7, 7)));
        }

        [Fact]
        public void WrapSupportsArrayOfInt()
        {
            var internalArray = TableValuedParameterWrapperHelper.WrapPlainValueInSinglePropertyPoco(Enumerable.Range(7, 7).ToArray());
            PAssert.That(() => internalArray.Select(o => o.QueryTableValue).SequenceEqual(Enumerable.Range(7, 7)));
        }
    }
}
