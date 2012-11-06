using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils;
using NUnit.Framework;

namespace ProgressOnderwijsUtilsTests
{

	public class EnumHelpersTest
	{
		public enum EnumForTesting
		{
			[MpLabel("Waarde A", "Value A")]
			AValue = 1,
			[MpTooltip("B", "B")]
			BValue,
			XmlValue,
			ValueA,
		}


		[Test]
		public void ListsValuesInOrder()
		{
			PAssert.That(() => new[] { EnumForTesting.AValue, EnumForTesting.BValue, EnumForTesting.XmlValue, EnumForTesting.ValueA }.SequenceEqual(EnumHelpers.GetValues<EnumForTesting>()));
		}

		[Test]
		public void NonEnumsThrow()
		{
			Assert.Throws<ArgumentException>(() => EnumHelpers.TryParse<int>("123"));
		}

		[Test]
		public void TryParsePlain()
		{
			PAssert.That(() => EnumHelpers.TryParse<EnumForTesting>("XmlValue") == EnumForTesting.XmlValue);
			PAssert.That(() => EnumHelpers.TryParse<EnumForTesting>("XmlValues") == null);
			PAssert.That(() => EnumHelpers.TryParse<EnumForTesting>("AValue") == EnumForTesting.AValue);
			PAssert.That(() => EnumHelpers.TryParse<EnumForTesting>("Xml Value") == null);
			PAssert.That(() => EnumHelpers.TryParse<EnumForTesting>("xmlvalue") == EnumForTesting.XmlValue);
			PAssert.That(() => EnumHelpers.TryParse<EnumForTesting>("XMLValue") == EnumForTesting.XmlValue);
			PAssert.That(() => EnumHelpers.TryParse<EnumForTesting>("ValueA") == EnumForTesting.ValueA);
			PAssert.That(() => EnumHelpers.TryParse<EnumForTesting>("avalue") == EnumForTesting.AValue);
		}

		[Test]
		public void GetLabel()
		{
			PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.XmlValue).Translate(Taal.DU).Text == "Xml Value");
			PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.XmlValue).Translate(Taal.EN).Text == "Xml Value");
			PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.BValue).Translate(Taal.NL).Text == "B Value");
			PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.BValue).Translate(Taal.EN).ExtraText == "B");
			PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.BValue).Translate(Taal.DU).ExtraText == "~B");
			PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.AValue).Translate(Taal.DU).Text == "~Waarde A");
			PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.XmlValue).Translate(Taal.DU).ExtraText == null);
			PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.AValue).Translate(Taal.NL).Text == "Waarde A");
			PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.AValue).Translate(Taal.EN).Text == "Value A");
			Assert.Throws<ArgumentOutOfRangeException>(() => EnumHelpers.GetLabel((EnumForTesting)(-1)));
		}

		[Test]
		public void TryParseLabelUntyped()
		{
			PAssert.That(() => EnumHelpers.TryParseLabel(typeof(EnumForTesting), "Waarde A", Taal.NL).SequenceEqual(new Enum[] { EnumForTesting.AValue }));
			PAssert.That(() => EnumHelpers.TryParseLabel(typeof(EnumForTesting), "Waarde A", Taal.EN).SequenceEqual(new Enum[] { }));
			PAssert.That(() => EnumHelpers.TryParseLabel(typeof(EnumForTesting), "AValue", Taal.DU).SequenceEqual(new Enum[] { }));
			PAssert.That(() => EnumHelpers.TryParseLabel(typeof(EnumForTesting), "Value A", Taal.EN).SequenceEqual(new Enum[] { EnumForTesting.AValue, EnumForTesting.ValueA, }));
		}

		[Test]
		public void TryParseLabelTyped()
		{
			PAssert.That(() => EnumHelpers.TryParseLabel<EnumForTesting>("Waarde A", Taal.NL).SequenceEqual(new[] { EnumForTesting.AValue }));
			PAssert.That(() => !EnumHelpers.TryParseLabel<EnumForTesting>("Waarde A", Taal.EN).Any());
			PAssert.That(() => !EnumHelpers.TryParseLabel<EnumForTesting>("AValue", Taal.DU).Any());
			PAssert.That(() => EnumHelpers.TryParseLabel<EnumForTesting>("Value A", Taal.EN).SequenceEqual(new[] { EnumForTesting.AValue, EnumForTesting.ValueA, }));
			PAssert.That(() => EnumHelpers.TryParseLabel<EnumForTesting>("Xml Value", Taal.EN).SequenceEqual(new[] { EnumForTesting.XmlValue }));
			PAssert.That(() => EnumHelpers.TryParseLabel<EnumForTesting>("xml value", Taal.EN).SequenceEqual(new[] { EnumForTesting.XmlValue }));
		}

		[Test]
		public void BasicConverteerTests()
		{
			foreach (var value in EnumHelpers.GetValues<EnumForTesting>())
				foreach (var taal in EnumHelpers.GetValues<Taal>())
					if (taal == Taal.EN && (value == EnumForTesting.ValueA || value == EnumForTesting.AValue))
						PAssert.That(() => Converteer.TryParse(Converteer.ToString(value, taal), typeof(EnumForTesting), taal).State == Converteer.ParseState.Malformed);
					else
						PAssert.That(() => value.Equals(Converteer.Parse(Converteer.ToString(value, taal), typeof(EnumForTesting), taal)));
		}

		[Test]
		public void NullableConverteerTests()
		{
			PAssert.That(() => Converteer.TryParse("waarde a", typeof(EnumForTesting?), Taal.NL).Equals(Converteer.ParseResult.Ok(EnumForTesting.AValue)));
			PAssert.That(() => Converteer.TryParse("XML value", typeof(EnumForTesting?), Taal.EN).Equals(Converteer.ParseResult.Ok(EnumForTesting.XmlValue)));
			PAssert.That(() => Converteer.TryParse("XmlValue", typeof(EnumForTesting), Taal.EN).State == Converteer.ParseState.Malformed);
		}

	}
}
