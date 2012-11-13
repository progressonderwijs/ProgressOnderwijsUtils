using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using Progress.Business;
using ProgressOnderwijsUtils;
using NUnit.Framework;
using ProgressOnderwijsUtils.Test;
using Progress.WebApp.Base;

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

		[Flags]
		public enum FlagsEnumForTesting
		{
			Nothing = 0,

			[MpLabel("Waarde A", "Value A")]
			AValue = 1,
			[MpTooltip("B", "B")]
			BValue = 2,
			ABValue = 3,
			ValueC = 4,
			BCValue = 6,
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
			PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.XmlValue).Translate(Taal.DU).Text == "xml value");
			PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.XmlValue).Translate(Taal.EN).Text == "xml value");
			PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.BValue).Translate(Taal.NL).Text == "B value");
			PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.BValue).Translate(Taal.EN).ExtraText == "B");
			PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.BValue).Translate(Taal.DU).ExtraText == "~B");
			PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.AValue).Translate(Taal.DU).Text == "~Waarde A");
			PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.XmlValue).Translate(Taal.DU).ExtraText == "");
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
				foreach (var taal in EnumHelpers.GetValues<Taal>().Except(new[] { Taal.None }))
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

		[Test]
		public void FlagsEnumGetLabel()
		{
			PAssert.That(() => EnumHelpers.GetLabel(FlagsEnumForTesting.BValue).Translate(Taal.NL).ExtraText == "B");
			PAssert.That(() => EnumHelpers.GetLabel(FlagsEnumForTesting.ValueC).Translate(Taal.NL).Text == "value C");
			PAssert.That(() => EnumHelpers.GetLabel(FlagsEnumForTesting.ABValue).Translate(Taal.NL).Text == "AB value");
			PAssert.That(() => EnumHelpers.GetLabel(FlagsEnumForTesting.AValue | FlagsEnumForTesting.ValueC).Translate(Taal.NL).Text == "Waarde A, value C");
			PAssert.That(() => EnumHelpers.GetLabel(FlagsEnumForTesting.AValue | FlagsEnumForTesting.BValue | FlagsEnumForTesting.ValueC).Translate(Taal.NL).Text == "Waarde A, BC value");
		}

		[Test]
		public void EnumRoundTrips()
		{
			foreach (var taal in EnumHelpers.GetValues<Taal>().Except(new[] { Taal.None }))
				foreach (var val in EnumHelpers.GetValues<EnumForTesting>())
					if (taal != Taal.EN || val != EnumForTesting.AValue && val != EnumForTesting.ValueA)
					{
						var str = EnumHelpers.GetLabel(val).Translate(taal).Text;
						PAssert.That(() => EnumHelpers.TryParseLabel<EnumForTesting>(str, taal).SequenceEqual(new[] { val }));
					}
		}

		[Test]
		public void FlagsEnumRoundTrips()
		{
			var values = (
				from flag1 in EnumHelpers.GetValues<FlagsEnumForTesting>()
				from flag2 in EnumHelpers.GetValues<FlagsEnumForTesting>()
				select flag1 | flag2).Distinct();

			foreach (var taal in EnumHelpers.GetValues<Taal>().Except(new[] { Taal.None }))
				foreach (var combo in values)
				{
					var str = EnumHelpers.GetLabel(combo).Translate(taal).Text;
					PAssert.That(() => EnumHelpers.TryParseLabel<FlagsEnumForTesting>(str, taal).SequenceEqual(new[] { combo }));
				}
		}

		[NightlyOnly, Test]
		public void AllEnumsHaveUniqueLabels()
		{
			var enumTypes =
				from coreType in new[] { typeof(BusinessConnection), typeof(TreeExtensions), typeof(SessionManager) }
				from enumType in coreType.Assembly.GetTypes()
				where enumType.IsEnum && !enumType.ContainsGenericParameters
				select enumType;
			foreach (var enumType in enumTypes)
				foreach (var taal in new[] { Taal.NL, Taal.EN, Taal.DU })
					foreach (var val in EnumHelpers.GetValues(enumType))
					{
						var str = EnumHelpers.GetLabel(val).Translate(taal).Text;
						PAssert.That(() => EnumHelpers.TryParseLabel(enumType, str, taal).SequenceEqual(new[] { val }));
					}
		}
	}
}
