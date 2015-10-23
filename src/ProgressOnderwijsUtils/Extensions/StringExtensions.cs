using System.Linq;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using NUnit.Framework;
using ProgressOnderwijsUtils.Test;

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

        public static T ToEnum<T>(this string value, bool canAddValue = false)
        {
            T outEnum;
            int valueIsNumeric;
            if (Enum.IsDefined(typeof(T), value) || (canAddValue && int.TryParse(value, out valueIsNumeric))) {
               outEnum = (T) Enum.Parse(typeof(T), value);
            } else {
                throw new InvalidEnumArgumentException($"{value} niet in {typeof(T).FullName} {(canAddValue ? "canAddValue:true kan alleen met numerieke waarden" : "")}");
            }
            return outEnum;
        }

        [Pure]
        public static string Replace(this string s, IEnumerable<KeyValuePair<string, string>> replacements) => replacements.Aggregate(s, (current, replacement) => current.Replace(replacement.Key, replacement.Value));
    }

    public static class TestData
    {

        public enum BerichtTypes
        {
            vchmsg06onderhoudennaw
        }

        public enum Opleiding
        { }
    
    }

    [Continuous]
    public sealed class ExtensionsTest
    {
        [Test]
        public void ToEnumTest()
        {
            var n23 = "23".ToEnum<TestData.Opleiding>(true);
            var n75612 = 75612.ToString().ToEnum<TestData.Opleiding>(true);
            var msg06Type = "vchmsg06onderhoudennaw".ToEnum<TestData.BerichtTypes>();
            PAssert.That(() => msg06Type == TestData.BerichtTypes.vchmsg06onderhoudennaw);
            Assert.Throws<InvalidEnumArgumentException>(() => "vchmsg25herinschrijving".ToEnum<TestData.BerichtTypes>());
            Assert.Throws<InvalidEnumArgumentException>(() => 23.ToString().ToEnum<TestData.BerichtTypes>());
            Assert.Throws<InvalidEnumArgumentException>(() => "vchmsg03inschrijving".ToEnum<TestData.BerichtTypes>(true));
            Assert.DoesNotThrow(() => 75612.ToString().ToEnum<TestData.BerichtTypes>(true));
            PAssert.That(() => n23 == (TestData.Opleiding)23);
            PAssert.That(() => n75612 != (TestData.Opleiding)23);
            PAssert.That(() => n75612 == (TestData.Opleiding)75612);
            var add2berichttype = 75612.ToString().ToEnum<TestData.BerichtTypes>(true);
            PAssert.That(() =>  add2berichttype == (TestData.BerichtTypes)75612);
        }
    }
}
