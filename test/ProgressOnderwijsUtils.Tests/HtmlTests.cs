namespace ProgressOnderwijsUtils.Tests;

using ProgressOnderwijsUtils.Html;
using static Tags;

public sealed class HtmlTests
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
        PAssert.That(() => new[] { HtmlFragment.Empty, _div, }.AsFragment().IsElement());
        PAssert.That(() => new[] { _div, _div, }.AsFragment().IsElement() == false);
    }

    [Fact]
    public void EmptyIfNull_ReturnsEmptyOnlyForNull()
    {
        PAssert.That(() => default(HtmlFragment?).EmptyIfNull().IsEmpty);
        PAssert.That(() => ((HtmlFragment?)_div).EmptyIfNull().IsEmpty == false);
    }

    [Fact]
    public void IsEmpty_SetCorrectly_With_EmptyString()
    {
        PAssert.That(() => HtmlFragment.TextContent(null).IsEmpty);
        PAssert.That(() => HtmlFragment.TextContent("").IsEmpty);
    }

    [Fact]
    public void IsEmpty_SetCorrectly_With_WhiteSpace()
        => PAssert.That(() => HtmlFragment.TextContent(" ").IsEmpty == false);

    [Fact]
    public void IFrameGetsClosingTag()
    {
        var html = _iframe.ToStringWithoutDoctype();
        PAssert.That(() => html == "<iframe></iframe>");
    }

    [Fact]
    public void AppendingToFragmentsWorks()
    {
        var html = _div.AsFragment().Append("test").ToStringWithoutDoctype();
        PAssert.That(() => html == "<div></div>test");
    }

    [Fact]
    public void AppendingMultipleClassesAffectsOutput()
    {
        var example = _div._class("A", "B", null, "D").AsFragment().ToStringWithoutDoctype();
        PAssert.That(() => example == @"<div class=""A B D""></div>");
    }

    [Fact]
    public void CanAppendAnEnumerableOfAttributes()
    {
        var example = _div.Attributes(Enumerable.Range(1, 5).Select(i => $"data-{i}").Select(s => new HtmlAttribute(s, "x"))).AsFragment().ToStringWithoutDoctype();
        PAssert.That(() => example == @"<div data-1=""x"" data-2=""x"" data-3=""x"" data-4=""x"" data-5=""x""></div>");
    }

    [Fact]
    public void AppendingThreeThingsWorks()
    {
        var html = _div.AsFragment().Append("test", _b.Content("la")).ToStringWithoutDoctype();
        PAssert.That(() => html == "<div></div>test<b>la</b>");
    }

    [Fact]
    public void PerfIterativeAppendingDoesNotCreateLotsOfArrays()
    {
        var html = "1".AsFragment().Append("2").Append("3").Append("4");
        var kids = (HtmlFragment[])html.Implementation!;
        PAssert.That(() => kids.First().ToStringWithoutDoctype() == "1");
        PAssert.That(() => kids.Last().ToStringWithoutDoctype() == "4");
    }

    [Fact]
    public void TemplateElementIsParsedIncludingContent()
    {
        var exampleFragmentIncludingTemplate = "<div>D<button>C<template>B<button>A</button></template></button></div>";
        var fragment = HtmlFragment.ParseFragment(exampleFragmentIncludingTemplate);
        var reserialized = fragment.ToStringWithoutDoctype();
        PAssert.That(() => exampleFragmentIncludingTemplate == reserialized);
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

        var typeReflectionHack = new { a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, };
        PAssert.That(() => typeReflectionHack.GetType().GetProperties().Select(fi => fi.PropertyType).Distinct().SequenceEqual(new[] { typeof(HtmlFragment), }));
    }

    [Fact]
    public void SetOfEmptyHtmlFragmentsIsEmpty()
    {
        PAssert.That(() => HtmlFragment.Fragment(HtmlFragment.Empty, HtmlFragment.Empty).IsEmpty, "special case two-arg overload");
        PAssert.That(() => HtmlFragment.Fragment(new[] { HtmlFragment.Empty, HtmlFragment.Empty, }).IsEmpty, "params case");
        PAssert.That(() => HtmlFragment.Fragment(HtmlFragment.Empty, HtmlFragment.Empty, "").IsEmpty, "params case including empty via empty string");
        PAssert.That(() => HtmlFragment.Fragment(new HtmlFragment[] { }).IsEmpty, "params case empty array");
        PAssert.That(() => HtmlFragment.Fragment(new HtmlFragment[] { "", }).IsEmpty, "params case with singleton array of empty string");
        PAssert.That(() => Enumerable.Repeat(HtmlFragment.Empty, 1000).ToArray().AsFragment().IsEmpty, "params case with lots of content");
    }

    [Fact]
    public void AppendingMultipleClassesFromObjectsAffectsOutput()
    {
        var example = _div._classFromObjects(new [] {new CssClass("A"), new CssClass("B"), new CssClass("D"),}).AsFragment().ToStringWithoutDoctype();
        PAssert.That(() => example == @"<div class=""A B D""></div>");
    }
}
