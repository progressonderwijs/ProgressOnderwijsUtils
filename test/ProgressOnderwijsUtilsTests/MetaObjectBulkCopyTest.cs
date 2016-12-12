using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business;
using Progress.Business.Test;
using ProgressOnderwijsUtils;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtilsTests
{
    [PullRequestTest]
    public sealed class MetaObjectBulkCopyTest : TestSuiteBase
    {
        static readonly BlaOk[] SampleObjects = new[] {
            new BlaOk { Bla = "bl34ga", Bla2 = "blaasdfgasfg2", Id = -1 },
            new BlaOk { Bla = "bla", Bla2 = "bla2", Id = 0 },
            new BlaOk { Bla = "dfg", Bla2 = "bla342", Id = 1 },
            new BlaOk { Bla = "blfgjha", Bla2 = "  bla2  ", Id = 2 },
            new BlaOk { Bla2 = "", Id = 3 },
        };

        public sealed class BlaOk : ValueBase<BlaOk>, IMetaObject
        {
            public int Id { get; set; }
            public string Bla2 { get; set; }
            public string Bla { get; set; }
        }

        public sealed class BlaOk2 : ValueBase<BlaOk2>, IMetaObject
        {
            public int Id { get; set; }
            public string Bla { get; set; }
            public string Bla2 { get; set; }
        }

        public sealed class BlaWithMispelledColumns : ValueBase<BlaWithMispelledColumns>, IMetaObject
        {
            public int Idd { get; set; }
            public string Bla { get; set; }
            public string Bla2 { get; set; }
        }

        public sealed class BlaWithMistypedColumns : ValueBase<BlaWithMistypedColumns>, IMetaObject
        {
            public int Bla { get; set; }
            public int Id { get; set; }
            public string Bla2 { get; set; }
        }

        public sealed class BlaWithMistypedColumns2 : ValueBase<BlaWithMistypedColumns2>, IMetaObject
        {
            public int Bla { get; set; }
            public string Id { get; set; }
            public string Bla2 { get; set; }
        }

        public sealed class BlaWithExtraClrFields : ValueBase<BlaWithExtraClrFields>, IMetaObject
        {
            public string ExtraBla { get; set; }
            public string Id { get; set; }
            public int Bla { get; set; }
            public string Bla2 { get; set; }
        }

        public sealed class BlaWithMissingClrFields : ValueBase<BlaWithMissingClrFields>, IMetaObject
        {
            public int Id { get; set; }
            public string Bla2 { get; set; }
        }

        public void CreateTempTable()
        {
            SQL($@"create table #MyTable (id int not null primary key, bla nvarchar(max) null, bla2 nvarchar(max) not null)").ExecuteNonQuery(conn);
        }

        [Test]
        public void BulkCopyChecksNames()
        {
            CreateTempTable();
            Assert.Catch(() => new BlaWithMispelledColumns[0].BulkCopyToSqlServer(conn.SqlConnection, "#MyTable"));
        }

        [Test]
        public void BulkCopyChecksTypes()
        {
            CreateTempTable();
            Assert.Catch(() => new BlaWithMistypedColumns[0].BulkCopyToSqlServer(conn.SqlConnection, "#MyTable"));
        }

        [Test]
        public void BulkCopyChecksTypes2()
        {
            CreateTempTable();
            Assert.Catch(() => new BlaWithMistypedColumns2[0].BulkCopyToSqlServer(conn.SqlConnection, "#MyTable"));
        }

        [Test]
        public void BulkCopyVerifiesExistanceOfDestinationColumns()
        {
            CreateTempTable();
            Assert.Catch(() => new BlaWithExtraClrFields[0].BulkCopyToSqlServer(conn.SqlConnection, "#MyTable"));
        }

        [Test]
        public void BulkCopyAllowsExtraDestinationColumns()
        {
            CreateTempTable();
            new BlaWithMissingClrFields[0].BulkCopyToSqlServer(conn.SqlConnection, "#MyTable");
        }

        [Test]
        public void BulkCopyAllowsExactMatch()
        {
            CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(conn.SqlConnection, "#MyTable");
            var fromDb = SQL($"select * from #MyTable order by Id").ReadMetaObjects<BlaOk>(conn);
            PAssert.That(() => SampleObjects.SequenceEqual(fromDb));
        }

        [Test]
        public void BulkCopySupportsColumnReordering()
        {
            CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(conn.SqlConnection, "#MyTable");
            var fromDb = SQL($"select * from #MyTable order by Id").ReadMetaObjects<BlaOk2>(conn);
            PAssert.That(() => SampleObjects.SequenceEqual(fromDb.Select(x => new BlaOk { Id = x.Id, Bla = x.Bla, Bla2 = x.Bla2 })));
        }
    }
}
