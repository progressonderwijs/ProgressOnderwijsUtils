using System.Buffers;

namespace ProgressOnderwijsUtils.Html;

public static class HtmlToStringExtensions
{
    const int InitialBufferSize = 1 << 16;

    public static string ToStringWithDoctype(this IConvertibleToFragment rootElem)
    {
        var fastStringBuilder = new FastShortStringSink(InitialBufferSize);
        fastStringBuilder.AppendText("<!DOCTYPE html>");
        AppendToBuilder(fastStringBuilder, rootElem.AsFragment());
        return fastStringBuilder.Underlying.FinishBuilding();
    }

    public static string ToCSharp(this IConvertibleToFragment rootElem)
        => rootElem.AsFragment().ToCSharp();

    public static string ToStringWithoutDoctype(this IConvertibleToFragment rootElem)
    {
        var fastStringBuilder = new FastShortStringSink(InitialBufferSize);
        AppendToBuilder(fastStringBuilder, rootElem.AsFragment());
        return fastStringBuilder.Underlying.FinishBuilding();
    }

    public static void SaveHtmlFragmentToStream(this HtmlFragment rootElem, Stream outputStream, Encoding contentEncoding)
    {
        var fastStringBuilder = new FastShortStringSink(InitialBufferSize);
        fastStringBuilder.AppendText("<!DOCTYPE html>");
        AppendToBuilder(fastStringBuilder, rootElem);
        const int charsPerBuffer = 2048;
        var maxBufferSize = contentEncoding.GetMaxByteCount(charsPerBuffer);
        var byteBuffer = ArrayPool<byte>.Shared.Rent(maxBufferSize);

        var charCount = fastStringBuilder.Underlying.CurrentLength;
        var charsWritten = 0;
        while (charsWritten < charCount) {
            var charsToConvert = Math.Min(charCount - charsWritten, charsPerBuffer);
            var bytesWritten = contentEncoding.GetBytes(fastStringBuilder.Underlying.CurrentCharacterBuffer, charsWritten, charsToConvert, byteBuffer, 0);
            outputStream.Write(byteBuffer, 0, bytesWritten);
            charsWritten += charsPerBuffer;
        }
        ArrayPool<byte>.Shared.Return(byteBuffer);
    }

    static void AppendToBuilder(FastShortStringSink stringBuilder, HtmlFragment fragment)
    {
        if (fragment.Implementation is string stringContent) {
            AppendEscapedText(stringBuilder, stringContent);
        } else if (fragment.Implementation is IHtmlElement htmlTag) {
            stringBuilder.AppendText(htmlTag.TagStart);
            if (htmlTag.Attributes.Count > 0) {
                AppendAttributes(stringBuilder, htmlTag.Attributes);
            }
            stringBuilder.AppendText(">");

            if (htmlTag is IHtmlElementAllowingContent htmlTagAllowingContent) {
                AppendTagContentAndEnd(stringBuilder, htmlTagAllowingContent);
            }
        } else if (fragment.Implementation is HtmlFragment[] fragments) {
            foreach (var child in fragments) {
                AppendToBuilder(stringBuilder, child);
            }
        }
    }

    static void AppendTagContentAndEnd(FastShortStringSink stringBuilder, IHtmlElementAllowingContent htmlElementAllowingContent)
    {
        var contents = htmlElementAllowingContent.GetContent();
        if (htmlElementAllowingContent.TagName.EqualsOrdinalCaseInsensitive("SCRIPT") || htmlElementAllowingContent.TagName.EqualsOrdinalCaseInsensitive("STYLE")) {
            if (contents.Implementation is HtmlFragment[] fragments) {
                foreach (var childNode in fragments) {
                    AppendAsRawTextToBuilder(stringBuilder, childNode);
                }
            } else if (!contents.IsEmpty) {
                AppendAsRawTextToBuilder(stringBuilder, contents);
            }
        } else {
            if (contents.Implementation is HtmlFragment[] fragments) {
                foreach (var childNode in fragments) {
                    AppendToBuilder(stringBuilder, childNode);
                }
            } else if (!contents.IsEmpty) {
                AppendToBuilder(stringBuilder, contents);
            }
        }
        stringBuilder.AppendText(htmlElementAllowingContent.EndTag);
    }

    static void AppendAttributes(FastShortStringSink stringBuilder, HtmlAttributes attributes)
    {
        var className = default(string);
        foreach (var htmlAttribute in attributes) {
            if (htmlAttribute.Name == "class") {
                className = className == null ? htmlAttribute.Value : $"{className} {htmlAttribute.Value}";
            } else {
                AppendAttribute(stringBuilder, htmlAttribute);
            }
        }
        if (className != null) {
            AppendAttribute(stringBuilder, new("class", className));
        }
    }

    static void AppendAttribute(FastShortStringSink stringBuilder, HtmlAttribute htmlAttribute)
    {
        stringBuilder.AppendText(" ");
        stringBuilder.AppendText(htmlAttribute.Name);
        if (htmlAttribute.Value != "") {
            stringBuilder.AppendText("=\"");
            AppendEscapedAttributeValue(stringBuilder, htmlAttribute.Value);
            stringBuilder.AppendText("\"");
        }
    }

    static void AppendAsRawTextToBuilder(FastShortStringSink stringBuilder, HtmlFragment fragment)
    {
        if (fragment.Implementation is HtmlFragment[] fragments) {
            foreach (var childNode in fragments) {
                AppendAsRawTextToBuilder(stringBuilder, childNode);
            }
        } else if (fragment.Implementation is string contentString) {
            stringBuilder.AppendText(contentString);
        } else if (fragment.Implementation is IHtmlElement) {
            throw new InvalidOperationException("script and style tags cannot contain child elements");
        }
    }

    static void AppendEscapedText(FastShortStringSink stringBuilder, string stringContent)
    {
        var uptoIndex = 0;
        for (var textIndex = 0; textIndex < stringContent.Length; textIndex++) {
            var c = stringContent[textIndex];
            if (c <= '>') {
                //https://html.spec.whatwg.org/#elements-2:normal-elements-4
                //normal text must not contain < or & unescaped
                if (c == '<') {
                    stringBuilder.AppendText(stringContent.AsSpan(uptoIndex, textIndex - uptoIndex));
                    stringBuilder.AppendText("&lt;");
                    uptoIndex = textIndex + 1;
                } else if (c == '&') {
                    stringBuilder.AppendText(stringContent.AsSpan(uptoIndex, textIndex - uptoIndex));
                    stringBuilder.AppendText("&amp;");
                    uptoIndex = textIndex + 1;
                } else if (c == '>') {
                    //not strictly necessary
                    stringBuilder.AppendText(stringContent.AsSpan(uptoIndex, textIndex - uptoIndex));
                    stringBuilder.AppendText("&gt;");
                    uptoIndex = textIndex + 1;
                }
            }
        }
        stringBuilder.AppendText(stringContent.AsSpan(uptoIndex, stringContent.Length - uptoIndex));
    }

    static void AppendEscapedAttributeValue(FastShortStringSink stringBuilder, string attrValue)
    {
        var uptoIndex = 0;
        for (var textIndex = 0; textIndex < attrValue.Length; textIndex++) {
            var c = attrValue[textIndex];
            if (c <= '&') {
                //https://html.spec.whatwg.org/#attributes-2
                //quoted attribute values must not contain " or & unescaped
                if (c == '&') {
                    stringBuilder.AppendText(attrValue.AsSpan(uptoIndex, textIndex - uptoIndex));
                    stringBuilder.AppendText("&amp;");
                    uptoIndex = textIndex + 1;
                } else if (c == '"') {
                    stringBuilder.AppendText(attrValue.AsSpan(uptoIndex, textIndex - uptoIndex));
                    stringBuilder.AppendText("&quot;");
                    uptoIndex = textIndex + 1;
                }
            }
        }
        stringBuilder.AppendText(attrValue.AsSpan(uptoIndex, attrValue.Length - uptoIndex));
    }
}
