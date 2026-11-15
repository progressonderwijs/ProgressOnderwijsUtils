namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// The layout of the connection usage in slots: one per database slot and purpose. Sampling, collecting and storing
/// all calculate through this layout, so that it is fixed in one place. The slots of a database lie next to each
/// other, in the order of the purposes.
/// </summary>
public sealed class ConnectionPoolSlots<TDatabase, TPoolPurpose>(MetricsDimension<TDatabase> databases)
    where TDatabase : struct, Enum
    where TPoolPurpose : struct, Enum
{
    static readonly TPoolPurpose[] purposes = Enum.GetValues<TPoolPurpose>();

    public static IReadOnlyList<TPoolPurpose> Purposes
        => purposes;

    public int DatabaseCount
        => databases.Values.Length;

    public int Count
        => DatabaseCount * purposes.Length;

    public int SlotOf(int databaseSlot, TPoolPurpose purpose)
        => databaseSlot * purposes.Length + PurposeIndexOf(purpose);

    public int SlotOf(TDatabase? database, TPoolPurpose purpose)
        => SlotOf(databases.SlotOf(database), purpose);

    public int DatabaseSlotOf(int slot)
        => slot / purposes.Length;

    public TPoolPurpose PurposeOf(int slot)
        => purposes[slot % purposes.Length];

    static int PurposeIndexOf(TPoolPurpose purpose)
        => Array.IndexOf(purposes, purpose) is var index and >= 0
            ? index
            : throw new ArgumentOutOfRangeException(nameof(purpose), purpose, "Expected a defined connection pool purpose.");
}
