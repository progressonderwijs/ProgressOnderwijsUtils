using System;
using System.Linq;
using System.Text;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
	public static class HexHelper
	{
		public static byte[] HexToBytes(string hex)
		{
			if (hex.Length % 2 != 0) throw new ArgumentException("Cannot convert odd number of hex digits to byte array");
			var arr = new byte[hex.Length / 2];
			for (var i = 0; i < arr.Length; i++)
			{
				var b = (byteLookup[hex[2 * i]] << 4) | byteLookup[hex[2 * i + 1]];
				if (b < 0)
				{
					var bad = byteLookup[hex[2 * i]] < 0 ? hex[2 * i] : hex[2 * i + 1];
					throw new InvalidOperationException("Invalid character '" + bad + "' (" + ((int)bad).ToString("x") + ")");
				}
				arr[i] = (byte)b;
			}
			return arr;
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