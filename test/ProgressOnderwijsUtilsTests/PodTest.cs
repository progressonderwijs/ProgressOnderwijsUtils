using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public class PodTest
	{
		public static void ComparePod(object a, object b)
		{

			Type aType = a.GetType();
			Type bType = b.GetType();
			PAssert.That(() => aType.GetProperties().Select(pi => pi.Name).OrderBy(s => s).SequenceEqual(bType.GetProperties().Select(pi => pi.Name).OrderBy(s => s)));

			object[] empty = new object[0];

			PAssert.That(
				() =>
					!(from aProp in aType.GetProperties()
					  join bProp in bType.GetProperties() on aProp.Name equals bProp.Name
					  where !Equals(aProp.GetValue(a, empty), bProp.GetValue(b, empty))
					  select aProp.Name).Any());
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
		}

		[Test]
		public void OlapCommonTest()
		{
			ComparePod(
				new OlapCommon(2, 3, 4, 5, false, OlapCommon.VoltijdType.Deeltijd, OlapCommon.EoiType.Eoi, OlapCommon.NrOplType.MeerOpl,
					OlapCommon.Per1OktType.Alle, OlapCommon.VooroplType.NonVwo, OlapCommon.HerinschrijverType.HerinschrOpl,
					OlapCommon.RijDimensieType.Cohorten, OlapCommon.CelSomType.AbsenPercPerRij, 9,
					OlapCommon.StudieStaakType.AlleenstudiestakersOpl)
					{
						EcGrenswaarde = 42
					},
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
		}


		[Test]
		public void OlapSlicerTest()
		{
			ComparePod(new OlapSlicer(1, 2, 3, 4)
			{
				LangeLijst = true,
				Opleiding = 6,
				PreciesEenCohort = false,
				Rendement = true,
				Rijen = "abc",
				Samenvatting = false,
				ZuiverCohort = true,
			},
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
			PAssert.That(() => !ReferenceEquals(a, b) && Equals(a, b) && a.GetHashCode()==b.GetHashCode() && a.GetHashCode()!=c.GetHashCode() && !Equals(a,c));
			

		}
		//StudieStaak =  OlapSlicer.StudieStaakType.metstudiestakers,
		//ShowOpleidingen = true,
	}
}
