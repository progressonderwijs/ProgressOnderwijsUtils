using System.Linq;
using System.Collections.Generic;
using System;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils.Test;
using MoreLinq;

namespace ProgressOnderwijsUtils
{
    [Continuous]
    public sealed class TestQueryComponent
    {
        [Test]
        public void ValidatesArgumentsOK()
        {
            Assert.Throws<ArgumentNullException>(() => QueryComponent.CreateString(null));
            Assert.DoesNotThrow(() => QueryComponent.CreateString("bla"));

            PAssert.That(() => QueryComponent.CreateString("bla" + 0).GetHashCode() == QueryComponent.CreateString("bla0").GetHashCode());
            PAssert.That(() => QueryComponent.CreateString("bla" + 0).GetHashCode() != QueryComponent.CreateString("bla").GetHashCode());
            PAssert.That(() => QueryComponent.CreateString("bla" + 0).Equals(QueryComponent.CreateString("bla0")));

            PAssert.That(() => QueryComponent.CreateParam("bla" + 0).GetHashCode() == QueryComponent.CreateParam("bla0").GetHashCode());
            PAssert.That(() => QueryComponent.CreateParam("bla" + 0).GetHashCode() != QueryComponent.CreateParam("bla").GetHashCode());
            PAssert.That(() => QueryComponent.CreateParam("bla" + 0).Equals(QueryComponent.CreateParam("bla0")));

            var someday = new DateTime(2012, 3, 4);
            PAssert.That(() => QueryComponent.CreateParam(someday).ToDebugText(null) == "'2012-03-04 00:00:00.0000000'");
            PAssert.That(() => QueryComponent.CreateParam(null).ToDebugText(null) == "null");
            PAssert.That(() => QueryComponent.CreateParam("abc").ToDebugText(null) == "'abc'");
            PAssert.That(() => QueryComponent.CreateParam("ab'c").ToDebugText(null) == "'ab''c'");
            PAssert.That(() => QueryComponent.CreateParam(12345).ToDebugText(null) == "12345");
            PAssert.That(() => QueryComponent.CreateParam(12345.6m).ToDebugText(null) == "12345.6");
            PAssert.That(() => QueryComponent.CreateParam(12345.6m).ToDebugText(Taal.NL) == "12345.6");
            PAssert.That(() => QueryComponent.CreateParam(12345.6m).ToDebugText(Taal.EN) == "12345.6"); //ToString niet taal afhankelijk
            PAssert.That(() => QueryComponent.CreateParam(new object()).ToDebugText(null) == "{!System.Object!}");
            Assert.Throws<ConverteerException>(() => QueryComponent.CreateParam(new object()).ToDebugText(Taal.NL));
        }
    }
}
