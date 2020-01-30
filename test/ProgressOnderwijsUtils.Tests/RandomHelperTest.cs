using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ExpressionToCodeLib;
using MoreLinq;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class RandomHelperTest
    {
        [Fact]
        public void CheckRandomBasic()
        {
            var numTo37 = new HashSet<uint>(Enumerable.Range(0, 37).Select(i => (uint)i));
            PAssert.That(() => MoreEnumerable.GenerateByIndex(i => RandomHelper.Secure.GetUInt32()).Take(10000).Any(num => num > int.MaxValue));
            PAssert.That(() => MoreEnumerable.GenerateByIndex(i => RandomHelper.Secure.GetInt64()).Take(10000).Any(num => num > uint.MaxValue));
            PAssert.That(() => MoreEnumerable.GenerateByIndex(i => RandomHelper.Secure.GetUInt64()).Take(10000).Any(num => num > long.MaxValue));
            PAssert.That(() => numTo37.SetEquals(MoreEnumerable.GenerateByIndex(i => RandomHelper.Secure.GetUInt32(37)).Take(10000))); //kans op fout ~= 37 * (1-1/37)^10000  < 10^-117
        }

        [Fact]
        public void CheckString()
        {
            for (var i = 0; i < 50; i++) {
                var len = (int)RandomHelper.Secure.GetUInt32(300);
                var str = RandomHelper.Secure.GetStringOfLatinLower(len);
                var StR = RandomHelper.Secure.GetStringOfLatinUpperOrLower(len);
                PAssert.That(() => str.Length == len);
                PAssert.That(() => StR.Length == len);
                PAssert.That(() => !str.AsEnumerable().Any(c => c < 'a' || c > 'z'));
                PAssert.That(() => !StR.AsEnumerable().Any(c => (c < 'a' || c > 'z') && (c < 'A' || c > 'Z')));
            }
        }

        [Fact]
        public void CheckUriPrintable()
        {
            for (var i = 0; i < 50; i++) {
                var str = RandomHelper.Secure.GetStringOfUriPrintableCharacters(i);
                var escaped = Uri.EscapeDataString(str);
                PAssert.That(() => str == escaped);
                PAssert.That(() => str.Length == i);
                PAssert.That(() => Regex.IsMatch(str, "^[-_~0-9A-Za-z]*$"));
            }
        }

        [Fact]
        public void CheckStrings()
        {
            PAssert.That(() => Regex.IsMatch(RandomHelper.Secure.GetStringOfNumbers(10), "[0-9]{10}"));
            PAssert.That(() => Regex.IsMatch(RandomHelper.Secure.GetStringCapitalized(10), "[A-Z][a-z]{9}"));
            PAssert.That(() => Regex.IsMatch(RandomHelper.Secure.GetStringOfLatinLower(7), "[a-z]{7}"));
            PAssert.That(() => Regex.IsMatch(RandomHelper.Secure.GetStringOfLatinUpperOrLower(10), "[a-zA-Z]{10}"));
        }

        [Fact]
        public void ImplicitlyInsecure_Gedrag_hangt_af_van_regelnummer()
        {
            var randomHelper1 = RandomHelper.ImplicitlyInsecure();
            var randomHelper2 = RandomHelper.ImplicitlyInsecure();
            PAssert.That(() => randomHelper1.GetInt32() != randomHelper2.GetInt32());
        }

        [Fact]
        public void ImplicitlyInsecure_Gedrag_is_deterministisch()
        {
            var randomHelper = RandomHelper.ImplicitlyInsecure();
            var pseudoRandomInteger = randomHelper.GetInt32();
            PAssert.That(() => pseudoRandomInteger == -20762718);
        }
    }
}
