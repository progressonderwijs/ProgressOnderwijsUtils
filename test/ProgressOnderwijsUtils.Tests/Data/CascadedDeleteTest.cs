namespace ProgressOnderwijsUtils.Tests.Data;

public sealed class CascadedDeleteTest : TransactedLocalConnection
{
    enum AId { One = 1, Two = 2, }

    [Fact]
    public void CascadedDeleteFollowsAForeignKey()
    {
        SQL(
            $@"
                create table T1 ( A int primary key, B int);
                create table T2 ( C int primary key, A int references T1 (A) );

                insert into T1 values (1,11), (2,22), (3, 33);
                insert into T2 values (111,1), (333,3);
            "
        ).ExecuteNonQuery(Connection);

        var initialDependentValues = SQL($"select C from T2").ReadPlain<int>(Connection);

        PAssert.That(() => initialDependentValues.SetEqual(new[] { 111, 333, }));

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.T1"), false, null, null, "A", AId.One, AId.Two);

        var finalDependentValues = SQL($"select C from T2").ReadPlain<int>(Connection);
        PAssert.That(() => finalDependentValues.SetEqual(new[] { 333, }));
        PAssert.That(() => deletionReport.Select(t => t.Table).SequenceEqual(new[] { "dbo.T2", "dbo.T1", }));
    }

    [Fact]
    public void CascadedDeleteFollowsAUniqueConstraintBasedForeignKey()
    {
        SQL(
            $@"
                create table T1 ( A int not null unique, B int);
                create table T2 ( C int not null, A int references T1 (A) );

                insert into T1 values (1,11), (2,22), (3, 33);
                insert into T2 values (111,1), (333,3);
            "
        ).ExecuteNonQuery(Connection);

        var initialDependentValues = SQL($"select C from T2").ReadPlain<int>(Connection);

        PAssert.That(() => initialDependentValues.SetEqual(new[] { 111, 333, }));

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.T1"), false, null, null, "A", AId.One, AId.Two);

        var finalDependentValues = SQL($"select C from T2").ReadPlain<int>(Connection);
        PAssert.That(() => finalDependentValues.SetEqual(new[] { 333, }));
        PAssert.That(() => deletionReport.Select(t => t.Table).SequenceEqual(new[] { "dbo.T2", "dbo.T1", }));
    }

    [Fact]
    public void CascadedDeleteDetectsCycles()
    {
        SQL(
            $@"
                create table T1 ( A int not null unique, B int);
                create table T2 ( B int not null unique, A int references T1 (A) );
                alter table T1 add constraint T1FK foreign key (B) references T2(B);

                insert into T1 values (1,null), (2,null), (3, null);
                insert into T2 values (111,1), (333,3);
                
                update T1 set B = 111 where A = 1;
                update T1 set B = 333 where A = 3;

            "
        ).ExecuteNonQuery(Connection);

        var initialDependentValues = SQL($"select B from T2").ReadPlain<int>(Connection);

        PAssert.That(() => initialDependentValues.SetEqual(new[] { 111, 333, }));

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var ex = Assert.ThrowsAny<Exception>(() => CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.T1"), false, null, null, "A", AId.One, AId.Two));
        PAssert.That(() => ex.Message.Contains("dbo.T2->dbo.T1->dbo.T2->dbo.T1"));
    }

    [Fact]
    public void CascadedDeleteWorksWithIdentityKey()
    {
        static DataTable PksToDelete(string name, params int[] pks)
        {
            var table = new DataTable();
            _ = table.Columns.Add(name, typeof(int));
            foreach (var pk in pks) {
                _ = table.Rows.Add(pk);
            }
            return table;
        }

        SQL(
            $@"
                create table T1 ( A int primary key identity(1,1), B int);

                insert into T1 values (11), (22), (33);
            "
        ).ExecuteNonQuery(Connection);

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.T1"), false, null, null, PksToDelete("A", 1, 2));
        var finalValues = SQL($"select B from T1").ReadPlain<int>(Connection);

        PAssert.That(() => deletionReport.Select(t => t.Table).SequenceEqual(new[] { "dbo.T1", }));
        PAssert.That(() => finalValues.SetEqual(new[] { 33, }));
    }

    public struct RootId : IWrittenImplicitly, IReadImplicitly
    {
        public int Root { get; set; }
    }

    [Fact]
    public void CascadedDeleteFollowsADiamondOfForeignKey()
    {
        SQL(
            $@"
                create table TRoot ( root int primary key, B int);
                create table T1 ( C int primary key, root int references TRoot (root));
                create table T2 ( D int primary key, root int references TRoot (root));
                create table TLeaf ( Z int primary key, C int references T1 (C), D int references T2 (D) );
            "
        ).ExecuteNonQuery(Connection);

        SQL(
            $@"
                insert into TRoot values (1,11), (2,22), (3, 33);
                insert into T1 values (4,1), (5, 2);
                insert into T2 values (4,2), (5, 3);

                insert into TLeaf values (1, 4, null), (2, null, 4), (3, null, 5), (4, null, null);
            "
        ).ExecuteNonQuery(Connection);

        var initialTLeafKeys = SQL($"select Z from TLeaf").ReadPlain<int>(Connection);

        PAssert.That(() => initialTLeafKeys.SetEqual(new[] { 1, 2, 3, 4, }));

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.TRoot"), true, null, null, new RootId { Root = 1, }, new RootId { Root = 2, });

        var finalT2 = SQL($"select D from T2").ReadPlain<int>(Connection);
        PAssert.That(() => finalT2.SetEqual(new[] { 5, }));

        var finalTLeafKeys = SQL($"select Z from TLeaf").ReadPlain<int>(Connection);
        PAssert.That(() => finalTLeafKeys.SetEqual(new[] { 3, 4, }));

        var rowsFromT1 = deletionReport.Where(t => t.Table == "dbo.T1").ToArray().Single().DeletedRows.AssertNotNull().Rows.Cast<DataRow>();
        PAssert.That(() => rowsFromT1.Select(dr => (int)dr["C"]).SetEqual(new[] { 4, 5, }));
    }

    [Fact]
    public void NonUniqueForeignKeyDoesNotLeadToExplosionOfDeletions()
    {
        SQL(
            $@"
                create table TRoot ( root int primary key, B int);
                create table T1 ( C int primary key, root int references TRoot (root));
                create table TLeaf ( Z int primary key, C int references T1 (C));
            "
        ).ExecuteNonQuery(Connection);

        SQL(
            $@"
                insert into TRoot values (1, 11), (2, 22), (3, 33);
                insert into T1 values (1,1), (2, 2), (3, 3), (11,1), (22,2), (33,3);

                insert into TLeaf values (1, 1);
            "
        ).ExecuteNonQuery(Connection);

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.TRoot"), true, null, null, new RootId { Root = 1, });

        var rowsFromTRoot = deletionReport.Single(t => t.Table == "dbo.TRoot");
        PAssert.That(() => rowsFromTRoot.DeletedAtMostRowCount == 1);
        var rowsFromT1 = deletionReport.Single(t => t.Table == "dbo.T1");
        PAssert.That(() => rowsFromT1.DeletedAtMostRowCount == 2);
        var rowsFromTLeaf = deletionReport.Single(t => t.Table == "dbo.TLeaf");
        PAssert.That(() => rowsFromTLeaf.DeletedAtMostRowCount == 1);
    }

    [Fact]
    public void CascadeDeleteBreaksOnSpecificTable()
    {
        SQL(
            $@"
                create table T1 (A int primary key);
                create table T2 (B int primary key, A int references T1 (A) on delete set null);
                create table T3 (C int primary key, A int references T1 (A));

                insert into T1 values (1);
                insert into T2 values (2, 1);
                insert into T3 values (3, 1);
            "
        ).ExecuteNonQuery(Connection);

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.T1"), false, null, fk => fk.ReferencingChildTable.QualifiedName != "dbo.T2", "A", AId.One);

        PAssert.That(() => deletionReport.Select(t => t.Table).SequenceEqual(new[] { "dbo.T3", "dbo.T1", }));
    }

    [Fact]
    public void CascadeDeleteFollowsNullableForeignKeyAndThenSameTableForeignKey()
    {
        SQL(
            $"""
            create table T1 (A int primary key);
            create table T2 (B int primary key, A int references T1, C int references T2);

            insert into T1 values (1);
            insert into T2 values (2, 1, null);
            insert into T2 values (3, null, 2);
            """
        ).ExecuteNonQuery(Connection);

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.T1"), true, null, null, "A", AId.One)
            .Select(StringifyDeletionReportRow).JoinStrings("\n");

        Assert.Equal(
            """
            dbo.T2 (at most #1)
                B:3; A:; C:2

            dbo.T2 (at most #1)
                B:2; A:1; C:

            dbo.T1 (at most #1)
                A:1

            """,
            deletionReport
        );
    }

    static string StringifyDeletionReportRow(CascadedDelete.DeletionReport r)
    {
        return $"{r.Table} (at most #{r.DeletedAtMostRowCount})\n" +
            r.DeletedRows switch {
                { } dt => dt.Rows.Cast<DataRow>().Select(dr => dt.Columns.Cast<DataColumn>().Select(col => col.ColumnName + ":" + dr[col].ToString()).JoinStrings("; ")).Select(line => $"    {line}\n").JoinStrings(),
                null => "",
            };
    }

    sealed record C_rec(int iid) : IReadImplicitly;

    [Fact]
    public void CascadeDeleteDoesntDeleteTooManyRecordsForSelfRefFk()
    {
        SQL(
            $"""
            create table C (iid int primary key);
            create table AB (bid int primary key, iid int references C, ic int references AB);

            insert into C values (4);
            insert into C values (2);
            insert into C values (1);
            insert into AB values (12, 4, null);
            insert into AB values (3, 2, null);
            insert into AB values (5, 1, 3);
            """
        ).ExecuteNonQuery(Connection);

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.C"), true, null, null, new C_rec(4))
            .Select(StringifyDeletionReportRow).JoinStrings("\n");

        PAssert.That(
            () =>
                """
                dbo.AB (at most #1)
                    bid:12; iid:4; ic:

                dbo.C (at most #1)
                    iid:4

                """ ==
                deletionReport
        );
    }

    [Fact]
    public void CascadeDeleteAllowsSameTableCyclesWhenThoseAreSimultaneouslyDeletable()
    {
        SQL(
            $@"
                create table T1 (A int primary key);
                create table T2 (B int primary key, A int references T1, C int references T2);

                insert into T1 values (1);
                insert into T2 values (2, 1, null);
                insert into T2 values (3, 1, 2);
                update T2 set C=3 where B=2
            "
        ).ExecuteNonQuery(Connection);

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.T1"), false, null, null, "A", AId.One);

        PAssert.That(() => deletionReport.Select(t => t.Table).SequenceEqual(new[] { "dbo.T2", "dbo.T1", }));
    }


    [Fact]
    public void CascadeDeleteWithForeignKeySpecificationOnDeleteSetNull_is_honored()
    {
        SQL(
            $@"
                create table T1 (A int primary key);
                create table T2 (B int primary key, A int references T1 on delete set null);

                insert into T1 values (1);
                insert into T2 values (2, 1);
                insert into T2 values (3, 1);
            "
        ).ExecuteNonQuery(Connection);

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var initialValues = SQL($"select A from T2").ReadPlain<int?>(Connection);
        var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.T1"), false, null, fk => fk.DeleteReferentialAction != FkReferentialAction.SetNull, "A", AId.One);
        var finalValues = SQL($"select A from T2").ReadPlain<int?>(Connection);

        PAssert.That(() => deletionReport.Select(t => t.Table).SequenceEqual(new[] { "dbo.T1", }));
        PAssert.That(() => initialValues.All(o => o == 1));
        PAssert.That(() => finalValues.All(o => o == null));
    }


    [Fact]
    public void CascadeDelete_also_reports_on_tables_with_rowversion_column()
    {
        SQL($"create table T1 (A int primary key, V rowversion);").ExecuteNonQuery(Connection);
        SQL($"insert into T1 (A) values (1);").ExecuteNonQuery(Connection);

        var version = SQL($"select t.V from T1 t").ReadScalar<byte[]>(Connection).AssertNotNull();

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.T1"), true, null, null, "A", AId.One).Single();
        var deletedRow = deletionReport.DeletedRows.AssertNotNull().Rows.Cast<DataRow>().Single();

        PAssert.That(() => deletionReport.Table == "dbo.T1");
        PAssert.That(() => ((byte[])deletedRow["V"]).SequenceEqual(version));
    }

    [Fact]
    public void CascadeDelete_also_reports_on_tables_with_triggers_and_rowversion_column()
    {
        SQL($"create table T1 (A int primary key, V rowversion);").ExecuteNonQuery(Connection);
        SQL($"create trigger T1t on T1 after delete as insert into T1 (A) values (2);").ExecuteNonQuery(Connection);
        SQL($"insert into T1 (A) values (1);").ExecuteNonQuery(Connection);

        var version = SQL($"select t.V from T1 t").ReadScalar<byte[]>(Connection).AssertNotNull();

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.T1"), true, null, null, "A", AId.One).Single();
        var deletedRow = deletionReport.DeletedRows.AssertNotNull().Rows.Cast<DataRow>().Single();

        PAssert.That(() => deletionReport.Table == "dbo.T1");
        PAssert.That(() => ((byte[])deletedRow["V"]).SequenceEqual(version));
    }
}
