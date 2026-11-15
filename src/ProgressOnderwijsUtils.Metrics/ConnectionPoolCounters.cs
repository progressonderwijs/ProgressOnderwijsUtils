using System.Data;
using System.Data.Common;

namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// The usage of a connection pool, kept for the connections of that pool that are opened through <see cref="Open"/>.
/// Counting is optional per connection: a connection that is opened without it works just as well and is simply not
/// counted.
/// SqlClient itself publishes nothing usable about this: only EventCounters over all pools of the process together,
/// and no count of who is waiting for a connection. SqlClient keeps a pool of its own per connection string, so
/// these counters do too; the collector sums them per database and purpose.
/// All levels are absolute, just like those of the thread pool, so that the sampler can read them at any moment and
/// it does not matter whether connections were already open before the collector started.
/// </summary>
public sealed class ConnectionPoolCounters<TDatabase, TPurpose>
    where TDatabase : struct, Enum
    where TPurpose : struct, Enum
{
    int inUse;
    int pending;
    long opened;

    internal ConnectionPoolCounters(string connectionString, TPurpose purpose, Func<string, TDatabase?> classifyCatalog)
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
    public TPurpose Purpose { get; }

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
    /// Opens a connection of this pool and follows it until it is closed. The connection has to be one of the connection
    /// string these counters were requested for, and stays the caller's to dispose, also when opening fails.
    /// </summary>
    public void Open(DbConnection connection)
    {
        // Subscribe before opening: an open that fails gives no state change, and a reopened connection counts as well this way.
        connection.StateChange += StateChangeHandler;
        OpenStarted();
        var succeeded = false;
        try {
            connection.Open();
            succeeded = true;
        } finally {
            OpenFinished(succeeded);
        }
    }

    /// <summary>
    /// Follows a connection from open to closed. There is one per pool, so that subscribing to a connection
    /// allocates nothing. A connection that breaks (Broken) stays in use until it is closed.
    /// </summary>
    internal StateChangeEventHandler StateChangeHandler { get; }

    void OnStateChange(object sender, StateChangeEventArgs change)
    {
        if (change.CurrentState is ConnectionState.Open && change.OriginalState is not ConnectionState.Open) {
            _ = Interlocked.Increment(ref inUse);
        } else if (change.CurrentState is ConnectionState.Closed && change.OriginalState is not ConnectionState.Closed) {
            _ = Interlocked.Decrement(ref inUse);
        }
    }

    internal void OpenStarted()
        => Interlocked.Increment(ref pending);

    internal void OpenFinished(bool succeeded)
    {
        _ = Interlocked.Decrement(ref pending);
        if (succeeded) {
            _ = Interlocked.Increment(ref opened);
        }
    }
}
