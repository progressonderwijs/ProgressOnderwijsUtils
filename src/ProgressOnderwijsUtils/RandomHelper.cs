using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ExpressionToCodeLib;
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

        [UsefulToKeep("library method")]
        public static byte GetByte() => GetBytes(1)[0];

        [UsefulToKeep("library method")]
        public static int GetNonNegativeInt32() => (int)GetUInt32((uint)int.MaxValue + 1);

        public static int GetInt32() => BitConverter.ToInt32(GetBytes(sizeof(int)), 0);

        [UsefulToKeep("library method")]
        public static long GetInt64() => BitConverter.ToInt64(GetBytes(sizeof(long)), 0);

        [CLSCompliant(false), UsefulToKeep("library method")]
        public static uint GetUInt32()
        {
            return BitConverter.ToUInt32(GetBytes(sizeof(uint)), 0);
        }

        [CLSCompliant(false), UsefulToKeep("library method")]
        public static ulong GetUInt64()
        {
            return BitConverter.ToUInt64(GetBytes(sizeof(ulong)), 0);
        }

        [CLSCompliant(false), UsefulToKeep("library method")]
        public static uint GetUInt32(uint excludedBound)
        {
            // Proved in: http://www.google.com/url?q=http%3A%2F%2Fstackoverflow.com%2Fquestions%2F11758809%2Fwhat-is-the-optimal-algorithm-for-generating-an-unbiased-random-integer-within-a&sa=D&sntz=1&usg=AFQjCNEtQkf0HYEkTn6Npvmyu2TDKPQCxA
            uint modErr = (uint.MaxValue % excludedBound + 1) % excludedBound;
            uint safeIncBound = uint.MaxValue - modErr;

            while (true) {
                uint val = GetUInt32();
                if (val <= safeIncBound) {
                    return val % excludedBound;
                }
            }
        }

        [CLSCompliant(false), UsefulToKeep("library method")]
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

        public static string GetStringOfLatinLower(int length) => GetString(length, 'a', 'z');
        public static string GetStringCapitalized(int length) => GetString(1, 'A', 'Z') + GetString(length - 1, 'a', 'z');
        public static string GetStringOfLatinUpperOrLower(int length) => GetStringUpperAndLower(length, 'a', 'z');
        public static string GetStringOfNumbers(int length) => GetString(1, '1', '9') + GetString(length - 1, '0', '9');

        public static string GetString(int length, char min, char max)
        {
            var letters = (uint)max - min + 1;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++) {
                sb.Append((char)(GetUInt32(letters) + min));
            }
            return sb.ToString();
        }

        public static string GetStringUpperAndLower(int length, char min, char max)
        {
            var letters = (uint)max - min + 1;
            var MIN = char.ToUpper(min);
            var sb = new StringBuilder();
            for (var i = 0; i < length; i++) {
                sb.Append((char)(GetUInt32(letters) + (GetUInt32(100) < 50 ? min : MIN)));
            }
            return sb.ToString();
        }

        static readonly char[] UriPrintableCharacters =
            Enumerable.Range('A', 26).Concat(Enumerable.Range('a', 26)).Concat(Enumerable.Range('0', 10)).Select(i => (char)i).Concat("_-~").ToArray();

        public static string GetStringOfUriPrintableCharacters(int length)
        {
            return new string(Enumerable.Range(0, length).Select(_ => UriPrintableCharacters[GetUInt32((uint)UriPrintableCharacters.Length)]).ToArray());
        }

        public static string GetPasswordString(int length)
        {
            return System.Web.Security.Membership.GeneratePassword(length, 0);
        }
    }

    sealed class RndTest
    {
        [Test]
        public void CheckRandomBasic()
        {
            var numTo37 = new HashSet<uint>(Enumerable.Range(0, 37).Select(i => (uint)i));
            PAssert.That(() => MoreEnumerable.GenerateByIndex(i => RandomHelper.GetUInt32()).Take(10000).Any(num => num > int.MaxValue));
            PAssert.That(() => MoreEnumerable.GenerateByIndex(i => RandomHelper.GetInt64()).Take(10000).Any(num => num > uint.MaxValue));
            PAssert.That(() => MoreEnumerable.GenerateByIndex(i => RandomHelper.GetUInt64()).Take(10000).Any(num => num > long.MaxValue));
            PAssert.That(() => numTo37.SetEquals(MoreEnumerable.GenerateByIndex(i => RandomHelper.GetUInt32(37)).Take(10000))); //kans op fout ~= 37 * (1-1/37)^10000  < 10^-117
        }

        [Test]
        public void CheckString()
        {
            for (var i = 0; i < 50; i++) {
                var len = (int)RandomHelper.GetUInt32(300);
                var str = RandomHelper.GetStringOfLatinLower(len);
                var StR = RandomHelper.GetStringOfLatinUpperOrLower(len);
                Assert.That(str.Length == len);
                Assert.That(StR.Length == len);
                PAssert.That(() => !str.AsEnumerable().Any(c => c < 'a' || c > 'z'));
                PAssert.That(() => !StR.AsEnumerable().Any(c => (c < 'a' || c > 'z') && (c < 'A' || c > 'Z')));
            }
        }

        [Test]
        public void CheckUriPrintable()
        {
            for (var i = 0; i < 50; i++) {
                var str = RandomHelper.GetStringOfUriPrintableCharacters(i);
                var escaped = Uri.EscapeDataString(str);
                PAssert.That(() => str == escaped);
                PAssert.That(() => str.Length == i);
                PAssert.That(() => Regex.IsMatch(str, "^[-_~0-9A-Za-z]*$"));
            }
        }

        [Test]
        public void CheckStrings()
        {
            PAssert.That(() => Regex.IsMatch(RandomHelper.GetStringOfNumbers(10), "[0-9]{10}"));
            PAssert.That(() => Regex.IsMatch(RandomHelper.GetStringCapitalized(10), "[A-Z][a-z]{9}"));
            PAssert.That(() => Regex.IsMatch(RandomHelper.GetStringOfLatinLower(7), "[a-z]{7}"));
            PAssert.That(() => Regex.IsMatch(RandomHelper.GetStringOfLatinUpperOrLower(10), "[a-zA-Z]{10}"));
        }
    }
}
