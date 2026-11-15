namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// The layout of the connection usage in slots: one per database slot and pool. Sampling, collecting and storing
/// all calculate through this layout, so that it is fixed in one place. The slots of a database lie next to each
/// other, in the order of the pools.
/// </summary>
public sealed class ConnectionPoolSlots<TDatabase, TPool>(MetricsDimension<TDatabase> databases)
    where TDatabase : struct, Enum
    where TPool : struct, Enum
{
    static readonly TPool[] pools = Enum.GetValues<TPool>();

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
