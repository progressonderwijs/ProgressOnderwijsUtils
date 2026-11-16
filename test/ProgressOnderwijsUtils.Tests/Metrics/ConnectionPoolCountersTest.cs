using ProgressOnderwijsUtils.Metrics;
using ProgressOnderwijsUtils.Tests.Data;

namespace ProgressOnderwijsUtils.Tests.Metrics;

/// <summary>
/// Builds its own connection strings on top of the local server of <see cref="TransactedLocalConnection"/>: these tests
/// are about opening, closing and the pool itself, which that class hides behind one open connection in a transaction.
/// </summary>
[Collection(nameof(MetricsCollectorCollection))]
public sealed class ConnectionPoolCountersTest
{
    readonly ConnectionPoolRegistry<TestDatabase, TestPool> registry = TestMetrics.Registry();

    /// <summary>
    /// The pools are only counted while a collector runs.
    /// </summary>
    MetricsCollector<TestEndpoint, TestDatabase, TestPool> StartedCollector()
    {
        var collector = new MetricsCollector<TestEndpoint, TestDatabase, TestPool>(TestMetrics.Description(), registry);
        collector.Start(TestMetrics.HostWithoutRequests, _ => { });
        return collector;
    }

    /// <summary>
    /// A pool of a single connection, with an application name of its own so that it is shared with nothing else:
    /// SqlClient keeps a pool per connection string.
    /// </summary>
    static string PoolOfOne()
        => new SqlConnectionStringBuilder(TransactedLocalConnection.ConnectionString) {
            MaxPoolSize = 1,
            ApplicationName = $"{nameof(ConnectionPoolCountersTest)}-{Guid.NewGuid():N}",
            ConnectTimeout = 15,
        }.ConnectionString;

    static SqlConnection Open(string connectionString)
    {
        var connection = new SqlConnection(connectionString);
        try {
            connection.Open();
            return connection;
        } catch {
            connection.Dispose();
            throw;
        }
    }

    static async Task WaitUntil(Func<bool> condition)
    {
        for (var attempt = 0; attempt < 500 && !condition(); attempt++) {
            await Task.Delay(10);
        }
    }

    /// <summary>
    /// When the pool is full, a next open waits until a connection comes back; as long as that takes it counts as
    /// pending. A connection counts as in use from open to closed, and every succeeded open counts once.
    /// </summary>
    [Fact]
    public async Task A_full_pool_makes_the_next_open_wait()
    {
        using var collector = StartedCollector();
        var connectionString = PoolOfOne();
        var pool = registry.For(connectionString, TestPool.ReadWrite);

        var first = Open(connectionString);
        PAssert.That(() => pool.InUse == 1 && pool.Pending == 0 && pool.Opened == 1);

        var second = Task.Run(() => Open(connectionString));
        await WaitUntil(() => pool.Pending == 1);
        PAssert.That(() => pool.Pending == 1 && pool.InUse == 1);

        first.Dispose();
        await using (await second) {
            PAssert.That(() => pool.InUse == 1 && pool.Pending == 0 && pool.Opened == 2);
        }

        PAssert.That(() => pool.InUse == 0);
    }

    /// <summary>
    /// Only registered pools are counted: the registry knows the pools and the opens of the rest of the process
    /// should not register pools of their own.
    /// </summary>
    [Fact]
    public void A_connection_string_that_was_never_registered_is_not_counted()
    {
        using var collector = StartedCollector();
        using var connection = Open(PoolOfOne());
        var registeredPools = registry.All.Length;

        PAssert.That(() => connection.State == ConnectionState.Open);
        PAssert.That(() => registeredPools == 0);
    }

    /// <summary>
    /// The pool follows the connection from the start of each open until it is closed, so a reopened connection is
    /// followed once and not twice.
    /// </summary>
    [Fact]
    public void A_reopened_connection_is_in_use_once()
    {
        using var collector = StartedCollector();
        var connectionString = PoolOfOne();
        var pool = registry.For(connectionString, TestPool.ReadWrite);

        using var connection = Open(connectionString);
        connection.Close();
        connection.Open();

        PAssert.That(() => pool.InUse == 1 && pool.Pending == 0 && pool.Opened == 2);
        connection.Close();
        PAssert.That(() => pool.InUse == 0);
    }

    [Fact]
    public void A_connection_that_is_opened_without_a_running_collector_is_opened_but_not_counted()
    {
        var connectionString = PoolOfOne();
        var pool = registry.For(connectionString, TestPool.ReadWrite);
        StartedCollector().Dispose();

        using var connection = Open(connectionString);

        PAssert.That(() => connection.State == ConnectionState.Open);
        PAssert.That(() => pool.InUse == 0 && pool.Pending == 0 && pool.Opened == 0);
    }

    /// <summary>
    /// SqlClient has several listeners with the same name; a disposed collector must have let go of all of them, or
    /// the next collector of the same registry counts every open twice.
    /// </summary>
    [Fact]
    public void A_disposed_collector_no_longer_counts_next_to_its_successor()
    {
        var connectionString = PoolOfOne();
        var pool = registry.For(connectionString, TestPool.ReadWrite);
        StartedCollector().Dispose();
        using var collector = StartedCollector();

        using var connection = Open(connectionString);

        PAssert.That(() => pool.InUse == 1 && pool.Pending == 0 && pool.Opened == 1);
    }

    /// <summary>
    /// A connection that was counted is followed until it is closed, also when the collector stopped in the meantime;
    /// otherwise it would stay in use forever.
    /// </summary>
    [Fact]
    public void A_counted_connection_that_is_closed_after_the_collector_stopped_is_no_longer_in_use()
    {
        var connectionString = PoolOfOne();
        var pool = registry.For(connectionString, TestPool.ReadWrite);
        var collector = StartedCollector();
        var connection = Open(connectionString);
        collector.Dispose();

        connection.Dispose();

        PAssert.That(() => pool.InUse == 0 && pool.Opened == 1);
    }

    [Fact]
    public void An_open_that_fails_is_no_longer_pending_and_not_counted_as_opened()
    {
        using var collector = StartedCollector();
        var connectionString = new SqlConnectionStringBuilder(TransactedLocalConnection.ConnectionString) {
            InitialCatalog = $"NoSuchDatabase{Guid.NewGuid():N}",
            ConnectTimeout = 15,
            // SqlClient counts "cannot open database" as transient and would otherwise wait 10 seconds and try again.
            ConnectRetryCount = 0,
            Pooling = false,
        }.ConnectionString;
        var pool = registry.For(connectionString, TestPool.ReadOnly);

        _ = Assert.Throws<SqlException>(() => Open(connectionString));

        PAssert.That(() => pool.InUse == 0 && pool.Pending == 0 && pool.Opened == 0);
    }

    [Fact]
    public void The_same_connection_string_shares_its_counters()
    {
        var pool = UnopenedPool("Alpha", TestPool.ReadWrite);
        var all = registry.All.ToArray();

        PAssert.That(() => all.Single() == pool);
    }

    /// <summary>
    /// The counters of a pool that is never opened: only the connection string counts.
    /// </summary>
    ConnectionPoolCounters<TestDatabase, TestPool> UnopenedPool(string catalog, TestPool pool, int? maxPoolSize = null)
    {
        var builder = new SqlConnectionStringBuilder {
            DataSource = "localhost",
            InitialCatalog = catalog,
            IntegratedSecurity = true,
            ApplicationName = $"{nameof(ConnectionPoolCountersTest)}-{Guid.NewGuid():N}",
        };
        if (maxPoolSize is { } size) {
            builder.MaxPoolSize = size;
        }

        var counter = registry.For(builder.ConnectionString, pool);
        PAssert.That(() => ReferenceEquals(counter, registry.For(builder.ConnectionString, pool)));
        return counter;
    }

    [Fact]
    public void A_pool_takes_its_size_and_database_from_the_connection_string_and_keeps_its_id()
    {
        var pool = UnopenedPool("Beta", TestPool.ReadWrite, 30);

        PAssert.That(() => pool.MaxPoolSize == 30);
        PAssert.That(() => pool.Pool == TestPool.ReadWrite);
        PAssert.That(() => pool.Catalog == "Beta" && pool.Database == TestDatabase.Beta);
    }

    [Fact]
    public void A_pool_without_a_size_gets_the_default_of_a_hundred()
    {
        var pool = UnopenedPool("Alpha", TestPool.ReadWrite);

        PAssert.That(() => pool.MaxPoolSize == 100);
    }

    /// <summary>
    /// A connection string belongs to exactly one pool; another pool means one of the places where it is built
    /// is wrong, and its usage would then count for the wrong pool.
    /// </summary>
    [Fact]
    public void The_same_connection_string_with_another_pool_is_rejected()
    {
        const string connectionString = "Data Source=localhost;Initial Catalog=Alpha;Integrated Security=True";
        _ = registry.For(connectionString, TestPool.ReadOnly);

        _ = Assert.Throws<InvalidOperationException>(() => registry.For(connectionString, TestPool.ReadWrite));
    }

    /// <summary>
    /// Summed together a full readwrite pool would be cancelled out by a practically empty debuglog pool.
    /// </summary>
    [Fact]
    public void Different_pools_are_never_added_up()
    {
        var sizes = ConnectionPoolRegistry<TestDatabase, TestPool>.PoolSizes([UnopenedPool("Alpha", TestPool.ReadWrite, 100), UnopenedPool("Alpha", TestPool.DebugLog, 10),]);

        PAssert.That(
            () => Enumerable.SequenceEqual(
                sizes,
                new ConnectionPoolSize<TestDatabase, TestPool>[] { new(TestDatabase.Alpha, TestPool.ReadWrite, 100), new(TestDatabase.Alpha, TestPool.DebugLog, 10), }
            )
        );
    }

    /// <summary>
    /// Pools within the same database and are both real room; the databases that are not tracked
    /// coincide under an empty database, just as with the usage.
    /// </summary>
    [Fact]
    public void Pools_of_the_same_database_are_added_up()
    {
        var sizes = ConnectionPoolRegistry<TestDatabase, TestPool>.PoolSizes(
            [
                UnopenedPool("Alpha", TestPool.Sysadmin, 100),
                UnopenedPool("Alpha", TestPool.Sysadmin, 50),
                UnopenedPool("master", TestPool.Sysadmin, 20),
                UnopenedPool("Blob", TestPool.Sysadmin, 30),
            ]
        );

        PAssert.That(
            () => Enumerable.SequenceEqual(
                sizes,
                new ConnectionPoolSize<TestDatabase, TestPool>[] { new(null, TestPool.Sysadmin, 50), new(TestDatabase.Alpha, TestPool.Sysadmin, 150), }
            )
        );
    }

    /// <summary>
    /// Whoever stores the sizes skips its work as long as they are the same array; so it may only be replaced when a
    /// pool is added, and then it really has to be replaced.
    /// </summary>
    [Fact]
    public void The_sizes_are_computed_once_per_new_pool()
    {
        var before = registry.Sizes;
        var reread = registry.Sizes;

        _ = UnopenedPool("Gamma", TestPool.Sysadmin, 10);
        var after = registry.Sizes;

        PAssert.That(() => ReferenceEquals(before, reread));
        PAssert.That(() => !ReferenceEquals(before, after));
        PAssert.That(() => after.Single() == new ConnectionPoolSize<TestDatabase, TestPool>(TestDatabase.Gamma, TestPool.Sysadmin, 10));
    }
}
