﻿using System.Linq;
using ProgressOnderwijsUtils;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtilsTests
{
    public class EnumerateMetaObjectsTest : TestsWithLocalConnection
    {
        static ParameterizedSql ExampleQuery => SQL($@"
                select content='bla', id= 3
                union all
                select content='hmm', id= 37
                union all
                select content=null, id= 1337
                ");

        public sealed class ExampleRow : ValueBase<ExampleRow>, IMetaObject
        {
            public int Id { get; set; }
            public string Content { get; set; }
            public static int HackyHackyCounter;
            public ExampleRow() => HackyHackyCounter++;
        }

        public EnumerateMetaObjectsTest()
        {
            ExampleRow.HackyHackyCounter = 0;
        }

        [Fact]
        public void Calling_EnumerateMetaObjects_create_no_row_objects()
        {
            // ReSharper disable once UnusedVariable
            var enumerable = ExampleQuery.EnumerateMetaObjects<ExampleRow>(conn);
            Assert.Equal(0, ExampleRow.HackyHackyCounter);
        }

        [Fact]
        public void Enumerating_EnumerateMetaObjects_creates_one_row_object_per_row()
        {
            var enumerable = ExampleQuery.EnumerateMetaObjects<ExampleRow>(conn);
            var array = enumerable.ToArray();
            Assert.Equal(3, ExampleRow.HackyHackyCounter);
            Assert.Equal(3, array.Length);
        }

        [Fact]
        public void Stopping_early_creates_fewer_objects()
        {
            var enumerable = ExampleQuery.EnumerateMetaObjects<ExampleRow>(conn);
            // ReSharper disable once UnusedVariable
            var value = enumerable.Skip(1).First();
            Assert.Equal(2, ExampleRow.HackyHackyCounter);
        }

        [Fact]
        public void Sets_row_object_properties()
        {
            var enumerable = ExampleQuery.EnumerateMetaObjects<ExampleRow>(conn);
            var value = enumerable.Skip(1).First();
            Assert.Equal(new ExampleRow { Id = 37, Content = "hmm" }, value);
        }
    }
}
