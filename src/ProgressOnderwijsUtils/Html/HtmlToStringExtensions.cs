using System.Buffers;
using System.IO.Pipelines;

namespace ProgressOnderwijsUtils.Html;

sealed record PipeSink(PipeWriter writer) : IStringSink, IDisposable
{
    const int blockSize = 1 << 14;
    static readonly Encoding Encoding = StringUtils.Utf8WithoutBom;
    Memory<byte> buffer = Memory<byte>.Empty;
    int bufferBytes;

    public void AppendText(ReadOnlySpan<char> text)
    {
        var byteCount = Encoding.GetByteCount(text);
        if (buffer.Length <= byteCount) {
            Flush();
            if (byteCount >= blockSize) {
                _ = Encoding.GetBytes(text, writer);
                return;
            }
            buffer = writer.GetMemory(blockSize);
        }
        var actualBytesWritten = Encoding.GetBytes(text, buffer.Span);
        bufferBytes += actualBytesWritten;
        buffer = buffer.Slice(actualBytesWritten);
    }

    public void Flush()
    {
        if (bufferBytes > 0) {
            writer.Advance(bufferBytes);
            buffer = Memory<byte>.Empty;
            bufferBytes = 0;
        }
    }

    public void AppendUtf8(ReadOnlySpan<byte> text)
    {
        var byteCount = text.Length;
        if (buffer.Length <= byteCount) {
            Flush();
            if (byteCount >= blockSize) {
                writer.Write(text);
                return;
            }
            buffer = writer.GetMemory(blockSize);
        }
        text.CopyTo(buffer.Span);
        bufferBytes += byteCount;
        buffer = buffer.Slice(byteCount);
    }

    public void Dispose()
        => Flush();
}

readonly record struct WriterSink(StreamWriter writer) : IStringSink
{
    public void AppendText(ReadOnlySpan<char> text)
        => writer.Write(text);
}

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

    public static void SaveHtmlFragmentToPipe(this HtmlFragment rootElem, PipeWriter outputStream, Encoding contentEncoding)
    {
        var sink = new PipeSink(contentEncoding, outputStream);
        sink.AppendText("<!DOCTYPE html>");
        AppendFragment(sink, rootElem);
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

    public static void SaveHtmlFragmentToStreamViaWriter(this HtmlFragment rootElem, Stream outputStream, Encoding contentEncoding)
    {
        using var writer = new StreamWriter(outputStream, contentEncoding, leaveOpen: true);
        var sink = new WriterSink(writer);
        sink.AppendText("<!DOCTYPE html>");
        AppendFragment(sink, rootElem);
    }

    static void AppendFragment<TSink>(TSink sink, HtmlFragment fragment)
        where TSink : IStringSink
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

    static void AppendTagContentAndEnd<TSink>(TSink sink, IHtmlElementAllowingContent htmlElementAllowingContent)
        where TSink : IStringSink
    {
        var contents = htmlElementAllowingContent.GetContent();
        if (htmlElementAllowingContent.TagName.EqualsOrdinalCaseInsensitive("script") || htmlElementAllowingContent.TagName.EqualsOrdinalCaseInsensitive("style")) {
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

    static void AppendAttributes<TSink>(TSink sink, HtmlAttributes attributes)
        where TSink : IStringSink
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

    static void AppendAttribute<TSink>(TSink sink, HtmlAttribute htmlAttribute)
        where TSink : IStringSink
    {
        sink.AppendText(" ");
        sink.AppendText(htmlAttribute.Name);
        if (htmlAttribute.Value != "") {
            sink.AppendText("=\"");
            AppendEscapedAttributeValue(sink, htmlAttribute.Value);
            sink.AppendText("\"");
        }
    }

    static void AppendAsRawText<TSink>(TSink sink, HtmlFragment fragment)
        where TSink : IStringSink
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

    static void AppendEscapedText<TSink>(TSink sink, string stringContent)
        where TSink : IStringSink
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

    static void AppendEscapedAttributeValue<TSink>(TSink sink, string attrValue)
        where TSink : IStringSink
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
