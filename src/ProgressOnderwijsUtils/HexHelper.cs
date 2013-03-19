using System;
using System.Linq;
using System.Text;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
	public static class HexHelper
	{

		/// <summary>
		/// Very fast hex-to-bytearray conversion.  Verifies whether all characters are legal [0-9a-fA-F] and throws if this is not so.
		/// </summary>
		/// <param name="hex">a hexadecimal string without spaces, dashes, and without a '0x' prefix.</param>
		/// <returns>the byte array it represents.</returns>
		public static byte[] HexToBytes(string hex)
		{
			if (hex.Length % 2 != 0) throw new ArgumentException("Cannot convert odd number of hex digits to byte array");
			var arr = new byte[hex.Length / 2];
			for (var i = 0; i < arr.Length; i++)
			{

				int b;
				try
				{
					b = (byteLookup[hex[2 * i]] << 4) | byteLookup[hex[2 * i + 1]];
				}
				catch (IndexOutOfRangeException)
				{
					if (hex[2 * i] >= byteLookup.Length)
						throw badCharExn(2 * i, hex[2 * i]);
					else
						throw badCharExn(2 * i + 1, hex[2 * i + 1]);
				}

				if (b < 0)
				{
					if (byteLookup[hex[2 * i]] < 0)
						throw badCharExn(2 * i, hex[2 * i]);
					else
						throw badCharExn(2 * i + 1, hex[2 * i + 1]);
				}
				arr[i] = (byte)b;
			}
			return arr;
		}
		static Exception badCharExn(int index, char c)
		{
			throw new InvalidOperationException("Invalid character '" + c + "' (" + ((int)c).ToString("x") + ") at index " + index);
		}


		public static string BytesToHex(byte[] arr)
		{
			var sb = new StringBuilder(arr.Length * 2);
			foreach (var b in arr)
			{
				sb.Append(charLookup[b >> 4]);
				sb.Append(charLookup[b & 0xf]);
			}
			return sb.ToString();
		}

		public static readonly ArrayComparer<byte> ByteArrayComparer = ArrayComparer<byte>.Default;

		static readonly char[] charLookup = Enumerable.Range('0', 10).Concat(Enumerable.Range('a', 6)).Select(i => (char)i).ToArray();

		static readonly int[] byteLookup = Enumerable.Range(0, 'f' + 1)
			.Select(c => '0' <= c && c <= '9' ? c - '0' : 'A' <= c && c <= 'F' ? c - 'A' + 10 : 'a' <= c && c <= 'f' ? c - 'a' + 10 : -1)
			.ToArray();
	}
}