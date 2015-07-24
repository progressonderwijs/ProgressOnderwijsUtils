//#define ENABLE_GDI_MEASUREMENT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using NUnit.Framework;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtils
{
    public static class StringMeasurement
    {
#if ENABLE_GDI_MEASUREMENT
		public static string LimitDLengthGdi(string s, int maxWidth)
		{
			using (var bmp = new Bitmap(1, 1))
			using (Graphics g = Graphics.FromImage(bmp))
			{
				int hei, len;
				g.MeasureString(s.Replace('\n', ' ').Replace('\t', ' '), new Font("Verdana", 8), new SizeF((float)(maxWidth * pix_per_char), 10.0f), StringFormat.GenericDefault, out len, out hei);
				return len >= 2 && len < s.Length ? s.Substring(0, len - 2) + ellipsis : s;
			}
		}
		public static double MeasureGdi(string s)//MeasureGdi(s.Trim()) is roughly MeasureWpf(s.Trim()+" ") - no idea why gdi adds a space, makes no sense.
		{
			using (var bmp = new Bitmap(1, 1))
			using (Graphics g = Graphics.FromImage(bmp))
			{
				return g.MeasureString(s.Replace('\n', ' ').Replace('\t', ' '), new Font("Verdana", 8)).Width / pix_per_char * ems_per_char;
			}
		}
		const double pix_per_char = 7.0;
#endif

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
        {
            return Measure(s) < ems ? Tuple.Create(s, false) : Tuple.Create(TrimToEms(s, ems - ellipsis_ems) + ellipsis, true);
        }

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
#if ENABLE_GDI_MEASUREMENT
		[Test]
		public void TestGdiVersion()
		{
			PAssert.That(() => StringMeasurement.LimitDLengthGdi("Dit is een wat langer verhaal, en zal afgebroken worden: en dat gebeurt hier!", 60) == "Dit is een wat langer verhaal, en zal afgebroken worden: en dat gebeurt ...");

			PAssert.That(() => StringMeasurement.LimitDLengthGdi("123456789", 5) == "12...");
			PAssert.That(() => StringMeasurement.LimitDLengthGdi("0,123456789", 5) == "0,...");
			PAssert.That(() => StringMeasurement.LimitDLengthGdi("1.024", 5) == "1....");
			PAssert.That(() => StringMeasurement.LimitDLengthGdi("1.02", 5) == "1.02");
			PAssert.That(() => StringMeasurement.LimitDLengthGdi("testjes", 5) == "test...");
			PAssert.That(() => StringMeasurement.LimitDLengthGdi("empty.pdf", 5) == "em...");
			PAssert.That(() => StringMeasurement.LimitDLengthGdi("wheee!", 5) == "wh...");
			PAssert.That(() => StringMeasurement.LimitDLengthGdi("wheee!\ntwolines!", 5) == "wh...");

			PAssert.That(() => StringMeasurement.LimitDLengthGdi("wheee!\ntwolines!", 10) == "wheee!\nt...");
		}
#endif

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
