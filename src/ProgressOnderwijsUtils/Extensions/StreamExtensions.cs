#nullable disable
using System.IO;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Reads the specified number of bytes.
        /// </summary>
        /// <exception cref="EndOfStreamException">thrown when EOF is reached before the needed number of bytes are read</exception>
        [NotNull]
        public static byte[] ReadUntil(this Stream stream, int numberOfBytesToRead)
        {
            var result = new byte[numberOfBytesToRead];
            var offset = 0;
            while (offset < result.Length) {
                var n = stream.Read(result, offset, result.Length - offset);
                if (n == 0) {
                    throw new EndOfStreamException();
                }
                offset += n;
            }
            return result;
        }
    }
}
