using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class StringUtils
    {
        /// <summary>
        /// Removes all 'diakriet' from the string.
        /// </summary>
        /// <param name="input">the string to change</param>
        /// <returns>the changed string</returns>
        [NotNull]
        [Pure]
        public static string VerwijderDiakrieten([NotNull] string input)
            => new string(
                input
                    .Normalize(NormalizationForm.FormD)
                    .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    .ToArray()
            ).Normalize(NormalizationForm.FormC);

        const RegexOptions CommonOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace;

        static class PrettyPrintValues
        {
            //perf: put this in a seperate class so usage of stringutils doesn't imply loading (and compiling) these just yet.
            public static readonly Regex
                whiteSpaceSequence = new Regex(@"[ \t_]+", CommonOptions),
                capLetter = new Regex(@"
                        (?<=[a-zA-Z])[0-9]+
                        |(?<![A-Z])[A-Z][a-z]*
                        |(?<=[A-Z])[A-Z]
                            (
                                [a-rt-z]
                                |s[a-z]
                            )[a-z]*", CommonOptions);
        }

        static class SepaStripperRegexes
        {
            //perf: put this in a seperate class so usage of stringutils doesn't imply loading (and compiling) these just yet.
            public static readonly Regex
                sepaStripper = new Regex(@"[^a-zA-z0-9 /-?:().,'+]+", CommonOptions);
        }

        [NotNull]
        [Pure]
        [CodeThatsOnlyUsedForTests]
        public static string PrettyPrintCamelCased([NotNull] string rawString)
        {
            var withSpace =
                PrettyPrintValues.capLetter.Replace(
                    rawString,
                    m => (m.Index == 0 ? "" : " ") + (IsUpperAscii(m.Value) ? m.Value : DecapitalizeAscii(m.Value))
                );
            return PrettyPrintValues.whiteSpaceSequence.Replace(withSpace, " ");
        }

        [Pure]
        static bool IsUpperAscii([NotNull] string str)
        {
            foreach (var c in str) {
                if (c < 'A' || c > 'Z') {
                    return false;
                }
            }
            return true;
        }

        [NotNull]
        [Pure]
        static string DecapitalizeAscii([NotNull] string str)
        {
            if (str[0] >= 'A' && str[0] <= 'Z') {
                return (char)(str[0] + ('a' - 'A')) + str.Substring(1);
            } else {
                return str;
            }
        }

        [NotNull]
        [Pure]
        public static string PrettyCapitalizedPrintCamelCased([NotNull] string rawString)
        {
            var withSpace =
                PrettyPrintValues.capLetter.Replace(
                    rawString,
                    m => m.Index == 0 ? m.Value : " " + (IsUpperAscii(m.Value) ? m.Value : DecapitalizeAscii(m.Value))
                );
            return PrettyPrintValues.whiteSpaceSequence.Replace(withSpace, " ");
        }

        [NotNull]
        [Pure]
        public static string VervangRingelS([NotNull] string str, bool upper)
            => str.Replace("ß", upper ? "SS" : "ss");

        [NotNull]
        [Pure]
        public static string SepaTekenset([NotNull] string s)
            => SepaStripperRegexes.sepaStripper.Replace(s, "");

        [Pure]
        public static string? SepaTekensetEnModificaties(string s)
        {
            if (s == null) {
                return null;
            }

            s = VerwijderDiakrieten(s);
            s = VervangRingelS(s, false);
            s = SepaTekenset(s);
            return s;
        }

        [NotNull]
        [Pure]
        public static string Capitalize([NotNull] string name)
            => name.Substring(0, 1).ToUpperInvariant() + name.Substring(1);

        [Pure]
        public static int LevenshteinDistance([NotNull] string s, [NotNull] string t)
            => LevenshteinDistance(s, t, 1);

        [Pure]
        public static int LevenshteinDistance([NotNull] string s, [NotNull] string t, int substitutionCost)
        {
            //modified from:http://www.merriampark.com/ldcsharp.htm by Eamon Nerbonne
            var n = s.Length; //length of s
            var m = t.Length; //length of t
            var d = new int[n + 1, m + 1]; // matrix
            if (n == 0) {
                return m;
            }
            if (m == 0) {
                return n;
            }
            for (var i = 0; i <= n; i++) {
                d[i, 0] = i;
            }
            for (var j = 0; j <= m; j++) {
                d[0, j] = j;
            }
            for (var i = 0; i < n; i++) {
                for (var j = 0; j < m; j++) {
                    var cost = t[j] == s[i] ? 0 : substitutionCost; // cost
                    d[i + 1, j + 1] = Math.Min(Math.Min(d[i, j + 1] + 1, d[i + 1, j] + 1), d[i, j] + cost);
                }
            }
            return d[n, m];
        }

        [Pure]
        [CodeThatsOnlyUsedForTests]
        public static double LevenshteinDistanceScaled([NotNull] string s, [NotNull] string t)
            => LevenshteinDistance(s, t) / (double)Math.Max(1, Math.Max(s.Length, t.Length));

        [NotNull]
        [Pure]
        public static string ToFlatDebugString<T>(IEnumerable<T>? self)
            => "[" +
                self
                    .EmptyIfNull()
                    .Select(item => item == null ? "" : item.ToString())
                    .JoinStrings(", ")
                + "]";

        /// <summary>
        /// Vervang in een [naam]string beginletters door hoofdletters,
        /// rekening houdend met tussenvoegsels en interpunctie
        /// </summary>
        /// <remarks>
        /// tussenvoegsels zouden ook uit database kunnen worden gehaald:
        /// [SELECT voorvoegsels FROM student group by voorvoegsels]
        /// </remarks>
        [Pure]
        [CodeThatsOnlyUsedForTests]
        public static string Name2UpperCasedName(string inp)
        {
            //string wat opschonen
            inp = Regex.Replace(inp, @"\s+", " ");
            inp = Regex.Replace(inp, @"\-+", "-");
            inp = Regex.Replace(inp, @"('s)([a-zA-Z]+)", "$1 $2"); //'sgravenhage bv
            inp = Regex.Replace(inp, @"^\-+|\-+$", "").Trim();
            const string expression = @"d'|o'
                                        | 's | 's-|'s| op 't | op ten | op de
                                        | van het | van der | van de | van den | van ter
                                        | auf dem | auf der | von der | von den
                                        | in het | in 't | in de
                                        | uit de | uit den | uit het 
                                        | voor de | voor 't 
                                        | aan het | aan 't | aan de | aan den | bij de | de la 
                                        | del | van | von | het | de 
                                        | der | den | des | di | dos | do | du | el | le | la
                                        | lo | los | op | te | ten | ter | uit 
                                        | vd | v.d. | v\/d
                                        | au | aux | a | à | à la | a la 
                                        | \- |\s|\s+|\-+";
            var newstr = Regex.Split(inp, Regex.Replace(expression, @"\s+", " "));
            return newstr.Aggregate(
                inp,
                (current, t) =>
                    Regex.Replace(current, t, t.Length > 0 ? Capitalize(t.ToLowerInvariant()) : t)
            );
        }

        [Pure]
        static bool isVowel(char c)
            => c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u' || c == 'A' || c == 'E' || c == 'I' || c == 'O' || c == 'U';

        [NotNull]
        [Pure]
        public static string Depluralize([NotNull] string pluralstring)
        {
            if (pluralstring.EndsWith("s", StringComparison.Ordinal)) {
                return pluralstring.Remove(pluralstring.Length - 1);
            }
            if (pluralstring.EndsWith("en", StringComparison.Ordinal)) {
                if (pluralstring.Length >= 4 && isVowel(pluralstring[pluralstring.Length - 4]) && (pluralstring.Length < 5 || !isVowel(pluralstring[pluralstring.Length - 5]))) {
                    return pluralstring.Remove(pluralstring.Length - 3) + pluralstring.Substring(pluralstring.Length - 4, 2).ToLowerInvariant();
                } else if (pluralstring.Length >= 4 && pluralstring[pluralstring.Length - 4] == pluralstring[pluralstring.Length - 3]) {
                    return pluralstring.Remove(pluralstring.Length - 3);
                } else {
                    return pluralstring.Remove(pluralstring.Length - 2);
                }
            } else {
                return pluralstring;
            }
        }

        public static int? TryParseInt32(this string input)
        {
            if (int.TryParse(input, out var output)) {
                return output;
            } else {
                return null;
            }
        }

        public static long? TryParseInt64(this string input)
        {
            if (long.TryParse(input, out var output)) {
                return output;
            } else {
                return null;
            }
        }
    }
}
