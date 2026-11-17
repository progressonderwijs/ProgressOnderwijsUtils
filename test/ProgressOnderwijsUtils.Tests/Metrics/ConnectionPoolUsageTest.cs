using ProgressOnderwijsUtils.Metrics;

namespace ProgressOnderwijsUtils.Tests.Metrics;

public sealed class ConnectionPoolUsageTest
{
    /// <summary>
    /// A clock whose timestamp starts at 0 and only moves when the test advances it, so that the second of a ring
    /// created before the first advance is <see cref="Second"/>.
    /// </summary>
    sealed class MutableClock : TimeProvider
    {
        static readonly DateTimeOffset start = DateTimeOffset.UnixEpoch.AddDays(20_000);
        DateTimeOffset now = start;

        public override DateTimeOffset GetUtcNow()
            => now;

        public override long GetTimestamp()
            => (now - start).Ticks;

        public override long TimestampFrequency
            => TimeSpan.TicksPerSecond;

        public int Second
            => checked((int)((now - start).Ticks / TimeSpan.TicksPerSecond));

        public void Advance(int seconds)
            => now = now.AddSeconds(seconds);
    }

    readonly ConnectionPoolRegistry<TestDatabase, TestPool> registry = TestMetrics.Registry();

    /// <summary>
    /// Nothing is opened; the test operates the counters along the same way as the open events of SqlClient do.
    /// </summary>
    ConnectionPoolCounters<TestDatabase, TestPool> Pool(string catalog, TestPool pool)
        => registry.For($"Data Source=nowhere;Initial Catalog={catalog};Application Name={pool}", pool);

    static readonly SqlConnection unopened = new();

    static void Open(ConnectionPoolCounters<TestDatabase, TestPool> pool)
    {
        using var connection = new SqlConnection();
        pool.OpenStarted(connection);
        pool.StateChangeHandler(connection, new(ConnectionState.Closed, ConnectionState.Open));
    }

    static void Close(ConnectionPoolCounters<TestDatabase, TestPool> pool)
        => pool.StateChangeHandler(unopened, new(ConnectionState.Open, ConnectionState.Closed));

    ConnectionPoolUsage<TestDatabase, TestPool> Usage(MutableClock clock, ConnectionPoolSlots<TestDatabase, TestPool> slots)
        => new(new SecondsSinceStart(clock), 64, slots, registry);

    /// <summary>
    /// Every pool fills up on its own, so the pools of a database stay apart: a waiting caller can be
    /// attributed to the pool that is full this way. Of the gauges the highest sampled level counts, of
    /// the opens the increase since the level from before the window.
    /// </summary>
    [Fact]
    public void The_pools_of_a_database_are_kept_apart()
    {
        var clock = new MutableClock();
        var slots = new ConnectionPoolSlots<TestDatabase, TestPool>(new TestDatabaseDuration());
        var usage = Usage(clock, slots);
        var alphaReadWrite = Pool("Alpha", TestPool.ReadWrite);
        var alphaDebugLog = Pool("Alpha", TestPool.DebugLog);
        var master = Pool("master", TestPool.Sysadmin);
        var blob = Pool("Blob", TestPool.Sysadmin);

        // a starting level from just before the window, so that all opens below fall within the window
        usage.Measurement();
        clock.Advance(1);
        var firstSecond = clock.Second;

        Open(alphaReadWrite);
        Open(alphaReadWrite);
        Open(alphaDebugLog);
        Open(master);
        Open(blob);
        usage.Measurement();
        using (var waiting = new SqlConnection()) {
            alphaReadWrite.OpenStarted(waiting); // someone is waiting for a connection from the readwrite pool
            usage.Measurement();
            alphaReadWrite.OpenFailed();
        }

        clock.Advance(1);
        Close(alphaReadWrite);
        usage.Measurement();
        var lastSecond = clock.Second;

        var window = usage.Collect(firstSecond, lastSecond).AssertNotNull();
        var readWrite = slots.SlotOf(TestDatabase.Alpha, TestPool.ReadWrite);
        var debugLog = slots.SlotOf(TestDatabase.Alpha, TestPool.DebugLog);
        var otherSysadmin = slots.SlotOf(null, TestPool.Sysadmin);
        var betaReadWrite = slots.SlotOf(TestDatabase.Beta, TestPool.ReadWrite);

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
        _ = Pool("Gamma", TestPool.ReadOnly);

        usage.Measurement();
        var sampled = clock.Second;
        clock.Advance(1);
        var unsampled = clock.Second;

        var window = usage.Collect(sampled, sampled).AssertNotNull();
        PAssert.That(() => window.MaxUsedConnections.All(used => used == 0) && window.OpenedConnections.All(opened => opened == 0));
        PAssert.That(() => usage.Collect(unsampled, unsampled) == null);
    }

    /// <summary>
    /// Sampling, collecting and storing all calculate through the same layout; every combination of database and
    /// pool therefore has to have exactly one slot of its own, and that slot has to point back to the same
    /// database and the same pool.
    /// </summary>
    [Fact]
    public void Every_database_and_pool_has_its_own_slot_that_points_back_to_it()
    {
        var databases = new TestDatabaseDuration();
        var slots = new ConnectionPoolSlots<TestDatabase, TestPool>(databases);

        var combinations = (
            from databaseSlot in Enumerable.Range(0, databases.Values.Length)
            from pool in ConnectionPoolSlots<TestDatabase, TestPool>.Pools
            select (databaseSlot, pool, slot: slots.SlotOf(databaseSlot, pool))
        ).ToArray();

        var everySlotPointsBack = combinations.All(o => slots.DatabaseSlotOf(o.slot) == o.databaseSlot && slots.PoolOf(o.slot) == o.pool);
        var distinctSlots = combinations.Select(o => o.slot).Distinct().Count();
        PAssert.That(() => everySlotPointsBack);
        PAssert.That(() => distinctSlots == slots.Count && combinations.All(o => o.slot >= 0 && o.slot < slots.Count));
    }
}
