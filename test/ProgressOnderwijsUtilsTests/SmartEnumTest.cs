using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business;
using Progress.Business.Inschrijvingen;
using Progress.Business.Test;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    public sealed class SmartEnumTest : TestsWithBusinessConnection
    {
        struct Inschrijving : IMetaObject
        {
            public SmartPeriodeStudiejaar PeriodeStudiejaar { get; set; }
        }

        [Test]
        public void SmartEnum_can_be_read_using_QueryBuilder()
        {
            var inschrijving = SafeSql.SQL($@"
                    select top 1
                        si.periodestudiejaar
                    from studentinschrijving si
                    where 1=1
                        and si.periodestudiejaar = {SmartPeriodeStudiejaar.C2015}
                ").ReadMetaObjects<Inschrijving>(conn).Single();

            PAssert.That(() => inschrijving.PeriodeStudiejaar == SmartPeriodeStudiejaar.C2015);
        }
    }
}
