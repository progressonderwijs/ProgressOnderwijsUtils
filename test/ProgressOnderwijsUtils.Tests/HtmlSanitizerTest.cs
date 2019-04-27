using System;
using System.Xml.Linq;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.Html;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class HtmlSanitizerTest
    {
        const string sample =
            @"<p><b>HTML sanitization</b> is the process of examining an HTML document and producing a new HTML document that preserves only whatever tags are designated ""safe"". HTML sanitization can be used to protect against <a href=""http://yadayada.com/wiki/Cross-site_scripting"" title=""Cross-site scripting"">cross-site scripting</a> attacks by sanitizing any HTML code submitted by a user.</p> 
<p><br /></p> ";

        [Fact]
        public void TextLimitWorks()
        {
            var sampleParsed = HtmlFragment.Parse(sample);
            var tidiedSample = sampleParsed.SerializeToStringWithoutDoctype();
            PAssert.That(() => StringUtils.LevenshteinDistanceScaled(sample, tidiedSample) < 0.05);
            var lastLength = tidiedSample.Length + 15; //15 chars fudge-factor due to xhtml vs. html serialization differences. not ideal.
            for (var i = tidiedSample.Length + 14; i >= 0; i--) {
                var limitedVer = sampleParsed.LimitLength(i).SerializeToStringWithoutDoctype();
                PAssert.That(() => limitedVer.Length <= i);
                if (limitedVer.Length < i) { // if more than "needed" trimmed then:
                    PAssert.That(() => lastLength == i + 1 || lastLength == limitedVer.Length);
                    //either the string was already this short or it was just trimmed due to real need - but it wasn't unnecessarily trimmed!
                }
                lastLength = limitedVer.Length;
            }
        }

        [Fact]
        public void LimitIsIgnoredIfNull()
        {
            var limitedVer = HtmlFragment.Parse(sample).LimitLength(null).SerializeToStringWithoutDoctype();
            var tidiedSample = HtmlFragment.Parse(sample).SerializeToStringWithoutDoctype();
            PAssert.That(() => limitedVer == tidiedSample);
        }

        const string sample2 =
            @"<p><script>lalala<b>innerlala</b></script>test this! <!--lalala<p>--> <div class=""lalala"" style=""lalala"">hmm</div> <unknown>include this though</unknown></p>";

        [Fact]
        public void CleanerWorks()
        {
            var tidiedSample = HtmlFragment.Parse(sample2).Sanitize().SerializeToStringWithoutDoctype();
            PAssert.That(() => !tidiedSample.Contains("lalala") && !tidiedSample.Contains("script") && !tidiedSample.Contains("innerlala"));
            PAssert.That(() => !tidiedSample.Contains("class") && !tidiedSample.Contains("style") && !tidiedSample.Contains("unknown"));
            PAssert.That(() => tidiedSample.Contains("include this") && tidiedSample.Contains("test this")
                && tidiedSample.Contains("<div>") && tidiedSample.StartsWith("<p>", StringComparison.Ordinal));
        }

        const string sample3 = "<p>&nbsp;whee</p>";
        const string sample4 = "<p>1 < 2 && 3 > 0</p>";

        [Fact]
        public void FixerWorks()
        {
            PAssert.That(() => HtmlFragment.Parse(sample3).Sanitize().SerializeToStringWithoutDoctype() == "<p>\u00A0whee</p>");
            PAssert.That(() => HtmlFragment.Parse(sample4).Sanitize().SerializeToStringWithoutDoctype() == "<p>1 &lt; 2 &amp;&amp; 3 &gt; 0</p>");
        }

        [Fact]
        public void SupportsUnicode()
            => PAssert.That(() => HtmlFragment.Parse("ÂÅÉéé").Sanitize().SerializeToStringWithoutDoctype() == "ÂÅÉéé");

        [Fact]
        public void DontMisinterpretEncodedXml()
        {
            var xEl = new XElement("b", sample3);
            var xElEncoded = xEl.ToString(SaveOptions.None);

            PAssert.That(() => HtmlFragment.Parse("<p>" + xElEncoded + "</p>").Sanitize().SerializeToStringWithoutDoctype() == "<p>" + xElEncoded + "</p>");
        }

        [Fact]
        public void DecodeUnnecessaryEntities()
            => PAssert.That(() => HtmlFragment.Parse("<p>&Acirc;&Eacute;&eacute;&amp;<br /></p>").Sanitize().SerializeToStringWithoutDoctype() == "<p>ÂÉé&amp;<br></p>");

        [Fact]
        public void RemovesUnknownElements()
            => PAssert.That(() => HtmlFragment.Parse("This <p>paragraph contains <unknown>nonsense</unknown></p>.").Sanitize().SerializeToStringWithoutDoctype() == "This <p>paragraph contains nonsense</p>.");

        [Fact]
        public void RemoveStyleWithDisallowedValues()
            => PAssert.That(() => HtmlFragment.Parse(@"This is not <span style=""display: none"">invisible</span>.").Sanitize().SerializeToStringWithoutDoctype() == "This is not <span>invisible</span>.");

        [Fact]
        public void KeepStyleWithAllowedValues()
            => PAssert.That(() => HtmlFragment.Parse(@"<div style=""margin-left: 20px"">this style is kept</div>.").Sanitize().SerializeToStringWithoutDoctype() == @"<div style=""margin-left: 20px"">this style is kept</div>.");

        [Fact]
        public void RemoveNonHttpHrefs()
            => PAssert.That(() => HtmlFragment.Parse(@"This link is <a href=""javascript:alert('noxss')"">not a link</a>.").Sanitize().SerializeToStringWithoutDoctype() == "This link is not a link.");

        [Fact]
        public void HttpHrefsAreAllowed()
            => PAssert.That(() => HtmlFragment.Parse(@"This is <a href=""https://somelink.com/?saynomore=1"">a link</a>.").Sanitize().SerializeToStringWithoutDoctype() == @"This is <a href=""https://somelink.com/?saynomore=1"">a link</a>.");

        [Fact]
        public void AllowsMarginStyleTags()
            => PAssert.That(
                () => HtmlFragment.Parse(@"This <p style=""margin-left: 40px;"">is indented</p>!").Sanitize().SerializeToStringWithoutDoctype() == @"This <p style=""margin-left: 40px;"">is indented</p>!");
    }
}
