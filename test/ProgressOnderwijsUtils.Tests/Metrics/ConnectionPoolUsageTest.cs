using ProgressOnderwijsUtils.Metrics;

namespace ProgressOnderwijsUtils.Tests.Metrics;

public sealed class ConnectionPoolUsageTest
{
    sealed class MutableClock : TimeProvider
    {
        DateTimeOffset now = DateTimeOffset.UnixEpoch.AddDays(20_000);

        public override DateTimeOffset GetUtcNow()
            => now;

        public long UnixSeconds
            => now.ToUnixTimeSeconds();

        public void Advance(int seconds)
            => now = now.AddSeconds(seconds);
    }

    readonly ConnectionPoolRegistry<TestDatabase, TestPurpose> registry = TestMetrics.Registry();

    /// <summary>
    /// Nothing is opened; the test operates the counters along the same way as <see cref="ConnectionPoolCounters{TDatabase,TPurpose}.Open"/>.
    /// </summary>
    ConnectionPoolCounters<TestDatabase, TestPurpose> Pool(string catalog, TestPurpose purpose)
        => registry.For($"Data Source=nowhere;Initial Catalog={catalog};Application Name={purpose}", purpose);

    static void Open(ConnectionPoolCounters<TestDatabase, TestPurpose> pool)
    {
        pool.OpenStarted();
        pool.StateChangeHandler(pool, new(ConnectionState.Closed, ConnectionState.Open));
        pool.OpenFinished(true);
    }

    static void Close(ConnectionPoolCounters<TestDatabase, TestPurpose> pool)
        => pool.StateChangeHandler(pool, new(ConnectionState.Open, ConnectionState.Closed));

    ConnectionPoolUsage<TestDatabase, TestPurpose> Usage(MutableClock clock, ConnectionPoolSlots<TestDatabase, TestPurpose> slots)
        => new(new(SecondsClock.Following(clock), 64), slots, registry);

    /// <summary>
    /// Every pool fills up on its own, so the pools of a database stay apart per purpose: a waiting caller can be
    /// attributed to the pool that is full this way. Only pools with the same database and the same purpose coincide,
    /// like the databases that are not tracked on the other slot. Of the gauges the highest sampled level counts, of
    /// the opens the increase since the level from before the window.
    /// </summary>
    [Fact]
    public void The_pools_of_a_database_are_kept_apart_per_purpose()
    {
        var clock = new MutableClock();
        var slots = new ConnectionPoolSlots<TestDatabase, TestPurpose>(new TestDatabaseDuration());
        var usage = Usage(clock, slots);
        var alphaReadWrite = Pool("Alpha", TestPurpose.ReadWrite);
        var alphaDebugLog = Pool("Alpha", TestPurpose.DebugLog);
        var master = Pool("master", TestPurpose.Sysadmin);
        var blob = Pool("Blob", TestPurpose.Sysadmin);

        // a starting level from just before the window, so that all opens below fall within the window
        usage.Measurement();
        clock.Advance(1);
        var firstSecond = clock.UnixSeconds;

        Open(alphaReadWrite);
        Open(alphaReadWrite);
        Open(alphaDebugLog);
        Open(master);
        Open(blob);
        usage.Measurement();
        alphaReadWrite.OpenStarted(); // someone is waiting for a connection from the readwrite pool
        usage.Measurement();
        alphaReadWrite.OpenFinished(false);

        clock.Advance(1);
        Close(alphaReadWrite);
        usage.Measurement();
        var lastSecond = clock.UnixSeconds;

        var window = usage.Collect(firstSecond, lastSecond).AssertNotNull();
        var readWrite = slots.SlotOf(TestDatabase.Alpha, TestPurpose.ReadWrite);
        var debugLog = slots.SlotOf(TestDatabase.Alpha, TestPurpose.DebugLog);
        var otherSysadmin = slots.SlotOf(null, TestPurpose.Sysadmin);
        var betaReadWrite = slots.SlotOf(TestDatabase.Beta, TestPurpose.ReadWrite);

        PAssert.That(() => window.MaxUsedConnections[readWrite] == 2 && window.MaxPendingConnections[readWrite] == 1 && window.OpenedConnections[readWrite] == 2);
        PAssert.That(() => window.MaxUsedConnections[debugLog] == 1 && window.MaxPendingConnections[debugLog] == 0 && window.OpenedConnections[debugLog] == 1);
        PAssert.That(() => window.MaxUsedConnections[otherSysadmin] == 2 && window.OpenedConnections[otherSysadmin] == 2);
        PAssert.That(() => window.MaxUsedConnections[betaReadWrite] == 0 && window.OpenedConnections[betaReadWrite] == 0);
    }

    /// <summary>
    /// A zero is a real level here (a database without traffic), so whether a second was sampled follows from the
    /// stamp of the sample and not from the levels. A sampled second with only zeros is a measurement, and a second
    /// without a sample yields nothing.
    /// </summary>
    [Fact]
    public void A_sampled_second_without_traffic_is_still_a_measurement()
    {
        var clock = new MutableClock();
        var usage = Usage(clock, new(new TestDatabaseDuration()));
        _ = Pool("Gamma", TestPurpose.ReadOnly);

        usage.Measurement();
        var sampled = clock.UnixSeconds;
        clock.Advance(1);
        var unsampled = clock.UnixSeconds;

        var window = usage.Collect(sampled, sampled).AssertNotNull();
        PAssert.That(() => window.MaxUsedConnections.All(used => used == 0) && window.OpenedConnections.All(opened => opened == 0));
        PAssert.That(() => usage.Collect(unsampled, unsampled) == null);
    }

    /// <summary>
    /// Sampling, collecting and storing all calculate through the same layout; every combination of database and
    /// purpose therefore has to have exactly one slot of its own, and that slot has to point back to the same
    /// database and the same purpose.
    /// </summary>
    [Fact]
    public void Every_database_and_purpose_has_its_own_slot_that_points_back_to_it()
    {
        var databases = new TestDatabaseDuration();
        var slots = new ConnectionPoolSlots<TestDatabase, TestPurpose>(databases);

        var combinations = (
            from databaseSlot in Enumerable.Range(0, databases.Values.Length)
            from purpose in ConnectionPoolSlots<TestDatabase, TestPurpose>.Purposes
            select (databaseSlot, purpose, slot: slots.SlotOf(databaseSlot, purpose))
        ).ToArray();

        var everySlotPointsBack = combinations.All(o => slots.DatabaseSlotOf(o.slot) == o.databaseSlot && slots.PurposeOf(o.slot) == o.purpose);
        var distinctSlots = combinations.Select(o => o.slot).Distinct().Count();
        PAssert.That(() => everySlotPointsBack);
        PAssert.That(() => distinctSlots == slots.Count && combinations.All(o => o.slot >= 0 && o.slot < slots.Count));
    }
}
