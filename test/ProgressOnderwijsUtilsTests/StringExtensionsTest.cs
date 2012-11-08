using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public class StringExtensionsTest
	{
		[TestCase("é", "e")]
		[TestCase("Ü", "U")]
		[TestCase("ß", "ß")]
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

		[Test]
		[TestCase(null, default(string))]
		[TestCase("", "")]
		[TestCase("test", "test")]
		[TestCase(" test ", "test")]
		[TestCase("\ttest\t", "test")]
		[TestCase("\ntest\n", "test")]
		[TestCase(" \t\ntest\n\t ", "test")]
		[TestCase("een test", "een test")]
		[TestCase("een  test", "een test")]
		[TestCase("een\ttest", "een test")]
		[TestCase("een\t\ttest", "een test")]
		[TestCase("een\ntest", "een test")]
		[TestCase("een\n\ntest", "een test")]
		public void NormalizeWhitespace(string str, string expected)
		{
			Assert.That(str.NormalizeWhitespace(), Is.EqualTo(expected));
		}

		[Test]
		[TestCase(null, default(string))]
		[TestCase("", "")]
		[TestCase("test", "test")]
		[TestCase(" test ", " test ")]
		[TestCase("\ttest\t", " test ")]
		[TestCase("\ntest\n", " test ")]
		[TestCase(" \t\ntest\n\t ", " test ")]
		[TestCase("een test", "een test")]
		[TestCase("een  test", "een test")]
		[TestCase("een\ttest", "een test")]
		[TestCase("een\t\ttest", "een test")]
		[TestCase("een\ntest", "een test")]
		[TestCase("een\n\ntest", "een test")]
		public void CollapseWhitespace(string str, string expected)
		{
			Assert.That(str.CollapseWhitespace(), Is.EqualTo(expected));
		}

		[Test]
		[TestCase("", "", Result = 0)]
		[TestCase("test", "tset", Result = 2)]
		[TestCase(" test ", "\ttest\t", Result = 2)]
		[TestCase("Ziggy Stardust", "ziggy stradust", Result = 4)]
		[TestCase("a", "b", Result = 1)]
		[TestCase("a", "", Result = 1)]
		[TestCase("aba", "aa", Result = 1)]
		[TestCase("simple", "Simpler", Result = 2)]
		[TestCase("hmmm", "yummy", Result = 3)]
		[TestCase("World-wide", "wordy", Result = 7)]//"W"=>"w",drop "l", replace "-wide"
		public int TestLevenshtein(string str1, string str2) { return StringUtils.LevenshteinDistance(str1, str2); }

		[Test]
		[TestCase("joepje jofel", Result = "Joepje Jofel")]
		[TestCase("carolien Kaasteen", Result = "Carolien Kaasteen")]
		[TestCase("maarten middelmaat--meloen", Result = "Maarten Middelmaat-Meloen")]
		[TestCase("carolien    Kaasteen", Result = "Carolien Kaasteen")]
		[TestCase("miep boezeroen-jansen van der sloot op 't gootje v.d. geest de la terrine du soupe au beurre à demi v/d zo-is-het-wel-genoeg ja"
					, Result = "Miep Boezeroen-Jansen van der Sloot op 't Gootje v.d. Geest de la Terrine du Soupe au Beurre à Demi v/d Zo-Is-Het-Wel-Genoeg Ja")]
		[TestCase("'s-gravenhage", Result = "'s-Gravenhage")]
		[TestCase("'s gravenhage", Result = "'s Gravenhage")]
		[TestCase("'sgravenhage", Result = "'s Gravenhage")]
		[TestCase("sieb op de kast", Result = "Sieb op de Kast")]
		public string testNaam2Upper(string inp)
		{
			return StringUtils.Name2UpperCasedName(inp);
		}

		[TestCase("Tests", Result = "Test")]
		[TestCase("Testen", Result = "Test")]
		[TestCase("Taal", Result = "Taal")]
		[TestCase("Talen", Result = "Taal")]
		[TestCase("Cases", Result = "Case")]
		[TestCase("Fouten", Result = "Fout")]
		[TestCase("Sappen", Result = "Sap")]
		[TestCase("Apen", Result = "Aap")]
		public string testDepluralize(string inp)
		{
			return StringUtils.Depluralize(inp);
		}



		[Test]
		public void ToStringInvariantTest()
		{
			var oldCulture = Thread.CurrentThread.CurrentCulture;
			try
			{
				var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
				culture.NumberFormat = new NumberFormatInfo { NegativeSign = "X", };
				Thread.CurrentThread.CurrentCulture = culture;
				PAssert.That(() => (-3000).ToString() == "X3000");
				PAssert.That(() => (-3000m).ToString() == "X3000");
				PAssert.That(() => (-3000).ToStringInvariant() == "-3000");
				PAssert.That(() => (3000U).ToStringInvariant() == "3000");
				PAssert.That(() => (-3000m).ToStringInvariant() == "-3000");
				PAssert.That(() => (-3000.0).ToStringInvariant() == "-3000");
				PAssert.That(() => (-3000.0f).ToStringInvariant() == "-3000");
				PAssert.That(() => (-3000L).ToStringInvariant() == "-3000");
				PAssert.That(() => (3000UL).ToStringInvariant() == "3000");
				PAssert.That(() => ((short)-3000).ToStringInvariant() == "-3000");
				PAssert.That(() => ((ushort)3000).ToStringInvariant() == "3000");
				PAssert.That(() => true.ToStringInvariant() == "True");
				PAssert.That(() => new DateTime(2000, 1, 2).ToStringInvariant() == "01/02/2000 00:00:00");
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = oldCulture;
			}
		}

		[Test]
		public void PrettyPrintCamelCased()
		{
			var tranlations = new Dictionary<string, string> {
				{"SMMutatie", "SM mutatie"},
				{"XmlReader", "xml reader"},
				//{"S0Xval", "S 0 xval"},
				{"bla bla bla", "bla bla bla"},
				{"iSXReader0Bla", "i SX reader 0 bla"},
				{"Channel99", "channel 99"},
				{"SM99", "SM 99"},
				{"is_dit_echtZo", "is dit echt zo"},
				{"Administratienummer_OWI", "administratienummer OWI"},
				{"Bla_Bla", "bla bla"},
				{"MT940Sluit","MT 940 sluit"},
				{"Accoord2Afwijzen","accoord 2 afwijzen"},
				{"_Multi _Space", " multi space"},
				{"trailing Space\t", "trailing space "},
			};

			foreach (var kv in tranlations)
			{
				var initial = kv.Key;
				var ideal = kv.Value;
				PAssert.That(() => StringUtils.PrettyPrintCamelCased(initial) == ideal);
			}
		}


	}
}
