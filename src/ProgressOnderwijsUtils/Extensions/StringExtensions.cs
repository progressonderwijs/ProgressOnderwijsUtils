using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils
{
	public static class StringExtensions
	{
		/// <summary>
		/// Check either string is null or contains whitespace only)
		/// </summary>
		/// <param name="s">String to check</param>
		/// <returns>True if string is empty. False otherwise.</returns>
		public static bool IsNullOrEmpty(this string s)
		{
			return s == null || s.Trim().Length == 0;
		}

		/// <summary>
		/// Return string or "" if string is null
		/// </summary>
		/// <param name="s">string</param>
		/// <returns>String or "" if string is null</returns>
		public static string GetValueOrEmptyString(this string s)
		{
			return s == null ? "" : s;
		}
	}
}
