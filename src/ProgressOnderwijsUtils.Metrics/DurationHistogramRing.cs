namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// A ring of duration histograms: per second a series of cells as laid out by <see cref="MetricsDimension{T}"/>.
/// Which value and which outcome belong to a measurement is up to the dimension; here only the cell that goes with
/// it is counted.
/// </summary>
sealed class DurationHistogramRing<T>
    where T : struct, Enum
{
    readonly SecondsRingBuffer secondsRing;
    readonly MetricsDimension<T> dimension;
    readonly int[] counts;
    readonly Action<int> clearSlot;

    public DurationHistogramRing(SecondsRingBuffer secondsRing, MetricsDimension<T> dimension)
    {
        this.secondsRing = secondsRing;
        this.dimension = dimension;
        counts = new int[secondsRing.Length * dimension.Size];
        clearSlot = ClearSlot;
    }

    void ClearSlot(int index)
        // The release of the slot in TryEnterSlot acts as a release barrier, so plain writes are sufficiently
        // visible to the other writers here.
        => counts.AsSpan(index * dimension.Size, dimension.Size).Clear();

    /// <summary>
    /// The dimension knows which tags the instrument passes along, so parsing them belongs there.
    /// </summary>
    public void Measurement(double duration, ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        if (!secondsRing.TryEnterSlot(clearSlot, out var slotIndex)) {
            return;
        }

        var cellIndex = dimension.Index(tags, duration);
        _ = Interlocked.Increment(ref counts[slotIndex * dimension.Size + cellIndex]);
    }

    /// <summary>
    /// Sums the histograms of the requested seconds. A slot without a stamp is a second without measurements and so
    /// counts as zero.
    /// </summary>
    public int[] Collect(long firstSecond, long lastSecond)
    {
        var cellsPerSecond = dimension.Size;
        var totals = new int[cellsPerSecond];
        // With a slot per tracked value a second is too large for the stack, so the snapshot lives on the heap;
        // collecting happens rarely enough to afford that.
        var snapshot = new int[cellsPerSecond];
        for (var second = firstSecond; second <= lastSecond; second++) {
            var index = secondsRing.IndexOf(second);
            if (!secondsRing.HoldsSecond(second, index)) {
                continue;
            }

            var slot = counts.AsSpan(index * cellsPerSecond, cellsPerSecond);
            for (var cell = 0; cell < cellsPerSecond; cell++) {
                snapshot[cell] = Volatile.Read(ref slot[cell]);
            }

            // Only after the whole copy do we know whether the slot was cleared for a new second in the meantime;
            // the snapshot is then a mixture of two seconds and is thrown away.
            if (!secondsRing.HoldsSecond(second, index)) {
                continue;
            }

            for (var cell = 0; cell < cellsPerSecond; cell++) {
                totals[cell] += snapshot[cell];
            }
        }

        return totals;
    }
}
