using System.IO;
using System.Linq;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Reads the specified number of bytes.
        /// </summary>
        /// <exception cref="EndOfStreamException">thrown when EOF is reached before the needed number of bytes are read</exception>
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

    public class StreamExtensionsTestClass
    {
        [Fact]
        public void ChecksEOF()
        {
            using (var stream = new MemoryStream(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray())) {
                Assert.Throws<EndOfStreamException>(() => stream.ReadUntil(257));
                stream.Seek(0, SeekOrigin.Begin);
                PAssert.That(() => stream.ReadUntil(256).SequenceEqual(Enumerable.Range(0, 256).Select(i => (byte)i)));
            }
        }
    }
}
