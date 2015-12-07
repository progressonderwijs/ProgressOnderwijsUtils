using System.Linq;
using NUnit.Framework;
using Progress.Business;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    public class TableValuedParameterTest
    {
        [Test]
        public void DatabaseCanProcessTableValuedParameters()
        {
            DevelopmentDbSelector.PreferredDevDb.ReadWriteNoTransaction(
                conn => {
                    var q = SQL($@"select sum(x.querytablevalue) from ") + QueryBuilder.TableParamDynamic(Enumerable.Range(1, 100).ToArray()) + SQL($" x");
                    int sum = q.ReadScalar<int>(conn);
                    Assert.That(sum, Is.EqualTo((100 * 100 + 100) / 2));
                });
        }

        [Test]
        public void QueryBuildersCanIncludeTvps()
        {
            var q = SQL($@"select sum(x.querytablevalue) from {Enumerable.Range(1, 100)} x");

            DevelopmentDbSelector.PreferredDevDb.ReadWriteNoTransaction(
                conn => {
                    int sum = q.ReadScalar<int>(conn);
                    Assert.That(sum, Is.EqualTo((100 * 100 + 100) / 2));
                });
        }
    }
}
