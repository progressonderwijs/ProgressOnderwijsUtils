using System.Globalization;
using System.Text;

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
	}
}
