using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Media;
using NUnit.Framework;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils.Converteer
{
	public static class StringMeasurement
	{
		public static string LimitTextLengthGdi(string s, int maxWidth)
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
		public static double MeasureWpf(string str) { return str.Sum(c => char_to_width[c]); }
		public static string LimitTextLengthWpf(string s, int maxWidth) { return LimitTextLength(s, maxWidth).Item1; }
		public static Tuple<string, bool> LimitTextLength(string s, int maxWidth) { return ElideIfNecessary(s, maxWidth * ems_per_char); }
		static Tuple<string, bool> ElideIfNecessary(string s, double ems) { return MeasureWpf(s) < ems ? Tuple.Create(s, false) : Tuple.Create(TrimToEms(s, ems - ellipsis_ems) + ellipsis, true); }
		static string TrimToEms(string s, double ems) { return s.Substring(0, CanFitChars(s, ems)); }

		const double ems_per_char = 0.638;
		const double pix_per_char = 7.0;
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
			ellipsis_ems = MeasureWpf(ellipsis);
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
		[Test]
		public void TestGdiVersion()
		{
			PAssert.That(() => StringMeasurement.LimitTextLengthGdi("Dit is een wat langer verhaal, en zal afgebroken worden: en dat gebeurt hier!", 60) == "Dit is een wat langer verhaal, en zal afgebroken worden: en dat gebeurt ...");

			PAssert.That(() => StringMeasurement.LimitTextLengthGdi("123456789", 5) == "12...");
			PAssert.That(() => StringMeasurement.LimitTextLengthGdi("0,123456789", 5) == "0,...");
			PAssert.That(() => StringMeasurement.LimitTextLengthGdi("1.024", 5) == "1....");
			PAssert.That(() => StringMeasurement.LimitTextLengthGdi("1.02", 5) == "1.02");
			PAssert.That(() => StringMeasurement.LimitTextLengthGdi("testjes", 5) == "test...");
			PAssert.That(() => StringMeasurement.LimitTextLengthGdi("empty.pdf", 5) == "em...");
			PAssert.That(() => StringMeasurement.LimitTextLengthGdi("wheee!", 5) == "wh...");
			PAssert.That(() => StringMeasurement.LimitTextLengthGdi("wheee!\ntwolines!", 5) == "wh...");

			PAssert.That(() => StringMeasurement.LimitTextLengthGdi("wheee!\ntwolines!", 10) == "wheee!\nt...");
		}
		[Test]
		public void TestWpfVersion()
		{
			PAssert.That(() => StringMeasurement.LimitTextLengthWpf("Dit is een wat langer verhaal, en zal afgebroken worden: en dat gebeurt hier!", 60) == "Dit is een wat langer verhaal, en zal afgebroken worden: en dat gebeurt ...");

			PAssert.That(() => StringMeasurement.LimitTextLengthWpf("123456789", 5) == "123...");
			PAssert.That(() => StringMeasurement.LimitTextLengthWpf("0,123456789", 5) == "0,1...");
			PAssert.That(() => StringMeasurement.LimitTextLengthWpf("1.024", 5) == "1.024");
			PAssert.That(() => StringMeasurement.LimitTextLengthWpf("testjes", 5) == "test...");
			PAssert.That(() => StringMeasurement.LimitTextLengthWpf("empty.pdf", 5) == "em...");
			PAssert.That(() => StringMeasurement.LimitTextLengthWpf("wheee!", 5) == "whe...");
			PAssert.That(() => StringMeasurement.LimitTextLengthWpf("wheee!\ntwolines!", 5) == "whe...");

			PAssert.That(() => StringMeasurement.LimitTextLengthWpf("wheee!\ntwolines!", 10) == "wheee!\ntw...");
		}
	}
}
