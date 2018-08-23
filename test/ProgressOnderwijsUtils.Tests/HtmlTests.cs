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
            PAssert.That(() => _div.Content("bla").AsFragment().IsElement());
            PAssert.That(() => ((HtmlFragment)"bla").IsElement() == false);
            PAssert.That(() => new[] { HtmlFragment.Empty, _div }.WrapInHtmlFragment().IsElement());
            PAssert.That(() => new[] { _div, _div }.WrapInHtmlFragment().IsElement() == false);
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
            var kids = (HtmlFragment[])html.Implementation;
            PAssert.That(() => kids.First().SerializeToStringWithoutDoctype() == "1");
            PAssert.That(() => kids.Last().SerializeToStringWithoutDoctype() == "4");
        }

        [Fact]
        public void AllInterestingPlusOperatorOrderingsCompileAndReturnHtmlFragment()
        {
            var a = _b + _p;
            var b = "asd" + _q;
            var c = _i + "asd";
            var d = _a + new CustomHtmlElement("bla");
            var e = new CustomHtmlElement("bla") + _s;
            var f = new CustomHtmlElement("bla") + new CustomHtmlElement("bla");
            var g = "test" + new CustomHtmlElement("bla");
            var h = new CustomHtmlElement("bla") + "test";
            var i = HtmlFragment.Empty + _q;
            var j = _i + HtmlFragment.Empty;
            var k = HtmlFragment.Empty + new CustomHtmlElement("bla");
            var l = new CustomHtmlElement("bla") + HtmlFragment.Empty;
            var m = HtmlFragment.Empty + HtmlFragment.Empty;
            var n = "test" + HtmlFragment.Empty;
            var o = HtmlFragment.Empty + "test";

            var typeReflectionHack = new { a, b, c, d, e, f, g, h, i, j, k, l, m, n, o };
            PAssert.That(() => typeReflectionHack.GetType().GetProperties().Select(fi => fi.PropertyType).Distinct().SequenceEqual(new[] { typeof(HtmlFragment) }));
        }
    }
}
