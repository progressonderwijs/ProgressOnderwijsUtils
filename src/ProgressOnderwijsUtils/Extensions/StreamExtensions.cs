using System.IO;

namespace ProgressOnderwijsUtils.Extensions
{
	public static class StreamExtensions
	{
		/// <summary>
		/// Reads the specified number of bytes.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="size">the number of bytes to read</param>
		/// <returns>the bytes read, its length equals the specified size</returns>
		/// <exception cref="EndOfStreamException">thrown when EOF is reached before the needed number of bytes are read</exception>
		public static byte[] ReadUntil(this Stream stream, int size)
		{
			byte[] result = new byte[size];
			int offset = 0;
			int count = result.Length;
			while (count > 0)
			{
				int n = stream.Read(result, offset, count);
				if (n == 0)
					throw new EndOfStreamException();
				offset += n;
				count -= n;
			}
			return result;
		}
	}
}
