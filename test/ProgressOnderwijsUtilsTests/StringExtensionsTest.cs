using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    public sealed class StringExtensionsTest
    {
        [Test]
        public void ToCamelCase_is_robuust_bij_null()
        {
            PAssert.That(() => ((string)null).ToCamelCase() == null);
        }

        [Test]
        public void ToCamelCase_werkt_ook_bij_lege_string()
        {
            PAssert.That(() => string.Empty.ToCamelCase() == string.Empty);
        }

        [Test]
        public void ToCamelCase_ADIS_ABEBA_wordt_Adis_Abeba()
        {
            PAssert.That(() => "ADIS ABEBA".ToCamelCase() == "Adis Abeba");
        }

        [Test]
        public void ToCamelCase_JAN_BENJAMIN_wordt_Jan_Benjamin()
        {
            PAssert.That(() => "JAN-BENJAMIN".ToCamelCase() == "Jan-Benjamin");
        }

        [Test]
        public void ToCamelCase_S_GRAVENHAGE_wordt_s_Gravenhage()
        {
            PAssert.That(() => "'S-GRAVENHAGE".ToCamelCase() == "'s-Gravenhage");
        }
    }
}
