namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// The layout of the connection usage in slots: one per database slot and pool. Sampling, collecting and storing
/// all calculate through this layout, so that it is fixed in one place. The slots of a database lie next to each
/// other, in the order of the pools.
/// </summary>
public sealed class ConnectionPoolSlots<TDatabase, TPool>
    where TDatabase : struct, Enum
    where TPool : struct, Enum
{
    /// <summary>
    /// Sampling and collecting keep a few longs per slot on the stack, so that neither allocates anything but the
    /// collected result; at this maximum that stays within 5 KB.
    /// </summary>
    public const int MaxCount = 128;

    static readonly TPool[] pools = Enum.GetValues<TPool>();

    readonly MetricsDimension<TDatabase> databases;

    public ConnectionPoolSlots(MetricsDimension<TDatabase> databases)
    {
        if (databases.Values.Length * pools.Length > MaxCount) {
            throw new ArgumentException(
                $"""
                The connection usage is too large: {databases.Values.Length} database slots of {typeof(TDatabase)} times {pools.Length} pools of {typeof(TPool)}
                need {databases.Values.Length * pools.Length} slots, but at most {MaxCount} fit on the stack when sampling and collecting.
                Track fewer databases separately, or define fewer pools.
                """,
                nameof(databases)
            );
        }

        this.databases = databases;
    }

    public static IReadOnlyList<TPool> Pools
        => pools;

    public int DatabaseCount
        => databases.Values.Length;

    public int Count
        => DatabaseCount * pools.Length;

    public int SlotOf(int databaseSlot, TPool pool)
        => databaseSlot * pools.Length + PoolIndexOf(pool);

    public int SlotOf(TDatabase? database, TPool pool)
        => SlotOf(databases.SlotOf(database), pool);

    public int DatabaseSlotOf(int slot)
        => slot / pools.Length;

    public TPool PoolOf(int slot)
        => pools[slot % pools.Length];

    static int PoolIndexOf(TPool pool)
        => Array.IndexOf(pools, pool) is var index and >= 0
            ? index
            : throw new ArgumentOutOfRangeException(nameof(pool), pool, "Expected a defined connection pool.");
}
