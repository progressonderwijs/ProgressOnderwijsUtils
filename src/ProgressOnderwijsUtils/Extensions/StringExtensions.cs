using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;

namespace ProgressOnderwijsUtils
{
	public static class StringExtensions
	{
		/// <summary>
		/// Check either string is null or contains whitespace only
		/// </summary>
		/// <param name="s">String to check</param>
		/// <returns>True if string is empty. False otherwise.</returns>
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
		public static string WithoutDiakriet(this string s)
		{
			StringBuilder result = new StringBuilder();
			foreach (char c in s.ToString().Normalize(NormalizationForm.FormD))
			{
				if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
				{
					result.Append(c);
				}
			}
			return result.ToString().Normalize(NormalizationForm.FormC);
		}

		///<summary>
		/// Vervang 1 of meer substrings in de (huidige) string
		/// door een andere string. Zoek en vervang is een array van strings
		/// [searchreplace], waarin 1 of meer paren van 'n reguliere expressie 
		/// of substring (zoek) en een vervangstring zitten.
		/// In de 3e parameter de opties, in de vorm van een string met 
		/// optie-letters gescheiden door een komma, die via ReOpts (zie 
		/// Tools.Utils.ReOpts) naar een RegexOptions type worden omgezet. 
		/// Mag ook een lege string zijn (geen opties).
		/// </summary>
		/// <param name="searchreplace">new Tuple([Zoek], [Vervang])</param>
		/// <example>
		/// <code>
		///  string jantje = "Jantje zag eens pruimen";
		///  jantje = 
		///   jantje.MultiReplace(
		///     RegexOptions.IgnoreCase,
		///		new Tuple<string,string>[] 
		///		              {@"pruimen","pruimen hangen",
		///					   @"(hangen)","$1.<br />O, als eieren!"});
		///	 </code>	
		///	 => de string [jantje] is na deze operatie:
		///	 => "Jantje zag eens pruimen hangen.<br />O, als eieren!"
		/// </example>
		/// <seealso cref="Tools.Utils.ToTuples"/>
		/// <canblame>Renzo Kooi</canblame>
		/// <datelast value="2009/08/15"/>
		/// <returns>gemodificeerde string</returns>
		/// <remarks>
		/// *door verplaatsing naar StringExtensions en er dus een
		/// extension method van te maken, kan method chaining
		/// worden gebruikt: als in
		/// [string].MultiReplace([params]).MultiReplace([params])...
		/// *voor het produceren van een reeks Tuples kun je 
		/// de utility functie ToTuples gebruiken
		/// </remarks>
		public static string MultiReplace(this string initial, RegexOptions opts, params Tuple<string,string>[] searchreplace)
		{
			foreach(var replaceTuple in searchreplace) {
				string regex = replaceTuple.a, replacewith = replaceTuple.b;
				initial = Regex.Replace(initial, regex, replacewith, RegexOptions.Compiled| opts);
			}
			return initial;
		}

	}
}

