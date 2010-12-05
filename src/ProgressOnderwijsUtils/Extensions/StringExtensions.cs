using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System;

namespace ProgressOnderwijsUtils
{
	public static class StringExtensions
	{
		/// <summary>
		/// Check either string is null or contains whitespace only
		/// </summary>
		/// <param name="s">String to check</param>
		/// <returns>true if string is empty or is null, false otherwise</returns>
		public static bool IsNullOrEmpty(this string s)
		{
			return s == null || s.Trim().Length == 0;
		}

		/// <summary>
		/// Return s or "" if string is null
		/// </summary>
		/// <param name="s">string</param>
		/// <returns>string</returns>
		public static string GetValueOrEmptyString(this string s)
		{
			return s == null ? "" : s;
		}

		/// <summary>
		/// Replace equal characters on left side of string with other characters
		/// </summary>
		/// <param name="s">string</param>
		/// <param name="src">character to replace</param>
		/// <param name="dest">character to replace with</param>
		/// <returns>string</returns>
		public static string ReplaceLeftChars(this string s, char src, char dest)
		{
			if (s != null)
			{
				int length = s.Length;
				return s.TrimStart(src).PadLeft(length, dest);
			}
			else
			{
				return s;
			}
		}

		/// <summary>
		/// Removes all 'diakriet' from the string.
		/// </summary>
		/// <param name="s">the string to change</param>
		/// <returns>the changed string</returns>
		public static string VerwijderDiakrieten(this string s)
		{
			StringBuilder result = new StringBuilder(s.Length);
			foreach (char c in s.Normalize(NormalizationForm.FormD))
			{
				if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
				{
					result.Append(c);
				}
			}
			return result.ToString().Normalize(NormalizationForm.FormC);
		}

		public static string VervangRingelS(this string str, bool upper)
		{
			return str.Replace("ß", upper ? "SS" : "ss");
		}

		public static string RegexReplace(this string input, string pattern, string replacement, RegexOptions options = RegexOptions.None)
		{
			return Regex.Replace(input, pattern, replacement, options);
		}

		private static readonly Regex COLLAPSE_WHITESPACE = new Regex(@"\s+", RegexOptions.Compiled);

		/// <summary>
		/// HTML-alike whitespace collapsing of this string; however, this method also trims.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string CollapseWhitespace(this string str)
		{
			return COLLAPSE_WHITESPACE.Replace(str, " ").Trim();
		}

		//modified from:http://www.merriampark.com/ldcsharp.htm by Eamon Nerbonne
		public static int LevenshteinDistance(this string s, string t, int? substitutionCost = null)
		{
			int subsCost = substitutionCost ?? 1;
			int n = s.Length; //length of s
			int m = t.Length; //length of t
			int[,] d = new int[n + 1, m + 1]; // matrix
			int cost; // cost
			// Step 1
			if (n == 0) return m;
			if (m == 0) return n;
			// Step 2
			for (int i = 0; i <= n; d[i, 0] = i++) ;
			for (int j = 0; j <= m; d[0, j] = j++) ;
			// Step 3
			for (int i = 0; i < n; i++)
			{
				//Step 4
				for (int j = 0; j < m; j++)
				{
					// Step 5
					cost = (t[j] == s[i] ? 0 : subsCost);
					// Step 6
					d[i + 1, j + 1] = System.Math.Min(System.Math.Min(d[i, j + 1] + 1, d[i + 1, j] + 1), d[i, j] + cost);
				}
			}
			// Step 7
			return d[n, m];
		}
		public static double LevenshteinDistanceScaled(this string s, string t)
		{
			return LevenshteinDistance(s, t) / (double)Math.Max(1, Math.Max(s.Length, t.Length));
		}

		/// <summary>
		/// Vervang in een [naam]string beginletters door hoofdletters,
		/// rekening houdend met tussenvoegsels en interpunctie
		/// </summary>
		/// <remarks>
		/// tussenvoegsels zouden ook uit database kunnen worden gehaald:
		/// [SELECT voorvoegsels FROM student group by voorvoegsels]
		/// </remarks>
		/// <param name="inp"></param>
		/// <returns></returns>
		public static string Name2UpperCasedName(this string inp)
		{
			//string wat opschonen
			inp = Regex.Replace(inp, @"\s+", " ");
			inp = Regex.Replace(inp, @"\-+", "-");
			inp = Regex.Replace(inp, @"('s)([a-zA-Z]+)", "$1 $2"); //'sgravenhage bv
			inp = Regex.Replace(inp, @"^\-+|\-+$", "").Trim();
			const string expression = @"d'|o'
										| 's | 's-|'s| op 't | op ten | op de
										| van het | van der | van de | van den | van ter
										| auf dem | auf der | von der | von den
										| in het | in 't | in de
										| uit de | uit den | uit het 
										| voor de | voor 't 
										| aan het | aan 't | aan de | aan den | bij de | de la 
										| del | van | von | het | de 
										| der | den | des | di | dos | do | du | el | le | la
										| lo | los | op | te | ten | ter | uit 
										| vd | v.d. | v\/d
										| au | aux | a | à | à la | a la 
										| \- |\s|\s+|\-+";
			string[] newstr = Regex.Split(inp, Regex.Replace(expression, @"\s+", " "));
			return newstr.Aggregate(inp, (current, t) =>
										 Regex.Replace(current, t, t.Length > 0 ? t.Substring(0, 1).ToUpper() + t.Substring(1) : t)
								   );
		}
	}


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
		[TestCase("test","tset", Result = 2)]
		[TestCase(" test ","\ttest\t", Result =2)]
		[TestCase("Ziggy Stardust","ziggy stradust", Result = 4)]
		public int TestLevenshtein(string str1, string str2) { return str1.LevenshteinDistance(str2); }

		[Test]
		[TestCase("joepje jofel",Result = "Joepje Jofel")]
		[TestCase("carolien Kaasteen",Result = "Carolien Kaasteen")]
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

