using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ProgressOnderwijsUtils.Html
{
    public static class HtmlToStringExtensions
    {
        public static string SerializeToString(this IConvertibleToFragment rootElem)
        {
            var fastStringBuilder = FastShortStringBuilder.Create(1u << 16);
            fastStringBuilder.AppendText("<!DOCTYPE html>");
            AppendToBuilder(ref fastStringBuilder, rootElem.AsFragment());
            return fastStringBuilder.Value;
        }

        public static string ToCSharp(this IConvertibleToFragment rootElem) => rootElem.AsFragment().ToCSharp();

        public static string SerializeToStringWithoutDoctype(this IConvertibleToFragment rootElem)
        {
            var fastStringBuilder = FastShortStringBuilder.Create(1u << 16);
            AppendToBuilder(ref fastStringBuilder, rootElem.AsFragment());
            return fastStringBuilder.Value;
        }

        public static void SaveHtmlFragmentToStream(HtmlFragment rootElem, Stream outputStream, Encoding contentEncoding)
        {
            var fastStringBuilder = FastShortStringBuilder.Create(1u << 16);
            fastStringBuilder.AppendText("<!DOCTYPE html>");
            AppendToBuilder(ref fastStringBuilder, rootElem);
            const int charsPerBuffer = 2048;
            var maxBufferSize = contentEncoding.GetMaxByteCount(charsPerBuffer);
            var byteBuffer = PooledExponentialBufferAllocator<byte>.GetByLength((uint)maxBufferSize);

            var charCount = fastStringBuilder.CurrentLength;
            var charsWritten = 0;
            while (charsWritten < charCount) {
                var charsToConvert = Math.Min(charCount - charsWritten, charsPerBuffer);
                var bytesWritten = contentEncoding.GetBytes(fastStringBuilder.CurrentCharacterBuffer, charsWritten, charsToConvert, byteBuffer, 0);
                outputStream.Write(byteBuffer, 0, bytesWritten);
                charsWritten += charsPerBuffer;
            }
            PooledExponentialBufferAllocator<byte>.ReturnToPool(byteBuffer);
        }

        static void AppendToBuilder(ref FastShortStringBuilder stringBuilder, HtmlFragment fragment)
        {
            if (fragment.Content is string stringContent) {
                AppendEscapedText(ref stringBuilder, stringContent);
            } else if (fragment.Content is IHtmlTag htmlTag) {
                stringBuilder.AppendText(htmlTag.TagStart);
                if (htmlTag.Attributes != null) {
                    AppendAttributes(ref stringBuilder, htmlTag.Attributes);
                }
                stringBuilder.AppendText(">");

                if (htmlTag is IHtmlTagAllowingContent htmlTagAllowingContent) {
                    AppendTagContentAndEnd(ref stringBuilder, htmlTagAllowingContent);
                }
            } else if (fragment.Content is HtmlFragment[] fragments) {
                foreach (var child in fragments) {
                    AppendToBuilder(ref stringBuilder, child);
                }
            }
        }

        static void AppendTagContentAndEnd(ref FastShortStringBuilder stringBuilder, IHtmlTagAllowingContent htmlTagAllowingContent)
        {
            var contents = htmlTagAllowingContent.Contents ?? Array.Empty<HtmlFragment>();
            if (htmlTagAllowingContent.TagName == "script" || htmlTagAllowingContent.TagName == "style") {
                foreach (var childNode in contents) {
                    AppendAsRawTextToBuilder(ref stringBuilder, childNode);
                }
            } else {
                foreach (var childNode in contents) {
                    AppendToBuilder(ref stringBuilder, childNode);
                }
            }
            stringBuilder.AppendText(htmlTagAllowingContent.EndTag);
        }

        static void AppendAttributes(ref FastShortStringBuilder stringBuilder, HtmlAttribute[] attributes)
        {
            var className = default(string);
            foreach (var htmlAttribute in attributes) {
                if (htmlAttribute.Name == "class") {
                    className = className == null ? htmlAttribute.Value : className + " " + htmlAttribute.Value;
                } else {
                    AppendAttribute(ref stringBuilder, htmlAttribute);
                }
            }
            if (className != null) {
                AppendAttribute(ref stringBuilder, new HtmlAttribute("class", className));
            }
        }

        static void AppendAttribute(ref FastShortStringBuilder stringBuilder, HtmlAttribute htmlAttribute)
        {
            stringBuilder.AppendText(" ");
            stringBuilder.AppendText(htmlAttribute.Name);
            if (htmlAttribute.Value != "") {
                stringBuilder.AppendText("=\"");
                AppendEscapedAttributeValue(ref stringBuilder, htmlAttribute.Value);
                stringBuilder.AppendText("\"");
            }
        }

        static void AppendAsRawTextToBuilder(ref FastShortStringBuilder stringBuilder, HtmlFragment fragment)
        {
            if (fragment.Content is HtmlFragment[] fragments) {
                foreach (var childNode in fragments) {
                    AppendAsRawTextToBuilder(ref stringBuilder, childNode);
                }
            } else if (fragment.Content is string contentString) {
                Debug.Assert(fragment.IsTextContent);
                stringBuilder.AppendText(contentString);
            } else if (fragment.Content is IHtmlTag) {
                throw new InvalidOperationException("script and style tags cannot contain child elements");
            }
        }

        static void AppendEscapedText(ref FastShortStringBuilder stringBuilder, string stringContent)
        {
            var uptoIndex = 0;
            for (var textIndex = 0; textIndex < stringContent.Length; textIndex++) {
                var c = stringContent[textIndex];
                if (c <= '>') {
                    //https://html.spec.whatwg.org/#elements-2:normal-elements-4
                    //normal text must not contain < or & unescaped
                    if (c == '<') {
                        stringBuilder.AppendText(stringContent, uptoIndex, textIndex - uptoIndex);
                        stringBuilder.AppendText("&lt;");
                        uptoIndex = textIndex + 1;
                    } else if (c == '&') {
                        stringBuilder.AppendText(stringContent, uptoIndex, textIndex - uptoIndex);
                        stringBuilder.AppendText("&amp;");
                        uptoIndex = textIndex + 1;
                    } else if (c == '>') {
                        //not strictly necessary
                        stringBuilder.AppendText(stringContent, uptoIndex, textIndex - uptoIndex);
                        stringBuilder.AppendText("&gt;");
                        uptoIndex = textIndex + 1;
                    }
                }
            }
            stringBuilder.AppendText(stringContent, uptoIndex, stringContent.Length - uptoIndex);
        }

        static void AppendEscapedAttributeValue(ref FastShortStringBuilder stringBuilder, string attrValue)
        {
            var uptoIndex = 0;
            for (var textIndex = 0; textIndex < attrValue.Length; textIndex++) {
                var c = attrValue[textIndex];
                if (c <= '&') {
                    //https://html.spec.whatwg.org/#attributes-2
                    //quoted attribute values must not contain " or & unescaped
                    if (c == '&') {
                        stringBuilder.AppendText(attrValue, uptoIndex, textIndex - uptoIndex);
                        stringBuilder.AppendText("&amp;");
                        uptoIndex = textIndex + 1;
                    } else if (c == '"') {
                        stringBuilder.AppendText(attrValue, uptoIndex, textIndex - uptoIndex);
                        stringBuilder.AppendText("&quot;");
                        uptoIndex = textIndex + 1;
                    }
                }
            }
            stringBuilder.AppendText(attrValue, uptoIndex, attrValue.Length - uptoIndex);
        }
    }
}
