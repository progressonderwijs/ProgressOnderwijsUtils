using System;
using System.Data.SqlClient;
using System.Linq;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class MetaObjectBulkCopyTest : TransactedLocalConnection
    {
        static readonly BlaOk[] SampleObjects = {
            new BlaOk { Bla = "bl34ga", Bla2 = "blaasdfgasfg2", Id = -1 },
            new BlaOk { Bla = "bla", Bla2 = "bla2", Id = 0 },
            new BlaOk { Bla = "dfg", Bla2 = "bla342", Id = 1 },
            new BlaOk { Bla = "blfgjha", Bla2 = "  bla2  ", Id = 2 },
            new BlaOk { Bla2 = "", Id = 3 }
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

        [NotNull]
        BulkInsertTarget CreateTempTable()
        {
            var tableName = SQL($"#MyTable");
            SQL($@"
                create table {tableName} (
                    id int not null primary key
                    , bla nvarchar(max) null
                    , bla2 nvarchar(max) not null
                )
            ").ExecuteNonQuery(Context);

            return BulkInsertTarget.LoadFromTable(Context, tableName);
        }

        sealed class ComputedColumnExample : ValueBase<ComputedColumnExample>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Id { get; set; }
            public bool Computed { internal get; set; }
            public string Bla { get; set; }
        }

        [Fact]
        public void BulkCopyAllowsExactMatch()
        {
            var target
                = CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Context.Connection, target);
            var fromDb = SQL($"select * from #MyTable order by Id").ReadMetaObjects<BlaOk>(Context);
            PAssert.That(() => SampleObjects.SequenceEqual(fromDb));
        }

        [Fact]
        public void BulkCopyChecksNames()
        {
            var target
                = CreateTempTable();
            Assert.ThrowsAny<Exception>(() => new BlaWithMispelledColumns[0].BulkCopyToSqlServer(Context.Connection, target));
        }

        [Fact]
        public void BulkCopyChecksTypes()
        {
            var target
                = CreateTempTable();
            Assert.ThrowsAny<Exception>(() => new BlaWithMistypedColumns[0].BulkCopyToSqlServer(Context.Connection, target));
        }

        [Fact]
        public void BulkCopyChecksTypes2()
        {
            var target
                = CreateTempTable();
            Assert.ThrowsAny<Exception>(() => new BlaWithMistypedColumns2[0].BulkCopyToSqlServer(Context.Connection, target));
        }

        [Fact]
        public void BulkCopySupportsColumnReordering()
        {
            var target
                = CreateTempTable();
            SampleObjects.BulkCopyToSqlServer(Context.Connection, target);
            var fromDb = SQL($"select * from #MyTable order by Id").ReadMetaObjects<BlaOk2>(Context);
            PAssert.That(() => SampleObjects.SequenceEqual(fromDb.Select(x => new BlaOk { Id = x.Id, Bla = x.Bla, Bla2 = x.Bla2 })));
        }

        [Fact]
        public void BulkCopySupportsCumputedColumn()
        {
            var tableName = SQL($"#MyTable");
            SQL($@"
                create table {tableName} (
                    Id int not null primary key
                    , Computed as convert(bit, 1) -- deliberately not placed at the end
                    , Bla nvarchar(max) not null
                )
            ").ExecuteNonQuery(Context);

            new[] {
                new ComputedColumnExample {
                    Id = 11,
                    Bla = "Something"
                }
            }.BulkCopyToSqlServer(Context, BulkInsertTarget.LoadFromTable(Context, tableName));

            var fromDb = SQL($@"
                select *
                from {tableName}
            ").ReadMetaObjects<ComputedColumnExample>(Context).Single();
            PAssert.That(() => fromDb.Computed);
        }

        sealed class IncludingIdentityColumn : ValueBase<IncludingIdentityColumn>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Id { get; set; }
            public int AnIdentity { get; set; }
            public string Bla { get; set; }
        }

        sealed class ExcludingIdentityColumn : ValueBase<ExcludingIdentityColumn>, IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Id { get; set; }
            public string Bla { get; set; }
        }

        [Fact]
        public void BulkCopyAllowsOmittingSourcePropertiesForIdentityColumns()
        {
            var tableName = SQL($"#MyTable");
            SQL($@"
                create table {tableName} (
                    Id int not null primary key
                    , AnIdentity int not null identity(1,1) -- deliberately not placed at the end or start
                    , Bla nvarchar(max) not null
                )
            ").ExecuteNonQuery(Context);

            new[] {
                new ExcludingIdentityColumn {
                    Id = 11,
                    Bla = "Something"
                }
            }.BulkCopyToSqlServer(Context, BulkInsertTarget.LoadFromTable(Context, tableName));

            var fromDb = SQL($@"
                select *
                from {tableName}
            ").ReadMetaObjects<IncludingIdentityColumn>(Context).Single();
            PAssert.That(() => fromDb.AnIdentity == 1);
        }

        [Fact]
        public void BulkCopyIgnoresPropertiesCorrespondingIdentityColumns()
        {
            var tableName = SQL($"#MyTable");
            SQL($@"
                create table {tableName} (
                    Id int not null primary key
                    , AnIdentity int not null identity(1,1) -- deliberately not placed at the end or start
                    , Bla nvarchar(max) not null
                )
            ").ExecuteNonQuery(Context);

            new[] {
                new IncludingIdentityColumn {
                    Id = 11,
                    AnIdentity = 37,
                    Bla = "Something"
                }
            }.BulkCopyToSqlServer(Context, BulkInsertTarget.LoadFromTable(Context, tableName));

            var fromDb = SQL($@"
                select *
                from {tableName}
            ").ReadMetaObjects<IncludingIdentityColumn>(Context).Single();
            PAssert.That(() => fromDb.AnIdentity == 1);
        }

        [Fact]
        public void BulkCopySupportsKeepIdentity()
        {
            var tableName = SQL($"#MyTable");
            SQL($@"
                create table {tableName} (
                    Id int not null primary key
                    , AnIdentity int not null identity(1,1) -- deliberately not placed at the end or start
                    , Bla nvarchar(max) not null
                )
            ").ExecuteNonQuery(Context);

            new[] {
                new IncludingIdentityColumn {
                    Id = 11,
                    Bla = "Something"
                }
            }.BulkCopyToSqlServer(Context, BulkInsertTarget.LoadFromTable(Context, tableName).With(SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.CheckConstraints));

            var fromDb = SQL($@"
                select *
                from {tableName}
            ").ReadMetaObjects<IncludingIdentityColumn>(Context).Single();
            PAssert.That(() => fromDb.AnIdentity == 0);
        }

        [Fact]
        public void BulkCopySupportsCumputedColumnEvenAfterDropTable()
        {
            var tableName = SQL($"#MyTable");
            SQL($@"
                create table {tableName} (
                    Id int not null primary key
                    , ToDrop int null
                    , Computed as convert(bit, 1) -- deliberately not placed at the end
                    , Bla nvarchar(max) not null
                )
            ").ExecuteNonQuery(Context);

            SQL($@"
                alter table {tableName}
                drop column ToDrop;
            ").ExecuteNonQuery(Context);

            new[] {
                new ComputedColumnExample {
                    Id = 11,
                    Bla = "Something"
                }
            }.BulkCopyToSqlServer(Context, BulkInsertTarget.LoadFromTable(Context, tableName));

            var fromDb = SQL($@"
                select *
                from {tableName}
            ").ReadMetaObjects<ComputedColumnExample>(Context).Single();
            PAssert.That(() => fromDb.Computed);
        }

        [Fact]
        public void BulkCopyVerifiesExistanceOfDestinationColumns()
        {
            var target
                = CreateTempTable();
            Assert.ThrowsAny<Exception>(() => new BlaWithExtraClrFields[0].BulkCopyToSqlServer(Context.Connection, target));
        }
    }
}
