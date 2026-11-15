using System.Collections.Concurrent;

namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// The connection pools of a process, one <see cref="ConnectionPoolCounters{TDatabase,TPoolPurpose}"/> per connection
/// string.
/// </summary>
/// <param name="classifyCatalog">
/// The value a database name is tracked as, or null for databases that are not tracked separately. Called once per
/// pool, when it is first requested.
/// </param>
public sealed class ConnectionPoolRegistry<TDatabase, TPoolPurpose>(Func<string, TDatabase?> classifyCatalog)
    where TDatabase : struct, Enum
    where TPoolPurpose : struct, Enum
{
    readonly ConcurrentDictionary<string, ConnectionPoolCounters<TDatabase, TPoolPurpose>> byConnectionString = new(StringComparer.Ordinal);
    readonly Lock registration = new();
    ConnectionPoolCounters<TDatabase, TPoolPurpose>[] all = [];
    ConnectionPoolSize<TDatabase, TPoolPurpose>[] sizes = [];

    /// <summary>
    /// The counters of the pool of this connection string; every connection string gets exactly one. A connection
    /// string belongs to exactly one purpose: when it is requested with another purpose, one of the places where it
    /// is built is wrong.
    /// </summary>
    public ConnectionPoolCounters<TDatabase, TPoolPurpose> For(string connectionString, TPoolPurpose purpose)
        => byConnectionString.TryGetValue(connectionString, out var counters) ? EnsurePurpose(counters, purpose) : Register(connectionString, purpose);

    static ConnectionPoolCounters<TDatabase, TPoolPurpose> EnsurePurpose(ConnectionPoolCounters<TDatabase, TPoolPurpose> counters, TPoolPurpose purpose)
        => EqualityComparer<TPoolPurpose>.Default.Equals(counters.Purpose, purpose)
            ? counters
            : throw new InvalidOperationException($"The pool of database {counters.Catalog} was registered as {counters.Purpose}, but is now requested as {purpose}.");

    ConnectionPoolCounters<TDatabase, TPoolPurpose> Register(string connectionString, TPoolPurpose purpose)
    {
        lock (registration) {
            if (byConnectionString.TryGetValue(connectionString, out var existing)) {
                return EnsurePurpose(existing, purpose);
            }

            var counters = new ConnectionPoolCounters<TDatabase, TPoolPurpose>(connectionString, purpose, classifyCatalog);
            // Copy on write: the sampler runs over all pools without a lock and without an enumerator this way.
            ConnectionPoolCounters<TDatabase, TPoolPurpose>[] registered = [.. all, counters,];
            Volatile.Write(ref all, registered);
            // The sizes only change here, so they are calculated here instead of every time they are requested.
            Volatile.Write(ref sizes, PoolSizes(registered));
            byConnectionString[connectionString] = counters;
            return counters;
        }
    }

    internal void OpenStarted(SqlConnection conn)
    {
        if (byConnectionString.TryGetValue(conn.ConnectionString, out var counters)) {
            counters.OpenStarted(conn);
        }
    }

    internal void OpenFailed(SqlConnection conn)
    {
        if (byConnectionString.TryGetValue(conn.ConnectionString, out var counters)) {
            counters.OpenFailed();
        }
    }

    /// <summary>
    /// All pools that have been used so far. Pools are only ever added, never removed.
    /// </summary>
    public ReadOnlySpan<ConnectionPoolCounters<TDatabase, TPoolPurpose>> All
        => Volatile.Read(ref all);

    /// <summary>
    /// <see cref="PoolSizes"/> of <see cref="All"/>, calculated when a pool registers. As long as no pool is added,
    /// this is the same array every time, so a caller can see with <see cref="object.ReferenceEquals"/> that nothing
    /// changed.
    /// </summary>
    public ConnectionPoolSize<TDatabase, TPoolPurpose>[] Sizes
        => Volatile.Read(ref sizes);

    /// <summary>
    /// The size of the pools per database and purpose. Pools of different purposes are never summed: a full pool of
    /// one purpose would then be cancelled out by a barely used pool of another. Within a purpose there usually is
    /// exactly one pool per database; when there are more, both are real room and they add up, like the databases
    /// that are not tracked separately and are taken together just as with the usage.
    /// </summary>
    public static ConnectionPoolSize<TDatabase, TPoolPurpose>[] PoolSizes(IEnumerable<ConnectionPoolCounters<TDatabase, TPoolPurpose>> pools)
        => [
            .. pools
                .GroupBy(pool => (pool.Database, pool.Purpose))
                .Select(group => new ConnectionPoolSize<TDatabase, TPoolPurpose>(group.Key.Database, group.Key.Purpose, group.Sum(pool => pool.MaxPoolSize)))
                .OrderBy(size => size.Database)
                .ThenBy(size => size.Purpose),
        ];
}

/// <summary>
/// The size of the pools of a database and purpose together; see
/// <see cref="ConnectionPoolRegistry{TDatabase,TPoolPurpose}.PoolSizes"/>. Database null are the databases that are not
/// tracked separately.
/// </summary>
public sealed record ConnectionPoolSize<TDatabase, TPoolPurpose>(TDatabase? Database, TPoolPurpose Purpose, int MaxPoolSize)
    where TDatabase : struct, Enum
    where TPoolPurpose : struct, Enum;
