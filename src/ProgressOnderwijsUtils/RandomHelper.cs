using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using MoreLinq;
using NUnit.Framework;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtils
{
    public static class RandomHelper
    {
        static readonly RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(); //threadsafe

        static byte[] GetBytes(int numBytes)
        {
            var bytes = new byte[numBytes];
            rng.GetBytes(bytes);
            return bytes;
        }

        public static byte GetByte() { return GetBytes(1)[0]; }
        public static int GetInt32() { return BitConverter.ToInt32(GetBytes(sizeof(int)), 0); }
        public static long GetInt64() { return BitConverter.ToInt64(GetBytes(sizeof(long)), 0); }

        [CLSCompliant(false)]
        public static uint GetUInt32() { return BitConverter.ToUInt32(GetBytes(sizeof(uint)), 0); }

        [CLSCompliant(false)]
        public static ulong GetUInt64() { return BitConverter.ToUInt64(GetBytes(sizeof(ulong)), 0); }

        [CLSCompliant(false)]
        public static uint GetUInt32(uint bound)
        {
            uint modErr = (uint.MaxValue % bound + 1) % bound;
            uint safeIncBound = uint.MaxValue - modErr;

            while (true) {
                uint val = GetUInt32();
                if (val <= safeIncBound) {
                    return val % bound;
                }
            }
        }

        [CLSCompliant(false)]
        public static ulong GetUInt64(ulong bound)
        {
            ulong modErr = (ulong.MaxValue % bound + 1) % bound;
            ulong safeIncBound = ulong.MaxValue - modErr;

            while (true) {
                ulong val = GetUInt64();
                if (val <= safeIncBound) {
                    return val % bound;
                }
            }
        }

        public static string GetStringOfLatinLower(int length) { return GetString(length, 'a', 'z'); }
        public static string GetStringCapitalized(int length) { return GetString(1, 'A', 'Z') + GetString(length - 1, 'a', 'z'); }
        public static string GetStringOfNumbers(int length) { return GetString(1, '1', '9') + GetString(length - 1, '0', '9'); }
        public static string GetStringOfLong2Hex() { return GetHexLong(1000000000000, 1000000000000000000, new Random()).ToString("X"); }

        static long GetHexLong(long min, long max, Random rand)
        {
            var buf = new byte[8];
            rand.NextBytes(buf);
            var longRand = BitConverter.ToInt64(buf, 0);
            return (Math.Abs(longRand % (max - min)) + min);
        }

        public static string GetString(int length, char min, char max)
        {
            var letters = (uint)max - min + 1;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++) {
                sb.Append((char)(GetUInt32(letters) + min));
            }
            return sb.ToString();
        }
    }

    [Continuous]
    sealed class RndTest
    {
        [Test]
        public void CheckRandomBasic()
        {
            HashSet<uint> numTo37 = new HashSet<uint>(Enumerable.Range(0, 37).Select(i => (uint)i));
            Assert.IsTrue(MoreEnumerable.GenerateByIndex(i => RandomHelper.GetUInt32()).Take(10000).Any(num => num > Int32.MaxValue));
            Assert.IsTrue(MoreEnumerable.GenerateByIndex(i => RandomHelper.GetInt64()).Take(10000).Any(num => num > UInt32.MaxValue));
            Assert.IsTrue(MoreEnumerable.GenerateByIndex(i => RandomHelper.GetUInt64()).Take(10000).Any(num => num > Int64.MaxValue));
            Assert.IsTrue(numTo37.SetEquals(MoreEnumerable.GenerateByIndex(i => RandomHelper.GetUInt32(37)).Take(10000))); //kans op fout ~= 37 * (1-1/37)^10000  < 10^-117
        }

        [Test]
        public void CheckString()
        {
            for (int i = 0; i < 50; i++) {
                int len = (int)RandomHelper.GetUInt32(300);
                string str = RandomHelper.GetStringOfLatinLower(len);
                Assert.That(str.Length == len);
                Assert.IsFalse(str.AsEnumerable().Any(c => c < 'a' || c > 'z'));
            }
        }

        [Test]
        public void CheckStrings()
        {
            Assert.That(RandomHelper.GetStringOfNumbers(10), Is.StringMatching("[0-9]{10}"));
            Assert.That(RandomHelper.GetStringCapitalized(10), Is.StringMatching("[A-Z][a-z]{9}"));
            Assert.That(RandomHelper.GetStringOfLatinLower(7), Is.StringMatching("[a-z]{7}"));
            //Assert.That(RandomHelper. (10), Is.StringMatching("[0-9]{10}"));
        }

        [Test]
        public void CheckHex()
        {
            Assert.That(RandomHelper.GetStringOfLong2Hex(), Is.StringMatching("[0-9A-E]"));
        }
    }
}
