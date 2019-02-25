using System;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class TestSqlParameterComponent
    {
        [Fact]
        public void ValidatesArgumentsOK()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => ParameterizedSql.CreateDynamic(null));

            PAssert.That(() => ParameterizedSql.CreateDynamic("bla" + 0).GetHashCode() == ParameterizedSql.CreateDynamic("bla0").GetHashCode());
            PAssert.That(() => ParameterizedSql.CreateDynamic("bla" + 0).GetHashCode() != ParameterizedSql.CreateDynamic("bla").GetHashCode());
            PAssert.That(() => ParameterizedSql.CreateDynamic("bla" + 0).Equals(ParameterizedSql.CreateDynamic("bla0")));

            PAssert.That(() => ParameterizedSql.Param("bla" + 0).GetHashCode() == ParameterizedSql.Param("bla0").GetHashCode());
            PAssert.That(() => ParameterizedSql.Param("bla" + 0).GetHashCode() != ParameterizedSql.Param("bla").GetHashCode());
            PAssert.That(() => ParameterizedSql.Param("bla" + 0).Equals(ParameterizedSql.Param("bla0")));

            var someday = new DateTime(2012, 3, 3, 23, 0, 0, DateTimeKind.Utc);
            PAssert.That(() => ParameterizedSql.Param(someday).DebugText() == "'2012-03-03T23:00:00.000Z'");
            PAssert.That(() => ParameterizedSql.Param(null).DebugText() == "NULL");
            PAssert.That(() => ParameterizedSql.Param("abc").DebugText() == "'abc'");
            PAssert.That(() => ParameterizedSql.Param("ab'c").DebugText() == "'ab''c'");
            PAssert.That(() => ParameterizedSql.Param(12345).DebugText() == "12345");
            PAssert.That(() => ParameterizedSql.Param(12345.6m).DebugText() == "12345.6");
            PAssert.That(() => ParameterizedSql.Param(new object()).DebugText() == "{!System.Object!}");
        }
    }
}
