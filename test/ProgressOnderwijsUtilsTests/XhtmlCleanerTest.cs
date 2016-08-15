using System.Xml.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business.Test;
using Progress.Business.Tools;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    public sealed class XhtmlCleanerTest
    {
        const string sample =
            @"<p><b>HTML sanitization</b> is the process of examining an HTML document and producing a new HTML document that preserves only whatever tags are designated ""safe"". HTML sanitization can be used to protect against <a href=""/wiki/Cross-site_scripting"" title=""Cross-site scripting"">cross-site scripting</a> attacks by sanitizing any HTML code submitted by a user.</p> 
<p><br /></p> ";

        [Test]
        public void TextLimitWorks()
        {
            var tidiedSample = XhtmlCleaner.SanitizeHtmlString(sample);
            Assert.That(StringUtils.LevenshteinDistanceScaled(sample, tidiedSample), Is.LessThan(0.05));
            int lastLength = tidiedSample.Length;
            for (int i = tidiedSample.Length + 10; i >= 0; i--) {
                var limitedVer = XhtmlCleaner.TidyHtmlStringAndLimitLength(sample, i);
                Assert.That(limitedVer.Length, Is.LessThanOrEqualTo(i));
                if (limitedVer.Length < i) { // if more than "needed" trimmed then:
                    PAssert.That(() => lastLength == i + 1 || lastLength == limitedVer.Length);
                    //either the string was already this short or it was just trimmed due to real need - but it wasn't unnecessarily trimmed!
                }
                lastLength = limitedVer.Length;
            }
        }

        [Test]
        public void LimitIsIgnoredIfNull() {
            var limitedVer = XhtmlCleaner.TidyHtmlStringAndLimitLength(sample, null);

            PAssert.That(() => limitedVer == sample);
        }

        const string sample2 =
            @"<p><script>lalala<b>innerlala</b></script>test this! <!--lalala<p>--> <div class=""lalala"" style=""lalala"">hmm</div> <unknown>include this though</unknown></p>";

        [Test]
        public void CleanerWorks()
        {
            var tidiedSample = XhtmlCleaner.SanitizeHtmlString(sample2);
            PAssert.That(() => !tidiedSample.Contains("lalala") && !tidiedSample.Contains("script") && !tidiedSample.Contains("innerlala"));
            PAssert.That(() => !tidiedSample.Contains("class") && !tidiedSample.Contains("style") && !tidiedSample.Contains("unknown"));
            PAssert.That(() => tidiedSample.Contains("include this") && tidiedSample.Contains("test this") && 
                               tidiedSample.Contains("<div>") && tidiedSample.StartsWith("<p>"));
        }

        const string sample3 = @"<p>&nbsp;whee</p>";
        const string sample4 = @"<p>1 < 2 && 3 > 0</p>";

        [Test]
        public void FixerWorks()
        {
            PAssert.That(() => XhtmlCleaner.SanitizeHtmlString(sample3) == "<p>\u00A0whee</p>");
            PAssert.That(() => XhtmlCleaner.SanitizeHtmlString(sample4) == new XText(@"1 < 2 && 3 > 0").ToString());
            PAssert.That(() => XhtmlCleaner.HtmlToTextParser(sample4) == @"1 < 2 && 3 > 0");
        }

        [Test]
        public void SupportsUnicode()
        {
            PAssert.That(() => XhtmlCleaner.SanitizeHtmlString(@"ÂÅÉéé") == @"ÂÅÉéé");
        }

        [Test]
        public void DontMisinterpretEncodedXml()
        {
            var xEl = new XElement("div", sample3);
            var xElEncoded = xEl.ToString(SaveOptions.None);

            PAssert.That(() => XhtmlCleaner.SanitizeHtmlString("<p>" + xElEncoded + "</p>") == "<p>" + xElEncoded + "</p>");
        }

        [Test]
        public void DecodeUnnecessaryEntities()
        {
            PAssert.That(() => XhtmlCleaner.SanitizeHtmlString("<p>&Acirc;&Eacute;&eacute;&amp;<br /></p>") == "<p>ÂÉé&amp;<br /></p>");
        }

        [Test]
        public void RemovesUnknownElements()
        {
            PAssert.That(() => XhtmlCleaner.SanitizeHtmlString(@"This <p>paragraph contains <unknown>nonsense</unknown></p>.") == @"This <p>paragraph contains nonsense</p>.");
        }

        [Test]
        public void RemovesStyleTags()
        {
            PAssert.That(() => XhtmlCleaner.SanitizeHtmlString(@"This is not <span style=""display: none"">invisible</span>.") == @"This is not <span>invisible</span>.");
        }

        [Test]
        public void AllowsMarginStyleTags()
        {
            PAssert.That(
                () => XhtmlCleaner.SanitizeHtmlString(@"This <p style=""margin-left: 40px;"">is indented</p>!") == @"This <p style=""margin-left: 40px;"">is indented</p>!");
        }

        [Test]
        public void DetectsNormalTextAsNonHtml()
        {
            PAssert.That(() => XhtmlCleaner.CannotBeValidHtml(@"This is just text!"));
        }

        [Test]
        public void DetectsAnHtmlFragmentAsHtml()
        {
            PAssert.That(() => !XhtmlCleaner.CannotBeValidHtml(@"This is <b>not</b> just text!"));
        }

        [Test]
        public void DetectsAnHtmlFragmentWithNestedEntityRefAsHtml()
        {
            PAssert.That(() => !XhtmlCleaner.CannotBeValidHtml(@"This is <b id=""&amp;"">not</b> just text!"));
        }

        [Test]
        public void DetectsAnHtmlFragmentWithOpenTagsNestedAsInvalid()
        {
            PAssert.That(() => XhtmlCleaner.CannotBeValidHtml(@"This is <b <b>bla</b> id>invalid</b>!"));
        }

        [Test]
        public void DetectsAnHtmlFragmentWithTagInEntityAsInvalid()
        {
            PAssert.That(() => XhtmlCleaner.CannotBeValidHtml(@"This is &amp <br/> ;"));
        }

        [Test]
        public void DetectsUnencodedAmpersandTextAsNonHtml()
        {
            PAssert.That(() => XhtmlCleaner.CannotBeValidHtml(@"Text & so on is <i>subtly</i> wrong."));
        }

        [Test]
        public void DetectsUnmatchedLessThanAsNonHtml()
        {
            PAssert.That(() => XhtmlCleaner.CannotBeValidHtml(@"x < y"));
        }
    }
}
