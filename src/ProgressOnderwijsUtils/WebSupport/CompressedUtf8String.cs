using System.IO;
using System.IO.Compression;
using System.Text;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.WebSupport
{
    public sealed class CompressedUtf8String
    {
        [NotNull]
        static byte[] ReadFully([NotNull] Stream stream)
        {
            using (var ms = new MemoryStream()) {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        [NotNull]
        public string StringData
        {
            get {
                using (var gzipStream = new GZipStream(new MemoryStream(GzippedUtf8String), CompressionMode.Decompress, false)) {
                    return Encoding.UTF8.GetString(ReadFully(gzipStream));
                }
            }
        }

        public readonly byte[] GzippedUtf8String;

        [CodeThatsOnlyUsedForTests]
        public CompressedUtf8String(byte[] compressedData)
        {
            GzippedUtf8String = compressedData;
        }

        public CompressedUtf8String([NotNull] string stringData)
        {
            using (var compressedData = new MemoryStream()) {
                var encodedData = Encoding.UTF8.GetBytes(stringData);
                using (var gzipStream = new GZipStream(compressedData, CompressionMode.Compress)) {
                    gzipStream.Write(encodedData, 0, encodedData.Length);
                }

                GzippedUtf8String = compressedData.ToArray();
            }
        }
    }
}
