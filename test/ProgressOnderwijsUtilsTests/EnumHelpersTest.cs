using System;
using System.Linq;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using NUnit.Framework;
using Progress.Business;
using Progress.Business.DomainUnits;
using Progress.Business.Test;
using Progress.Business.Text;
using ProgressOnderwijsUtils;

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
            [UsedImplicitly]
            Nothing = 0,

            [MpLabel("Waarde A", "Value A")]
            AValue = 1,

            [MpTooltip("B", "B")]
            BValue = 2,
            ABValue = 3,
            ValueC = 4,

            [UsedImplicitly]
            BCValue = 6,
        }

        [Test, Continuous]
        public void ListsValuesInOrder()
        {
            PAssert.That(
                () =>
                    new[] { EnumForTesting.AValue, EnumForTesting.BValue, EnumForTesting.XmlValue, EnumForTesting.ValueA }.SequenceEqual(
                        EnumHelpers.GetValues<EnumForTesting>()));
        }

        [Test, Continuous]
        public void NonEnumsThrow()
        {
            Assert.Throws<ArgumentException>(() => EnumHelpers.TryParse<int>("123"));
        }

        [Test, Continuous]
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

        [Test, Continuous]
        public void GetLabel()
        {
            PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.XmlValue).Translate(Taal.DU).Text == "Xml value");
            PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.XmlValue).Translate(Taal.EN).Text == "Xml value");
            PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.BValue).Translate(Taal.NL).Text == "B value");
            PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.BValue).Translate(Taal.EN).ExtraText == "B");
            PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.BValue).Translate(Taal.DU).ExtraText == "~B");
            PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.AValue).Translate(Taal.DU).Text == "~Waarde A");
            PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.XmlValue).Translate(Taal.DU).ExtraText == "");
            PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.AValue).Translate(Taal.NL).Text == "Waarde A");
            PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.AValue).Translate(Taal.EN).Text == "Value A");
            PAssert.That(() => EnumHelpers.GetLabel((EnumForTesting)(-1)).Translate(Taal.NL).Text == "-1");
            //Assert.Throws<ArgumentOutOfRangeException>(() => EnumHelpers.GetLabel((EnumForTesting)(-1)));
        }

        enum UntranslatedTestEnum
        {
            [MpLabelUntranslated("Label"), MpTooltipUntranslated("Tooltip")]
            Member
        }

        [Test, Continuous]
        public void GetLabelWorksWithUntranslatedLabel()
        {
            PAssert.That(() => EnumHelpers.GetLabel(UntranslatedTestEnum.Member).Translate(Taal.EN).Text == "Label");
        }

        [Test, Continuous]
        public void GetLabelWorksWithUntranslatedTooltip()
        {
            PAssert.That(() => EnumHelpers.GetLabel(UntranslatedTestEnum.Member).Translate(Taal.EN).ExtraText == "Tooltip");
        }

        [Test, Continuous]
        public void TryParseLabelUntyped()
        {
            PAssert.That(() => EnumHelpers.TryParseLabel(typeof(EnumForTesting), "Waarde A", Taal.NL).SequenceEqual(new Enum[] { EnumForTesting.AValue }));
            PAssert.That(() => EnumHelpers.TryParseLabel(typeof(EnumForTesting), "Waarde A", Taal.EN).SequenceEqual(new Enum[] { }));
            PAssert.That(() => EnumHelpers.TryParseLabel(typeof(EnumForTesting), "AValue", Taal.DU).SequenceEqual(new Enum[] { }));
            PAssert.That(
                () => EnumHelpers.TryParseLabel(typeof(EnumForTesting), "Value A", Taal.EN).SequenceEqual(new Enum[] { EnumForTesting.AValue, EnumForTesting.ValueA, }));
        }

        [Test, Continuous]
        public void TryParseLabelTyped()
        {
            PAssert.That(() => EnumHelpers.TryParseLabel<EnumForTesting>("Waarde A", Taal.NL).SequenceEqual(new[] { EnumForTesting.AValue }));
            PAssert.That(() => !EnumHelpers.TryParseLabel<EnumForTesting>("Waarde A", Taal.EN).Any());
            PAssert.That(() => !EnumHelpers.TryParseLabel<EnumForTesting>("AValue", Taal.DU).Any());
            PAssert.That(() => EnumHelpers.TryParseLabel<EnumForTesting>("Value A", Taal.EN).SequenceEqual(new[] { EnumForTesting.AValue, EnumForTesting.ValueA, }));
            PAssert.That(() => EnumHelpers.TryParseLabel<EnumForTesting>("Xml Value", Taal.EN).SequenceEqual(new[] { EnumForTesting.XmlValue }));
            PAssert.That(() => EnumHelpers.TryParseLabel<EnumForTesting>("xml value", Taal.EN).SequenceEqual(new[] { EnumForTesting.XmlValue }));
        }

        [Test, Continuous]
        public void TryParseLabelForFlagsEnumReturnsCorrectValues()
        {
            PAssert.That(() => EnumHelpers.TryParseLabel<FlagsEnumForTesting>("Waarde A, B value", Taal.NL).SequenceEqual(new[] { FlagsEnumForTesting.AValue | FlagsEnumForTesting.BValue }));
        }

        [Test, Continuous]
        public void TryParseLabelForFlagsEnumReturnsNothingForIncorrectInput()
        {
            PAssert.That(() => EnumHelpers.TryParseLabel<FlagsEnumForTesting>("Waarde A, XYZ", Taal.NL).None());
        }

        [Test, Continuous]
        public void BasicConverteerTests()
        {
            foreach (var value in EnumHelpers.GetValues<EnumForTesting>()) {
                foreach (var taal in Translator.AllLanguages) {
                    if (taal == Taal.EN && (value == EnumForTesting.ValueA || value == EnumForTesting.AValue)) {
                        PAssert.That(() => Converteer.TryParse<EnumForTesting>(Converteer.ToString(value, taal), taal).State == Converteer.ParseState.Malformed);
                    } else {
                        PAssert.That(() => value.Equals(Converteer.Parse<EnumForTesting>(Converteer.ToString(value, taal), taal)));
                    }
                }
            }
        }

        [Test, Continuous]
        public void NullableConverteerTests()
        {
            PAssert.That(() => Converteer.TryParse<EnumForTesting?>("waarde a", Taal.NL).Equals(Converteer.ParseResult.Ok(EnumForTesting.AValue)));
            PAssert.That(() => Converteer.TryParse<EnumForTesting?>("XML value", Taal.EN).Equals(Converteer.ParseResult.Ok(EnumForTesting.XmlValue)));
            PAssert.That(() => Converteer.TryParse<EnumForTesting>("XmlValue", Taal.EN).State == Converteer.ParseState.Malformed);
        }

        [Test, Continuous]
        public void FlagsEnumGetLabel()
        {
            PAssert.That(() => EnumHelpers.GetLabel(FlagsEnumForTesting.BValue).Translate(Taal.NL).ExtraText == "B");
            PAssert.That(() => EnumHelpers.GetLabel(FlagsEnumForTesting.ValueC).Translate(Taal.NL).Text == "Value C");
            PAssert.That(() => EnumHelpers.GetLabel(FlagsEnumForTesting.ABValue).Translate(Taal.NL).Text == "AB value");
            PAssert.That(() => EnumHelpers.GetLabel(FlagsEnumForTesting.AValue | FlagsEnumForTesting.ValueC).Translate(Taal.NL).Text == "Waarde A, Value C");
            PAssert.That(
                () =>
                    EnumHelpers.GetLabel(FlagsEnumForTesting.AValue | FlagsEnumForTesting.BValue | FlagsEnumForTesting.ValueC).Translate(Taal.NL).Text == "Waarde A, BC value");
        }

        [Test, Continuous]
        public void EnumRoundTrips()
        {
            foreach (var taal in Translator.AllLanguages) {
                foreach (var val in EnumHelpers.GetValues<EnumForTesting>()) {
                    if (taal != Taal.EN || val != EnumForTesting.AValue && val != EnumForTesting.ValueA) {
                        var str = EnumHelpers.GetLabel(val).Translate(taal).Text;
                        PAssert.That(() => EnumHelpers.TryParseLabel<EnumForTesting>(str, taal).SequenceEqual(new[] { val }));
                    }
                }
            }
        }

        [Test, Continuous]
        public void FlagsEnumRoundTrips()
        {
            var values = (
                from flag1 in EnumHelpers.GetValues<FlagsEnumForTesting>()
                from flag2 in EnumHelpers.GetValues<FlagsEnumForTesting>()
                select flag1 | flag2).Distinct();

            foreach (var taal in Translator.AllLanguages) {
                foreach (var combo in values) {
                    var str = EnumHelpers.GetLabel(combo).Translate(taal).Text;
                    PAssert.That(() => EnumHelpers.TryParseLabel<FlagsEnumForTesting>(str, taal).SequenceEqual(new[] { combo }));
                }
            }
        }

        [Test, Continuous]
        public void GetAttrsOn()
        {
            Assert.That(EnumHelpers.MetaDataForValue(VerblijfsvergunningType.AsielBepaaldeTijd).Attributes<BronHoCodeAttribute>().Single().Code, Is.EqualTo("3"));
        }

        [Test, Continuous]
        public void GetAttrsFrom()
        {
            Assert.That(
                EnumHelpers.GetAttrs<BronHoCodeAttribute>.From<VerblijfsvergunningType>(attr => attr.Code == "3").Single(),
                Is.EqualTo(VerblijfsvergunningType.AsielBepaaldeTijd));
        }
    }
}
