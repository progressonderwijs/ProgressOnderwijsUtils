using System.Globalization;
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

		public static string RegexReplace(this string input, string pattern, MatchEvaluator evaluator, RegexOptions options = RegexOptions.None)
		{
			return Regex.Replace(input, pattern, evaluator, options);
		}

        private static readonly Regex COLLAPSE_WHITESPACE = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>
        /// HTML-alike whitespace collapsing of this string; however, this method also trims.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string CollapseWhitespace(this string str)
        {
            return COLLAPSE_WHITESPACE.Replace(str," ").Trim();
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
	}
}

