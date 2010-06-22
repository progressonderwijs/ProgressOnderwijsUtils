using System;
using System.Linq;

namespace ProgressOnderwijsUtils
{
	public struct FileData : IEquatable<FileData>
	{
		public byte[] Content { get; set; }
		public string ContentType { get; set; }
		public string FileName { get; set; }

		public bool ContainsFile { get { return Content != null && FileName != null && (FileName.Length > 0 || Content.Length > 0); } }
		public override string ToString() { return ReadableString; }

		public string ReadableString { get { return string.Format("{0} ({1} KB)", FileName, Content.Length / 1000m); } }


		public override bool Equals(object other)
		{
			bool result = false;
			if (other != null && other.GetType() == typeof(FileData))
			{
				result = Equals((FileData)other);
			}
			return result;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (Content != null ? Content.GetHashCode() : 0);
				result = (result * 397) ^ (ContentType != null ? ContentType.GetHashCode() : 0);
				result = (result * 397) ^ (FileName != null ? FileName.GetHashCode() : 0);
				return result;
			}
		}

		public bool Equals(FileData other)
		{
			return
				   Equals(other.ContentType, ContentType) &&
				   Equals(other.FileName, FileName)
				   && ContentEqual(other);
		}

		private bool ContentEqual(FileData other)
		{
			return Content == other.Content || (Content != null && other.Content != null && Content.SequenceEqual(other.Content));
		}

		public static bool operator ==(FileData left, FileData right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(FileData left, FileData right)
		{
			return !left.Equals(right);
		}
	}
}
