using ProgressOnderwijsUtils.Metrics;

namespace ProgressOnderwijsUtils.Tests.Metrics;

public sealed class ConnectionPoolCountersTest
{
    static readonly string localConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? @"Server = (localdb)\MSSQLLocalDB; Integrated Security = true";

    readonly ConnectionPoolRegistry<TestDatabase, TestPurpose> registry = TestMetrics.Registry();

    /// <summary>
    /// A pool of a single connection, with an application name of its own so that it is shared with nothing else:
    /// SqlClient keeps a pool per connection string.
    /// </summary>
    static string PoolOfOne()
        => new SqlConnectionStringBuilder(localConnectionString) {
            MaxPoolSize = 1,
            ApplicationName = $"{nameof(ConnectionPoolCountersTest)}-{Guid.NewGuid():N}",
            ConnectTimeout = 15,
        }.ConnectionString;

    static SqlConnection Open(ConnectionPoolCounters<TestDatabase, TestPurpose> pool, string connectionString)
    {
        var connection = new SqlConnection(connectionString);
        try {
            pool.Open(connection);
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
        var connectionString = PoolOfOne();
        var pool = registry.For(connectionString, TestPurpose.ReadWrite);

        var first = Open(pool, connectionString);
        PAssert.That(() => pool.InUse == 1 && pool.Pending == 0 && pool.Opened == 1);

        var second = Task.Run(() => Open(pool, connectionString));
        await WaitUntil(() => pool.Pending == 1);
        PAssert.That(() => pool.Pending == 1 && pool.InUse == 1);

        first.Dispose();
        await using (await second) {
            PAssert.That(() => pool.InUse == 1 && pool.Pending == 0 && pool.Opened == 2);
        }

        PAssert.That(() => pool.InUse == 0);
    }

    /// <summary>
    /// Counting is a side concern: a connection of the same pool that is opened without the counters just works.
    /// </summary>
    [Fact]
    public void A_connection_that_is_opened_without_the_counters_is_not_counted()
    {
        var connectionString = PoolOfOne();
        var pool = registry.For(connectionString, TestPurpose.ReadWrite);

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        PAssert.That(() => connection.State == ConnectionState.Open);
        PAssert.That(() => pool.InUse == 0 && pool.Pending == 0 && pool.Opened == 0);
    }

    [Fact]
    public void An_open_that_fails_is_no_longer_pending_and_not_counted_as_opened()
    {
        var connectionString = new SqlConnectionStringBuilder(localConnectionString) { InitialCatalog = $"NoSuchDatabase{Guid.NewGuid():N}", ConnectTimeout = 15, Pooling = false, }.ConnectionString;
        var pool = registry.For(connectionString, TestPurpose.ReadOnly);

        _ = Assert.Throws<SqlException>(() => Open(pool, connectionString));

        PAssert.That(() => pool.InUse == 0 && pool.Pending == 0 && pool.Opened == 0);
    }

    [Fact]
    public void The_same_connection_string_shares_its_counters()
    {
        var pool = UnopenedPool("Alpha", TestPurpose.ReadWrite);
        var all = registry.All.ToArray();

        PAssert.That(() => all.Single() == pool);
    }

    /// <summary>
    /// The counters of a pool that is never opened: only the connection string counts.
    /// </summary>
    ConnectionPoolCounters<TestDatabase, TestPurpose> UnopenedPool(string catalog, TestPurpose purpose, int? maxPoolSize = null)
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

        var pool = registry.For(builder.ConnectionString, purpose);
        PAssert.That(() => ReferenceEquals(pool, registry.For(builder.ConnectionString, purpose)));
        return pool;
    }

    [Fact]
    public void A_pool_takes_its_size_and_database_from_the_connection_string_and_keeps_its_purpose()
    {
        var pool = UnopenedPool("Beta", TestPurpose.ReadWrite, 30);

        PAssert.That(() => pool.MaxPoolSize == 30);
        PAssert.That(() => pool.Purpose == TestPurpose.ReadWrite);
        PAssert.That(() => pool.Catalog == "Beta" && pool.Database == TestDatabase.Beta);
    }

    [Fact]
    public void A_pool_without_a_size_gets_the_default_of_a_hundred()
    {
        var pool = UnopenedPool("Alpha", TestPurpose.ReadWrite);

        PAssert.That(() => pool.MaxPoolSize == 100);
    }

    /// <summary>
    /// A connection string belongs to exactly one purpose; another purpose means one of the places where it is built
    /// is wrong, and its usage would then count for the wrong purpose.
    /// </summary>
    [Fact]
    public void The_same_connection_string_with_another_purpose_is_rejected()
    {
        const string connectionString = "Data Source=localhost;Initial Catalog=Alpha;Integrated Security=True";
        _ = registry.For(connectionString, TestPurpose.ReadOnly);

        _ = Assert.Throws<InvalidOperationException>(() => registry.For(connectionString, TestPurpose.ReadWrite));
    }

    /// <summary>
    /// Summed together a full readwrite pool would be cancelled out by a practically empty debuglog pool.
    /// </summary>
    [Fact]
    public void Pools_of_different_purposes_are_never_added_up()
    {
        var sizes = ConnectionPoolRegistry<TestDatabase, TestPurpose>.PoolSizes([UnopenedPool("Alpha", TestPurpose.ReadWrite, 100), UnopenedPool("Alpha", TestPurpose.DebugLog, 10),]);

        PAssert.That(
            () => Enumerable.SequenceEqual(
                sizes,
                new ConnectionPoolSize<TestDatabase, TestPurpose>[] { new(TestDatabase.Alpha, TestPurpose.ReadWrite, 100), new(TestDatabase.Alpha, TestPurpose.DebugLog, 10), }
            )
        );
    }

    /// <summary>
    /// Pools with the same database and the same purpose are both real room; the databases that are not tracked
    /// coincide under an empty database, just as with the usage.
    /// </summary>
    [Fact]
    public void Pools_of_the_same_database_and_purpose_are_added_up()
    {
        var sizes = ConnectionPoolRegistry<TestDatabase, TestPurpose>.PoolSizes(
            [
                UnopenedPool("Alpha", TestPurpose.Sysadmin, 100),
                UnopenedPool("Alpha", TestPurpose.Sysadmin, 50),
                UnopenedPool("master", TestPurpose.Sysadmin, 20),
                UnopenedPool("Blob", TestPurpose.Sysadmin, 30),
            ]
        );

        PAssert.That(
            () => Enumerable.SequenceEqual(
                sizes,
                new ConnectionPoolSize<TestDatabase, TestPurpose>[] { new(null, TestPurpose.Sysadmin, 50), new(TestDatabase.Alpha, TestPurpose.Sysadmin, 150), }
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

        _ = UnopenedPool("Gamma", TestPurpose.Sysadmin, 10);
        var after = registry.Sizes;

        PAssert.That(() => ReferenceEquals(before, reread));
        PAssert.That(() => !ReferenceEquals(before, after));
        PAssert.That(() => after.Single() == new ConnectionPoolSize<TestDatabase, TestPurpose>(TestDatabase.Gamma, TestPurpose.Sysadmin, 10));
    }
}
