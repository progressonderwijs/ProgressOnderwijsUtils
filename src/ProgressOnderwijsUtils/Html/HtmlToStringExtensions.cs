using System;
using System.IO;
using System.Text;

namespace ProgressOnderwijsUtils.Html
{
    public static class HtmlToStringExtensions
    {
        public static string SerializeToString(this HtmlFragment rootElem)
        {
            var fastStringBuilder = FastShortStringBuilder.Create(1u << 16);
            fastStringBuilder.AppendText("<!DOCTYPE html>");
            rootElem.AppendToBuilder(ref fastStringBuilder);
            return fastStringBuilder.Value;
        }

        public static void SaveHtmlFragmentToStream(HtmlFragment rootElem, Stream outputStream, Encoding contentEncoding)
        {
            var fastStringBuilder = FastShortStringBuilder.Create(1u << 16);
            fastStringBuilder.AppendText("<!DOCTYPE html>");
            rootElem.AppendToBuilder(ref fastStringBuilder);
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
    }
}
