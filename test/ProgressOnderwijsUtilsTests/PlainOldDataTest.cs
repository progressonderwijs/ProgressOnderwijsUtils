using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;
using TheJoyOfCode.QualityTools;

namespace ProgressOnderwijsUtilsTests
{

	/// <summary>
	/// Deze test is voor "Plain Old Data" objecten.  hij checked of waardes in de constructor goed gezet worden en geen velden over het hoofd gezien worden.
	/// Om eentje toe te voegen, maak een nieuwe test en roep ComparePod(A,B) aan, waarbij A en B propery-voor-property vergeleken worden.
	/// e.g. ComparePod(Tuple.Create(1,"z"), new {Item1 = 1, Item2 = "z"}) zou goed moeten gaan.
	/// </summary>
	[TestFixture]
	public sealed class PlainOldDataTest
	{
		static Func<T, S> MakeFunc<T, S>(Func<T, S> f) { return f; }

		public static void ComparePod(object a, object b)
		{
			object[] empty = new object[0];
			const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
			var getProperties =
				MakeFunc((object o) =>
					o.GetType().GetProperties(flags)
						.Select(pi =>
							new { pi.Name, Value = pi.GetValue(o, empty) })
						.Concat(
							o.GetType().GetFields(flags).Select(fi =>
								new { fi.Name, Value = fi.GetValue(o) }))
						.OrderBy(prop => prop.Name)
					);

			var aProps = getProperties(a);
			var bProps = getProperties(b);
			var differingPropertiesOfA = aProps.Except(bProps).ToArray();
			var differingPropertiesOfB = bProps.Except(aProps).ToArray();
			PAssert.That(() => !differingPropertiesOfA.Any() && !differingPropertiesOfB.Any());
		}

		public static void AutomaticClassTest<T>(T sample)
		{
			var ptester = new PropertyTester(sample);
			ptester.TestProperties();
			var ctester = new ConstructorTester(typeof(T));
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
					OlapCommon.StudieStaakType.AlleenstudiestakersOpl, "BlaBlaTestMenu")
					{
						EcGrenswaarde = 42,
                        NrJarenMax = 2,
						StartExmType = OlapStartUitstroomType.StartPropedeuse
					};
			ComparePod(
				olapcommon_sample
				,
				new
				{
					ExtraRijdimensie = "",
                    GeselecteerdeRijen = "",
					GeselecteerdeStudentCel = "",
					Rendement = false,
					Samenvatting = true,
					LangeLijst = false,
					TopZoveel = false,
                    SpecificMember = false,
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
					EcGrenswaarde = 42,
                    NrJarenMax = 2,
                    StartExmType = OlapStartUitstroomType.StartPropedeuse,
					ParentMenuName = "BlaBlaTestMenu",
					aanmeldstatus = default(OlapCommon.AanmstatusType),
                    alleeneerstejr = OlapCommon.AlleenEerstejaarType.Alle,
				}
				);

			AutomaticClassTest(olapcommon_sample);
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
			var a = new SLBerichtSamenvatting.Value
			{
				Berichttype = "abc",
				FormNaam = "def",
				Ontvanger = "qwerty",
				Organisatie = 1,
				Student = 2,
				StudielinkberichtId = 3,
				StudielinkNummer = 4,
				Tekst = "asdfg",
				Zender = "zxcvb",
			}.FinishBuilding();
			var b = new SLBerichtSamenvatting.Value
			{
				Berichttype = "abc",
				FormNaam = "def",
				Ontvanger = "qwerty",
				Organisatie = 1,
				Student = 2,
				StudielinkberichtId = 3,
				StudielinkNummer = 4,
				Tekst = "asdfg",
				Zender = "zxcvb",
			}.FinishBuilding();
			var c =
				new
				{
					Berichttype = "abc",
					FormNaam = "def",
					Ontvanger = "qwerty",
					Organisatie = 1,
					Student = 2,
					StudielinkberichtId = 3,
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
		}
	}
}
