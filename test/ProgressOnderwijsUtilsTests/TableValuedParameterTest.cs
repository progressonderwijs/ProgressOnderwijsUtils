using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business;
using Progress.Business.Data;
using Progress.Business.DomainUnits;
using Progress.Business.Test;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    public class TableValuedParameterTest : TestsWithBusinessConnection
    {
        [Test]
        public void DatabaseCanProcessTableValuedParameters()
        {
            var q = SQL($@"select sum(x.querytablevalue) from ") + QueryBuilder.TableParamDynamic(Enumerable.Range(1, 100).ToArray()) + SQL($" x");
            int sum = q.ReadScalar<int>(conn);
            Assert.That(sum, Is.EqualTo((100 * 100 + 100) / 2));
        }

        [Test]
        public void QueryBuildersCanIncludeTvps()
        {
            var q = SQL($@"select sum(x.querytablevalue) from {Enumerable.Range(1, 100)} x");
            int sum = q.ReadScalar<int>(conn);
            Assert.That(sum, Is.EqualTo((100 * 100 + 100) / 2));
        }

        [Test]
        public void Binary_columns_can_be_used_in_tvps()
        {
            var filedata = new FileData {
                FileName = "testje.txt",
                ContentType = MediaTypeNames.Text.Plain,
                Content = Encoding.ASCII.GetBytes("Iets om te kunnen testen die nog niet bestaat"),
            };
            var hashcode = new SHA256Managed().ComputeHash(filedata.Content);

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
        }

        public sealed class TestDataMetaObject : IMetaObject
        {
            public byte[] Data { get; set; }
        }

        static readonly byte[] testData = Enumerable.Range(0, 100).Select(i => (byte)i).ToArray();

        [Test]
        public void Test_DbDataReaderBase_GetBytes_works_the_same_as_in_SqlDataReader()
        {
            var testen = new[] { new TestDataMetaObject { Data =  testData } };
            var reader = new MetaObjectDataReader<TestDataMetaObject>(testen);
            Assert_DataReader_GetBytes_works(reader);
        }

        [Test]
        public void Test_SqlDataReader_GetBytes_for_its_spec()
        {
            SQL($@"
                create table get_bytes_test
                (
                    data varbinary(max) not null
                );

                insert into get_bytes_test
                values ({testData});
            ").ExecuteNonQuery(conn);

            using (var cmd = SQL($@"select data from get_bytes_test").CreateSqlCommand(conn.PNetCommandCreationContext))
            using (var reader = cmd.ExecuteReader(CommandBehavior.Default))
                Assert_DataReader_GetBytes_works(reader);
        }

        static void Assert_DataReader_GetBytes_works(DbDataReader reader)
        {
            PAssert.That(() => reader.Read());

            long nofRead;
            var buffer = new byte[10];

            // TODO: willen we dit ook zo ondersteunen?
            //nofRead = reader.GetBytes(0, 0, null, 0, 1);
            //PAssert.That(() => nofRead == 100);

            nofRead = reader.GetBytes(0, 0, buffer, 0, 1);
            PAssert.That(() => buffer[0] == 0);
            PAssert.That(() => nofRead == 1);

            nofRead = reader.GetBytes(0, 20, buffer, 0, 1);
            PAssert.That(() => buffer[0] == 20);
            PAssert.That(() => nofRead == 1);

            nofRead = reader.GetBytes(0, 30, buffer, 0, 2);
            PAssert.That(() => buffer[1] == 31);
            PAssert.That(() => nofRead == 2);

            nofRead = reader.GetBytes(0, 80, buffer, 5, 2);
            PAssert.That(() => buffer[6] == 81);
            PAssert.That(() => nofRead == 2);

            nofRead = reader.GetBytes(0, 99, buffer, 0, 10);
            PAssert.That(() => buffer[0] == 99);
            PAssert.That(() => buffer[1] == 31);
            PAssert.That(() => nofRead == 1);

            nofRead = reader.GetBytes(0, 100, buffer, 0, 1);
            PAssert.That(() => nofRead == 0);

            nofRead = reader.GetBytes(0, 110, buffer, 0, 1);
            PAssert.That(() => nofRead == 0);

            PAssert.That(() => !reader.Read());
        }
    }
}
