using System.Linq;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.Html;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    using static Tags;

    public class HtmlTests
    {
        [Fact]
        public void EmptyMeansEmpty()
        {
            PAssert.That(() => HtmlFragment.Empty.IsEmpty);
            PAssert.That(() => _p.AsFragment().IsEmpty == false);
        }

        [Fact]
        public void IsHtmlElementLooksAtOutermostNode()
        {
            PAssert.That(() => _div.Content("bla").AsFragment().IsHtmlElement);
            PAssert.That(() => ((HtmlFragment)"bla").IsHtmlElement == false);
            PAssert.That(() => new[] { HtmlFragment.Empty, _div }.WrapInHtmlFragment().IsHtmlElement);
            PAssert.That(() => new[] { _div, _div }.WrapInHtmlFragment().IsHtmlElement == false);
        }

        [Fact]
        public void EmptyIfNull_ReturnsEmptyOnlyForNull()
        {
            PAssert.That(() => default(HtmlFragment?).EmptyIfNull().IsEmpty);
            PAssert.That(() => ((HtmlFragment?)_div).EmptyIfNull().IsEmpty == false);
        }

        [Fact]
        public void IFrameGetsClosingTag()
        {
            var html = _iframe.SerializeToStringWithoutDoctype();
            PAssert.That(() => html == "<iframe></iframe>");
        }

        [Fact]
        public void AppendingToFragmentsWorks()
        {
            var html = _div.AsFragment().Append("test").SerializeToStringWithoutDoctype();
            PAssert.That(() => html == "<div></div>test");
        }

        [Fact]
        public void AppendingThreeThingsWorks()
        {
            var html = _div.AsFragment().Append("test", _b.Content("la")).SerializeToStringWithoutDoctype();
            PAssert.That(() => html == "<div></div>test<b>la</b>");
        }

        [Fact]
        public void PerfIterativeAppendingDoesNotCreateLotsOfArrays()
        {
            var html = "1".AsFragment().Append("2").Append("3").Append("4");
            var kids = (HtmlFragment[])html.Content;
            PAssert.That(() => kids.First().SerializeToStringWithoutDoctype() == "1");
            PAssert.That(() => kids.Last().SerializeToStringWithoutDoctype() == "4");
        }
    }
}
