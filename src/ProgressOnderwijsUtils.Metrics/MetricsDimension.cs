using System.Runtime.CompilerServices;

namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// Divides the measurements of a duration instrument over the values of <typeparamref name="T"/>: every tracked value
/// gets a slot of its own with a histogram for succeeded and one for failed measurements.
/// </summary>
public abstract class MetricsDimension<T>
    where T : struct, Enum
{
    /// <summary>
    /// The slot that receives everything that is not tracked separately, including measurements without a value.
    /// </summary>
    public const int OtherSlot = 0;

    /// <summary>
    /// The lookup table is as long as the highest tracked value; a value above this limit would cost an unreasonably
    /// large table for a handful of slots. With ints the table stays within 16 KB.
    /// </summary>
    const int MaxTrackedValue = 4096;

    /// <summary>
    /// The cells of a second are summed on the stack when collecting, so that collecting allocates nothing but its
    /// result; with ints that is at most 4 KB. This bounds the number of tracked values times the number of buckets.
    /// </summary>
    public const int MaxCellsPerSecond = 1024;

    /// <summary>
    /// The slot of every value, at the position of its enum value. An array is fastest here: the enums are small and
    /// lie close enough together, and every measurement looks one up. A value that is not in it, or has 0, falls on
    /// <see cref="OtherSlot"/>.
    /// </summary>
    readonly int[] slotByValue;

    protected abstract (T? Value, bool Failed) ReadTags(ReadOnlySpan<KeyValuePair<string, object?>> tags);

    /// <summary>
    /// The tracked values get a slot in this order, starting at 1; slot 0 is <see cref="OtherSlot"/>. The order is
    /// only fixed in memory.
    /// </summary>
    protected MetricsDimension(MetricsHistogram histogram, IEnumerable<T> tracked)
    {
        if (Enum.GetUnderlyingType(typeof(T)) != typeof(int)) {
            throw new ArgumentException($"{typeof(T)} must be an int-based enum to be used as a metrics dimension.");
        }

        var trackedValues = tracked.ToArray();
        Values = [null, .. trackedValues.Select(value => (T?)value),];

        var keys = trackedValues.Select(KeyOf).ToArray();
        foreach (var key in keys) {
            if (key is < 0 or > MaxTrackedValue) {
                throw new ArgumentException($"Tracked values of {typeof(T)} must lie in [0, {MaxTrackedValue}], but got {key}.", nameof(tracked));
            }
        }

        if (keys.Length != keys.Distinct().Count()) {
            throw new ArgumentException($"Every tracked value of {typeof(T)} may only be tracked once, but got [{trackedValues.Select(tv => tv.ToString()).JoinStrings(", ")}].", nameof(tracked));
        }

        slotByValue = new int[keys.Length is 0 ? 0 : keys.Max() + 1];
        for (var i = 0; i < keys.Length; i++) {
            slotByValue[keys[i]] = i + 1;
        }

        Histogram = histogram;

        if (Size > MaxCellsPerSecond) {
            throw new ArgumentException(
                $"""
                The metrics dimension of {typeof(T)} is too large: {trackedValues.Length} tracked values with {histogram.BucketCount} buckets need {Size} cells per second,
                but at most {MaxCellsPerSecond} fit on the stack when collecting.
                Track fewer values separately, or use a histogram with fewer buckets.
                """,
                nameof(tracked)
            );
        }
    }

    static int KeyOf(T value)
        => Unsafe.As<T, int>(ref value);

    public MetricsHistogram Histogram { get; }

    /// <summary>
    /// The value of every slot; <see cref="OtherSlot"/> holds null.
    /// </summary>
    public T?[] Values { get; }

    public int Size
        => Values.Length * Histogram.BucketCount * 2;

    /// <summary>
    /// The cell of a value slot, so that code that only works with slots (charts, windows) does not have to look up
    /// the value first.
    /// </summary>
    public int CellIndex(int valueSlot, bool failed, int bucketIndex)
        => valueSlot * Histogram.BucketCount * 2 + bucketIndex * 2 + (failed ? 1 : 0);

    /// <summary>
    /// The slot a value gets; values that are not tracked separately end up on <see cref="OtherSlot"/>.
    /// </summary>
    public int SlotOf(T? value)
        => value is { } v && KeyOf(v) is >= 0 and var key && key < slotByValue.Length ? slotByValue[key] : OtherSlot;

    public int Index(T? value, bool failed, double duration)
        => CellIndex(SlotOf(value), failed, Histogram.GetBucketIndex(duration));

    public int Index(ReadOnlySpan<KeyValuePair<string, object?>> tags, double duration)
    {
        var (value, failed) = ReadTags(tags);
        return Index(value, failed, duration);
    }

    public bool IsFailed(int index)
        => (index & 1) is 1;

    /// <summary>
    /// The number of measurements in a bucket of a value slot, for succeeded or for failed measurements.
    /// </summary>
    /// <param name="cells">The cells of this dimension, as collected over a window.</param>
    /// <param name="valueSlot">The slot of the value.</param>
    /// <param name="bucketIndex">The bucket of the histogram.</param>
    /// <param name="failed">Whether the failed or the succeeded measurements are counted.</param>
    public int Count(int[] cells, int valueSlot, int bucketIndex, bool failed)
        => cells[CellIndex(valueSlot, failed, bucketIndex)];

    /// <summary>
    /// The number of measurements in a bucket across all value slots.
    /// </summary>
    /// <inheritdoc cref="Count(int[],int,int,bool)" path="/param"/>
    public int Count(int[] cells, int bucketIndex, bool failed)
    {
        var total = 0;
        for (var valueSlot = 0; valueSlot < Values.Length; valueSlot++) {
            total += Count(cells, valueSlot, bucketIndex, failed);
        }

        return total;
    }

    /// <summary>
    /// The histogram of a value slot: per bucket the number of succeeded or of failed measurements.
    /// </summary>
    /// <inheritdoc cref="Count(int[],int,int,bool)" path="/param"/>
    public int[] Counts(int[] cells, int valueSlot, bool failed)
        => [.. Enumerable.Range(0, Histogram.BucketCount).Select(bucketIndex => Count(cells, valueSlot, bucketIndex, failed)),];

    /// <summary>
    /// The histogram across all value slots: per bucket the number of succeeded or of failed measurements.
    /// </summary>
    /// <inheritdoc cref="Count(int[],int,int,bool)" path="/param"/>
    public int[] Counts(int[] cells, bool failed)
        => [.. Enumerable.Range(0, Histogram.BucketCount).Select(bucketIndex => Count(cells, bucketIndex, failed)),];
}
