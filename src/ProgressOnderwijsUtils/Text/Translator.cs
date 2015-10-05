using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class Translator
    {
        public static readonly IReadOnlyCollection<Taal> AllLanguages = EnumHelpers.GetValues<Taal>().Where(taal => taal != Taal.None).ToArray();

        static readonly IDictionary<Taal, CultureInfo> CULTURES = new Dictionary<Taal, CultureInfo> {
            { Taal.NL, new CultureInfo("nl-NL", false) },
            { Taal.EN, new CultureInfo("en-GB", false) },
            { Taal.DU, new CultureInfo("de-DE", false) },
        };

        static Translator()
        {
            // TODO: ...
            CULTURES[Taal.NL].DateTimeFormat.ShortDatePattern = "dd-MM-yyyy";
            CULTURES[Taal.EN].DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            CULTURES[Taal.EN].NumberFormat.CurrencySymbol = CULTURES[Taal.NL].NumberFormat.CurrencySymbol;
            CULTURES[Taal.DU].DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
            CULTURES[Taal.LA] = CreateLatinCulture();
        }

        static CultureInfo CreateLatinCulture()
        {
            var info = (CultureInfo)CULTURES[Taal.NL].Clone(); // Geen .NET-ondersteuning voor Latijn

            info.DateTimeFormat.MonthGenitiveNames = new[] {
                "Ianuarii",
                "Februarii",
                "Martii",
                "Aprilis",
                "Maii",
                "Iunii",
                "Iulii",
                "Augusti",
                "Septembris",
                "Octobris",
                "Novembris",
                "Decembris",
                ""
            };

            return info;
        }

        [Pure]
        public static CultureInfo GetCulture(this Taal language) => CULTURES[language];
    }
}
