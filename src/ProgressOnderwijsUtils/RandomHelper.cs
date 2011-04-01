using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using MoreLinq;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace ProgressOnderwijsUtils
{
	public static class RandomHelper
	{
		static readonly RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();//threadsafe
		private static byte[] GetBytes(int numBytes)
		{
			var bytes = new byte[numBytes];
			rng.GetBytes(bytes);
			return bytes;
		}

		public static byte GetByte() { return GetBytes(1)[0]; }
		public static Int32 GetInt32() { return BitConverter.ToInt32(GetBytes(sizeof(Int32)), 0); }
		public static Int64 GetInt64() { return BitConverter.ToInt64(GetBytes(sizeof(Int64)), 0); }
		[CLSCompliant(false)]
		public static UInt32 GetUInt32() { return BitConverter.ToUInt32(GetBytes(sizeof(UInt32)), 0); }
		[CLSCompliant(false)]
		public static UInt64 GetUInt64() { return BitConverter.ToUInt64(GetBytes(sizeof(UInt64)), 0); }


		[CLSCompliant(false)]
		public static UInt32 GetUInt32(UInt32 bound)
		{
			UInt32 modErr = (UInt32.MaxValue % bound + 1) % bound;
			UInt32 safeIncBound = UInt32.MaxValue - modErr;

			while (true)
			{
				UInt32 val = GetUInt32();
				if (val <= safeIncBound)
					return (UInt32)(val % bound);
			}
		}


		[CLSCompliant(false)]
		public static UInt64 GetUInt64(UInt64 bound)
		{
			UInt64 modErr = (UInt64.MaxValue % bound + 1) % bound;
			UInt64 safeIncBound = UInt64.MaxValue - modErr;

			while (true)
			{
				UInt64 val = GetUInt64();
				if (val <= safeIncBound)
					return (UInt64)(val % bound);
			}
		}

		public static string GetStringOfLatinLower(int length) { return GetString(length, 'a', 'z'); }
		public static string GetStringCapitalized(int length) { return GetString(1, 'A', 'Z') + GetString(length-1, 'a', 'z'); }

		public static string GetStringOfNumbers(int length) { return GetString(length, '0', '9'); }

		public static string GetString(int length, char min, char max)
		{
			uint letters = (uint)max - (uint)min + 1;
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < length; i++)
				sb.Append((char)(GetUInt32(letters) + (uint)min));
			return sb.ToString();
		}


		[TestFixture]
		class RndTest
		{
			[Test]
			public void CheckRandomBasic()
			{
				HashSet<uint> numTo37 = new HashSet<uint>(Enumerable.Range(0, 37).Select(i => (uint)i));
				Assert.IsTrue(MoreEnumerable.GenerateByIndex(i => GetUInt32()).Take(10000).Where(num => num > int.MaxValue).Any());
				Assert.IsTrue(MoreEnumerable.GenerateByIndex(i => GetInt64()).Take(10000).Where(num => num > uint.MaxValue).Any());
				Assert.IsTrue(MoreEnumerable.GenerateByIndex(i => GetUInt64()).Take(10000).Where(num => num > Int64.MaxValue).Any());
				Assert.IsTrue(numTo37.SetEquals(MoreEnumerable.GenerateByIndex(i => GetUInt32(37)).Take(10000))); //kans op fout ~= 37 * (1-1/37)^10000  < 10^-117
			}

			[Test]
			public void CheckString()
			{
				for (int i = 0; i < 50; i++)
				{
					int len = (int)RandomHelper.GetUInt32(300);
					string str = GetStringOfLatinLower(len);
					Assert.That(str.Length == len);
					Assert.IsFalse(str.AsEnumerable().Any(c => c < 'a' || c > 'z'));
				}
			}

			[Test]
			public void CheckStrings()
			{
				Assert.That(RandomHelper.GetStringOfNumbers(10), Is.StringMatching("[0-9]{10}"));
				Assert.That(RandomHelper.GetStringCapitalized(10), Is.StringMatching("[A-Z][a-z]{9}"));
				Assert.That(RandomHelper.GetStringOfLatinLower(7) , Is.StringMatching("[a-z]{7}"));
				//Assert.That(RandomHelper. (10), Is.StringMatching("[0-9]{10}"));
			}
		}
	}
}
