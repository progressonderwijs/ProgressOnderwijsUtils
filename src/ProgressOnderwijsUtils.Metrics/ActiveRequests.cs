namespace ProgressOnderwijsUtils.Metrics;

sealed class ActiveRequests
{
    readonly SecondsRingBuffer secondsRing;
    readonly Action<int> resetSlot;
    readonly long[] peaks;
    readonly long[] starts;
    long current;

    public ActiveRequests(SecondsRingBuffer secondsRing)
    {
        this.secondsRing = secondsRing;
        peaks = new long[secondsRing.Length];
        starts = new long[secondsRing.Length];
        resetSlot = ResetSlot;
    }

    // A second starts at the running total: when requests only finish, the peak of that second is the value it
    // started with and not the lowest value measured.
    // That starting level is kept separately, because the peak rises after that and the starting level of a second
    // is at the same time the final level of the second before it.
    void ResetSlot(int index)
    {
        var value = Volatile.Read(ref current);
        Volatile.Write(ref peaks[index], value);
        Volatile.Write(ref starts[index], value);
    }

    /// <summary>
    /// Processes a measurement of the gauge and keeps track of the peak of the current second.
    /// Around a change of second a measurement can end up at the wrong second: whoever is scheduled out briefly
    /// after <see cref="SecondsRingBuffer.TryEnterSlot(Action{int}, out int)"/> for second N, only adds to the total
    /// when second N+1 has already begun, and then still writes that level into the peak of N. The starting level of
    /// N+1 in turn misses that measurement.
    /// That stays limited to that one change of second and does not accumulate: the running total itself is exact
    /// (<see cref="Interlocked.Add(ref long,long)"/>), and the next change records the starting level from that exact
    /// total again. At most a single second seems a few requests too busy. The alternative is reading the clock again
    /// per measurement or keeping the total per slot, and that costs every request time for a deviation that
    /// disappears by itself.
    /// </summary>
    public void Measurement(long upDown)
    {
        // First claim the slot and only then update the total: the first writer of a second sets the peak to the
        // level that second started with. The other way round a finishing request would already have lowered its own
        // second before the starting level is recorded.
        var entered = secondsRing.TryEnterSlot(resetSlot, out var index);
        var updated = Interlocked.Add(ref current, upDown);
        if (!entered) {
            return;
        }

        // The total only changes on a measurement, so the maximum over the measured values really is the peak
        // within this second.
        var peak = Volatile.Read(ref peaks[index]);
        while (updated > peak) {
            var witnessed = Interlocked.CompareExchange(ref peaks[index], updated, peak);
            if (witnessed == peak) {
                return;
            }

            peak = witnessed;
        }
    }

    /// <summary>
    /// Resets the running total to zero because the instrumentation stops with requests still running. First claim
    /// the slot of this second, just like <see cref="Measurement"/>: with that <see cref="ResetSlot"/> records the
    /// level from before clearing, so that a window from before this moment can still calculate that level back
    /// instead of dropping to zero.
    /// </summary>
    public void Reset()
    {
        _ = secondsRing.TryEnterSlot(resetSlot, out _);
        _ = Interlocked.Exchange(ref current, 0);
    }

    /// <summary>
    /// The highest peak over the requested seconds. A slot without a stamp means the gauge did not move that second;
    /// those seconds yield no peak of their own but do sit at the level the window ended with.
    /// </summary>
    public long Collect(int firstSecond, int lastSecond)
    {
        var peak = ValueAt(lastSecond);
        for (var second = firstSecond; second <= lastSecond; second++) {
            var index = secondsRing.IndexOf(second);
            if (!secondsRing.HoldsSecond(second, index)) {
                continue;
            }

            var value = Volatile.Read(ref peaks[index]);
            if (!secondsRing.HoldsSecond(second, index)) {
                continue;
            }

            peak = Math.Max(peak, value);
        }

        return peak;
    }

    /// <summary>
    /// The level of the gauge at the end of <paramref name="second"/>. When there were measurements after that
    /// second, the starting level of the first measured second after it is exactly that final level; only when
    /// nothing happened after it does the running total describe it.
    /// Without this calculation a window without measurements would report the level of now, and then a window full
    /// of running requests drops back to zero as soon as those finish during the settle time.
    /// </summary>
    long ValueAt(int second)
    {
        // The running total is read before searching: a measurement that opens a new second during the search is
        // then not already part of the level we fall back on.
        var runningTotal = Volatile.Read(ref current);
        for (var later = second + 1; later < second + secondsRing.Length; later++) {
            var index = secondsRing.IndexOf(later);
            if (!secondsRing.HoldsSecond(later, index)) {
                continue;
            }

            var start = Volatile.Read(ref starts[index]);
            if (secondsRing.HoldsSecond(later, index)) {
                return start;
            }
        }

        return runningTotal;
    }
}
