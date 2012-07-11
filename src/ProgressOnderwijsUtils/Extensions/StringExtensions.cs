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
		/// <param name="s">string to check</param>
		/// <returns>true if string is empty or is null, false otherwise</returns>
		public static bool IsNullOrEmpty(this string s)
		{
			return s == null || s.Trim().Length == 0;
		}




		public static bool EqualsOrdinalCaseInsensitive(this string a, string b){return StringComparer.OrdinalIgnoreCase.Equals(a, b);}
		public static string TrimToLength(this string s, int maxlength)
		{
			if (s == null || s.Length <= maxlength) return s;
			else return s.Remove(maxlength);
		}
	}
}

