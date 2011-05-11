using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;
using TheJoyOfCode.QualityTools;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public class PodTest
	{
		public static void ComparePod(object a, object b)
		{

			object[] empty = new object[0];
			var flags = BindingFlags.Public | BindingFlags.Instance;
			Func<object, IEnumerable<Tuple<string,object>>> getAccessors =
				o => o.GetType().GetProperties(flags).Select(pi => Tuple.Create(pi.Name, pi.GetValue(o, empty)))
				        	.Concat(
								o.GetType().GetFields(flags).Select(fi => Tuple.Create(fi.Name, fi.GetValue(o)))
				        	);

			var aProps = getAccessors(a);
			var bProps = getAccessors(b);
			PAssert.That(() => aProps.Select(p => p.Item1).OrderBy(s => s).SequenceEqual(bProps.Select(p => p.Item1).OrderBy(s => s)));

			PAssert.That(
				() =>
					!(from aProp in aProps
					  join bProp in bProps on aProp.Item1 equals bProp.Item1
					  where !Equals(aProp.Item2, bProp.Item2)
					  select aProp.Item1).Any());
		}

		public static void AutomaticClassTest<T>(T sample)
		{
			PropertyTester ptester = new PropertyTester(sample);
			ptester.TestProperties();
			ConstructorTester ctester = new ConstructorTester(typeof(T));
			ctester.TestConstructors(true);
		}

		[Test]
		public void SanityCheck()
		{
			Assert.Throws<PAssertFailedException>(() => ComparePod(new LabelCode("abc", false), new { waarde = "abc", Vrijveld = false }));//case-sensitive
			Assert.Throws<PAssertFailedException>(() => ComparePod(new LabelCode("abc", false), new { Waarde = "Abc", Vrijveld = false }));//value-sensitive
			Assert.Throws<PAssertFailedException>(() => ComparePod(new LabelCode(null, false), new { Waarde = "", Vrijveld = false }));//no weirdness
		}

		[Test]
		public void LabelCodeTest()
		{
			ComparePod(new LabelCode("abc", false), new { Waarde = "abc", Vrijveld = false });
			ComparePod(new LabelCode(null, true), new { Waarde = default(string), Vrijveld = true });
			ComparePod(new LabelCode("", true), new { Waarde = "", Vrijveld = true });
			AutomaticClassTest(new LabelCode("ha", false));
		}

		[Test]
		public void OlapCommonTest()
		{
			var olapcommon_sample = new OlapCommon(2, 3, 4, 5, false, OlapCommon.VoltijdType.Deeltijd, OlapCommon.EoiType.Eoi, OlapCommon.NrOplType.MeerOpl,
					OlapCommon.Per1OktType.Alle, OlapCommon.VooroplType.NonVwo, OlapCommon.HerinschrijverType.HerinschrOpl,
					OlapCommon.RijDimensieType.Cohorten, OlapCommon.CelSomType.AbsenPercPerRij, 9,
					OlapCommon.StudieStaakType.AlleenstudiestakersOpl)
					{
						EcGrenswaarde = 42
					};
			ComparePod(
				olapcommon_sample
				,
				new
				{
					ExtraRijdimensie = "",
					Rendement = false,
					Samenvatting = true,
					LangeLijst = false,
					TopZoveel = false,
					Organisatie = 2,
					StartJaar = 3,
					//StopJaar = stopjaar,//niet gebruikt
					ExamenType = 5,
					RijDimensie = OlapCommon.RijDimensieType.Cohorten,
					VolofDeeltijd = OlapCommon.VoltijdType.Deeltijd,
					isEoi = OlapCommon.EoiType.Eoi,
					eenOpl = OlapCommon.NrOplType.MeerOpl,
					per1Okt = OlapCommon.Per1OktType.Alle,
					Vooropleiding = OlapCommon.VooroplType.NonVwo,
					isHerinschrijver = OlapCommon.HerinschrijverType.HerinschrOpl,
					ToonPerCel = OlapCommon.CelSomType.AbsenPercPerRij,
					opbasisvanCohort = false,
					StudiejaarNr = 9,
					isStudiestaker = OlapCommon.StudieStaakType.AlleenstudiestakersOpl,
					EerstejrNietdef = DateTime.Now.CollegeJaar(),//blech
					ShowTijdsverloop = false,
                    EcGrenswaarde = 42
				}
				);

			AutomaticClassTest(olapcommon_sample);
		}


		[Test]
		public void OlapSlicerTest()
		{
			var olapslicer_sample = new OlapSlicer(1, 2, 3, 4)
			{
				LangeLijst = true,
				Opleiding = 6,
				PreciesEenCohort = false,
				Rendement = true,
				Rijen = "abc",
				Samenvatting = false,
				ZuiverCohort = true,
			};
			ComparePod(olapslicer_sample,
			new
			{
				ExamenType = 4,
				LangeLijst = true,
				Opleiding = 6,
				PreciesEenCohort = false,
				Rendement = true,
				Rijen = "abc",
				Samenvatting = false,
				ZuiverCohort = true,

				Organisatie = 1,
				StartJaar = 2,
				StopJaar = 3,
				StudieStaak = OlapSlicer.StudieStaakType.metstudiestakers,
				ShowOpleidingen = true,
			});
			AutomaticClassTest(olapslicer_sample);
		}

		[Test]
		public void RegioTest()
		{
			PAssert.That(() => new Regio("wonderland", new[] { "abc", "def", "ghi" }).Member ==
			 "member [Provincie].[provincie].[wonderland] as Sum({ [Provincie].[provincie].[abc],[Provincie].[provincie].[def],[Provincie].[provincie].[ghi]}) ");
		}

		[Test]
		public void SLBerichtSamenvattingTest()
		{
			var a = new SLBerichtSamenvatting
			{
				Berichttype = "abc",
				FormNaam = "def",
				Ontvanger = "qwerty",
				Organisatie = 1,
				Student = 2,
				Studielinkberichtid = 3,
				StudielinkNummer = 4,
				Tekst = "asdfg",
				Zender = "zxcvb",
			};
			var b = new SLBerichtSamenvatting
			{
				Berichttype = "abc",
				FormNaam = "def",
				Ontvanger = "qwerty",
				Organisatie = 1,
				Student = 2,
				Studielinkberichtid = 3,
				StudielinkNummer = 4,
				Tekst = "asdfg",
				Zender = "zxcvb",
			};
			var c =
				new
				{
					Berichttype = "abc",
					FormNaam = "def",
					Ontvanger = "qwerty",
					Organisatie = 1,
					Student = 2,
					Studielinkberichtid = 3,
					StudielinkNummer = 4,
					Tekst = "asdfg",
					Zender = "zxcvb",
				};
			ComparePod(a, b);
			ComparePod(a, c);
			PAssert.That(() => !ReferenceEquals(a, b) && Equals(a, b) && a.GetHashCode() == b.GetHashCode() && a.GetHashCode() != c.GetHashCode() && !Equals(a, c));

			AutomaticClassTest(a);
		}

		[Test]
		public void ServiceTest()
		{
			AutomaticClassTest(new ServiceOnderwijs());
			AutomaticClassTest(new ServiceOrganisatie());
			AutomaticClassTest(new ServiceVakInformatie());
			AutomaticClassTest(new VakPeriode());
			AutomaticClassTest(new EntreeVoorwaarde());
			AutomaticClassTest(new ToetsVorm());
			AutomaticClassTest(new OnderwijsVorm());
			AutomaticClassTest(new OnderwijsNiveau());
			AutomaticClassTest(new Literatuur());
			AutomaticClassTest(new BSADossierData());
			ComparePod(new School("abc", "def"), new { Brincode = "abc", Volgnummer = "def" });
		}
	}
}
