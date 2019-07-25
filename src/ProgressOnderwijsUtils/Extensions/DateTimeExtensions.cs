using System;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Geeft het collegejaar van een bepaalde datum
        /// voorbeelden :
        /// 04-05-2009 -> 2008
        /// 01-09-2008 -> 2008
        /// </summary>
        [Pure]
        public static int CollegeJaar(this DateTime datetime)
            //Als startdatum januari t/m augustus dan is het collegejaar een jaar eerder
            => datetime.Year - (datetime.Month < 9 ? 1 : 0);

        [Pure]
        public static bool Overlapt(this Periode periode1, Periode periode2)
            => periode1.DatumVan < periode2.DatumTot && periode1.DatumTot > periode2.DatumVan;

        public struct Periode
        {
            public DateTime DatumVan { get; set; }
            public DateTime DatumTot { get; set; }

            public static Periode Create(DateTime datumVan, DateTime datumTot)
                => new Periode { DatumVan = datumVan, DatumTot = datumTot };
        }
    }
}
