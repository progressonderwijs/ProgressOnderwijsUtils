namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// Sampled levels per second, for a fixed number of slots (for example one per database). Per slot there are gauges,
/// of which the highest sampled level within a second is kept, and counters that only ever increase, of which the
/// last level is kept so that a window can later calculate the difference over that window.
/// Only the sampler thread of the collector writes, one sample after the other; so there is never more than one
/// writer at a time. That is why a read-compare-write suffices and no
/// <see cref="Interlocked.CompareExchange(ref long,long,long)"/> is needed as in <see cref="ActiveRequests"/>, which
/// is called from arbitrary request threads.
/// The writes do stay <see cref="Volatile"/>, because the collecting thread reads along.
/// </summary>
sealed class SampledRing
{
    /// <summary>
    /// The course over a window: per slot and gauge the highest level, and per slot and counter the increase.
    /// The layout is slot times count plus number, as when sampling.
    /// </summary>
    public sealed record Window(long[] GaugePeaks, long[] CounterIncreases);

    readonly SecondsRingBuffer secondsRing;
    readonly Action<int> resetSlot;
    readonly long[] gauges;
    readonly long[] counters;

    /// <summary>
    /// Per slot the second of which the levels have been written completely. The slot itself is already released
    /// before the sample writes anything into it; whoever reads in between, still sees the zeros of
    /// <see cref="ResetSlot"/>. A zero can however also be a real level (a database without opens), so the sample
    /// only sets this stamp after the levels, and readers require it.
    /// </summary>
    readonly long[] sampledSeconds;

    public SampledRing(SecondsRingBuffer secondsRing, int slotCount, int gaugeCount, int counterCount)
    {
        this.secondsRing = secondsRing;
        SlotCount = slotCount;
        GaugesPerSecond = slotCount * gaugeCount;
        CountersPerSecond = slotCount * counterCount;
        gauges = new long[secondsRing.Length * GaugesPerSecond];
        counters = new long[secondsRing.Length * CountersPerSecond];
        sampledSeconds = new long[secondsRing.Length];
        Array.Fill(sampledSeconds, long.MinValue);
        resetSlot = ResetSlot;
    }

    public int SlotCount { get; }
    int GaugesPerSecond { get; }
    int CountersPerSecond { get; }

    /// <summary>
    /// A reused slot starts empty; otherwise the peak of the second that was in this slot a round earlier would
    /// still count. The release of the slot in TryEnterSlot acts as a release barrier.
    /// </summary>
    void ResetSlot(int index)
    {
        gauges.AsSpan(index * GaugesPerSecond, GaugesPerSecond).Clear();
        counters.AsSpan(index * CountersPerSecond, CountersPerSecond).Clear();
    }

    /// <summary>
    /// Records a sample at the current second: the gauges as peak, the counters as last level.
    /// The last level within a second, not the highest: counters only ever increase, so the last level is at the same
    /// time the highest and leads to the smallest gap with the second after it.
    /// </summary>
    public void Record(ReadOnlySpan<long> gaugeValues, ReadOnlySpan<long> counterValues)
    {
        if (!secondsRing.TryEnterSlot(resetSlot, out var index, out var second)) {
            return;
        }

        var gaugeSlot = gauges.AsSpan(index * GaugesPerSecond, GaugesPerSecond);
        for (var i = 0; i < gaugeSlot.Length; i++) {
            if (gaugeValues[i] > Volatile.Read(ref gaugeSlot[i])) {
                Volatile.Write(ref gaugeSlot[i], gaugeValues[i]);
            }
        }

        var counterSlot = counters.AsSpan(index * CountersPerSecond, CountersPerSecond);
        for (var i = 0; i < counterSlot.Length; i++) {
            Volatile.Write(ref counterSlot[i], counterValues[i]);
        }

        Volatile.Write(ref sampledSeconds[index], second);
    }

    /// <summary>
    /// The course over the requested seconds, or null when no sample was taken in that window.
    /// The gauges yield their highest level in the window; seconds without a sample are skipped, because those say
    /// nothing about the level and could, as zero, only lower the peak.
    /// The counters yield their increase over the window: that is the level at the end of the last sampled second
    /// minus the level from just before the window.
    /// </summary>
    public Window? Collect(long firstSecond, long lastSecond)
    {
        var gaugeBuffer = new long[GaugesPerSecond];
        var counterBuffer = new long[CountersPerSecond];
        var peaks = new long[GaugesPerSecond];
        var firstCounters = new long[CountersPerSecond];
        var lastCounters = new long[CountersPerSecond];
        var sawSample = false;

        for (var second = firstSecond; second <= lastSecond; second++) {
            if (!TryRead(second, gaugeBuffer, counterBuffer)) {
                continue;
            }

            for (var i = 0; i < peaks.Length; i++) {
                peaks[i] = Math.Max(peaks[i], gaugeBuffer[i]);
            }

            counterBuffer.CopyTo(lastCounters, 0);
            if (!sawSample) {
                counterBuffer.CopyTo(firstCounters, 0);
                sawSample = true;
            }
        }

        if (!sawSample) {
            return null;
        }

        // Without a starting level from just before the window the increase within the first sampled second does
        // not count; that second then has no earlier level to calculate from.
        var baseCounters = TryReadLastBefore(firstSecond, gaugeBuffer, counterBuffer) ? counterBuffer : firstCounters;
        var increases = new long[CountersPerSecond];
        for (var i = 0; i < increases.Length; i++) {
            increases[i] = lastCounters[i] - baseCounters[i];
        }

        return new(peaks, increases);
    }

    /// <summary>
    /// The last levels from before <paramref name="second"/> that the ring still holds. The slot of the second
    /// before it is usually not sampled, so we search for the nearest one that is.
    /// The buffers are scratch space and hold the levels of that second afterwards.
    /// </summary>
    bool TryReadLastBefore(long second, long[] gaugeBuffer, long[] counterBuffer)
    {
        for (var earlier = second - 1; earlier > second - secondsRing.Length; earlier--) {
            if (TryRead(earlier, gaugeBuffer, counterBuffer)) {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// What is recorded for <paramref name="second"/>, or false when that second was not sampled (completely) or has
    /// been overwritten by the ring in the meantime.
    /// </summary>
    bool TryRead(long second, Span<long> gaugesOut, Span<long> countersOut)
    {
        var index = secondsRing.IndexOf(second);
        if (!secondsRing.HoldsSecond(second, index) || Volatile.Read(ref sampledSeconds[index]) != second) {
            return false;
        }

        var gaugeSlot = gauges.AsSpan(index * GaugesPerSecond, GaugesPerSecond);
        for (var i = 0; i < gaugeSlot.Length; i++) {
            gaugesOut[i] = Volatile.Read(ref gaugeSlot[i]);
        }

        var counterSlot = counters.AsSpan(index * CountersPerSecond, CountersPerSecond);
        for (var i = 0; i < counterSlot.Length; i++) {
            countersOut[i] = Volatile.Read(ref counterSlot[i]);
        }

        // A slot that was switched while reading describes a different second by now.
        return secondsRing.HoldsSecond(second, index);
    }
}
