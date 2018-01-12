using System;
using System.Globalization;
using System.Threading;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public class StringExtensionsTest
    {
        [Fact]
        public void ToCamelCase_is_robuust_bij_null()
        {
            PAssert.That(() => ((string) null).ToCamelCase() == null);
        }

        [Fact]
        public void ToCamelCase_werkt_ook_bij_lege_string()
        {
            PAssert.That(() => string.Empty.ToCamelCase() == string.Empty);
        }

        [Fact]
        public void ToCamelCase_ADIS_ABEBA_wordt_Adis_Abeba()
        {
            PAssert.That(() => "ADIS ABEBA".ToCamelCase() == "Adis Abeba");
        }

        [Fact]
        public void ToCamelCase_JAN_BENJAMIN_wordt_Jan_Benjamin()
        {
            PAssert.That(() => "JAN-BENJAMIN".ToCamelCase() == "Jan-Benjamin");
        }

        [Fact]
        public void ToCamelCase_S_GRAVENHAGE_wordt_s_Gravenhage()
        {
            PAssert.That(() => "'S-GRAVENHAGE".ToCamelCase() == "'s-Gravenhage");
        }

        [Fact]
        public void ToCamelCase_Hoodletters_met_lees_tekens_worden_ook_meegenomen()
        {
            PAssert.That(() => "ÖSTERREICH".ToCamelCase() == "Österreich");
        }

        [Fact]
        public void ToCamelCase_Combi_van_meerdere_omzettingen()
        {
            PAssert.That(() => "'S-ÖSTERREICH".ToCamelCase() == "'s-Österreich");
        }

        [Fact]
        public void WithoutDiakriet()
        {
            PAssert.That(() => StringUtils.VerwijderDiakrieten("é") == "e");
            PAssert.That(() => StringUtils.VerwijderDiakrieten("Ü") == "U");
            PAssert.That(() => StringUtils.VerwijderDiakrieten("ß") == "ß");
        }

        [Fact]
        public void ReplaceRingelS()
        {
            PAssert.That(() => StringUtils.VervangRingelS("ß", false) == "ss");
            PAssert.That(() => StringUtils.VervangRingelS("ß", true) == "SS");
            PAssert.That(() => StringUtils.VervangRingelS("aßb", false) == "assb");
            PAssert.That(() => StringUtils.VervangRingelS("ßsß", true) == "SSsSS");
            PAssert.That(() => StringUtils.VervangRingelS("", false) == "");
        }

        [Fact]
        public void NormalizeWhitespace()
        {
            PAssert.That(() => "".NormalizeWhitespace() == "");
            PAssert.That(() => "test".NormalizeWhitespace() == "test");
            PAssert.That(() => " test ".NormalizeWhitespace() == "test");
            PAssert.That(() => "\ttest\t".NormalizeWhitespace() == "test");
            PAssert.That(() => "\ntest\n".NormalizeWhitespace() == "test");
            PAssert.That(() => " \t\ntest\n\t ".NormalizeWhitespace() == "test");
            PAssert.That(() => "een test".NormalizeWhitespace() == "een test");
            PAssert.That(() => "een  test".NormalizeWhitespace() == "een test");
            PAssert.That(() => "een\ttest".NormalizeWhitespace() == "een test");
            PAssert.That(() => "een\t\ttest".NormalizeWhitespace() == "een test");
            PAssert.That(() => "een\ntest".NormalizeWhitespace() == "een test");
            PAssert.That(() => "een\n\ntest".NormalizeWhitespace() == "een test");
        }

        [Fact]
        public void CollapseWhitespace()
        {
            PAssert.That(() => "".CollapseWhitespace() == "");
            PAssert.That(() => "test".CollapseWhitespace() == "test");
            PAssert.That(() => " test ".CollapseWhitespace() == " test ");
            PAssert.That(() => "\ttest\t".CollapseWhitespace() == " test ");
            PAssert.That(() => "\ntest\n".CollapseWhitespace() == " test ");
            PAssert.That(() => " \t\ntest\n\t ".CollapseWhitespace() == " test ");
            PAssert.That(() => "een test".CollapseWhitespace() == "een test");
            PAssert.That(() => "een  test".CollapseWhitespace() == "een test");
            PAssert.That(() => "een\ttest".CollapseWhitespace() == "een test");
            PAssert.That(() => "een\t\ttest".CollapseWhitespace() == "een test");
            PAssert.That(() => "een\ntest".CollapseWhitespace() == "een test");
            PAssert.That(() => "een\n\ntest".CollapseWhitespace() == "een test");
        }

        [Fact]
        public void TestLevenshtein()
        {
            PAssert.That(() => StringUtils.LevenshteinDistance("", "") == 0);
            PAssert.That(() => StringUtils.LevenshteinDistance("test", "tset") == 2);
            PAssert.That(() => StringUtils.LevenshteinDistance(" test ", "\ttest\t") == 2);
            PAssert.That(() => StringUtils.LevenshteinDistance("Ziggy Stardust", "ziggy stradust") == 4);
            PAssert.That(() => StringUtils.LevenshteinDistance("a", "b") == 1);
            PAssert.That(() => StringUtils.LevenshteinDistance("a", "") == 1);
            PAssert.That(() => StringUtils.LevenshteinDistance("aba", "aa") == 1);
            PAssert.That(() => StringUtils.LevenshteinDistance("simple", "Simpler") == 2);
            PAssert.That(() => StringUtils.LevenshteinDistance("hmmm", "yummy") == 3);
            PAssert.That(() => StringUtils.LevenshteinDistance("World-wide", "wordy") == 7);
            //"W"=>"w",drop "l", replace "-wide"
        }

        [Fact]
        public void testNaam2Upper()
        {
            PAssert.That(() => StringUtils.Name2UpperCasedName("joepje jofel") == "Joepje Jofel");
            PAssert.That(() => StringUtils.Name2UpperCasedName("carolien Kaasteen") == "Carolien Kaasteen");
            PAssert.That(
                () => StringUtils.Name2UpperCasedName("maarten middelmaat--meloen") == "Maarten Middelmaat-Meloen");
            PAssert.That(() => StringUtils.Name2UpperCasedName("carolien    Kaasteen") == "Carolien Kaasteen");
            PAssert.That(() => StringUtils.Name2UpperCasedName("'s-gravenhage") == "'s-Gravenhage");
            PAssert.That(() => StringUtils.Name2UpperCasedName("'s gravenhage") == "'s Gravenhage");
            PAssert.That(() => StringUtils.Name2UpperCasedName("'sgravenhage") == "'s Gravenhage");
            PAssert.That(() => StringUtils.Name2UpperCasedName("sieb op de kast") == "Sieb op de Kast");
        }

        [Fact]
        public void testLangeNaam2Upper()
        {
            PAssert.That(
                () =>
                    StringUtils.Name2UpperCasedName(
                        "miep boezeroen-jansen van der sloot op 't gootje v.d. geest de la terrine du soupe au beurre à demi v/d zo-is-het-wel-genoeg ja")
                    ==
                    "Miep Boezeroen-Jansen van der Sloot op 't Gootje v.d. Geest de la Terrine du Soupe au Beurre à Demi v/d Zo-Is-Het-Wel-Genoeg Ja");
        }

        [Fact]
        public void testDepluralize()
        {
            PAssert.That(() => StringUtils.Depluralize("Tests") == "Test");
            PAssert.That(() => StringUtils.Depluralize("Testen") == "Test");
            PAssert.That(() => StringUtils.Depluralize("Taal") == "Taal");
            PAssert.That(() => StringUtils.Depluralize("Talen") == "Taal");
            PAssert.That(() => StringUtils.Depluralize("Cases") == "Case");
            PAssert.That(() => StringUtils.Depluralize("Fouten") == "Fout");
            PAssert.That(() => StringUtils.Depluralize("Sappen") == "Sap");
            PAssert.That(() => StringUtils.Depluralize("Apen") == "Aap");
        }

        [Fact]
        public void ToStringInvariantTest()
        {
            var oldCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                var culture = (CultureInfo) CultureInfo.CurrentCulture.Clone();
                culture.NumberFormat = new NumberFormatInfo {NegativeSign = "X",};
                Thread.CurrentThread.CurrentCulture = culture;
                PAssert.That(() => (-3000).ToString() == "X3000");
                PAssert.That(() => (-3000m).ToString() == "X3000");
                PAssert.That(() => (-3000).ToStringInvariant() == "-3000");
                PAssert.That(() => 3000U.ToStringInvariant() == "3000");
                PAssert.That(() => (-3000m).ToStringInvariant() == "-3000");
                PAssert.That(() => (-3000.0).ToStringInvariant() == "-3000");
                PAssert.That(() => (-3000.0f).ToStringInvariant() == "-3000");
                PAssert.That(() => (-3000L).ToStringInvariant() == "-3000");
                PAssert.That(() => 3000UL.ToStringInvariant() == "3000");
                PAssert.That(() => ((short) -3000).ToStringInvariant() == "-3000");
                PAssert.That(() => ((ushort) 3000).ToStringInvariant() == "3000");
                PAssert.That(() => true.ToStringInvariant() == "True");
                PAssert.That(() => new DateTime(2000, 1, 2).ToStringInvariant() == "01/02/2000 00:00:00");
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = oldCulture;
            }
        }

        [Fact]
        public void PrettyPrintCamelCased()
        {
            var translations = new[,]
            {
                {"SMMutatie", "SM mutatie", "SM mutatie"},
                {"XmlReader", "xml reader", "Xml reader"},
                {"S0Xval", "S 0 xval", "S 0 xval"},
                {"bla bla bla", "bla bla bla", "bla bla bla"},
                {"iSXReader0Bla", "i SX reader 0 bla", "i SX reader 0 bla"},
                {"Channel99", "channel 99", "Channel 99"},
                {"SM99", "SM 99", "SM 99"},
                {"is_dit_echtZo", "is dit echt zo", "is dit echt zo"},
                {"Administratienummer_OWI", "administratienummer OWI", "Administratienummer OWI"},
                {"Bla_Bla", "bla bla", "Bla bla"},
                {"MT940Sluit", "MT 940 sluit", "MT 940 sluit"},
                {"Accoord2Afwijzen", "accoord 2 afwijzen", "Accoord 2 afwijzen"},
                {"_Multi _Space", " multi space", " multi space"},
                {"trailing Space\t", "trailing space ", "trailing space "},
            };
            for (var row = 0; row < translations.GetLength(0); row++)
            {
                var initial = translations[row, 0];
                var ideal = translations[row, 1];
                var idealCap = translations[row, 2];
                PAssert.That(() => StringUtils.PrettyPrintCamelCased(initial) == ideal);
                PAssert.That(() => StringUtils.PrettyCapitalizedPrintCamelCased(initial) == idealCap);
            }
        }

        [Fact]
        public void ToFlatDebugString()
        {
            PAssert.That(() => StringUtils.ToFlatDebugString((int[]) null) == "[]");
            PAssert.That(() => StringUtils.ToFlatDebugString(new string[0]) == "[]");
            PAssert.That(() => StringUtils.ToFlatDebugString(new[] {"single"}) == "[single]");
            PAssert.That(() => StringUtils.ToFlatDebugString(new[] {"first", "second"}) == "[first, second]");
            PAssert.That(() => StringUtils.ToFlatDebugString(new object[] {1, "2", null, 3}) == "[1, 2, , 3]");
        }
    }
}