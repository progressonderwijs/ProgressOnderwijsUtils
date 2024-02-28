using System.ComponentModel.DataAnnotations.Schema;

namespace ProgressOnderwijsUtils.Tests.Data;

public sealed class PocoBulkCopyTest : TransactedLocalConnection
{
    static readonly BlaOk[] SampleObjects = {
        new() { Bla = "bl34ga", Bla2 = "blaasdfgasfg2", Id = -1, },
        new() { Bla = "bla", Bla2 = "bla2", Id = 0, },
        new() { Bla = "dfg", Bla2 = "bla342", Id = 1, },
        new() { Bla = "blfgjha", Bla2 = "  bla2  ", Id = 2, },
        new() { Bla2 = "", Id = 3, },
    };

    public sealed record BlaOk : IWrittenImplicitly, IReadImplicitly
    {
        public int Id { get; set; }
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public string Bla2 { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
        public string? Bla { get; set; }

        public static BulkInsertTarget CreateTargetTable(SqlConnection conn, ParameterizedSql tableName)
        {
            SQL(
                $@"
                create table {tableName} (
                    id int not null primary key
                    , bla nvarchar(max) null
                    , bla2 nvarchar(max) not null
                )
            "
            ).ExecuteNonQuery(conn);

            return BulkInsertTarget.LoadFromTable(conn, tableName);
        }
    }

    public sealed record BlaOk2 : IWrittenImplicitly, IReadImplicitly
    {
        public int Id { get; set; }
        public string? Bla { get; set; }
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public string Bla2 { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
    }

    public sealed record BlaWithMispelledColumns : IWrittenImplicitly, IReadImplicitly
    {
        public int Idd { get; set; }
        public string? Bla { get; set; }
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public string Bla2 { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
    }

    public sealed record BlaWithMistypedColumns : IWrittenImplicitly, IReadImplicitly
    {
        public int Bla { get; set; }
        public int Id { get; set; }
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public string Bla2 { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
    }

    public sealed record BlaWithMistypedColumns2 : IWrittenImplicitly, IReadImplicitly
    {
        public int Bla { get; set; }
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public string Id { get; set; }
        public string Bla2 { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
    }

    public sealed record BlaWithExtraClrFields : IWrittenImplicitly, IReadImplicitly
    {
        public string? ExtraBla { get; set; }
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public string Id { get; set; }
        public int Bla { get; set; }
        public string Bla2 { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
    }

    public sealed record BlaWithMissingClrFields : IWrittenImplicitly, IReadImplicitly
    {
        public int Id { get; set; }
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public string Bla2 { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
    }

    sealed record ComputedColumnExample : IWrittenImplicitly, IReadImplicitly
    {
        public int Id { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public bool Computed { get; set; }

        public string? Bla { get; set; }

        public static BulkInsertTarget CreateTargetTable(SqlConnection conn, ParameterizedSql tableName)
        {
            SQL(
                $@"
                create table {tableName} (
                    Id int not null primary key
                    , Computed as convert(bit, 1) -- deliberately not placed at the end
                    , Bla nvarchar(max) not null
                )
            "
            ).ExecuteNonQuery(conn);
            return BulkInsertTarget.LoadFromTable(conn, tableName) with { SilentlySkipReadonlyTargetColumns = true, };
        }
    }

    sealed record ComputedColumnExample_LackingAnnotation : IWrittenImplicitly, IReadImplicitly
    {
        public int Id { get; set; }
        public bool Computed { get; set; }
        public string? Bla { get; set; }
    }

    sealed record ComputedColumnViaInternalExample : IWrittenImplicitly, IReadImplicitly
    {
        public int Id { get; set; }
        public bool Computed { internal get; set; }
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public string Bla { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
    }

    [Fact]
    public void BulkCopyAllowsExactMatch()
    {
        var target = BlaOk.CreateTargetTable(Connection, SQL($"#MyTable"));
        SampleObjects.BulkCopyToSqlServer(Connection, target);
        var fromDb = SQL($"select * from {target.TableNameSql} order by Id").ReadPocos<BlaOk>(Connection);
        PAssert.That(() => SampleObjects.SequenceEqual(fromDb));
    }

    [Fact]
    public void BulkCopyChecksNames()
    {
        var target = BlaOk.CreateTargetTable(Connection, SQL($"#MyTable"));
        _ = Assert.ThrowsAny<Exception>(() => new BlaWithMispelledColumns[1].BulkCopyToSqlServer(Connection, target));
    }

    [Fact]
    public void BulkCopyWontInsertComputedColumns()
    {
        var target = ComputedColumnExample.CreateTargetTable(Connection, SQL($"#tmp")) with { SilentlySkipReadonlyTargetColumns = false, };

        _ = Assert.ThrowsAny<Exception>(() => new ComputedColumnExample_LackingAnnotation[] { new() { Bla = "ja", Computed = true, Id = 13, }, }.BulkCopyToSqlServer(Connection, target));
    }

    [Fact]
    public void BulkCopyChecksTypes()
    {
        var target = BlaOk.CreateTargetTable(Connection, SQL($"#MyTable"));
        _ = Assert.ThrowsAny<Exception>(() => new BlaWithMistypedColumns[1].BulkCopyToSqlServer(Connection, target));
    }

    [Fact]
    public void BulkCopyChecksTypes2()
    {
        var target = BlaOk.CreateTargetTable(Connection, SQL($"#MyTable"));
        _ = Assert.ThrowsAny<Exception>(() => new BlaWithMistypedColumns2[1].BulkCopyToSqlServer(Connection, target));
    }

    [Fact]
    public void BulkCopySupportsColumnReordering()
    {
        var target = BlaOk.CreateTargetTable(Connection, SQL($"#MyTable"));
        SampleObjects.BulkCopyToSqlServer(Connection, target);
        var fromDb = SQL($"select * from {target.TableNameSql} order by Id").ReadPocos<BlaOk2>(Connection);
        PAssert.That(() => SampleObjects.SequenceEqual(fromDb.Select(x => new BlaOk { Id = x.Id, Bla = x.Bla, Bla2 = x.Bla2, })));
    }

    [Fact]
    public void BulkCopySupportsCumputedColumn()
    {
        var bulkInsertTarget = ComputedColumnExample.CreateTargetTable(Connection, SQL($"#MyTable"));

        new[] {
            new ComputedColumnExample {
                Id = 11,
                Bla = "Something",
            },
        }.BulkCopyToSqlServer(Connection, bulkInsertTarget);

        var fromDb = SQL(
            $@"
                select *
                from {bulkInsertTarget.TableNameSql}
            "
        ).ReadPocos<ComputedColumnExample>(Connection).Single();
        PAssert.That(() => fromDb.Computed);
    }

    [Fact]
    public void BulkCopySupportsCumputedColumn_ViaInternalGetter()
    {
        var bulkInsertTarget = ComputedColumnExample.CreateTargetTable(Connection, SQL($"#MyTable"));
        new[] {
            new ComputedColumnViaInternalExample {
                Id = 11,
                Bla = "Something",
            },
        }.BulkCopyToSqlServer(Connection, bulkInsertTarget);

        var fromDb = SQL(
            $@"
                select *
                from {bulkInsertTarget.TableNameSql}
            "
        ).ReadPocos<ComputedColumnExample>(Connection).Single();
        PAssert.That(() => fromDb.Computed);
    }

    [Fact]
    public void BulkCopySupportsCumputedColumn_WithoutSmallBatchOptimization()
    {
        var bulkInsertTarget = ComputedColumnExample.CreateTargetTable(Connection, SQL($"#MyTable"));

        var asInserted = Enumerable.Range(0, 2000).Select(
            id =>
                new ComputedColumnExample {
                    Id = id,
                    Bla = "Something",
                }
        ).ToArray();
        PAssert.That(() => asInserted.None(o => o.Computed));
        asInserted.BulkCopyToSqlServer(Connection, bulkInsertTarget);

        var fromDb = SQL(
            $@"
                select *
                from {bulkInsertTarget.TableNameSql}
            "
        ).ReadPocos<ComputedColumnExample>(Connection);
        PAssert.That(() => fromDb.All(o => o.Computed));
        PAssert.That(() => fromDb.Length == 2000);
    }

    sealed record IncludingIdentityColumn : IWrittenImplicitly, IReadImplicitly
    {
        public int Id { get; set; }
        public int AnIdentity { get; set; }
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public string Bla { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
        public static BulkInsertTarget CreateTargetTable(SqlConnection conn, ParameterizedSql tableName)
        {
            SQL(
                $@"
                create table {tableName} (
                    Id int not null primary key
                    , AnIdentity int not null identity(1,1) -- deliberately not placed at the end or start
                    , Bla nvarchar(max) not null
                )
            "
            ).ExecuteNonQuery(conn);
            return BulkInsertTarget.LoadFromTable(conn, tableName);
        }
    }

    sealed record ExcludingIdentityColumn : IWrittenImplicitly, IReadImplicitly
    {
        public int Id { get; set; }
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public string Bla { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
    }

    [Fact]
    public void BulkCopyAllowsOmittingSourcePropertiesForIdentityColumns()
    {
        var bulkInsertTarget = IncludingIdentityColumn.CreateTargetTable(Connection, SQL($"#MyTable"));

        new[] {
            new ExcludingIdentityColumn {
                Id = 11,
                Bla = "Something",
            },
        }.BulkCopyToSqlServer(Connection, bulkInsertTarget);

        var fromDb = SQL(
            $@"
                select *
                from {bulkInsertTarget.TableNameSql}
            "
        ).ReadPocos<IncludingIdentityColumn>(Connection).Single();
        PAssert.That(() => fromDb.AnIdentity == 1);
    }

    [Fact]
    public void BulkCopyIgnoresPropertiesCorrespondingIdentityColumns()
    {
        var bulkInsertTarget = IncludingIdentityColumn.CreateTargetTable(Connection, SQL($"#MyTable"));

        new[] {
            new IncludingIdentityColumn {
                Id = 11,
                AnIdentity = 37,
                Bla = "Something",
            },
        }.BulkCopyToSqlServer(Connection, bulkInsertTarget);

        var fromDb = SQL(
            $@"
                select *
                from {bulkInsertTarget.TableNameSql}
            "
        ).ReadPocos<IncludingIdentityColumn>(Connection).Single();
        PAssert.That(() => fromDb.AnIdentity == 1);
    }

    [Fact]
    public void BulkCopySupportsKeepIdentity()
    {
        var bulkInsertTarget = IncludingIdentityColumn.CreateTargetTable(Connection, SQL($"#MyTable"));

        new[] {
            new IncludingIdentityColumn {
                Id = 11,
                Bla = "Something",
            },
        }.BulkCopyToSqlServer(Connection, bulkInsertTarget with { Options = SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.CheckConstraints, });

        var fromDb = SQL(
            $@"
                select *
                from {bulkInsertTarget.TableNameSql}
            "
        ).ReadPocos<IncludingIdentityColumn>(Connection).Single();
        PAssert.That(() => fromDb.AnIdentity == 0);
    }

    [Fact]
    public void BulkCopySupportsCumputedColumnEvenAfterDropTable()
    {
        var tableName = SQL($"#MyTable");
        SQL(
            $@"
                create table {tableName} (
                    Id int not null primary key
                    , ToDrop int null
                    , Computed as convert(bit, 1) -- deliberately not placed at the end
                    , Bla nvarchar(max) not null
                )
            "
        ).ExecuteNonQuery(Connection);

        SQL(
            $@"
                alter table {tableName}
                drop column ToDrop;
            "
        ).ExecuteNonQuery(Connection);

        var bulkInsertTarget = BulkInsertTarget.LoadFromTable(Connection, tableName) with { SilentlySkipReadonlyTargetColumns = true, };

        new[] {
            new ComputedColumnExample {
                Id = 11,
                Bla = "Something",
            },
        }.BulkCopyToSqlServer(Connection, bulkInsertTarget);

        var fromDb = SQL(
            $@"
                select *
                from {bulkInsertTarget.TableNameSql}
            "
        ).ReadPocos<ComputedColumnExample>(Connection).Single();
        PAssert.That(() => fromDb.Computed);
    }

    [Fact]
    public void BulkCopyVerifiesExistanceOfDestinationColumns()
    {
        var target = BlaOk.CreateTargetTable(Connection, SQL($"#MyTable"));
        _ = Assert.ThrowsAny<Exception>(() => new BlaWithExtraClrFields[1].BulkCopyToSqlServer(Connection, target));
    }
}
