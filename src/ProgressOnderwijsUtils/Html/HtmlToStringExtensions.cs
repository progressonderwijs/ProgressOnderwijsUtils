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
        var byteCount = Encoding.GetMaxByteCount(text.Length); // Encoding.GetByteCount(text);
        if (buffer.Length <= byteCount + bufferBytes) {
            Flush();
            if (byteCount >= blockSize) {
                _ = Encoding.GetBytes(text, writer);
                return;
            }
            buffer = writer.GetMemory(blockSize);
        }
        var actualBytesWritten = Encoding.GetBytes(text, buffer.Span.Slice(bufferBytes));
        bufferBytes += actualBytesWritten;
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
        if (buffer.Length <= byteCount + bufferBytes) {
            Flush();
            if (byteCount >= blockSize) {
                writer.Write(text);
                return;
            }
            buffer = writer.GetMemory(blockSize);
        }
        text.CopyTo(buffer.Span.Slice(bufferBytes));
        bufferBytes += byteCount;
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

    public static void SaveHtmlFragmentToPipe(this HtmlFragment rootElem, PipeWriter outputStream)
    {
        using var sink = new PipeSink(outputStream);
        sink.AppendText("<!DOCTYPE html>");
        AppendFragment(sink, rootElem);
    }

    public static void SaveHtmlFragmentToStream(this HtmlFragment rootElem, Stream outputStream, Encoding contentEncoding)
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
            if (sink is PipeSink pipeSink) {
                pipeSink.AppendUtf8(htmlTag.TagStartUtf8);
            } else {
                sink.AppendText(htmlTag.TagStart);
            }
            if (htmlTag.Attributes.Count > 0) {
                AppendAttributes(sink, htmlTag.Attributes);
            }
            if (sink is PipeSink pipeSink2) {
                pipeSink2.AppendUtf8(">"u8);
            } else {
                sink.AppendText(">");
            }

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
        if (htmlElementAllowingContent.ContainsUnescapedText) {
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
        if (sink is PipeSink pipeSink) {
            pipeSink.AppendUtf8(htmlElementAllowingContent.EndTagUtf8);
        } else {
            sink.AppendText(htmlElementAllowingContent.EndTag);
        }
    }

    static void AppendAttributes<TSink>(TSink sink, HtmlAttributes attributes)
        where TSink : IStringSink
    {
        var hasClassName = false;
        foreach (var htmlAttribute in attributes) {
            if (htmlAttribute.Name == "class") {
                hasClassName = true;
            } else {
                AppendAttribute(sink, htmlAttribute);
            }
        }
        if (hasClassName) {
            if (sink is PipeSink pipeSink0) {
                pipeSink0.AppendUtf8(" class=\""u8);
            } else {
                sink.AppendText(" class=\"");
            }
            var subsequent = false;
            foreach (var htmlAttribute in attributes) {
                if (htmlAttribute.Name == "class") {
                    if (subsequent) {
                        if (sink is PipeSink pipeSink1) {
                            pipeSink1.AppendUtf8(" "u8);
                        } else {
                            sink.AppendText(" ");
                        }
                    } else {
                        subsequent = true;
                    }
                    AppendEscapedAttributeValue(sink, htmlAttribute.Value);
                }
            }
            if (sink is PipeSink pipeSink2) {
                pipeSink2.AppendUtf8("\""u8);
            } else {
                sink.AppendText("\"");
            }
        }
    }

    static void AppendAttribute<TSink>(TSink sink, HtmlAttribute htmlAttribute)
        where TSink : IStringSink
    {
        if (sink is PipeSink pipeSink0) {
            pipeSink0.AppendUtf8(" "u8);
        } else {
            sink.AppendText(" ");
        }
        sink.AppendText(htmlAttribute.Name);
        if (htmlAttribute.Value != "") {
            if (sink is PipeSink pipeSink1) {
                pipeSink1.AppendUtf8("=\""u8);
            } else {
                sink.AppendText("=\"");
            }
            AppendEscapedAttributeValue(sink, htmlAttribute.Value);
            if (sink is PipeSink pipeSink2) {
                pipeSink2.AppendUtf8("\""u8);
            } else {
                sink.AppendText("\"");
            }
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
