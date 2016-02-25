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
            PAssert.That(() => Equals(result, Filter.CreateCriterium("a", BooleanComparer.Equal, new ColumnReference("b"))));
        }

        [Test]
        public void Can_parse_column_is_not_null_criterium()
        {
            var result = Filter.ParseFilterLanguage("a is not null");
            PAssert.That(() => Equals(result, Filter.CreateCriterium("a", BooleanComparer.IsNotNull, null)));
        }
    }
}
