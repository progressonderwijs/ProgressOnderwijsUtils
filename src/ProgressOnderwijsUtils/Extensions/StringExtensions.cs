using System.Linq;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class StringExtensions
    {
        /// <summary>
        /// Check either string is null or contains whitespace only
        /// </summary>
        /// <param name="s">string to check</param>
        /// <returns>true if string is empty or is null, false otherwise</returns>
        [Pure]
        public static bool IsNullOrWhiteSpace(this string s) => string.IsNullOrWhiteSpace(s);

        [Pure]
        public static string NullIfWhiteSpace(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) {
                return null;
            } else {
                return str;
            }
        }

        static readonly Regex COLLAPSE_WHITESPACE = new Regex(@"\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

        /// <summary>
        /// HTML-alike whitespace collapsing of this string; however, this method also trims.
        /// </summary>
        [Pure]
        public static string NormalizeWhitespace(this string str) => str.CollapseWhitespace().Trim();

        /// <summary>
        /// HTML-alike whitespace collapsing of this string. This method does not trim.
        /// </summary>
        [Pure]
        public static string CollapseWhitespace(this string str) => COLLAPSE_WHITESPACE.Replace(str, " ");

        [Pure]
        public static bool EqualsOrdinalCaseInsensitive(this string a, string b) => StringComparer.OrdinalIgnoreCase.Equals(a, b);

        [Pure]
        public static bool Contains(this string str, string value, StringComparison compare) => str.IndexOf(value, compare) >= 0;

        [Pure]
        public static string TrimToLength(this string s, int maxlength)
        {
            if (s == null || s.Length <= maxlength) {
                return s;
            } else {
                return s.Remove(maxlength);
            }
        }

        [Pure]
        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        [Pure]
        public static string Replace(this string s, IEnumerable<KeyValuePair<string, string>> replacements) => replacements.Aggregate(s, (current, replacement) => current.Replace(replacement.Key, replacement.Value));
    }
}
