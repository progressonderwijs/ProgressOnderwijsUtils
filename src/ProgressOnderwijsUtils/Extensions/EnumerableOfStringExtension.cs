using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
    using static System.String;

    public static class EnumerableOfStringExtension
    {
        /// <summary>
        /// Concatenate a sequence of strings.
        /// </summary>
        /// <param name="strings">string sequence</param>
        /// <returns>a string</returns>
        public static string JoinStrings(this IEnumerable<string?> strings)
            => JoinStrings(strings, "");

        //don't use optional params to allow usage in expression trees
        /// <summary>
        /// Concatenate a sequence of strings with an optional separator.  A sequence of N items includes the separator N-1 times.
        /// </summary>
        /// <param name="strings">string sequence</param>
        /// <param name="separator">separator string</param>
        /// <returns>a string</returns>
        public static string JoinStrings(this IEnumerable<string?> strings, string separator)
            => Join(separator, strings);

        public static string JoinStringsLimitLength(
            this IReadOnlyCollection<string> strings,
            string separator,
            int maxCount)
            => JoinStrings(strings.Take(maxCount), separator) + (strings.Count > maxCount ? separator + "..." : "");
    }
}
