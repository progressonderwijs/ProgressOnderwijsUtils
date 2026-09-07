using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace ProgressOnderwijsUtils.Tests.Data;

public sealed record BulkInsertTestSampleRow : IWrittenImplicitly, IReadImplicitly
{
    public DayOfWeek AnEnum { get; set; }
    public DateTime? ADateTime { get; set; }
    public string? SomeString { get; set; }
    public decimal? LotsOfMoney { get; set; }
    public double VagueNumber { get; set; }
    public TrivialValue<string> CustomBla { get; set; }
    public TrivialValue<string>? CustomBlaThanCanBeNull { get; set; }

    public static BulkInsertTarget CreateTable(SqlConnection sqlConnection, ParameterizedSql tempTableName)
    {
        SQL(
            $"""
            create table {tempTableName} (
                AnEnum int not null
                , ADateTime datetime2
                , SomeString nvarchar(max)
                , LotsOfMoney decimal(19, 5)
                , VagueNumber float not null
                , CustomBla nvarchar(max) not null
                , CustomBlaThanCanBeNull nvarchar(max) null
            )
            """
        ).ExecuteNonQuery(sqlConnection);

        return BulkInsertTarget.LoadFromTable(sqlConnection, tempTableName.CommandText());
    }

    static readonly BulkInsertTestSampleRow[] FourSampleRows = {
        new() {
            ADateTime = new DateTime(2003, 4, 5).AddHours(17.345),
            AnEnum = DayOfWeek.Saturday,
            LotsOfMoney = -12.34m,
            VagueNumber = 123.456,
            SomeString = "sdf",
            CustomBla = TrivialConvertibleValue.Create("aap"),
        },
        new() {
            ADateTime = new DateTime(2013, 8, 7),
            AnEnum = DayOfWeek.Monday,
            LotsOfMoney = null,
            //VagueNumer = double.NaN,
            SomeString = null,
            CustomBla = TrivialConvertibleValue.Create("aap"),
        },
        new() {
            ADateTime = null,
            AnEnum = (DayOfWeek)12345,
            LotsOfMoney = 6543,
            VagueNumber = 1 / 3.0,
            SomeString = "Hello world!",
            CustomBla = TrivialConvertibleValue.Create("aap"),
        },
        new() {
            ADateTime = DateTime.MaxValue,
            AnEnum = DayOfWeek.Friday,
            LotsOfMoney = 1000_000_000.00m,
            VagueNumber = Math.E,
            SomeString = "annual income",
            CustomBla = TrivialConvertibleValue.Create("aap"),
            CustomBlaThanCanBeNull = TrivialConvertibleValue.Create("noot"),
        },
    };

    public static BulkInsertTestSampleRow[] SampleRows(int n)
        => Enumerable.Range(0, (n + 3) / 4).SelectMany(_ => FourSampleRows).ToArray();
}

public sealed class BulkInsertTest : TransactedLocalConnection
{
    [Fact]
    public void BulkCopysWithConcurrentQueriesCrash()
    {
        var target = BulkInsertTestSampleRow.CreateTable(Connection, SQL($"#test"));
        var evilEnumerable = BulkInsertTestSampleRow.SampleRows(16).Where(_ => SQL($"select 1").ReadScalar<int>(Connection) == 1);
        _ = Assert.ThrowsAny<Exception>(() => evilEnumerable.BulkCopyToSqlServer(Connection, target));
    }

    [Fact]
    public void BulkInsertAndReadRoundTrips()
    {
        var target = BulkInsertTestSampleRow.CreateTable(Connection, SQL($"#test"));
        BulkInsertTestSampleRow.SampleRows(4).BulkCopyToSqlServer(Connection, target);
        var fromDb = SQL($"select * from #test").ReadPocos<BulkInsertTestSampleRow>(Connection);
        AssertCollectionsEquivalent(BulkInsertTestSampleRow.SampleRows(4), fromDb);
    }

    [Fact]
    public async Task BulkInsertAsyncAndReadRoundTrips_BelowSmallBatchThreshold()
    {
        // Row count strictly below SmallBatchInsertImplementation.ThresholdForUsingSqlBulkCopy (=6),
        // so BulkInsertAsync goes through TrySmallBatchInsertOptimizationAsync (per-row async INSERT path).
        var rowCount = SmallBatchInsertImplementation.ThresholdForUsingSqlBulkCopy - 2;
        var target = BulkInsertTestSampleRow.CreateTable(Connection, SQL($"#test"));
        await target.BulkInsertAsync(Connection, BulkInsertTestSampleRow.SampleRows(rowCount), cancel: TestContext.Current.CancellationToken);
        var fromDb = await SQL($"select * from #test").ReadPocosAsync<BulkInsertTestSampleRow>(Connection, TestContext.Current.CancellationToken);
        AssertCollectionsEquivalent(BulkInsertTestSampleRow.SampleRows(rowCount), fromDb);
    }

    [Fact]
    public async Task BulkInsertAsyncAndReadRoundTrips_AtOrAboveSmallBatchThreshold()
    {
        // Row count at/above the threshold so BulkInsertAsync goes through WriteToServerAsync (SqlBulkCopy path).
        var rowCount = SmallBatchInsertImplementation.ThresholdForUsingSqlBulkCopy + 2;
        var target = BulkInsertTestSampleRow.CreateTable(Connection, SQL($"#test"));
        await target.BulkInsertAsync(Connection, BulkInsertTestSampleRow.SampleRows(rowCount), cancel: TestContext.Current.CancellationToken);
        var fromDb = await SQL($"select * from #test").ReadPocosAsync<BulkInsertTestSampleRow>(Connection, TestContext.Current.CancellationToken);
        AssertCollectionsEquivalent(BulkInsertTestSampleRow.SampleRows(rowCount), fromDb);
    }

    [Fact]
    public void BulkInsertIsTraceable()
    {
        var sqlCommandTracer = SqlCommandTracer.CreateAlwaysOnTracer(SqlTracerAgumentInclusion.IncludingArgumentValues);
        var target = BulkInsertTestSampleRow.CreateTable(Connection, SQL($"#test"));
        Connection.Site = new SqlConnectionContext(sqlCommandTracer, CommandTimeoutDefaults.NoScalingNoTimeout);
        var rowCountToInsert = 40;
        BulkInsertTestSampleRow.SampleRows(rowCountToInsert).BulkCopyToSqlServer(Connection, target);
        var fromDb = SQL($"select * from #test").ReadPocos<BulkInsertTestSampleRow>(Connection);
        var tracedCommands = sqlCommandTracer.ListAllCommands();

        PAssert.That(() => tracedCommands.Length == 2); // tracedCommands[0].EventContent
        var expectedTraceEvent = $"Bulk inserted {rowCountToInsert} rows from {typeof(BulkInsertTestSampleRow).ToCSharpFriendlyTypeName()} into table {target.TableName}.";
        PAssert.That(() => tracedCommands[0].EventContent == expectedTraceEvent); // tracedCommands[0].EventContent

        PAssert.That(() => fromDb.Length == rowCountToInsert);
    }

    [Fact]
    public void BulkInsertAndReadRoundTrips_ManyRows()
    {
        var manyRows = BulkInsertTestSampleRow.SampleRows(400);
        for (var index = 0; index < manyRows.Length; index++) {
            manyRows[index].VagueNumber = index / 16.0; //make sure all rows are distinct for this test.
        }
        var target = BulkInsertTestSampleRow.CreateTable(Connection, SQL($"#test"));
        manyRows.BulkCopyToSqlServer(Connection, target);
        var fromDb = SQL($"select * from #test").ReadPocos<BulkInsertTestSampleRow>(Connection);
        AssertCollectionsEquivalent(manyRows, fromDb);
        var suspciousObjectsThatRoundTrippedFromDbAndAreReferenceEqualsToSource = manyRows.Intersect(fromDb, new ReferenceEqualityComparer<BulkInsertTestSampleRow>()).ToArray();
        PAssert.That(() => suspciousObjectsThatRoundTrippedFromDbAndAreReferenceEqualsToSource.None(), "just to make sure bulk insert actually isn't somehow staying in memory");
    }

    [Fact]
    public void CanInsertDatatable()
    {
        var target = BulkInsertTestSampleRow.CreateTable(Connection, SQL($"#test"));
        var target2 = BulkInsertTestSampleRow.CreateTable(Connection, SQL($"#test2"));
        target.BulkInsert(Connection, BulkInsertTestSampleRow.SampleRows(4), cancel: TestContext.Current.CancellationToken);

        var dataTable = SQL($"select * from #test").OfDataTable().Execute(Connection);
        target2.BulkInsert(Connection, dataTable);

        var fromDb = SQL($"select * from #test2").ReadPocos<BulkInsertTestSampleRow>(Connection);
        AssertCollectionsEquivalent(BulkInsertTestSampleRow.SampleRows(4), fromDb);
    }

    sealed record SampleRow2 : IWrittenImplicitly, IReadImplicitly
    {
        public int intNonNull { get; set; }
        public int? intNull { get; set; }
        public string? stringNull { get; set; }
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public string stringNonNull { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
    }

    [Fact]
    public void CanInsertDatareader()
    {
        using var conn2 = new SqlConnection(ConnectionString);
        conn2.Open();
        var query = SQL(
            $"""
            select *
            from (
                values (1, null, 'test', 'test2')
                , (2, 1, null, 'test3')
            ) x(intNonNull, intNull, stringNull, stringNonNull)
            """
        ).OfPocos<SampleRow2>();
        var expectedData = new[] {
            new SampleRow2 { intNonNull = 1, intNull = null, stringNull = "test", stringNonNull = "test2", },
            new SampleRow2 { intNonNull = 2, intNull = 1, stringNull = null, stringNonNull = "test3", },
        };
        AssertCollectionsEquivalent(expectedData, query.Execute(conn2)); //sanity check that we're testing consistent data

        SQL(
            $"""
            create table #tmp (
                intNonNull int not null
                , intNull int null
                , stringNull nvarchar(max) null
                , stringNonNull nvarchar(max) not null
            )
            """
        ).ExecuteNonQuery(Connection);
        var target = BulkInsertTarget.LoadFromTable(Connection, "#tmp");

        using (var cmd = query.Sql.CreateSqlCommand(conn2, new()))
        using (var reader = cmd.Command.ExecuteReader()) {
            target.BulkInsert(Connection, reader, "from query");
        }

        AssertCollectionsEquivalent(expectedData, SQL($"select * from #tmp").OfPocos<SampleRow2>().Execute(Connection));
    }

    static void AssertCollectionsEquivalent<T>(T[] sampleData, T[] fromDb)
        where T : IEquatable<T>
    {
        var missingInDb = sampleData.Except(fromDb);
        var extraInDb = fromDb.Except(sampleData);
        PAssert.That(() => missingInDb.None());
        PAssert.That(() => extraInDb.None());
        PAssert.That(() => fromDb.Length == sampleData.Length);
    }

    [Fact]
    public void BulkInsertSmallBatchesRespectsKeepNul()
    {
        SQL(
            $"""
            create table #tmp (
                intNonNull int not null
                , intNull int null default 37 
                , stringNull nvarchar(max) null
                , stringNonNull nvarchar(max) not null
            )
            """
        ).ExecuteNonQuery(Connection);
        var target = BulkInsertTarget.LoadFromTable(Connection, "#tmp");
        new[] { new SampleRow2 { intNonNull = 1, intNull = null, stringNull = "test", stringNonNull = "test", }, }
            .BulkCopyToSqlServer(Connection, target);
        new[] { new SampleRow2 { intNonNull = 2, intNull = null, stringNull = "test", stringNonNull = "test", }, }
            .BulkCopyToSqlServer(Connection, target with { Options = target.Options ^ SqlBulkCopyOptions.KeepNulls, });

        var fromDb = SQL($"select * from #tmp").ReadPocos<SampleRow2>(Connection);

        var expected =
            new[] {
                new SampleRow2 { intNonNull = 1, intNull = null, stringNull = "test", stringNonNull = "test", },
                new SampleRow2 { intNonNull = 2, intNull = 37, stringNull = "test", stringNonNull = "test", },
            };

        AssertCollectionsEquivalent(expected, fromDb);
    }

    [Fact]
    public void SmallBatchInsertImplementationPrefixSanityCheck()
    {
        CheckPostConditions(Enumerable.Range(1, 4), 2);
        CheckPostConditions(Enumerable.Range(1, 4), 5);
        CheckPostConditions(Enumerable.Range(1, 4).ToArray(), 5); //there's a special case for collections such as IReadOnlyList
        CheckPostConditions(Enumerable.Range(1, 4).ToArray(), 2);

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        static void CheckPostConditions(IEnumerable<int> enumerable, int count)
        {
            var output = SmallBatchInsertImplementation.PeekAtPrefix(enumerable, count);
            PAssert.That(() => output.head.SequenceEqual(enumerable.Take(count)));
            PAssert.That(() => output.fullSequence.SequenceEqual(enumerable));
        }
    }

    [Fact]
    public void EmptyBulkInsertAndReadRoundTrips()
    {
        var target = BulkInsertTestSampleRow.CreateTable(Connection, SQL($"#test"));
        BulkInsertTestSampleRow.SampleRows(4).Take(0).BulkCopyToSqlServer(Connection, target);
        var fromDb = SQL($"select * from #test").ReadPocos<BulkInsertTestSampleRow>(Connection);
        PAssert.That(() => fromDb.None());
    }

    [Fact]
    public void CanCreateDbColumnMetaData()
    {
        var pocoProperties = PocoProperties<BulkInsertTestSampleRow>.Instance;
        var dbProps = pocoProperties.Select(property => DbColumnMetaData.Create(
                property.Name,
                property.DataType,
                property.IsKey,
                null,
                null
            )
        );
        PAssert.That(() => pocoProperties.Count == dbProps.Count());
    }

    sealed record TableWithReadOnlyColumn(int X, byte[] ReadOnly) : IReadImplicitly, IWrittenImplicitly;

    [Fact]
    public void Writing_to_read_only_column_error_can_be_disabled()
    {
        var tableName = SQL($"#TableWithReadOnlyColumn");
        SQL($"create table {tableName} (X int not null, ReadOnly rowversion not null);").ExecuteNonQuery(Connection);
        var record = new TableWithReadOnlyColumn(1, [1, 2, 3, 4, 5, 6, 7, 8,]);
        var target = BulkInsertTarget.LoadFromTable(Connection, tableName);

        // by default, writing to read-only column is not allowed
        var notAllowed = Maybe.Try(() => target.BulkInsert(Connection, new[] { record, }, cancel: TestContext.Current.CancellationToken))
            .Catch<InvalidOperationException>();

        PAssert.That(() => notAllowed.AssertError().Message.Contains("Cannot fill readonly field ReadOnly", StringComparison.InvariantCulture));

        // but we can allow it
        (target with { SilentlySkipReadonlyTargetColumns = true, }).BulkInsert(Connection, new[] { record, }, cancel: TestContext.Current.CancellationToken);

        var allowed = SQL($"select * from {tableName}").ReadPocos<TableWithReadOnlyColumn>(Connection).Single();
        PAssert.That(() => !allowed.ReadOnly.AsEnumerable().SequenceEqual(record.ReadOnly));
    }

    static ParameterizedSql CreateSampleRow2Table(SqlConnection conn)
    {
        SQL(
            $"""
            create table #tmp (
                intNonNull int not null
                , intNull int null
                , stringNull nvarchar(max) null
                , stringNonNull nvarchar(max) not null
            )
            """
        ).ExecuteNonQuery(conn);
        return SQL($"#tmp");
    }

    static ParameterizedSql SampleRow2SourceQuery
        => SQL(
            $"""
            select *
            from (
                values (1, null, 'test', 'test2')
                , (2, 1, null, 'test3')
            ) x(intNonNull, intNull, stringNull, stringNonNull)
            """
        );

    [Fact]
    public void BulkInsertSync_WithReaderOnSameConnection_ShowsBehaviour()
    {
        var tableName = CreateSampleRow2Table(Connection);
        var target = BulkInsertTarget.LoadFromTable(Connection, tableName.CommandText());

        using var cmd = SampleRow2SourceQuery.CreateSqlCommand(Connection, new());
        using var reader = cmd.Command.ExecuteReader();
        // No assertion: this test documents whatever the sync bulk-copy path does when the
        // source reader is tied to the same SqlConnection as the bulk-copy destination.
        // Per the comment on Execute, sync bulk copy is expected to throw rather than deadlock.
        var observed = Record.Exception(() => BulkInsertImplementation.Execute(Connection, reader, target, "same-conn-sync", CommandTimeout.WithoutTimeout));
        TestContext.Current.TestOutputHelper?.WriteLine($"Sync observed: {observed?.GetType().FullName}: {observed?.Message}");
    }

    [Fact]
    public async Task BulkInsertAsync_WithReaderOnSameConnection_ThrowsInvalidOperationException()
    {
        var tableName = CreateSampleRow2Table(Connection);
        var target = BulkInsertTarget.LoadFromTable(Connection, tableName.CommandText());

        using var cmd = SampleRow2SourceQuery.CreateSqlCommand(Connection, new());
        await using var reader = await cmd.Command.ExecuteReaderAsync(TestContext.Current.CancellationToken);

        // ExecuteAsync contains a pre-check (ThrowIfSourceReaderUsesTheSameConnection) that
        // detects a SqlDataReader bound to the destination SqlConnection and fails fast with
        // InvalidOperationException instead of allowing the deadlock this scenario would cause.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => BulkInsertImplementation.ExecuteAsync(Connection, reader, target, "same-conn-async", CommandTimeout.WithoutTimeout, TestContext.Current.CancellationToken));
        PAssert.That(() => ex.Message.Contains("same SqlConnection", StringComparison.Ordinal));
    }

    /// <summary>
    /// Wraps an inner DbDataReader (backed by some OTHER connection so it can yield rows) but
    /// during every Read/ReadAsync call it fires a small query on <paramref name="sharedConn"/>.
    /// When <paramref name="sharedConn"/> is the same connection SqlBulkCopy is writing to,
    /// this simulates buggy user code touching the destination connection mid-copy.
    /// </summary>
    sealed class ConnectionAbusingDbDataReader(DbDataReader inner, SqlConnection sharedConn) : DbDataReader
    {
        void AbuseConnection()
            => _ = SQL($"select 1").ReadScalar<int>(sharedConn);

        public override bool Read()
        {
            var advanced = inner.Read();
            if (advanced) {
                AbuseConnection();
            }
            return advanced;
        }

        public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            var advanced = await inner.ReadAsync(cancellationToken).ConfigureAwait(false);
            if (advanced) {
                AbuseConnection();
            }
            return advanced;
        }

        // straight delegations
        public override int Depth
            => inner.Depth;

        public override int FieldCount
            => inner.FieldCount;

        public override bool HasRows
            => inner.HasRows;

        public override bool IsClosed
            => inner.IsClosed;

        public override int RecordsAffected
            => inner.RecordsAffected;

        public override object this[int ordinal]
            => inner[ordinal];

        public override object this[string name]
            => inner[name];

        public override bool GetBoolean(int ordinal)
            => inner.GetBoolean(ordinal);

        public override byte GetByte(int ordinal)
            => inner.GetByte(ordinal);

        public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
            => inner.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);

        public override char GetChar(int ordinal)
            => inner.GetChar(ordinal);

        public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
            => inner.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);

        public override string GetDataTypeName(int ordinal)
            => inner.GetDataTypeName(ordinal);

        public override DateTime GetDateTime(int ordinal)
            => inner.GetDateTime(ordinal);

        public override decimal GetDecimal(int ordinal)
            => inner.GetDecimal(ordinal);

        public override double GetDouble(int ordinal)
            => inner.GetDouble(ordinal);

        public override Type GetFieldType(int ordinal)
            => inner.GetFieldType(ordinal);

        public override float GetFloat(int ordinal)
            => inner.GetFloat(ordinal);

        public override Guid GetGuid(int ordinal)
            => inner.GetGuid(ordinal);

        public override short GetInt16(int ordinal)
            => inner.GetInt16(ordinal);

        public override int GetInt32(int ordinal)
            => inner.GetInt32(ordinal);

        public override long GetInt64(int ordinal)
            => inner.GetInt64(ordinal);

        public override string GetName(int ordinal)
            => inner.GetName(ordinal);

        public override int GetOrdinal(string name)
            => inner.GetOrdinal(name);

        public override string GetString(int ordinal)
            => inner.GetString(ordinal);

        public override object GetValue(int ordinal)
            => inner.GetValue(ordinal);

        public override int GetValues(object[] values)
            => inner.GetValues(values);

        public override bool IsDBNull(int ordinal)
            => inner.IsDBNull(ordinal);

        public override bool NextResult()
            => inner.NextResult();

        public override IEnumerator GetEnumerator()
            => ((IEnumerable)inner).GetEnumerator();
    }

    [Fact]
    public void BulkInsertSync_ReaderTouchesDestinationConnectionMidCopy_ShowsBehaviour()
    {
        using var sourceConn = new SqlConnection(ConnectionString);
        sourceConn.Open();

        var tableName = CreateSampleRow2Table(Connection);
        var target = BulkInsertTarget.LoadFromTable(Connection, tableName.CommandText());

        using var cmd = SampleRow2SourceQuery.CreateSqlCommand(sourceConn, new());
        using var innerReader = cmd.Command.ExecuteReader();
        using var evilReader = new ConnectionAbusingDbDataReader(innerReader, Connection);

        var observed = Record.Exception(() => BulkInsertImplementation.Execute(Connection, evilReader, target, "mid-copy-sync", CommandTimeout.WithoutTimeout));
        TestContext.Current.TestOutputHelper?.WriteLine($"Sync observed: {observed?.GetType().FullName}: {observed?.Message}");
        var guardEx = FindBulkCopyInProgressGuardException(observed);
        PAssert.That(() => guardEx != null, "Expected guard #2 InvalidOperationException to surface (possibly wrapped) when a query runs on the destination connection mid-copy.");
    }

    [Fact]
    public async Task BulkInsertAsync_ReaderTouchesDestinationConnectionMidCopy_ShowsBehaviour()
    {
        // Dedicated destConn (not the shared TransactedLocalConnection.Connection) so that a
        // stuck bulk-copy cannot hang test-fixture teardown. NOTE: destConn is intentionally NOT
        // wrapped in `await using` — on the deadlock path its DisposeAsync would itself block
        // forever trying to acquire the same internal semaphore the stuck bulk task holds
        // (which is precisely why the "deadlock detected" message never showed up before).
        // We dispose it manually only on the non-deadlock path; on deadlock we leak it (the
        // whole test host will be torn down anyway).
        var destConn = new SqlConnection(ConnectionString);
        await destConn.OpenAsync(TestContext.Current.CancellationToken);
        await using var sourceConn = new SqlConnection(ConnectionString);
        await sourceConn.OpenAsync(TestContext.Current.CancellationToken);

        var tableName = CreateSampleRow2Table(destConn);
        var target = BulkInsertTarget.LoadFromTable(destConn, tableName.CommandText());

        using var cmd = SampleRow2SourceQuery.CreateSqlCommand(sourceConn, new());
        await using var innerReader = await cmd.Command.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        var evilReader = new ConnectionAbusingDbDataReader(innerReader, destConn);

        var bulkTask = Task.Run(
            () => BulkInsertImplementation.ExecuteAsync(destConn, evilReader, target, "mid-copy-async", CommandTimeout.WithoutTimeout, TestContext.Current.CancellationToken),
            TestContext.Current.CancellationToken
        );
        var completed = await Task.WhenAny(bulkTask, Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
        if (completed != bulkTask) {
            _ = bulkTask.ContinueWith(t => _ = t.Exception, TaskScheduler.Default); // observe eventual exception
            // Deliberately do NOT dispose destConn here: it would block on the same semaphore.
            Assert.Fail("ExecuteAsync deadlocked when the source DbDataReader used the destination SqlConnection mid-copy.");
            return;
        }

        var observed = await Record.ExceptionAsync(() => bulkTask);
        TestContext.Current.TestOutputHelper?.WriteLine($"Async observed: {observed?.GetType().FullName}: {observed?.Message}");
        var guardEx = FindBulkCopyInProgressGuardException(observed);
        PAssert.That(() => guardEx != null, "Expected guard #2 InvalidOperationException to surface (possibly wrapped) when a query runs on the destination connection mid-copy.");
        await destConn.DisposeAsync();
    }

    /// <summary>
    /// Walks the exception chain (including <see cref="AggregateException.InnerExceptions"/>) looking for the
    /// <see cref="InvalidOperationException"/> raised by <c>BulkInsertImplementation.ThrowIfConnectionInBulkCopy</c>.
    /// SqlBulkCopy may wrap the source-reader exception; asserting on the wrapped exception's presence keeps the
    /// test resilient to wrapping while still verifying that guard #2 fired.
    /// </summary>
    static InvalidOperationException? FindBulkCopyInProgressGuardException(Exception? ex)
    {
        while (ex != null) {
            if (ex is InvalidOperationException ioe && ioe.Message.Contains("bulk copy is currently in progress", StringComparison.Ordinal)) {
                return ioe;
            }
            if (ex is AggregateException aggregate) {
                foreach (var inner in aggregate.InnerExceptions) {
                    if (FindBulkCopyInProgressGuardException(inner) is { } found) {
                        return found;
                    }
                }
                return null;
            }
            ex = ex.InnerException;
        }
        return null;
    }

    [Fact]
    public async Task BulkInsertAsync_CanBeCancelledMidCopy()
    {
        // Dedicated connection: cancellation of an in-flight WriteToServerAsync leaves the
        // SqlConnection in a broken state, so we don't want to disturb the shared fixture.
        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync(TestContext.Current.CancellationToken);

        var target = BulkInsertTestSampleRow.CreateTable(conn, SQL($"#test"));
        // Enough rows so cancellation reliably fires before the bulk copy completes.
        var manyRows = BulkInsertTestSampleRow.SampleRows(200_000);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));

        var ex = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => target.BulkInsertAsync(conn, manyRows, cancel: cts.Token));
        PAssert.That(() => ex.CancellationToken == cts.Token);
    }
}
