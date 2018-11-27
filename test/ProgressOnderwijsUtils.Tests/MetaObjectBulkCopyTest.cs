using System;
using System.Linq;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.SchemaReflection;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class MetaObjectBulkCopyTest : TransactedLocalConnection
    {
        static readonly BlaOk[] SampleObjects = new[] {
            new BlaOk { Bla = "bl34ga", Bla2 = "blaasdfgasfg2", Id = -1 },
            new BlaOk { Bla = "bla", Bla2 = "bla2", Id = 0 },
            new BlaOk { Bla = "dfg", Bla2 = "bla342", Id = 1 },
            new BlaOk { Bla = "blfgjha", Bla2 = "  bla2  ", Id = 2 },
            new BlaOk { Bla2 = "", Id = 3 },
        };

        public sealed class BlaOk : ValueBase<BlaOk>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Id { get; set; }
            public string Bla2 { get; set; }
            public string Bla { get; set; }
        }

        public sealed class BlaOk2 : ValueBase<BlaOk2>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Id { get; set; }
            public string Bla { get; set; }
            public string Bla2 { get; set; }
        }

        public sealed class BlaWithMispelledColumns : ValueBase<BlaWithMispelledColumns>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Idd { get; set; }
            public string Bla { get; set; }
            public string Bla2 { get; set; }
        }

        public sealed class BlaWithMistypedColumns : ValueBase<BlaWithMistypedColumns>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Bla { get; set; }
            public int Id { get; set; }
            public string Bla2 { get; set; }
        }

        public sealed class BlaWithMistypedColumns2 : ValueBase<BlaWithMistypedColumns2>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Bla { get; set; }
            public string Id { get; set; }
            public string Bla2 { get; set; }
        }

        public sealed class BlaWithExtraClrFields : ValueBase<BlaWithExtraClrFields>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public string ExtraBla { get; set; }
            public string Id { get; set; }
            public int Bla { get; set; }
            public string Bla2 { get; set; }
        }

        public sealed class BlaWithMissingClrFields : ValueBase<BlaWithMissingClrFields>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Id { get; set; }
            public string Bla2 { get; set; }
        }

        (string tableName, DbColumnMetaData[] columns) CreateTempTable()
        {
            var tableName = SQL($"#MyTable");
            SQL($@"
                create table {tableName} (
                    id int not null primary key
                    , bla nvarchar(max) null
                    , bla2 nvarchar(max) not null
                )
            ").ExecuteNonQuery(Context);

            return (tableName.CommandText(), DbColumnMetaData.ColumnMetaDatasOfTempDbTable(Context.Connection, tableName));
        }

        [Fact]
        public void BulkCopyChecksNames()
        {
            var (table, columns) = CreateTempTable();
            Assert.ThrowsAny<Exception>(() => new BlaWithMispelledColumns[0].BulkCopyToSqlServer(Context.Connection, table, columns));
        }

        [Fact]
        public void BulkCopyChecksTypes()
        {
            var (table, columns) = CreateTempTable();
            Assert.ThrowsAny<Exception>(() => new BlaWithMistypedColumns[0].BulkCopyToSqlServer(Context.Connection, table, columns));
        }

        [Fact]
        public void BulkCopyChecksTypes2()
        {
            var (table, columns) = CreateTempTable();
            Assert.ThrowsAny<Exception>(() => new BlaWithMistypedColumns2[0].BulkCopyToSqlServer(Context.Connection, table, columns));
        }

        [Fact]
        public void BulkCopyVerifiesExistanceOfDestinationColumns()
        {
            var (table, columns) = CreateTempTable();
            Assert.ThrowsAny<Exception>(() => new BlaWithExtraClrFields[0].BulkCopyToSqlServer(Context.Connection, table, columns));
        }

        [Fact]
        public void BulkCopyAllowsExtraDestinationColumns()
        {
            var (table, columns) = CreateTempTable();
            new BlaWithMissingClrFields[0].BulkCopyToSqlServer(Context.Connection, table, columns);
        }

        [Fact]
        public void BulkCopyAllowsExactMatch()
        {
            var (table, columns) = CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Context.Connection, table, columns);
            var fromDb = SQL($"select * from #MyTable order by Id").ReadMetaObjects<BlaOk>(Context);
            PAssert.That(() => SampleObjects.SequenceEqual(fromDb));
        }

        [Fact]
        public void BulkCopySupportsColumnReordering()
        {
            var (table, columns) = CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Context.Connection, table, columns);
            var fromDb = SQL($"select * from #MyTable order by Id").ReadMetaObjects<BlaOk2>(Context);
            PAssert.That(() => SampleObjects.SequenceEqual(fromDb.Select(x => new BlaOk { Id = x.Id, Bla = x.Bla, Bla2 = x.Bla2 })));
        }
    }
}
