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
                    var q = SQL($@"select sum(val) from ") + QueryBuilder.TableParam(Enumerable.Range(1, 100));
                    int sum = q.ReadScalar<int>(conn);
                    Assert.That(sum, Is.EqualTo((100 * 100 + 100) / 2));
                });
        }

        [Test]
        public void QueryBuildersCanIncludeTvps()
        {
            var q = SQL($@"select sum(val) from {Enumerable.Range(1, 100)}");

            DevelopmentDbSelector.PreferredDevDb.ReadWriteNoTransaction(
                conn => {
                    int sum = q.ReadScalar<int>(conn);
                    Assert.That(sum, Is.EqualTo((100 * 100 + 100) / 2));
                });
        }
    }
}
