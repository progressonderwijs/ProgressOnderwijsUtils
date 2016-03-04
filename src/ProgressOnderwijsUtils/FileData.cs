using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.Serialization.Formatters.Binary;

namespace ProgressOnderwijsUtils
{
    [Serializable]
    public struct FileData : IEquatable<FileData>
    {
        const int MAX_FILE_NAME = 64;
        public readonly byte[] Content;
        public readonly string MimeType;
        public readonly string FileName;

        public FileData(string fileName, byte[] content, string mimeType)
        {
            Content = content;
            MimeType = mimeType;
            FileName = TrimNameToLength(fileName, MAX_FILE_NAME);
        }

        public static string TrimNameToLength(string filePath, int maxFileNameLength)
        {
            var filename = Path.GetFileName(filePath);
            if (filename == null) {
                return null;
            }
            if (Path.HasExtension(filename)) {
                var extension = Path.GetExtension(filename);
                var basename = Path.GetFileNameWithoutExtension(filename);
                if (extension.Length + 8 > maxFileNameLength) {
                    //don't keep extension if it crowds out the name.
                    return basename.TrimToLength(maxFileNameLength).Replace('.', '_');
                } else {
                    var fileNameWithoutExtension = basename.TrimToLength(maxFileNameLength - extension.Length);
                    return fileNameWithoutExtension + extension;
                }
            }
            return filename.TrimToLength(maxFileNameLength);
        }

        public bool ContainsFile() => Content != null && FileName != null && (FileName.Length > 0 || Content.Length > 0);
        public override string ToString() => ContainsFile() ? $"{FileName} ({Content.Length / 1000m} KB)" : "";
        public override bool Equals(object other) => other is FileData && Equals((FileData)other);

        public override int GetHashCode()
        {
            unchecked {
                int result = Content == null || Content.Length < 1
                    ? 0
                    : Content[0] +
                        (Content[Content.Length / 3] << 8) +
                        (Content[Content.Length * 2 / 3] << 16) +
                        (Content[Content.Length - 1] << 24) +
                        Content.Length;
                result = (result * 397) ^ (MimeType != null ? MimeType.GetHashCode() : 0);
                result = (result * 397) ^ (FileName != null ? FileName.GetHashCode() : 0);
                return result;
            }
        }

        public bool Equals(FileData other)
        {
            return Equals(other.MimeType, MimeType) &&
                Equals(other.FileName, FileName) &&
                ContentEqual(other);
        }

        bool ContentEqual(FileData other)
        {
            return Content == other.Content ||
                (Content != null && other.Content != null && Content.SequenceEqual(other.Content));
        }

        public static bool operator ==(FileData left, FileData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FileData left, FileData right)
        {
            return !left.Equals(right);
        }

        public static FileData Serialize<T>(T obj, string fileName = null)
        {
            using (var stream = new MemoryStream()) {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                return new FileData(fileName ?? GetFileName<T>(), stream.ToArray(), MediaTypeNames.Application.Octet);
            }
        }

        public static T Deserialize<T>(FileData file)
        {
            if (file.ContainsFile()) {
                using (var stream = new MemoryStream(file.Content)) {
                    var formatter = new BinaryFormatter();
                    return (T)formatter.Deserialize(stream);
                }
            } else {
                return default(T);
            }
        }

        static string GetFileName<T>()
        {
            Type type = typeof(T);
            string result = type.Name;
            result = type.IsGenericType ? result.Substring(0, result.IndexOf("`", StringComparison.InvariantCulture)) : result;
            result += ".bin";
            return result;
        }
    }
}
