using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProgressOnderwijsUtils
{
	//TODO:make immutable.
	public struct FileData : IEquatable<FileData>, IMetaObject
	{
		private const int MAX_FILE_NAME = 64;

		private string fileName;

		public string ContentType { get; set; }
		public string FileName
		{
			get { return fileName; }
			set
			{
				if (value != null && value.Length > MAX_FILE_NAME)
				{
					if (Path.HasExtension(value))
					{
						fileName = string.Format("{0}{1}",
							Path.GetFileNameWithoutExtension(value).Substring(0, MAX_FILE_NAME - Path.GetExtension(value).Length),
							Path.GetExtension(value));
					}
					else
					{
						fileName = value.Substring(0, MAX_FILE_NAME);	
					}
				}
				else
				{
					fileName = value;
				}
			}
		}
		public byte[] Content { get; set; }

		public bool ContainsFile { get { return Content != null && FileName != null && (FileName.Length > 0 || Content.Length > 0); } }
		public override string ToString() { return ToUiString(); }
		public string ToUiString() { return ContainsFile ? string.Format("{0} ({1} KB)", FileName, Content.Length / 1000m) : ""; }

		public override bool Equals(object other) { return other is FileData && Equals((FileData)other); }

		public override int GetHashCode()
		{
			unchecked
			{
				int result = Content == null || Content.Length < 1 ? 0
					: Content[0] + (Content[Content.Length / 3] << 8) + (Content[Content.Length * 2 / 3] << 16) + (Content[Content.Length - 1] << 24) + Content.Length;
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

		bool ContentEqual(FileData other) { return Content == other.Content || (Content != null && other.Content != null && Content.SequenceEqual(other.Content)); }

		public static bool operator ==(FileData left, FileData right) { return left.Equals(right); }

		public static bool operator !=(FileData left, FileData right) { return !left.Equals(right); }
	}
}