//#define ENABLE_GDI_MEASUREMENT
using System;
using System.Linq;
using System.Windows.Media;
using ExpressionToCodeLib;
using NUnit.Framework;

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
		public static double Measure(string str) { return str.Sum(c => char_to_width[c]); }
		public static string LimitDisplayLength(string s, int maxWidth) { return LimitTextLength(s, maxWidth).Item1; }
		public static Tuple<string, bool> LimitTextLength(string s, int maxWidth) { return ElideIfNecessary(s, maxWidth * ems_per_char); }
		static Tuple<string, bool> ElideIfNecessary(string s, double ems) { return Measure(s) < ems ? Tuple.Create(s, false) : Tuple.Create(TrimToEms(s, ems - ellipsis_ems) + ellipsis, true); }
		static string TrimToEms(string s, double ems) { return s.Substring(0, CanFitChars(s, ems)); }

		const double ems_per_char = 0.638;
		const string ellipsis = "...";//zou ook … kunnen zijn (unicode ellipsis)
		static readonly double ellipsis_ems;
		static readonly double[] char_to_width;
		static StringMeasurement()
		{
			GlyphTypeface gFont;
			new Typeface("Verdana").TryGetGlyphTypeface(out gFont);
			char_to_width = (
					from c in Enumerable.Range(0, char.MaxValue + 1)
					let characterHasSymbol = gFont.CharacterToGlyphMap.ContainsKey(c)
					select characterHasSymbol ? gFont.AdvanceWidths[gFont.CharacterToGlyphMap[c]] : 0.0
				).ToArray();
			ellipsis_ems = Measure(ellipsis);
		}
		static int CanFitChars(string str, double ems)
		{
			for (int i = 0; i < str.Length; i++)
			{
				ems -= char_to_width[str[i]];
				if (ems < 0.0) return i;
			}
			return str.Length;
		}

	}

	[TestFixture]
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
			PAssert.That(() => StringMeasurement.LimitDisplayLength("Dit is een wat langer verhaal, en zal afgebroken worden: en dat gebeurt hier!", 60) == "Dit is een wat langer verhaal, en zal afgebroken worden: en dat gebeurt ...");

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
