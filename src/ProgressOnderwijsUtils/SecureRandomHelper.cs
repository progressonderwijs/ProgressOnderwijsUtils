using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using ExpressionToCodeLib;
using MoreLinq;
using NUnit.Framework;

namespace ProgressOnderwijsUtils
{
    public sealed class SecureRandomHelper : RandomHelper
    {
        public static readonly RandomHelper Instance = new SecureRandomHelper();

        readonly RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(); //threadsafe

        SecureRandomHelper() { }

        protected override byte[] GetBytes(int numBytes)
        {
            var bytes = new byte[numBytes];
            rng.GetBytes(bytes);
            return bytes;
        }
    }

    sealed class SecureRandomHelperTest
    {
        [Test]
        public void CheckRandomBasic()
        {
            var numTo37 = new HashSet<uint>(Enumerable.Range(0, 37).Select(i => (uint)i));
            PAssert.That(() => MoreEnumerable.GenerateByIndex(i => SecureRandomHelper.Instance.GetUInt32()).Take(10000).Any(num => num > int.MaxValue));
            PAssert.That(() => MoreEnumerable.GenerateByIndex(i => SecureRandomHelper.Instance.GetInt64()).Take(10000).Any(num => num > uint.MaxValue));
            PAssert.That(() => MoreEnumerable.GenerateByIndex(i => SecureRandomHelper.Instance.GetUInt64()).Take(10000).Any(num => num > long.MaxValue));
            PAssert.That(() => numTo37.SetEquals(MoreEnumerable.GenerateByIndex(i => SecureRandomHelper.Instance.GetUInt32(37)).Take(10000))); //kans op fout ~= 37 * (1-1/37)^10000  < 10^-117
        }

        [Test]
        public void CheckString()
        {
            for (var i = 0; i < 50; i++) {
                var len = (int)SecureRandomHelper.Instance.GetUInt32(300);
                var str = SecureRandomHelper.Instance.GetStringOfLatinLower(len);
                var StR = SecureRandomHelper.Instance.GetStringOfLatinUpperOrLower(len);
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
                var str = SecureRandomHelper.Instance.GetStringOfUriPrintableCharacters(i);
                var escaped = Uri.EscapeDataString(str);
                PAssert.That(() => str == escaped);
                PAssert.That(() => str.Length == i);
                PAssert.That(() => Regex.IsMatch(str, "^[-_~0-9A-Za-z]*$"));
            }
        }

        [Test]
        public void CheckStrings()
        {
            PAssert.That(() => Regex.IsMatch(SecureRandomHelper.Instance.GetStringOfNumbers(10), "[0-9]{10}"));
            PAssert.That(() => Regex.IsMatch(SecureRandomHelper.Instance.GetStringCapitalized(10), "[A-Z][a-z]{9}"));
            PAssert.That(() => Regex.IsMatch(SecureRandomHelper.Instance.GetStringOfLatinLower(7), "[a-z]{7}"));
            PAssert.That(() => Regex.IsMatch(SecureRandomHelper.Instance.GetStringOfLatinUpperOrLower(10), "[a-zA-Z]{10}"));
        }
    }
}
