#nullable disable
using System;
using System.Data.SqlClient;
using System.Linq;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests.Data
{
    public sealed class EnumeratePocosTest : TransactedLocalConnection
    {
        static ParameterizedSql ExampleQuery
            => SQL($@"
                select content='bla', id= 3
                union all
                select content='hmm', id= 37
                union all
                select content=null, id= 1337
                ");

        public sealed class ExampleRow : ValueBase<ExampleRow>, IWrittenImplicitly
        {
            public int Id { get; set; }
            public string Content { get; set; }
            public static int HackyHackyCounter;

            public ExampleRow()
                => HackyHackyCounter++;
        }

        public EnumeratePocosTest()
        {
            ExampleRow.HackyHackyCounter = 0;
        }

        [Fact]
        public void Executing_ToLazilyEnumeratedCommand_creates_no_row_objects()
        {
            // ReSharper disable once UnusedVariable
            var unused = ExampleQuery.OfPocos<ExampleRow>().ToLazilyEnumeratedCommand().Execute(Connection);
            Assert.Equal(0, ExampleRow.HackyHackyCounter);
        }

        [Fact]
        public void Enumerating_ToLazilyEnumeratedCommand_creates_one_row_object_per_row()
        {
            var enumerable = ExampleQuery.OfPocos<ExampleRow>().ToLazilyEnumeratedCommand().Execute(Connection);
            var array = enumerable.ToArray();
            Assert.Equal(3, ExampleRow.HackyHackyCounter);
            Assert.Equal(3, array.Length);
        }

        [Fact]
        public void Stopping_early_creates_fewer_objects()
        {
            var enumerable = ExampleQuery.OfPocos<ExampleRow>().ToLazilyEnumeratedCommand().Execute(Connection);
            // ReSharper disable once UnusedVariable
            var value = enumerable.Skip(1).First();
            Assert.Equal(2, ExampleRow.HackyHackyCounter);
        }

        [Fact]
        public void Sets_row_object_properties()
        {
            var enumerable = ExampleQuery.OfPocos<ExampleRow>().ToLazilyEnumeratedCommand().Execute(Connection);
            var value = enumerable.Skip(1).First();
            Assert.Equal(new ExampleRow { Id = 37, Content = "hmm" }, value);
        }

        [Fact]
        public void ConcurrentReadersCrash()
        {
            var enumerable = ExampleQuery.OfPocos<ExampleRow>().ToLazilyEnumeratedCommand().Execute(Connection);
            using (var enumerator = enumerable.GetEnumerator())
            using (var enumerator2 = enumerable.GetEnumerator()) {
                enumerator.MoveNext();
                Assert.ThrowsAny<Exception>(() => enumerator2.MoveNext());
            }
        }
    }
}
