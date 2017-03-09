using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.Html;
using Xunit;

namespace ProgressOnderwijsUtilsTests
{
    using static Tags;

    public class HtmlTests
    {
        [Fact]
        public void EmptyMeansEmpty()
        {
            PAssert.That(() => HtmlFragment.Empty.IsEmpty);
            PAssert.That(() => !_p.AsFragment().IsEmpty);
        }

        [Fact]
        public void IsHtmlElementLooksAtOutermostNode()
        {
            PAssert.That(() => _div.Content("bla").AsFragment().IsHtmlElement);
            PAssert.That(() => !((HtmlFragment)"bla").IsHtmlElement);
            PAssert.That(() => !new[] { HtmlFragment.Empty, _div }.WrapInHtmlFragment().IsHtmlElement);
        }

        [Fact]
        public void EmptyIfNull_ReturnsEmptyOnlyForNull()
        {
            PAssert.That(() => default(HtmlFragment?).EmptyIfNull().IsEmpty);
            PAssert.That(() => !((HtmlFragment?)_div).EmptyIfNull().IsEmpty);
        }
    }
}
