using System;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    public sealed class FilterLanguage
    {
        [Test]
        public void Can_parse_column_equals_column_criterium()
        {
            var result = Filter.ParseFilterLanguage("a=b");
            PAssert.That(() => result.Equals(Filter.CreateCriterium("a", BooleanComparer.Equal, new ColumnReference("b"))));
        }
    }
}
