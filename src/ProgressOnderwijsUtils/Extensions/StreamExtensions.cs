using System.IO;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtils
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Reads the specified number of bytes.
        /// </summary>
        /// <param name="size">the number of bytes to read</param>
        /// <returns>the bytes read, its length equals the specified size</returns>
        /// <exception cref="EndOfStreamException">thrown when EOF is reached before the needed number of bytes are read</exception>
        public static byte[] ReadUntil(this Stream stream, int size)
        {
            byte[] result = new byte[size];
            int offset = 0;
            while (offset < result.Length) {
                int n = stream.Read(result, offset, result.Length - offset);
                if (n == 0) {
                    throw new EndOfStreamException();
                }
                offset += n;
            }
            return result;
        }
    }

    [Continuous]
    public class StreamExtensionsTestClass
    {
        [Test]
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
