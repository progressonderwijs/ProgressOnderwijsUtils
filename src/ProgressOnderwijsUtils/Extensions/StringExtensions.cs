using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

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
		/// <param name="this initialstr">de string waarop de method wordt toegepast</param>
		/// <param name="searchreplace">(array) paren van zoek- (substring/regex) en vervangstring</param>
		/// <param name="opts">(string) opties voor vervanging</param>
		/// <example>
		/// <c>
		///  string jantje = "Jantje zag eens pruimen";
		///  jantje = 
		///   jantje.MultiReplace(
		///		new string[] {@"pruimen","pruimen hangen",
		///					  @"(hangen)","$1.<br />O, als eieren!"},
		///		"m");
		///	 </c>	
		///	 => de string [jantje] is na deze operatie:
		///	 => "Jantje zag eens pruimen hangen.<br />O, als eieren!"
		/// </example>
		/// <seealso>Tool.Utils.ReOpts en overload hieronder</seealso>
		/// <canblame>Renzo Kooi</canblame>
		/// <datelast value="2009/08/15"/>
		/// <returns>gemodificeerde string</returns>
		/// <remarks>
		/// door verplaatsing naar StringExtensions kan method chaining
		/// worden toegepast.
		/// TODO: tweede parameter zou efficiënter kunnen?
		/// </remarks>
		public static string MultiReplace(this string initial, string[] searchreplace, string opts)
		{
			if (searchreplace.Length % 2 == 0)
			{
				string regex = searchreplace[0], replacewith = searchreplace[1];
				RegexOptions ro = ProgressOnderwijsUtils.Utils.ReOpts(opts);
				initial = Regex.Replace(initial, regex, replacewith, ro);

				if (searchreplace.Length > 2)
				{
					for (int i = 2; i < searchreplace.Length; i += 2)
					{
						string[] s_ = new string[2] { searchreplace[i], searchreplace[i + 1] };
						initial = initial.MultiReplace(s_, opts);
					}
				}			
			}
			return initial;
		}
		/// <remarks>
		/// MultiReplace overload, waarbij de gemodificeerde string
		/// naar 'n out wordt teruggezet. Zo kun je dus (zie voorbeeld
		/// hierboven) [jantje] ook als volgt modificeren:
		/// jantje.MultiReplace(
		///		new string[] {@"pruimen","pruimen hangen",
		///					  @"(hangen)","$1.<br />O, als eieren!"},
		///		"m", 
		///		out jantje);
		/// </remarks>
		public static void MultiReplace(this string initial, string[] searchreplace, string opts, out string outstr)
		{
			if (searchreplace.Length % 2 == 0)
			{
				string regex = searchreplace[0], replacewith = searchreplace[1];
				RegexOptions ro = ProgressOnderwijsUtils.Utils.ReOpts(opts);
				initial = Regex.Replace(initial, regex, replacewith, ro);

				if (searchreplace.Length > 2)
				{
					for (int i = 2; i < searchreplace.Length; i += 2)
					{
						string[] s_ = new string[2] { searchreplace[i], searchreplace[i + 1] };
						initial.MultiReplace(s_, opts, out initial);
					}
				}
			}
			outstr = initial;
		}
	}
}

