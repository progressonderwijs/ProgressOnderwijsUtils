using System.Linq;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

namespace ProgressOnderwijsUtils
{
	public static class StringExtensions
	{
		/// <summary>
		/// Check either string is null or contains whitespace only
		/// </summary>
		/// <param name="s">string to check</param>
		/// <returns>true if string is empty or is null, false otherwise</returns>
		public static bool IsNullOrEmpty(this string s)
		{
			return s == null || s.Trim().Length == 0;
		}

		static readonly Regex COLLAPSE_WHITESPACE = new Regex(@"\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

		/// <summary>
		/// HTML-alike whitespace collapsing of this string; however, this method also trims.
		/// </summary>
		public static string NormalizeWhitespace(this string str)
		{
			return str == null ? null : str.CollapseWhitespace().Trim();
		}

		/// <summary>
		/// HTML-alike whitespace collapsing of this string. This method does not trim.
		/// </summary>
		public static string CollapseWhitespace(this string str)
		{
			return str == null ? null : COLLAPSE_WHITESPACE.Replace(str, " ");
		}

		public static bool EqualsOrdinalCaseInsensitive(this string a, string b) { return StringComparer.OrdinalIgnoreCase.Equals(a, b); }
		public static string TrimToLength(this string s, int maxlength)
		{
			if (s == null || s.Length <= maxlength) return s;
			else return s.Remove(maxlength);
		}
	}
}

