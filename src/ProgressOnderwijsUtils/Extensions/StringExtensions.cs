using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

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


		/// <summary>
		/// Vervang 1 of meer substrings in de (huidige) string
		/// door een andere string. Zoek en vervang is een array van strings
		/// [searchreplace], waarin 1 of meer paren van 'n reguliere expressie 
		/// of substring (zoek) en een vervangstring zitten.
		/// In de 3e parameter de opties, in de vorm van een string met 
		/// optie-letters gescheiden door een komma, die via ReOpts (zie 
		/// Tools.Utils.ReOpts) naar een RegexOptions type worden omgezet. 
		/// Mag ook een lege string zijn (geen opties).
		/// </summary>
		/// <param name="opts">RegexOptions enum waarden</param>
		/// <param name="searchreplace">Tuple(s)&lt;string,string&gt; met zoek/vervangstring 
		/// (zoek = Regex literal)</param>
		/// <seealso cref="ProgressOnderwijsUtils.Utils"/>
		/// <returns>gemodificeerde string</returns>
		/// <remarks>
		/// <para>Door verplaatsing naar StringExtensions en er dus een
		/// extension method van te maken, kan method chaining
		/// worden gebruikt: als in</para>
		/// <para>==&gt;[string].MultiReplace([params]).MultiReplace([params]);</para>
		/// <para>Voor het produceren van een reeks Tuples kun je ProgressOnderwijsUtils.Utils.ToTuples 
		/// gebruiken</para>
		/// <para>Let op: de string waarop deze extension method wordt toegepast wordt dus gewijzigd. De
		/// return value is alleen omdat dat in sommige gevallen handig is.</para>
		/// </remarks>
		/// <codefrom value="Renzo Kooi" date="2009/08/15"/>
		public static string MultiReplace(this string initial, RegexOptions opts, params Tuple<string, string>[] searchreplace)
		{
			foreach (var replaceTuple in searchreplace)
			{
				string regex = replaceTuple.Item1, replacewith = replaceTuple.Item2;
				initial = Regex.Replace(initial, regex, replacewith, RegexOptions.Compiled | opts);
			}
			return initial;
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
	}
}

