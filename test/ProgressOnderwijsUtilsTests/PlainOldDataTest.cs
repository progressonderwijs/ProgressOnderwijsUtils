using System;
using System.Linq;
using System.Reflection;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using NUnit.Framework;
using Progress.Business.DomainUnits;
using Progress.Business.Inschrijvingen.Studielink;
using Progress.Business.Test;
using ProgressOnderwijsUtils;
using TheJoyOfCode.QualityTools;

namespace ProgressOnderwijsUtilsTests
{
    /// <summary>
    /// Deze test is voor "Plain Old Data" objecten.  hij checked of waardes in de constructor goed gezet worden en geen velden over het hoofd gezien worden.
    /// Om eentje toe te voegen, maak een nieuwe test en roep ComparePod(A,B) aan, waarbij A en B propery-voor-property vergeleken worden.
    /// e.g. ComparePod(Tuple.Create(1,"z"), new {Item1 = 1, Item2 = "z"}) zou goed moeten gaan.
    /// </summary>
    [Continuous]
    public sealed class PlainOldDataTest
    {
        static Func<T, S> MakeFunc<T, S>(Func<T, S> f)
        {
            return f;
        }

        public static void ComparePod(object a, object b)
        {
            var emptyArray = new object[0];
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            var getProperties =
                MakeFunc(
                    (object o) =>
                        o.GetType().GetProperties(flags)
                            .Select(
                                pi =>
                                    new { pi.Name, Value = pi.GetValue(o, emptyArray) })
                            .Concat(
                                o.GetType().GetFields(flags).Select(
                                    fi =>
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
            Assert.Throws<AssertionException>(() => ComparePod(Tuple.Create("abc", false), new { item1 = "abc", item2 = false })); //case-sensitive
            Assert.Throws<AssertionException>(() => ComparePod(Tuple.Create("abc", false), new { Item1 = "Abc", Item2 = false })); //value-sensitive
            Assert.Throws<AssertionException>(() => ComparePod(Tuple.Create(default(string), false), new { Item1 = "", Item2 = false })); //no weirdness
            ComparePod(Tuple.Create(default(string), false), new { Item1 = default(string), Item2 = false });
        }

        [Test]
        public void SLBerichtSamenvattingTest()
        {
            var a = new SLBerichtSamenvatting.Value {
                BerichtType = BerichtType.vchmsg06onderhoudennaw,
                Ontvanger = "qwerty",
                Organisatie = RootOrganisatie.RUG,
                Student = (Id.Student)2,
                StudielinkBerichtId = 3,
                StudielinkNummer = (ExternalId.StudielinkNummer)4,
                Zender = "zxcvb",
            }.FinishBuilding();
            var b = new SLBerichtSamenvatting.Value {
                BerichtType = BerichtType.vchmsg06onderhoudennaw,
                Ontvanger = "qwerty",
                Organisatie = RootOrganisatie.RUG,
                Student = (Id.Student)2,
                StudielinkBerichtId = 3,
                StudielinkNummer = (ExternalId.StudielinkNummer)4,
                Zender = "zxcvb",
            }.FinishBuilding();
            var c =
                new {
                    BerichtType = BerichtType.vchmsg06onderhoudennaw,
                    Ontvanger = "qwerty",
                    Organisatie = RootOrganisatie.RUG,
                    Student = (Id.Student)2,
                    StudielinkBerichtId = 3,
                    StudielinkNummer = (ExternalId.StudielinkNummer)4,
                    Zender = "zxcvb",
                };
            ComparePod(a, b);
            ComparePod(a, c);
            PAssert.That(() => !ReferenceEquals(a, b) && Equals(a, b) && a.GetHashCode() == b.GetHashCode() && a.GetHashCode() != c.GetHashCode() && !Equals(a, c));

            AutomaticClassTest(a);
        }

        [Test]
        public void SomeTest()
        {
            AutomaticClassTest(new ExamplePoco());
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    class ExamplePoco
    {
        public int DossierID { get; set; }
        public string IngevoerdDoor { get; set; }
        public DateTime DatumInvoer { get; set; }
        public int TypeID { get; set; }
        public string TypeBeschrijving { get; set; }
        public string Onderwerp { get; set; }
        public string Inhoud { get; set; }
        public string Taal { get; set; }
        public int MediumTypeID { get; set; }
        public string MediumTypeBeschrijving { get; set; }
    }
}
