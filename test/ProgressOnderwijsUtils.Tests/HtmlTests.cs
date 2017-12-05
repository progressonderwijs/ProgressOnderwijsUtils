﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
