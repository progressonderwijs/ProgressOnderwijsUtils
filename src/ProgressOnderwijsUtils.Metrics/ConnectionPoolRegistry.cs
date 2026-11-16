using System.Collections.Concurrent;

namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// The connection pools of a process, one <see cref="ConnectionPoolCounters{TDatabase,TPool}"/> per connection
/// string.
/// </summary>
/// <param name="classifyCatalog">
/// The value a database name is tracked as, or null for databases that are not tracked separately. Called once per
/// pool, when it is first requested.
/// </param>
public sealed class ConnectionPoolRegistry<TDatabase, TPool>(Func<string, TDatabase?> classifyCatalog)
    where TDatabase : struct, Enum
    where TPool : struct, Enum
{
    readonly ConcurrentDictionary<string, ConnectionPoolCounters<TDatabase, TPool>> byConnectionString = new(StringComparer.Ordinal);
    readonly Lock registration = new();
    ConnectionPoolCounters<TDatabase, TPool>[] all = [];
    ConnectionPoolSize<TDatabase, TPool>[] sizes = [];

    /// <summary>
    /// The counters of the pool of this connection string; every connection string gets exactly one. A connection
    /// string belongs to exactly one pool: when it is requested with another pool, one of the places where it
    /// is built is wrong.
    /// </summary>
    public ConnectionPoolCounters<TDatabase, TPool> For(string connectionString, TPool pool)
        => byConnectionString.TryGetValue(connectionString, out var counters) ? EnsurePool(counters, pool) : Register(connectionString, pool);

    static ConnectionPoolCounters<TDatabase, TPool> EnsurePool(ConnectionPoolCounters<TDatabase, TPool> counters, TPool pool)
        => EqualityComparer<TPool>.Default.Equals(counters.Pool, pool)
            ? counters
            : throw new InvalidOperationException($"The pool of database {counters.Catalog} was registered as {counters.Pool}, but is now requested as {pool}.");

    ConnectionPoolCounters<TDatabase, TPool> Register(string connectionString, TPool pool)
    {
        lock (registration) {
            if (byConnectionString.TryGetValue(connectionString, out var existing)) {
                return EnsurePool(existing, pool);
            }

            var counters = new ConnectionPoolCounters<TDatabase, TPool>(connectionString, pool, classifyCatalog);
            // Copy on write: the sampler runs over all pools without a lock and without an enumerator this way.
            ConnectionPoolCounters<TDatabase, TPool>[] registered = [.. all, counters,];
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
            if (conn.State is System.Data.ConnectionState.Closed) {
                conn.StateChange -= counters.StateChangeHandler;
            }
            counters.OpenFailed();
        }
    }

    /// <summary>
    /// All pools that have been used so far. Pools are only ever added, never removed.
    /// </summary>
    public ReadOnlySpan<ConnectionPoolCounters<TDatabase, TPool>> All
        => Volatile.Read(ref all);

    /// <summary>
    /// <see cref="PoolSizes"/> of <see cref="All"/>, calculated when a pool registers. As long as no pool is added,
    /// this is the same array every time, so a caller can see with <see cref="object.ReferenceEquals"/> that nothing
    /// changed.
    /// </summary>
    public ConnectionPoolSize<TDatabase, TPool>[] Sizes
        => Volatile.Read(ref sizes);

    /// <summary>
    /// The size of the pools per database. Pools are never summed: a full pool of
    /// would then be cancelled out by a barely used pool of the same database. 
    /// </summary>
    public static ConnectionPoolSize<TDatabase, TPool>[] PoolSizes(IEnumerable<ConnectionPoolCounters<TDatabase, TPool>> counters
    )
        => [
            .. counters
                .GroupBy(counter => (counter.Database, counter.Pool))
                .Select(group => new ConnectionPoolSize<TDatabase, TPool>(group.Key.Database, group.Key.Pool, group.Sum(counter => counter.MaxPoolSize)))
                .OrderBy(size => size.Database)
                .ThenBy(size => size.Pool),
        ];
}

/// <summary>
/// The size of a pools of a database together; see
/// <see cref="ConnectionPoolRegistry{TDatabase,TPool}.PoolSizes"/>. Database null are the databases that are not
/// tracked separately.
/// </summary>
public sealed record ConnectionPoolSize<TDatabase, TPool>(TDatabase? Database, TPool Pool, int MaxSize)
    where TDatabase : struct, Enum
    where TPool : struct, Enum;
