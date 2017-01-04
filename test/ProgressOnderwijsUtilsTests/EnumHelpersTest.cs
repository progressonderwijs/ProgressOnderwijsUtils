using System;
using System.Linq;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using NUnit.Framework;
using Progress.Business;
using Progress.Business.DomainUnits;
using Progress.Business.Test;
using Progress.Business.Text;

namespace ProgressOnderwijsUtilsTests
{
    public class EnumHelpersTest
    {
        public enum EnumForTesting
        {
            [MpLabel("Waarde A")]
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

        [Test]
        [PullRequestTest]
        public void ListsValuesInOrder()
        {
            PAssert.That(
                () =>
                    new[] { EnumForTesting.AValue, EnumForTesting.BValue, EnumForTesting.XmlValue, EnumForTesting.ValueA }.SequenceEqual(
                        EnumHelpers.GetValues<EnumForTesting>()));
        }

        [Test]
        [PullRequestTest]
        public void NonEnumsThrow()
        {
            Assert.Throws<ArgumentException>(() => EnumHelpers.TryParse<int>("123"));
        }

        [Test]
        [PullRequestTest]
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
        [PullRequestTest]
        public void GetLabel()
        {
            PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.XmlValue).Translate(Taal.DU).Text == "Xml value");
            PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.XmlValue).Translate(Taal.EN).Text == "Xml value");
            PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.BValue).Translate(Taal.NL).Text == "B value");
            PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.BValue).Translate(Taal.EN).TooltipText == "B");
            PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.BValue).Translate(Taal.DU).TooltipText == "~B");
            PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.AValue).Translate(Taal.DU).Text == "~Waarde A");
            PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.XmlValue).Translate(Taal.DU).TooltipText == "");
            PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.AValue).Translate(Taal.NL).Text == "Waarde A");
            PAssert.That(() => EnumHelpers.GetLabel(EnumForTesting.AValue).Translate(Taal.EN).Text == "~Waarde A");
            PAssert.That(() => EnumHelpers.GetLabel((EnumForTesting)(-1)).Translate(Taal.NL).Text == "-1");
            //Assert.Throws<ArgumentOutOfRangeException>(() => EnumHelpers.GetLabel((EnumForTesting)(-1)));
        }

        enum UntranslatedTestEnum
        {
            [MpLabelUntranslated("Label")]
            [MpTooltipUntranslated("Tooltip")]
            Member
        }

        [Test]
        [PullRequestTest]
        public void GetLabelWorksWithUntranslatedLabel()
        {
            PAssert.That(() => EnumHelpers.GetLabel(UntranslatedTestEnum.Member).Translate(Taal.EN).Text == "Label");
        }

        [Test]
        [PullRequestTest]
        public void GetLabelWorksWithUntranslatedTooltip()
        {
            PAssert.That(() => EnumHelpers.GetLabel(UntranslatedTestEnum.Member).Translate(Taal.EN).TooltipText == "Tooltip");
        }

        [Test]
        [PullRequestTest]
        public void TryParseLabelUntyped()
        {
            PAssert.That(() => EnumHelpers.ParseLabelOrNull(typeof(EnumForTesting), "Waarde A", Taal.NL).Equals(EnumForTesting.AValue));
            PAssert.That(() => EnumHelpers.ParseLabelOrNull(typeof(EnumForTesting), "Waarde A", Taal.EN) == null);
            PAssert.That(() => EnumHelpers.ParseLabelOrNull(typeof(EnumForTesting), "AValue", Taal.DU) == null);
            PAssert.That(
                () => EnumHelpers.ParseLabelOrNull(typeof(EnumForTesting), "Value A", Taal.EN).Equals(EnumForTesting.ValueA));
        }

        [Test]
        [PullRequestTest]
        public void TryParseLabelForFlagsEnumReturnsCorrectValues()
        {
            PAssert.That(() => EnumHelpers.ParseLabelOrNull<FlagsEnumForTesting>("Waarde A, B value", Taal.NL) == (FlagsEnumForTesting.AValue | FlagsEnumForTesting.BValue));
        }

        [Test]
        [PullRequestTest]
        public void TryParseLabelForFlagsEnumReturnsNothingForIncorrectInput()
        {
            PAssert.That(() => EnumHelpers.ParseLabelOrNull<FlagsEnumForTesting>("Waarde A, XYZ", Taal.NL) == null);
        }

        [Test]
        [PullRequestTest]
        public void BasicConverteerTests()
        {
            foreach (var value in EnumHelpers.GetValues<EnumForTesting>()) {
                foreach (var taal in Translator.AllLanguages) {
                    PAssert.That(() => value.Equals(Converteer.Parse<EnumForTesting>(Converteer.ToString(value, taal), taal)));
                }
            }
        }

        [Test]
        [PullRequestTest]
        public void NullableConverteerTests()
        {
            PAssert.That(() => Converteer.TryParse<EnumForTesting?>("waarde a", Taal.NL).Equals(Converteer.ParseResult.Ok(EnumForTesting.AValue)));
            PAssert.That(() => Converteer.TryParse<EnumForTesting?>("XML value", Taal.EN).Equals(Converteer.ParseResult.Ok(EnumForTesting.XmlValue)));
            PAssert.That(() => Converteer.TryParse<EnumForTesting>("XmlValue", Taal.EN).State == Converteer.ParseState.Malformed);
        }

        [Test]
        [PullRequestTest]
        public void FlagsEnumGetLabel()
        {
            PAssert.That(() => EnumHelpers.GetLabel(FlagsEnumForTesting.BValue).Translate(Taal.NL).TooltipText == "B");
            PAssert.That(() => EnumHelpers.GetLabel(FlagsEnumForTesting.ValueC).Translate(Taal.NL).Text == "Value C");
            PAssert.That(() => EnumHelpers.GetLabel(FlagsEnumForTesting.ABValue).Translate(Taal.NL).Text == "AB value");
            PAssert.That(() => EnumHelpers.GetLabel(FlagsEnumForTesting.AValue | FlagsEnumForTesting.ValueC).Translate(Taal.NL).Text == "Waarde A, Value C");
            PAssert.That(
                () =>
                    EnumHelpers.GetLabel(FlagsEnumForTesting.AValue | FlagsEnumForTesting.BValue | FlagsEnumForTesting.ValueC).Translate(Taal.NL).Text == "Waarde A, BC value");
        }

        [Test]
        [PullRequestTest]
        public void EnumRoundTrips()
        {
            foreach (var taal in Translator.AllLanguages) {
                foreach (var val in EnumHelpers.GetValues<EnumForTesting>()) {
                    if (taal != Taal.EN || val != EnumForTesting.AValue && val != EnumForTesting.ValueA) {
                        var str = EnumHelpers.GetLabel(val).Translate(taal).Text;
                        PAssert.That(() => EnumHelpers.ParseLabelOrNull<EnumForTesting>(str, taal) == val);
                    }
                }
            }
        }

        [Test]
        [PullRequestTest]
        public void FlagsEnumRoundTrips()
        {
            var values = (
                from flag1 in EnumHelpers.GetValues<FlagsEnumForTesting>()
                from flag2 in EnumHelpers.GetValues<FlagsEnumForTesting>()
                select flag1 | flag2).Distinct();

            foreach (var taal in Translator.AllLanguages) {
                foreach (var combo in values) {
                    var str = EnumHelpers.GetLabel(combo).Translate(taal).Text;
                    PAssert.That(() => EnumHelpers.ParseLabelOrNull<FlagsEnumForTesting>(str, taal) == combo);
                }
            }
        }

        [Test]
        [PullRequestTest]
        public void GetAttrsOn()
        {
            Assert.That(EnumHelpers.MetaDataForValue(VerblijfsvergunningType.AsielBepaaldeTijd).Attributes<BronHoCodeAttribute>().Single().Code, Is.EqualTo("3"));
        }

        [Test]
        [PullRequestTest]
        public void GetAttrsFrom()
        {
            Assert.That(
                EnumTypeMetaData<VerblijfsvergunningType>.Instance.GetValuesByAttribute<BronHoCodeAttribute>(attr => attr.Code == "3").Single(),
                Is.EqualTo(VerblijfsvergunningType.AsielBepaaldeTijd));
        }
    }
}
