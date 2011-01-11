using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			Assert.That(from.VerwijderDiakrieten(), Is.EqualTo(to));
		}

		[Test]
		public void ReplaceRingelS()
		{
			Assert.That("ß".VervangRingelS(false), Is.EqualTo("ss"));
			Assert.That("ß".VervangRingelS(true), Is.EqualTo("SS"));
			Assert.That("aßb".VervangRingelS(false), Is.EqualTo("assb"));
			Assert.That("ßsß".VervangRingelS(true), Is.EqualTo("SSsSS"));
			Assert.That("".VervangRingelS(false), Is.EqualTo(""));
		}

		[Test]
		[TestCase("", Result = "")]
		[TestCase("test", Result = "test")]
		[TestCase(" test ", Result = "test")]
		[TestCase("\ttest\t", Result = "test")]
		[TestCase("\ntest\n", Result = "test")]
		[TestCase(" \t\ntest\n\t ", Result = "test")]
		[TestCase("een test", Result = "een test")]
		[TestCase("een  test", Result = "een test")]
		[TestCase("een\ttest", Result = "een test")]
		[TestCase("een\t\ttest", Result = "een test")]
		[TestCase("een\ntest", Result = "een test")]
		[TestCase("een\n\ntest", Result = "een test")]
		public string CollapseWhitespace(string str)
		{
			return str.CollapseWhitespace();
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
		public int TestLevenshtein(string str1, string str2) { return str1.LevenshteinDistance(str2); }

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
			return inp.Name2UpperCasedName();
		}
	}
}
