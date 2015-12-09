using System.Linq;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business;
using Progress.Business.Data;
using Progress.Business.DomainUnits;
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

        [Test]
        public void Binary_columns_can_be_used_in_tvps()
        {
            var filedata = new FileData {
                FileName = "testje.txt",
                ContentType = MediaTypeNames.Text.Plain,
                Content = Encoding.ASCII.GetBytes("Iets om te kunnen testen"),
            };
            var hashcode = new SHA256Managed().ComputeHash(filedata.Content);

            DevelopmentDbSelector.PreferredDevDb.ReadWriteNoTransaction(conn => {

                PAssert.That(() => SQL($@"
                    select fd.filedataid
                    from filedata fd
                    where fd.hashcode in {new[] { hashcode }}
                ").ReadPlain<Id.FileData>(conn).None());

                var id = FileDataStorage.SaveFileData(conn, filedata);
                PAssert.That(() => SQL($@"
                    select fd.filedataid
                    from filedata fd
                    where fd.hashcode in {new[] { hashcode }}
                ").ReadPlain<Id.FileData>(conn).Single() == id);
            });
        }
    }
}
