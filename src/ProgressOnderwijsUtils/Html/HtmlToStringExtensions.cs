using System.Buffers;

namespace ProgressOnderwijsUtils.Html;

public static class HtmlToStringExtensions
{
    const int InitialBufferSize = 1 << 16;

    public static string ToStringWithDoctype(this IConvertibleToFragment rootElem)
    {
        var sink = new FastShortStringSink(InitialBufferSize);
        sink.AppendText("<!DOCTYPE html>");
        AppendFragment(sink, rootElem.AsFragment());
        return sink.Underlying.FinishBuilding();
    }

    public static string ToCSharp(this IConvertibleToFragment rootElem)
        => rootElem.AsFragment().ToCSharp();

    public static string ToStringWithoutDoctype(this IConvertibleToFragment rootElem)
    {
        var sink = new FastShortStringSink(InitialBufferSize);
        AppendFragment(sink, rootElem.AsFragment());
        return sink.Underlying.FinishBuilding();
    }

    public static void SaveHtmlFragmentToStream(this HtmlFragment rootElem, Stream outputStream, Encoding contentEncoding)
    {
        var sink = new FastShortStringSink(InitialBufferSize);
        sink.AppendText("<!DOCTYPE html>");
        AppendFragment(sink, rootElem);
        const int charsPerBuffer = 2048;
        var maxBufferSize = contentEncoding.GetMaxByteCount(charsPerBuffer);
        var byteBuffer = ArrayPool<byte>.Shared.Rent(maxBufferSize);

        var charCount = sink.Underlying.CurrentLength;
        var charsWritten = 0;
        while (charsWritten < charCount) {
            var charsToConvert = Math.Min(charCount - charsWritten, charsPerBuffer);
            var bytesWritten = contentEncoding.GetBytes(sink.Underlying.CurrentCharacterBuffer, charsWritten, charsToConvert, byteBuffer, 0);
            outputStream.Write(byteBuffer, 0, bytesWritten);
            charsWritten += charsPerBuffer;
        }
        ArrayPool<byte>.Shared.Return(byteBuffer);
    }

    static void AppendFragment(FastShortStringSink sink, HtmlFragment fragment)
    {
        if (fragment.Implementation is string stringContent) {
            AppendEscapedText(sink, stringContent);
        } else if (fragment.Implementation is IHtmlElement htmlTag) {
            sink.AppendText(htmlTag.TagStart);
            if (htmlTag.Attributes.Count > 0) {
                AppendAttributes(sink, htmlTag.Attributes);
            }
            sink.AppendText(">");

            if (htmlTag is IHtmlElementAllowingContent htmlTagAllowingContent) {
                AppendTagContentAndEnd(sink, htmlTagAllowingContent);
            }
        } else if (fragment.Implementation is HtmlFragment[] fragments) {
            foreach (var child in fragments) {
                AppendFragment(sink, child);
            }
        }
    }

    static void AppendTagContentAndEnd(FastShortStringSink sink, IHtmlElementAllowingContent htmlElementAllowingContent)
    {
        var contents = htmlElementAllowingContent.GetContent();
        if (htmlElementAllowingContent.TagName.EqualsOrdinalCaseInsensitive("SCRIPT") || htmlElementAllowingContent.TagName.EqualsOrdinalCaseInsensitive("STYLE")) {
            if (contents.Implementation is HtmlFragment[] fragments) {
                foreach (var childNode in fragments) {
                    AppendAsRawText(sink, childNode);
                }
            } else if (!contents.IsEmpty) {
                AppendAsRawText(sink, contents);
            }
        } else {
            if (contents.Implementation is HtmlFragment[] fragments) {
                foreach (var childNode in fragments) {
                    AppendFragment(sink, childNode);
                }
            } else if (!contents.IsEmpty) {
                AppendFragment(sink, contents);
            }
        }
        sink.AppendText(htmlElementAllowingContent.EndTag);
    }

    static void AppendAttributes(FastShortStringSink sink, HtmlAttributes attributes)
    {
        var className = default(string);
        foreach (var htmlAttribute in attributes) {
            if (htmlAttribute.Name == "class") {
                className = className == null ? htmlAttribute.Value : $"{className} {htmlAttribute.Value}";
            } else {
                AppendAttribute(sink, htmlAttribute);
            }
        }
        if (className != null) {
            AppendAttribute(sink, new("class", className));
        }
    }

    static void AppendAttribute(FastShortStringSink sink, HtmlAttribute htmlAttribute)
    {
        sink.AppendText(" ");
        sink.AppendText(htmlAttribute.Name);
        if (htmlAttribute.Value != "") {
            sink.AppendText("=\"");
            AppendEscapedAttributeValue(sink, htmlAttribute.Value);
            sink.AppendText("\"");
        }
    }

    static void AppendAsRawText(FastShortStringSink sink, HtmlFragment fragment)
    {
        if (fragment.Implementation is HtmlFragment[] fragments) {
            foreach (var childNode in fragments) {
                AppendAsRawText(sink, childNode);
            }
        } else if (fragment.Implementation is string contentString) {
            sink.AppendText(contentString);
        } else if (fragment.Implementation is IHtmlElement) {
            throw new InvalidOperationException("script and style tags cannot contain child elements");
        }
    }

    static void AppendEscapedText(FastShortStringSink sink, string stringContent)
    {
        var uptoIndex = 0;
        for (var textIndex = 0; textIndex < stringContent.Length; textIndex++) {
            var c = stringContent[textIndex];
            if (c <= '>') {
                //https://html.spec.whatwg.org/#elements-2:normal-elements-4
                //normal text must not contain < or & unescaped
                if (c == '<') {
                    sink.AppendText(stringContent.AsSpan(uptoIndex, textIndex - uptoIndex));
                    sink.AppendText("&lt;");
                    uptoIndex = textIndex + 1;
                } else if (c == '&') {
                    sink.AppendText(stringContent.AsSpan(uptoIndex, textIndex - uptoIndex));
                    sink.AppendText("&amp;");
                    uptoIndex = textIndex + 1;
                } else if (c == '>') {
                    //not strictly necessary
                    sink.AppendText(stringContent.AsSpan(uptoIndex, textIndex - uptoIndex));
                    sink.AppendText("&gt;");
                    uptoIndex = textIndex + 1;
                }
            }
        }
        sink.AppendText(stringContent.AsSpan(uptoIndex, stringContent.Length - uptoIndex));
    }

    static void AppendEscapedAttributeValue(FastShortStringSink sink, string attrValue)
    {
        var uptoIndex = 0;
        for (var textIndex = 0; textIndex < attrValue.Length; textIndex++) {
            var c = attrValue[textIndex];
            if (c <= '&') {
                //https://html.spec.whatwg.org/#attributes-2
                //quoted attribute values must not contain " or & unescaped
                if (c == '&') {
                    sink.AppendText(attrValue.AsSpan(uptoIndex, textIndex - uptoIndex));
                    sink.AppendText("&amp;");
                    uptoIndex = textIndex + 1;
                } else if (c == '"') {
                    sink.AppendText(attrValue.AsSpan(uptoIndex, textIndex - uptoIndex));
                    sink.AppendText("&quot;");
                    uptoIndex = textIndex + 1;
                }
            }
        }
        sink.AppendText(attrValue.AsSpan(uptoIndex, attrValue.Length - uptoIndex));
    }
}
