namespace ProgressOnderwijsUtils;

public static class StringUtils
{
    /// <summary>
    /// Removes all 'diakriet' from the string.
    /// </summary>
    /// <param name="input">the string to change</param>
    /// <returns>the changed string</returns>
    [Pure]
    public static string VerwijderDiakrieten(string input)
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
            whiteSpaceSequence = new(@"[ \t_]+", CommonOptions),
            capLetter = new(
                @"
                        (?<=[a-zA-Z])[0-9]+
                        |(?<![A-Z])[A-Z][a-z]*
                        |(?<=[A-Z])[A-Z]
                            (
                                [a-rt-z]
                                |s[a-z]
                            )[a-z]*",
                CommonOptions
            );
    }

    static class SepaStripperRegexes
    {
        //perf: put this in a seperate class so usage of stringutils doesn't imply loading (and compiling) these just yet.
        public static readonly Regex
            sepaStripper = new(@"[^a-zA-z0-9 /-?:().,'+]+", CommonOptions);
    }

    [Pure]
    public static bool IsUpperAscii(string str)
    {
        foreach (var c in str) {
            if (c < 'A' || c > 'Z') {
                return false;
            }
        }
        return true;
    }

    [Pure]
    static string DecapitalizeAscii(string str)
    {
        if (str[0] >= 'A' && str[0] <= 'Z') {
            return (char)(str[0] + ('a' - 'A')) + str.Substring(1);
        } else {
            return str;
        }
    }

    [Pure]
    public static string PrettyPrintCamelCased(string rawString)
    {
        var withSpace =
            PrettyPrintValues.capLetter.Replace(
                rawString,
                m => m.Index == 0 ? m.Value : " " + (IsUpperAscii(m.Value) ? m.Value : DecapitalizeAscii(m.Value))
            );
        return PrettyPrintValues.whiteSpaceSequence.Replace(withSpace, " ");
    }

    [Pure]
    public static string VervangRingelS(string str)
        => str.Replace("ß", "ss").Replace("ẞ", "ss");

    [Pure]
    public static string SepaTekenset(string s)
        => SepaStripperRegexes.sepaStripper.Replace(s, "");

    [Pure]
    public static string? SepaTekensetEnModificaties(string? s)
    {
        if (s == null) {
            return null;
        }

        s = VerwijderDiakrieten(s);
        s = VervangRingelS(s);
        s = SepaTekenset(s);
        return s.NullIfWhiteSpace();
    }

    [Pure]
    public static string Capitalize(string name)
        => name.Substring(0, 1).ToUpperInvariant() + name.Substring(1);

    [Pure]
    public static int LevenshteinDistance(string s, string t)
        => LevenshteinDistance(s, t, 1);

    [Pure]
    public static int LevenshteinDistance(string s, string t, int substitutionCost)
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
    public static double LevenshteinDistanceScaled(string s, string t)
        => LevenshteinDistance(s, t) / (double)Math.Max(1, Math.Max(s.Length, t.Length));

    [Pure]
    public static string ToFlatDebugString<T>(IEnumerable<T>? self)
        => "[" +
            self
                .EmptyIfNull()
                .Select(item => item == null ? "" : item.ToString())
                .JoinStrings(", ")
            + "]";

    [Pure]
    static bool IsVowel(char c)
        => c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u' || c == 'A' || c == 'E' || c == 'I' || c == 'O' || c == 'U';

    [Pure]
    public static string Depluralize(string pluralstring)
    {
        if (pluralstring.EndsWith("s", StringComparison.Ordinal)) {
            return pluralstring.Remove(pluralstring.Length - 1);
        }
        if (pluralstring.EndsWith("en", StringComparison.Ordinal)) {
            if (pluralstring.Length >= 4 && IsVowel(pluralstring[pluralstring.Length - 4]) && (pluralstring.Length < 5 || !IsVowel(pluralstring[pluralstring.Length - 5]))) {
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

    public static int? TryParseInt32(this string? input)
    {
        if (int.TryParse(input, out var output)) {
            return output;
        } else {
            return null;
        }
    }

    public static long? TryParseInt64(this string? input)
    {
        if (long.TryParse(input, out var output)) {
            return output;
        } else {
            return null;
        }
    }
}
