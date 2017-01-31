using System;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using Xunit;

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
        {
            //Als startdatum januari t/m augustus dan is het collegejaar een jaar eerder
            return datetime.Year - (datetime.Month < 9 ? 1 : 0);
        }

        [Pure]
        public static bool Overlapt(this Periode periode1, Periode periode2) => periode1.DatumVan < periode2.DatumTot && periode1.DatumTot > periode2.DatumVan;

        public struct Periode
        {
            public DateTime DatumVan { get; set; }
            public DateTime DatumTot { get; set; }
            public static Periode Create(DateTime datumVan, DateTime datumTot) => new Periode { DatumVan = datumVan, DatumTot = datumTot };
        }
    }

    public static class DateTimeExtensionsTest
    {
        [Fact]
        public static void Overlapt_1_voor_en_in_2()
        {
            var periode1 = DateTimeExtensions.Periode.Create(new DateTime(2000, 1, 1), new DateTime(2000, 2, 1));
            var periode2 = DateTimeExtensions.Periode.Create(new DateTime(2000, 1, 15), new DateTime(2000, 2, 15));
            AssertOverlapt(periode1, periode2, true);
        }

        [Fact]
        public static void Overlapt_2_voor_en_in_1()
        {
            var periode1 = DateTimeExtensions.Periode.Create(new DateTime(2000, 1, 15), new DateTime(2000, 2, 15));
            var periode2 = DateTimeExtensions.Periode.Create(new DateTime(2000, 1, 1), new DateTime(2000, 2, 1));
            AssertOverlapt(periode1, periode2, true);
        }

        [Fact]
        public static void Overlapt_1_in_2()
        {
            var periode1 = DateTimeExtensions.Periode.Create(new DateTime(2000, 1, 15), new DateTime(2000, 1, 25));
            var periode2 = DateTimeExtensions.Periode.Create(new DateTime(2000, 1, 1), new DateTime(2000, 2, 1));
            AssertOverlapt(periode1, periode2, true);
        }

        [Fact]
        public static void Overlapt_2_in_1()
        {
            var periode1 = DateTimeExtensions.Periode.Create(new DateTime(2000, 1, 1), new DateTime(2000, 2, 1));
            var periode2 = DateTimeExtensions.Periode.Create(new DateTime(2000, 1, 15), new DateTime(2000, 1, 25));
            AssertOverlapt(periode1, periode2, true);
        }

        [Fact]
        public static void Overlapt_1_voor_2()
        {
            var periode1 = DateTimeExtensions.Periode.Create(new DateTime(2000, 1, 1), new DateTime(2000, 2, 1));
            var periode2 = DateTimeExtensions.Periode.Create(new DateTime(2000, 2, 1), new DateTime(2000, 3, 1));
            AssertOverlapt(periode1, periode2, false);
        }

        [Fact]
        public static void Overlapt_2_voor_1()
        {
            var periode1 = DateTimeExtensions.Periode.Create(new DateTime(2000, 2, 1), new DateTime(2000, 3, 1));
            var periode2 = DateTimeExtensions.Periode.Create(new DateTime(2000, 1, 1), new DateTime(2000, 2, 1));
            AssertOverlapt(periode1, periode2, false);
        }

        [Fact]
        public static void Overlapt_1_is_2()
        {
            var periode1 = DateTimeExtensions.Periode.Create(new DateTime(2000, 2, 1), new DateTime(2000, 3, 1));
            var periode2 = DateTimeExtensions.Periode.Create(new DateTime(2000, 2, 1), new DateTime(2000, 3, 1));
            AssertOverlapt(periode1, periode2, true);
        }

        static void AssertOverlapt(DateTimeExtensions.Periode periode1, DateTimeExtensions.Periode periode2, bool uitkomst)
        {
            PAssert.That(() => periode1.Overlapt(periode2) == uitkomst);
        }
    }
}
