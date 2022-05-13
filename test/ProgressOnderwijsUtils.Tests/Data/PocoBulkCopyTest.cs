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

    BulkInsertTarget CreateTempTable()
    {
        var tableName = SQL($"#MyTable");
        SQL(
            $@"
                create table {tableName} (
                    id int not null primary key
                    , bla nvarchar(max) null
                    , bla2 nvarchar(max) not null
                )
            "
        ).ExecuteNonQuery(Connection);

        return BulkInsertTarget.LoadFromTable(Connection, tableName);
    }

    sealed record ComputedColumnExample : IWrittenImplicitly, IReadImplicitly
    {
        public int Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public bool Computed { get; set; }
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public string Bla { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
        public static void CreateTargetTable(SqlConnection sqlConnection, ParameterizedSql tableName)
            => SQL(
                $@"
                create table {tableName} (
                    Id int not null primary key
                    , Computed as convert(bit, 1) -- deliberately not placed at the end
                    , Bla nvarchar(max) not null
                )
            "
            ).ExecuteNonQuery(sqlConnection);
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
        var target
            = CreateTempTable();
        SampleObjects.BulkCopyToSqlServer(Connection, target);
        var fromDb = SQL($"select * from #MyTable order by Id").ReadPocos<BlaOk>(Connection);
        PAssert.That(() => SampleObjects.SequenceEqual(fromDb));
    }

    [Fact]
    public void BulkCopyChecksNames()
    {
        var target
            = CreateTempTable();
        _ = Assert.ThrowsAny<Exception>(() => new BlaWithMispelledColumns[1].BulkCopyToSqlServer(Connection, target));
    }

    [Fact]
    public void BulkCopyChecksTypes()
    {
        var target
            = CreateTempTable();
        _ = Assert.ThrowsAny<Exception>(() => new BlaWithMistypedColumns[1].BulkCopyToSqlServer(Connection, target));
    }

    [Fact]
    public void BulkCopyChecksTypes2()
    {
        var target
            = CreateTempTable();
        _ = Assert.ThrowsAny<Exception>(() => new BlaWithMistypedColumns2[1].BulkCopyToSqlServer(Connection, target));
    }

    [Fact]
    public void BulkCopySupportsColumnReordering()
    {
        var target
            = CreateTempTable();
        SampleObjects.BulkCopyToSqlServer(Connection, target);
        var fromDb = SQL($"select * from #MyTable order by Id").ReadPocos<BlaOk2>(Connection);
        PAssert.That(() => SampleObjects.SequenceEqual(fromDb.Select(x => new BlaOk { Id = x.Id, Bla = x.Bla, Bla2 = x.Bla2, })));
    }

    [Fact]
    public void BulkCopySupportsCumputedColumn()
    {
        var tableName = SQL($"#MyTable");
        ComputedColumnExample.CreateTargetTable(Connection, tableName);

        new[] {
            new ComputedColumnExample {
                Id = 11,
                Bla = "Something",
            },
        }.BulkCopyToSqlServer(Connection, BulkInsertTarget.LoadFromTable(Connection, tableName));

        var fromDb = SQL(
            $@"
                select *
                from {tableName}
            "
        ).ReadPocos<ComputedColumnExample>(Connection).Single();
        PAssert.That(() => fromDb.Computed);
    }

    [Fact]
    public void BulkCopySupportsCumputedColumn_ViaInternalGetter()
    {
        var tableName = SQL($"#MyTable");
        ComputedColumnExample.CreateTargetTable(Connection, tableName);

        new[] {
            new ComputedColumnViaInternalExample {
                Id = 11,
                Bla = "Something",
            },
        }.BulkCopyToSqlServer(Connection, BulkInsertTarget.LoadFromTable(Connection, tableName));

        var fromDb = SQL(
            $@"
                select *
                from {tableName}
            "
        ).ReadPocos<ComputedColumnExample>(Connection).Single();
        PAssert.That(() => fromDb.Computed);
    }

    [Fact]
    public void BulkCopySupportsCumputedColumn_WithoutSmallBatchOptimization()
    {
        var tableName = SQL($"#MyTable");
        ComputedColumnExample.CreateTargetTable(Connection, tableName);

        var asInserted = Enumerable.Range(0, 2000).Select(
            id =>
                new ComputedColumnExample {
                    Id = id,
                    Bla = "Something",
                }
        ).ToArray();
        PAssert.That(() => asInserted.None(o => o.Computed));
        asInserted.BulkCopyToSqlServer(Connection, BulkInsertTarget.LoadFromTable(Connection, tableName));

        var fromDb = SQL(
            $@"
                select *
                from {tableName}
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
        var tableName = SQL($"#MyTable");
        SQL(
            $@"
                create table {tableName} (
                    Id int not null primary key
                    , AnIdentity int not null identity(1,1) -- deliberately not placed at the end or start
                    , Bla nvarchar(max) not null
                )
            "
        ).ExecuteNonQuery(Connection);

        new[] {
            new ExcludingIdentityColumn {
                Id = 11,
                Bla = "Something",
            },
        }.BulkCopyToSqlServer(Connection, BulkInsertTarget.LoadFromTable(Connection, tableName));

        var fromDb = SQL(
            $@"
                select *
                from {tableName}
            "
        ).ReadPocos<IncludingIdentityColumn>(Connection).Single();
        PAssert.That(() => fromDb.AnIdentity == 1);
    }

    [Fact]
    public void BulkCopyIgnoresPropertiesCorrespondingIdentityColumns()
    {
        var tableName = SQL($"#MyTable");
        SQL(
            $@"
                create table {tableName} (
                    Id int not null primary key
                    , AnIdentity int not null identity(1,1) -- deliberately not placed at the end or start
                    , Bla nvarchar(max) not null
                )
            "
        ).ExecuteNonQuery(Connection);

        new[] {
            new IncludingIdentityColumn {
                Id = 11,
                AnIdentity = 37,
                Bla = "Something",
            },
        }.BulkCopyToSqlServer(Connection, BulkInsertTarget.LoadFromTable(Connection, tableName));

        var fromDb = SQL(
            $@"
                select *
                from {tableName}
            "
        ).ReadPocos<IncludingIdentityColumn>(Connection).Single();
        PAssert.That(() => fromDb.AnIdentity == 1);
    }

    [Fact]
    public void BulkCopySupportsKeepIdentity()
    {
        var tableName = SQL($"#MyTable");
        SQL(
            $@"
                create table {tableName} (
                    Id int not null primary key
                    , AnIdentity int not null identity(1,1) -- deliberately not placed at the end or start
                    , Bla nvarchar(max) not null
                )
            "
        ).ExecuteNonQuery(Connection);

        new[] {
            new IncludingIdentityColumn {
                Id = 11,
                Bla = "Something",
            },
        }.BulkCopyToSqlServer(Connection, BulkInsertTarget.LoadFromTable(Connection, tableName).With(SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.CheckConstraints));

        var fromDb = SQL(
            $@"
                select *
                from {tableName}
            "
        ).ReadPocos<IncludingIdentityColumn>(Connection).Single();
        PAssert.That(() => fromDb.AnIdentity == 0);
    }

    [Fact]
    public void BulkCopySupportsCumputedColumnEvenAfterDropTable()
    {
        var tableName = SQL($"#MyTable");
        ComputedColumnExample.CreateTargetTable(Connection, tableName);

        SQL(
            $@"
                alter table {tableName}
                drop column ToDrop;
            "
        ).ExecuteNonQuery(Connection);

        new[] {
            new ComputedColumnExample {
                Id = 11,
                Bla = "Something",
            },
        }.BulkCopyToSqlServer(Connection, BulkInsertTarget.LoadFromTable(Connection, tableName));

        var fromDb = SQL(
            $@"
                select *
                from {tableName}
            "
        ).ReadPocos<ComputedColumnExample>(Connection).Single();
        PAssert.That(() => fromDb.Computed);
    }

    [Fact]
    public void BulkCopyVerifiesExistanceOfDestinationColumns()
    {
        var target
            = CreateTempTable();
        _ = Assert.ThrowsAny<Exception>(() => new BlaWithExtraClrFields[1].BulkCopyToSqlServer(Connection, target));
    }
}
