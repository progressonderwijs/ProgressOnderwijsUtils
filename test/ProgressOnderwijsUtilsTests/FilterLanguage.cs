using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business.DomainUnits;
using Progress.Business.Filters;
using Progress.Business.Test;

namespace ProgressOnderwijsUtilsTests
{
    [PullRequestTest]
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
