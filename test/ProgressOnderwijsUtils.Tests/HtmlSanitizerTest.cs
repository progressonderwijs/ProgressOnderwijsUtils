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
            var sampleParsed = HtmlFragment.ParseFragment(sample);
            var tidiedSample = sampleParsed.ToStringWithoutDoctype();
            PAssert.That(() => StringUtils.LevenshteinDistanceScaled(sample, tidiedSample) < 0.05);
            var lastLength = tidiedSample.Length + 15; //15 chars fudge-factor due to xhtml vs. html serialization differences. not ideal.
            for (var i = tidiedSample.Length + 14; i >= 0; i--) {
                var limitedVer = sampleParsed.LimitLength(i).ToStringWithoutDoctype();
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
            var limitedVer = HtmlFragment.ParseFragment(sample).LimitLength(null).ToStringWithoutDoctype();
            var tidiedSample = HtmlFragment.ParseFragment(sample).ToStringWithoutDoctype();
            PAssert.That(() => limitedVer == tidiedSample);
        }

        const string sample2 =
            @"<p><script>lalala<b>innerlala</b></script>test this! <!--lalala<p>--> <div class=""lalala"" style=""lalala"">hmm</div> <unknown>include this though</unknown></p>";

        [Fact]
        public void CleanerWorks()
        {
            var tidiedSample = HtmlFragment.ParseFragment(sample2).Sanitize().ToStringWithoutDoctype();
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
            PAssert.That(() => HtmlFragment.ParseFragment(sample3).Sanitize().ToStringWithoutDoctype() == "<p>\u00A0whee</p>");
            PAssert.That(() => HtmlFragment.ParseFragment(sample4).Sanitize().ToStringWithoutDoctype() == "<p>1 &lt; 2 &amp;&amp; 3 &gt; 0</p>");
        }

        [Fact]
        public void SupportsUnicode()
            => PAssert.That(() => HtmlFragment.ParseFragment("ÂÅÉéé").Sanitize().ToStringWithoutDoctype() == "ÂÅÉéé");

        [Fact]
        public void DontMisinterpretEncodedXml()
        {
            var xEl = new XElement("b", sample3);
            var xElEncoded = xEl.ToString(SaveOptions.None);

            PAssert.That(() => HtmlFragment.ParseFragment("<p>" + xElEncoded + "</p>").Sanitize().ToStringWithoutDoctype() == "<p>" + xElEncoded + "</p>");
        }

        [Fact]
        public void DecodeUnnecessaryEntities()
            => PAssert.That(() => HtmlFragment.ParseFragment("<p>&Acirc;&Eacute;&eacute;&amp;<br /></p>").Sanitize().ToStringWithoutDoctype() == "<p>ÂÉé&amp;<br></p>");

        [Fact]
        public void RemovesUnknownElements()
            => PAssert.That(() => HtmlFragment.ParseFragment("This <p>paragraph contains <unknown>nonsense</unknown></p>.").Sanitize().ToStringWithoutDoctype() == "This <p>paragraph contains nonsense</p>.");

        [Fact]
        public void RemoveStyleWithDisallowedValues()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"This is not <span style=""display: none"">invisible</span>.").Sanitize().ToStringWithoutDoctype() == "This is not <span>invisible</span>.");

        [Fact]
        public void KeepStyleWithAllowedValues()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"<div style=""margin-left: 20px"">this style is kept</div>.").Sanitize().ToStringWithoutDoctype() == @"<div style=""margin-left: 20px"">this style is kept</div>.");

        [Fact]
        public void RemoveNonHttpHrefs()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"This link is <a href=""javascript:alert('noxss')"">not a link</a>.").Sanitize().ToStringWithoutDoctype() == "This link is not a link.");

        [Fact]
        public void RemoveObfuscatedJsLinks()
            => PAssert.That(() => HtmlFragment.ParseFragment($@"This link is <a href=""java{"\r\n"}script:alert('noxss')"">not a link</a>.").Sanitize().ToStringWithoutDoctype() == "This link is not a link.");

        [Fact]
        public void HttpHrefsAreAllowed()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"This is <a href=""https://somelink.com/?saynomore=1"">a link</a>.").Sanitize().ToStringWithoutDoctype() == @"This is <a href=""https://somelink.com/?saynomore=1"">a link</a>.");

        [Fact]
        public void AllowsMarginStyleTags()
            => PAssert.That(
                () => HtmlFragment.ParseFragment(@"This <p style=""margin-left: 40px;"">is indented</p>!").Sanitize().ToStringWithoutDoctype() == @"This <p style=""margin-left: 40px;"">is indented</p>!");

        [Fact]
        public void OwaspCaseInsensitiveXssAttackVector()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"This link is a nice <IMG SRC=JaVaScRiPt:alert('XSS')> image.").Sanitize().ToStringWithoutDoctype() == "This link is a nice  image.");

        [Fact]
        public void Owasp_Malformed_IMG_tags()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<IMG """"""><SCRIPT>alert(""XSS"")</SCRIPT>"">").Sanitize().ToStringWithoutDoctype() == "Test element:<img>\"&gt;");

        [Fact]
        public void Owasp_On_error_alert()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<IMG SRC=/ onerror=""alert(String.fromCharCode(88,83,83))""></img>").Sanitize().ToStringWithoutDoctype() == "Test element:");

        [Fact]
        public void Owasp_Decimal_HTML_character_references_without_trailing_semicolons()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<IMG SRC=&#0000106&#0000097&#0000118&#0000097&#0000115&#0000099&#0000114&#0000105&#0000112&#0000116&#0000058&#0000097&
#0000108&#0000101&#0000114&#0000116&#0000040&#0000039&#0000088&#0000083&#0000083&#0000039&#0000041>").Sanitize().ToStringWithoutDoctype() == "Test element:");

        [Fact]
        public void Owasp_Extraneous_open_brackets()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<<SCRIPT>alert(""XSS"");//<</SCRIPT>").Sanitize().ToStringWithoutDoctype() == "Test element:&lt;");

        [Fact]
        public void Owasp_Double_open_angle_brackets()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<iframe src=http://xss.rocks/scriptlet.html <").Sanitize().ToStringWithoutDoctype() == "Test element:");

        [Fact]
        public void Owasp_IMG_Dynsrc()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<IMG DYNSRC=""javascript:alert('XSS')"">").Sanitize().ToStringWithoutDoctype() == "Test element:<img>");

        [Fact]
        public void Owasp_IMG_lowsrc()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<IMG LOWSRC=""javascript:alert('XSS')"">").Sanitize().ToStringWithoutDoctype() == "Test element:<img>");

        [Fact]
        public void Owasp_INPUT_image()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<INPUT TYPE=""IMAGE"" SRC=""javascript:alert('XSS');"">").Sanitize().ToStringWithoutDoctype() == "Test element:");

        [Fact]
        public void Owasp_BODY_background()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<BODY BACKGROUND=""javascript:alert('XSS')"">").Sanitize().ToStringWithoutDoctype() == "Test element:");

        [Fact]
        public void Owasp_List_style_image()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<STYLE>li {list-style-image: url(""javascript:alert('XSS')"");}</STYLE><UL><LI>XSS</br>").Sanitize().ToStringWithoutDoctype() == "Test element:<ul><li>XSS<br></li></ul>");

        [Fact]
        public void Owasp_Svg_object_tag()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<svg/onload=alert('XSS')>").Sanitize().ToStringWithoutDoctype() == "Test element:");

        [Fact]
        public void Owasp_STYLE_sheet()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<LINK REL=""stylesheet"" HREF=""javascript:alert('XSS');"">").Sanitize().ToStringWithoutDoctype() == "Test element:");

        [Fact]
        public void Owasp_Remote_style_sheet()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<LINK REL=""stylesheet"" HREF=""http://xss.rocks/xss.css"">").Sanitize().ToStringWithoutDoctype() == "Test element:");

        [Fact]
        public void Owasp_Remote_style_sheet_part_2()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<STYLE>@import'http://xss.rocks/xss.css';</STYLE>").Sanitize().ToStringWithoutDoctype() == "Test element:");

        [Fact]
        public void Owasp_Remote_style_sheet_part_3()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<META HTTP-EQUIV=""Link"" Content=""<http://xss.rocks/xss.css>; REL=stylesheet"">").Sanitize().ToStringWithoutDoctype() == "Test element:");

        [Fact]
        public void Owasp_Remote_style_sheet_part_4()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<STYLE>BODY{-moz-binding:url(""http://xss.rocks/xssmoz.xml#xss"")}</STYLE>").Sanitize().ToStringWithoutDoctype() == "Test element:");

        [Fact]
        public void Owasp_STYLE_tags_with_broken_up_JavaScript_for_XSS()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<STYLE>@im\port'\ja\vasc\ript:alert(""XSS"")';</STYLE>").Sanitize().ToStringWithoutDoctype() == "Test element:");

        [Fact]
        public void Owasp_IMG_STYLE_with_expression()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:exp/*<A STYLE='no\xss:noxss(""*//*"");
xss:ex/*XSS*//*/*/pression(alert(""XSS""))'>").Sanitize().ToStringWithoutDoctype() == "Test element:exp/*<a></a>");

        [Fact]
        public void Owasp_META_using_data()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<META HTTP-EQUIV=""refresh"" CONTENT=""0;url=data:text/html base64,PHNjcmlwdD5hbGVydCgnWFNTJyk8L3NjcmlwdD4K"">").Sanitize().ToStringWithoutDoctype() == "Test element:");

        [Fact]
        public void Owasp_META_with_additional_URL_parameter()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<META HTTP-EQUIV=""refresh"" CONTENT=""0; URL=http://;URL=javascript:alert('XSS');"">").Sanitize().ToStringWithoutDoctype() == "Test element:");

        [Fact]
        public void Owasp_TABLE()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<TABLE BACKGROUND=""javascript:alert('XSS')"">").Sanitize().ToStringWithoutDoctype() == "Test element:<table></table>");

        [Fact]
        public void blockquote_cite()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<blockquote cite=""javascript:alert('XSS')"">lala</blockquote>").Sanitize().ToStringWithoutDoctype() == "Test element:<blockquote>lala</blockquote>");

        [Fact]
        public void Owasp_TD()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<TABLE><TD BACKGROUND=""javascript:alert('XSS')"">").Sanitize().ToStringWithoutDoctype() == "Test element:<table><tbody><tr><td></td></tr></tbody></table>");

        [Fact]
        public void Owasp_DIV_background_image_with_unicoded_XSS_exploit()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<DIV STYLE=""background-image:\0075\0072\006C\0028'\006a\0061\0076\0061\0073\0063\0072\0069\0070\0074\003a\0061\006c\0065\0072\0074\0028.1027\0058.1053\0053\0027\0029'\0029"">").Sanitize().ToStringWithoutDoctype() == "Test element:<div></div>");

        [Fact]
        public void Owasp_DIV_expression()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<DIV STYLE=""width: expression(alert('XSS'));"">").Sanitize().ToStringWithoutDoctype() == "Test element:<div></div>");

        [Fact]
        public void DIV_expression_using_margins_since_we_normally_allow_that()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<DIV STYLE=""margin: expression(alert('XSS'));"">").Sanitize().ToStringWithoutDoctype() == "Test element:<div></div>");

        [Fact]
        public void Owasp_DownleveHiddenBlock()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<!--[if gte IE 4]>
 <SCRIPT>alert('XSS');</SCRIPT>
 <![endif]-->").Sanitize().ToStringWithoutDoctype() == "Test element:");

        [Fact]
        public void Owasp_OBJECT_tag()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<OBJECT TYPE=""text/x-scriptlet"" DATA=""http://xss.rocks/scriptlet.html""></OBJECT>").Sanitize().ToStringWithoutDoctype() == "Test element:");

        [Fact]
        public void Owasp_CookieManipulation()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<META HTTP-EQUIV=""Set-Cookie"" Content=""USERID=<SCRIPT>alert('XSS')</SCRIPT>"">").Sanitize().ToStringWithoutDoctype() == "Test element:");

        [Fact]
        public void Owasp_XssUsingHtmlQuoteEncapsulation()
            => PAssert.That(() => HtmlFragment.ParseFragment(@"Test element:<SCRIPT a=`>` SRC=""httx://xss.rocks/xss.js""></SCRIPT>").Sanitize().ToStringWithoutDoctype() == "Test element:");
    }
}
