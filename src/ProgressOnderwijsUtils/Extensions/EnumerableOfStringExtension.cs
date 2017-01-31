﻿using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
    public static class EnumerableOfStringExtension
    {
        /// <summary>
        /// Concatenate a sequence of strings.
        /// </summary>
        /// <param name="strings">string sequence</param>
        /// <returns>a string</returns>
        public static string JoinStrings(this IEnumerable<string> strings) => JoinStrings(strings, "");

        //don't use optional params to allow usage in expression trees
        /// <summary>
        /// Concatenate a sequence of strings with an optional separator.  A sequence of N items includes the separator N-1 times.
        /// </summary>
        /// <param name="strings">string sequence</param>
        /// <param name="separator">separator string</param>
        /// <returns>a string</returns>
        public static string JoinStrings(this IEnumerable<string> strings, string separator)
            => string.Join(separator, strings);

        public static string JoinStringsLimitLength(this IReadOnlyCollection<string> strings, string separator,
            int maxCount)
        {
            return string.Join(separator, strings.Take(maxCount)) + (strings.Count > maxCount ? separator + "..." : "");
        }
    }
}