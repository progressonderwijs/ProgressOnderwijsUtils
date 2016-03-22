using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business.Test;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    public class StringExtensionsTest
    {
        [Test]
        public void ToCamelCase_is_robuust_bij_null()
        {
            PAssert.That(() => ((string)null).ToCamelCase() == null);
        }

        [Test]
        public void ToCamelCase_werkt_ook_bij_lege_string()
        {
            PAssert.That(() => string.Empty.ToCamelCase() == string.Empty);
        }

        [Test]
        public void ToCamelCase_ADIS_ABEBA_wordt_Adis_Abeba()
        {
            PAssert.That(() => "ADIS ABEBA".ToCamelCase() == "Adis Abeba");
        }

        [Test]
        public void ToCamelCase_JAN_BENJAMIN_wordt_Jan_Benjamin()
        {
            PAssert.That(() => "JAN-BENJAMIN".ToCamelCase() == "Jan-Benjamin");
        }

        [Test]
        public void ToCamelCase_S_GRAVENHAGE_wordt_s_Gravenhage()
        {
            PAssert.That(() => "'S-GRAVENHAGE".ToCamelCase() == "'s-Gravenhage");
        }

        [TestCase("é", "e"), TestCase("Ü", "U"), TestCase("ß", "ß")]
        public void WithoutDiakriet(string from, string to)
        {
            Assert.That(StringUtils.VerwijderDiakrieten(@from), Is.EqualTo(to));
        }

        [Test]
        public void ReplaceRingelS()
        {
            Assert.That(StringUtils.VervangRingelS("ß", false), Is.EqualTo("ss"));
            Assert.That(StringUtils.VervangRingelS("ß", true), Is.EqualTo("SS"));
            Assert.That(StringUtils.VervangRingelS("aßb", false), Is.EqualTo("assb"));
            Assert.That(StringUtils.VervangRingelS("ßsß", true), Is.EqualTo("SSsSS"));
            Assert.That(StringUtils.VervangRingelS("", false), Is.EqualTo(""));
        }

        [Test, TestCase("", ""), TestCase("test", "test"), TestCase(" test ", "test"), TestCase("\ttest\t", "test"),
         TestCase("\ntest\n", "test"), TestCase(" \t\ntest\n\t ", "test"), TestCase("een test", "een test"), TestCase("een  test", "een test"),
         TestCase("een\ttest", "een test"), TestCase("een\t\ttest", "een test"), TestCase("een\ntest", "een test"), TestCase("een\n\ntest", "een test")]
        public void NormalizeWhitespace(string str, string expected)
        {
            Assert.That(str.NormalizeWhitespace(), Is.EqualTo(expected));
        }

        [Test, TestCase("", ""), TestCase("test", "test"), TestCase(" test ", " test "), TestCase("\ttest\t", " test "),
         TestCase("\ntest\n", " test "), TestCase(" \t\ntest\n\t ", " test "), TestCase("een test", "een test"), TestCase("een  test", "een test"),
         TestCase("een\ttest", "een test"), TestCase("een\t\ttest", "een test"), TestCase("een\ntest", "een test"), TestCase("een\n\ntest", "een test")]
        public void CollapseWhitespace(string str, string expected)
        {
            Assert.That(str.CollapseWhitespace(), Is.EqualTo(expected));
        }

        [Test, TestCase("", "", ExpectedResult = 0), TestCase("test", "tset", ExpectedResult = 2), TestCase(" test ", "\ttest\t", ExpectedResult = 2),
         TestCase("Ziggy Stardust", "ziggy stradust", ExpectedResult = 4), TestCase("a", "b", ExpectedResult = 1), TestCase("a", "", ExpectedResult = 1), TestCase("aba", "aa", ExpectedResult = 1),
         TestCase("simple", "Simpler", ExpectedResult = 2), TestCase("hmmm", "yummy", ExpectedResult = 3), TestCase("World-wide", "wordy", ExpectedResult = 7)]
        //"W"=>"w",drop "l", replace "-wide"
        public int TestLevenshtein(string str1, string str2) => StringUtils.LevenshteinDistance(str1, str2);

        [Test, TestCase("joepje jofel", ExpectedResult = "Joepje Jofel"), TestCase("carolien Kaasteen", ExpectedResult = "Carolien Kaasteen"),
         TestCase("maarten middelmaat--meloen", ExpectedResult = "Maarten Middelmaat-Meloen"), TestCase("carolien    Kaasteen", ExpectedResult = "Carolien Kaasteen"),
         TestCase("'s-gravenhage", ExpectedResult = "'s-Gravenhage"), TestCase("'s gravenhage", ExpectedResult = "'s Gravenhage"), TestCase("'sgravenhage", ExpectedResult = "'s Gravenhage"),
         TestCase("sieb op de kast", ExpectedResult = "Sieb op de Kast")]
        public string testNaam2Upper(string inp) => StringUtils.Name2UpperCasedName(inp);

        [Test]
        public void testLangeNaam2Upper()
        {
            Assert.That(StringUtils.Name2UpperCasedName("miep boezeroen-jansen van der sloot op 't gootje v.d. geest de la terrine du soupe au beurre à demi v/d zo-is-het-wel-genoeg ja"),
                Is.EqualTo("Miep Boezeroen-Jansen van der Sloot op 't Gootje v.d. Geest de la Terrine du Soupe au Beurre à Demi v/d Zo-Is-Het-Wel-Genoeg Ja"));
        }

        [TestCase("Tests", ExpectedResult = "Test"), TestCase("Testen", ExpectedResult = "Test"), TestCase("Taal", ExpectedResult = "Taal"), TestCase("Talen", ExpectedResult = "Taal"),
         TestCase("Cases", ExpectedResult = "Case"), TestCase("Fouten", ExpectedResult = "Fout"), TestCase("Sappen", ExpectedResult = "Sap"), TestCase("Apen", ExpectedResult = "Aap")]
        public string testDepluralize(string inp) => StringUtils.Depluralize(inp);

        [Test]
        public void ToStringInvariantTest()
        {
            var oldCulture = Thread.CurrentThread.CurrentCulture;
            try {
                var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                culture.NumberFormat = new NumberFormatInfo { NegativeSign = "X", };
                Thread.CurrentThread.CurrentCulture = culture;
                PAssert.That(() => (-3000).ToString() == "X3000");
                PAssert.That(() => (-3000m).ToString() == "X3000");
                PAssert.That(() => (-3000).ToStringInvariant() == "-3000");
                PAssert.That(() => 3000U.ToStringInvariant() == "3000");
                PAssert.That(() => (-3000m).ToStringInvariant() == "-3000");
                PAssert.That(() => (-3000.0).ToStringInvariant() == "-3000");
                PAssert.That(() => (-3000.0f).ToStringInvariant() == "-3000");
                PAssert.That(() => (-3000L).ToStringInvariant() == "-3000");
                PAssert.That(() => 3000UL.ToStringInvariant() == "3000");
                PAssert.That(() => ((short)-3000).ToStringInvariant() == "-3000");
                PAssert.That(() => ((ushort)3000).ToStringInvariant() == "3000");
                PAssert.That(() => true.ToStringInvariant() == "True");
                PAssert.That(() => new DateTime(2000, 1, 2).ToStringInvariant() == "01/02/2000 00:00:00");
            } finally {
                Thread.CurrentThread.CurrentCulture = oldCulture;
            }
        }

        [Test]
        public void PrettyPrintCamelCased()
        {
            var translations = new[,] {
                { "SMMutatie", "SM mutatie", "SM mutatie" },
                { "XmlReader", "xml reader", "Xml reader" },
                { "S0Xval", "S 0 xval", "S 0 xval" },
                { "bla bla bla", "bla bla bla", "bla bla bla" },
                { "iSXReader0Bla", "i SX reader 0 bla", "i SX reader 0 bla" },
                { "Channel99", "channel 99", "Channel 99" },
                { "SM99", "SM 99", "SM 99" },
                { "is_dit_echtZo", "is dit echt zo", "is dit echt zo" },
                { "Administratienummer_OWI", "administratienummer OWI", "Administratienummer OWI" },
                { "Bla_Bla", "bla bla", "Bla bla" },
                { "MT940Sluit", "MT 940 sluit", "MT 940 sluit" },
                { "Accoord2Afwijzen", "accoord 2 afwijzen", "Accoord 2 afwijzen" },
                { "_Multi _Space", " multi space", " multi space" },
                { "trailing Space\t", "trailing space ", "trailing space " },
            };
            for (int row = 0; row < translations.GetLength(0); row++) {
                var initial = translations[row, 0];
                var ideal = translations[row, 1];
                var idealCap = translations[row, 2];
                PAssert.That(() => StringUtils.PrettyPrintCamelCased(initial) == ideal);
                PAssert.That(() => StringUtils.PrettyCapitalizedPrintCamelCased(initial) == idealCap);
            }
        }

        [TestCase(null, "[]"), TestCase(new string[0], "[]"), TestCase(new[] { "single" }, "[single]"), TestCase(new[] { "first", "second" }, "[first, second]"),
         TestCase(new object[] { 1, "2", null, 3 }, "[1, 2, , 3]"), Test]
        public void ToFlatDebugString(IEnumerable<object> sut, string expected)
        {
            Assert.That(StringUtils.ToFlatDebugString(sut), Is.EqualTo(expected));
        }
    }
}
