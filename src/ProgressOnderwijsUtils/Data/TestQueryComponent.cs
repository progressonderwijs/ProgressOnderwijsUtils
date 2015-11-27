using System;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtils
{
    [Continuous]
    public sealed class TestQueryComponent
    {
        [Test]
        public void ValidatesArgumentsOK()
        {
            Assert.Throws<ArgumentNullException>(() => QueryBuilder.CreateDynamic(null));
            Assert.DoesNotThrow(() => QueryBuilder.CreateDynamic("bla"));

            PAssert.That(() => QueryBuilder.CreateDynamic("bla" + 0).GetHashCode() == QueryBuilder.CreateDynamic("bla0").GetHashCode());
            PAssert.That(() => QueryBuilder.CreateDynamic("bla" + 0).GetHashCode() != QueryBuilder.CreateDynamic("bla").GetHashCode());
            PAssert.That(() => QueryBuilder.CreateDynamic("bla" + 0).Equals(QueryBuilder.CreateDynamic("bla0")));

            PAssert.That(() => QueryBuilder.Param("bla" + 0).GetHashCode() == QueryBuilder.Param("bla0").GetHashCode());
            PAssert.That(() => QueryBuilder.Param("bla" + 0).GetHashCode() != QueryBuilder.Param("bla").GetHashCode());
            PAssert.That(() => QueryBuilder.Param("bla" + 0).Equals(QueryBuilder.Param("bla0")));

            var someday = new DateTime(2012, 3, 4);
            PAssert.That(() => QueryBuilder.Param(someday).DebugText() == "'2012-03-03T23:00:00.000Z'");
            PAssert.That(() => QueryBuilder.Param(null).DebugText() == "NULL");
            PAssert.That(() => QueryBuilder.Param("abc").DebugText() == "'abc'");
            PAssert.That(() => QueryBuilder.Param("ab'c").DebugText() == "'ab''c'");
            PAssert.That(() => QueryBuilder.Param(12345).DebugText() == "12345");
            PAssert.That(() => QueryBuilder.Param(12345.6m).DebugText() == "12345.6");
            PAssert.That(() => QueryBuilder.Param(new object()).DebugText() == "{!System.Object!}");
        }
    }
}
