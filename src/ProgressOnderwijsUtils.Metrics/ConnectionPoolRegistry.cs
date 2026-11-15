using System.Collections.Concurrent;

namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// The connection pools of a process, one <see cref="ConnectionPoolCounters{TDatabase,TPurpose}"/> per connection
/// string.
/// </summary>
/// <param name="classifyCatalog">
/// The value a database name is tracked as, or null for databases that are not tracked separately. Called once per
/// pool, when it is first requested.
/// </param>
public sealed class ConnectionPoolRegistry<TDatabase, TPurpose>(Func<string, TDatabase?> classifyCatalog)
    where TDatabase : struct, Enum
    where TPurpose : struct, Enum
{
    readonly ConcurrentDictionary<string, ConnectionPoolCounters<TDatabase, TPurpose>> byConnectionString = new(StringComparer.Ordinal);
    readonly Lock registration = new();
    ConnectionPoolCounters<TDatabase, TPurpose>[] all = [];
    ConnectionPoolSize<TDatabase, TPurpose>[] sizes = [];

    /// <summary>
    /// The counters of the pool of this connection string; every connection string gets exactly one. A connection
    /// string belongs to exactly one purpose: when it is requested with another purpose, one of the places where it
    /// is built is wrong.
    /// </summary>
    public ConnectionPoolCounters<TDatabase, TPurpose> For(string connectionString, TPurpose purpose)
        => byConnectionString.TryGetValue(connectionString, out var counters) ? EnsurePurpose(counters, purpose) : Register(connectionString, purpose);

    static ConnectionPoolCounters<TDatabase, TPurpose> EnsurePurpose(ConnectionPoolCounters<TDatabase, TPurpose> counters, TPurpose purpose)
        => EqualityComparer<TPurpose>.Default.Equals(counters.Purpose, purpose)
            ? counters
            : throw new InvalidOperationException($"The pool of database {counters.Catalog} was registered as {counters.Purpose}, but is now requested as {purpose}.");

    ConnectionPoolCounters<TDatabase, TPurpose> Register(string connectionString, TPurpose purpose)
    {
        lock (registration) {
            if (byConnectionString.TryGetValue(connectionString, out var existing)) {
                return EnsurePurpose(existing, purpose);
            }

            var counters = new ConnectionPoolCounters<TDatabase, TPurpose>(connectionString, purpose, classifyCatalog);
            // Copy on write: the sampler runs over all pools without a lock and without an enumerator this way.
            ConnectionPoolCounters<TDatabase, TPurpose>[] registered = [.. all, counters,];
            Volatile.Write(ref all, registered);
            // The sizes only change here, so they are calculated here instead of every time they are requested.
            Volatile.Write(ref sizes, PoolSizes(registered));
            byConnectionString[connectionString] = counters;
            return counters;
        }
    }

    /// <summary>
    /// All pools that have been used so far. Pools are only ever added, never removed.
    /// </summary>
    public ReadOnlySpan<ConnectionPoolCounters<TDatabase, TPurpose>> All
        => Volatile.Read(ref all);

    /// <summary>
    /// <see cref="PoolSizes"/> of <see cref="All"/>, calculated when a pool registers. As long as no pool is added,
    /// this is the same array every time, so a caller can see with <see cref="object.ReferenceEquals"/> that nothing
    /// changed.
    /// </summary>
    public ConnectionPoolSize<TDatabase, TPurpose>[] Sizes
        => Volatile.Read(ref sizes);

    /// <summary>
    /// The size of the pools per database and purpose. Pools of different purposes are never summed: a full pool of
    /// one purpose would then be cancelled out by a barely used pool of another. Within a purpose there usually is
    /// exactly one pool per database; when there are more, both are real room and they add up, like the databases
    /// that are not tracked separately and are taken together just as with the usage.
    /// </summary>
    public static ConnectionPoolSize<TDatabase, TPurpose>[] PoolSizes(IEnumerable<ConnectionPoolCounters<TDatabase, TPurpose>> pools)
        => [
            .. pools
                .GroupBy(pool => (pool.Database, pool.Purpose))
                .Select(group => new ConnectionPoolSize<TDatabase, TPurpose>(group.Key.Database, group.Key.Purpose, group.Sum(pool => pool.MaxPoolSize)))
                .OrderBy(size => size.Database)
                .ThenBy(size => size.Purpose),
        ];
}

/// <summary>
/// The size of the pools of a database and purpose together; see
/// <see cref="ConnectionPoolRegistry{TDatabase,TPurpose}.PoolSizes"/>. Database null are the databases that are not
/// tracked separately.
/// </summary>
public sealed record ConnectionPoolSize<TDatabase, TPurpose>(TDatabase? Database, TPurpose Purpose, int MaxPoolSize)
    where TDatabase : struct, Enum
    where TPurpose : struct, Enum;
