using System;
using System.Windows.Media;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using NUnit.Framework;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtils
{
    public static class StringMeasurement
    {
        [Pure]
        public static double Measure(string str)
        {
            double sum = 0;
            if (str != null) {
                foreach (char c in str) {
                    sum += char_to_width[c];
                }
            }
            return sum;
        }

        [Pure]
        public static string LimitDisplayLength(string s, int maxWidth) => LimitTextLength(s, maxWidth).Item1;

        [Pure]
        public static Tuple<string, bool> LimitTextLength(string s, int maxWidth) => ElideIfNecessary(s, maxWidth * ems_per_char);

        [Pure]
        static Tuple<string, bool> ElideIfNecessary(string s, double ems)
            => Measure(s) < ems ? Tuple.Create(s, false) : Tuple.Create(TrimToEms(s, ems - ellipsis_ems) + ellipsis, true);

        [Pure]
        static string TrimToEms(string s, double ems) => s.Substring(0, CanFitChars(s, ems));

        const double ems_per_char = 0.638;
        const string ellipsis = "..."; //zou ook … kunnen zijn (unicode ellipsis)
        static readonly double ellipsis_ems;
        static readonly double[] char_to_width;

        static StringMeasurement()
        {
            GlyphTypeface gFont;
            new Typeface("Verdana").TryGetGlyphTypeface(out gFont);
            if (gFont == null) {
                new Typeface("Tahoma").TryGetGlyphTypeface(out gFont);
            }
            char_to_width = new double[char.MaxValue + 1];
            for (int i = 0; i < char_to_width.Length; i++) {
                var c = (char)i;
                if (gFont.CharacterToGlyphMap.ContainsKey(c)) {
                    char_to_width[i] = gFont.AdvanceWidths[gFont.CharacterToGlyphMap[c]];
                }
            }
            ellipsis_ems = Measure(ellipsis);
        }

        [Pure]
        static int CanFitChars(string str, double ems)
        {
            for (int i = 0; i < str.Length; i++) {
                ems -= char_to_width[str[i]];
                if (ems < 0.0) {
                    return i;
                }
            }
            return str.Length;
        }
    }

    [Continuous]
    public class StringMeasurementTest
    {
        [Test]
        public void TestWpfVersion()
        {
            PAssert.That(
                () =>
                    StringMeasurement.LimitDisplayLength("Dit is een wat langer verhaal, en zal afgebroken worden: en dat gebeurt hier!", 60)
                        == "Dit is een wat langer verhaal, en zal afgebroken worden: en dat gebeurt ...");

            PAssert.That(() => StringMeasurement.LimitDisplayLength("123456789", 5) == "123...");
            PAssert.That(() => StringMeasurement.LimitDisplayLength("0,123456789", 5) == "0,1...");
            PAssert.That(() => StringMeasurement.LimitDisplayLength("1.024", 5) == "1.024");
            PAssert.That(() => StringMeasurement.LimitDisplayLength("testjes", 5) == "test...");
            PAssert.That(() => StringMeasurement.LimitDisplayLength("empty.pdf", 5) == "em...");
            PAssert.That(() => StringMeasurement.LimitDisplayLength("wheee!", 5) == "whe...");
            PAssert.That(() => StringMeasurement.LimitDisplayLength("wheee!\ntwolines!", 5) == "whe...");

            PAssert.That(() => StringMeasurement.LimitDisplayLength("wheee!\ntwolines!", 10) == "wheee!\ntw...");
        }
    }
}
