using System.Data;
using System.Data.Common;

namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// The usage of a connection pool, counted while a <see cref="MetricsCollector{TEndpoint,TDatabase,TPoolPurpose}"/> runs: from
/// the open events of SqlClient and the StateChange events of the connections.
/// SqlClient itself publishes nothing usable about this: only EventCounters over all pools of the process together,
/// and no count of who is waiting for a connection. SqlClient keeps a pool of its own per connection string, so
/// these counters do too; the collector sums them per database and purpose.
/// All levels are absolute, just like those of the thread pool, so that the sampler can read them at any moment.
/// </summary>
public sealed class ConnectionPoolCounters<TDatabase, TPoolPurpose>
    where TDatabase : struct, Enum
    where TPoolPurpose : struct, Enum
{
    int inUse;
    int pending;
    long opened;

    internal ConnectionPoolCounters(string connectionString, TPoolPurpose purpose, Func<string, TDatabase?> classifyCatalog)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        Catalog = builder.InitialCatalog;
        Database = classifyCatalog(Catalog);
        Purpose = purpose;
        MaxPoolSize = builder.MaxPoolSize;
        StateChangeHandler = OnStateChange;
    }

    /// <summary>
    /// What this pool is for.
    /// </summary>
    public TPoolPurpose Purpose { get; }

    /// <summary>
    /// How many connections this pool has open at most at the same time (Max Pool Size from the connection string).
    /// A pool belongs to exactly one connection string, so this does not change while the process runs.
    /// </summary>
    public int MaxPoolSize { get; }

    /// <summary>
    /// The database of this pool.
    /// </summary>
    public string Catalog { get; }

    /// <summary>
    /// The value <see cref="Catalog"/> is classified as, or null for databases that are not tracked separately.
    /// </summary>
    public TDatabase? Database { get; }

    /// <summary>
    /// The connections that were taken from the pool and have not been returned yet.
    /// </summary>
    public int InUse
        => Volatile.Read(ref inUse);

    /// <summary>
    /// The callers that are waiting for a connection from this pool right now. When the pool is full, they wait
    /// until a connection comes back; otherwise this only takes as long as setting up a new physical connection.
    /// </summary>
    public int Pending
        => Volatile.Read(ref pending);

    /// <summary>
    /// The total number of succeeded opens since the start of the process; only ever increases, just like
    /// <see cref="ThreadPool.CompletedWorkItemCount"/>.
    /// </summary>
    public long Opened
        => Volatile.Read(ref opened);

    /// <summary>
    /// Follows a connection from open to closed. There is one per pool, so that subscribing to a connection
    /// allocates nothing. A connection that breaks (Broken) stays in use until it is closed.
    /// </summary>
    internal StateChangeEventHandler StateChangeHandler { get; }

    /// <summary>
    /// Unsubscribes when the connection closes, so that every open it counts was counted as pending first. A reopen of
    /// the same connection object is only counted when its pool can be found again: after the first open SqlClient
    /// leaves the password out of the connection string, and without the subscription such a reopen is then not
    /// counted at all instead of being counted as an open that never was pending.
    /// </summary>
    void OnStateChange(object sender, StateChangeEventArgs change)
    {
        if (change.CurrentState is ConnectionState.Open && change.OriginalState is not ConnectionState.Open) {
            _ = Interlocked.Decrement(ref pending);
            _ = Interlocked.Increment(ref opened);
            _ = Interlocked.Increment(ref inUse);
        } else if (change.CurrentState is ConnectionState.Closed && change.OriginalState is not ConnectionState.Closed) {
            _ = Interlocked.Decrement(ref inUse);
            ((DbConnection)sender).StateChange -= StateChangeHandler;
        }
    }

    /// <summary>
    /// Unsubscribing first keeps a single subscription when the same connection object starts an open twice, for
    /// instance a retry after a failed open.
    /// </summary>
    internal void OpenStarted(DbConnection connection)
    {
        connection.StateChange -= StateChangeHandler;
        connection.StateChange += StateChangeHandler;
        _ = Interlocked.Increment(ref pending);
    }

    internal void OpenFailed()
        => Interlocked.Decrement(ref pending);
}
