using System.Numerics;

namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// Sampled levels per second, for a fixed number of slots (for example one per database). Per slot there are gauges,
/// of which the highest sampled level within a second is kept, and counters that only ever increase, of which the
/// last level is kept so that a window can later calculate the difference over that window.
/// Only the sampler thread of the collector writes, one sample after the other; so there is never more than one
/// writer at a time. That is why a stamp per second suffices here, and the claiming of
/// <see cref="SecondsRingBuffer"/>, which is called from arbitrary request threads, is not needed.
/// The writes do stay <see cref="Volatile"/>, because the collecting thread reads along.
/// </summary>
sealed class SampledRing
{
    /// <summary>
    /// The stamp of a second that has never been sampled, or is being written right now. Not a second of the
    /// timeline, also not for whoever looks back before the start.
    /// </summary>
    const int NoSecond = int.MinValue;

    readonly SecondsSinceStart clock;
    readonly long[] gauges;
    readonly long[] counters;

    /// <summary>
    /// Per second of the ring the second of which the levels have been written completely. The sample takes the
    /// stamp away before it writes and only puts it back after; readers require it before and after reading, so
    /// they never take a half written sample or the levels of another second for a sample.
    /// </summary>
    readonly int[] sampledSeconds;

    public SampledRing(SecondsSinceStart clock, int length, int slotCount, int gaugeCount, int counterCount)
    {
        if (!BitOperations.IsPow2(length)) {
            throw new ArgumentOutOfRangeException(nameof(length), length, "Expected a power of two, so that the index of a second can be found with a mask.");
        }

        this.clock = clock;
        SlotCount = slotCount;
        GaugesPerSecond = slotCount * gaugeCount;
        CountersPerSecond = slotCount * counterCount;
        gauges = new long[length * GaugesPerSecond];
        counters = new long[length * CountersPerSecond];
        sampledSeconds = new int[length];
        Array.Fill(sampledSeconds, NoSecond);
    }

    public int SlotCount { get; }
    public int GaugesPerSecond { get; }
    public int CountersPerSecond { get; }

    /// <summary>
    /// Every index is reused every length of the ring, so the index alone does not tell which second it holds now;
    /// that is what the stamp is for.
    /// </summary>
    int IndexOf(int second)
        => second & (sampledSeconds.Length - 1);

    /// <summary>
    /// Records a sample at the current second: the gauges as peak, the counters as last level.
    /// The last level within a second, not the highest: counters only ever increase, so the last level is at the same
    /// time the highest and leads to the smallest gap with the second after it.
    /// The first sample of a second overwrites what the second that was at this index a round earlier left behind;
    /// otherwise the peak of that second would still count.
    /// </summary>
    public void Record(ReadOnlySpan<long> gaugeValues, ReadOnlySpan<long> counterValues)
    {
        var second = clock.Second;
        var index = IndexOf(second);

        // An exchange and not a volatile write: the writes below may not become visible before the stamp is gone.
        var isFirstSampleOfSecond = Interlocked.Exchange(ref sampledSeconds[index], NoSecond) != second;

        var gaugesOfSecond = gauges.AsSpan(index * GaugesPerSecond, GaugesPerSecond);
        for (var i = 0; i < gaugesOfSecond.Length; i++) {
            if (isFirstSampleOfSecond || gaugeValues[i] > Volatile.Read(ref gaugesOfSecond[i])) {
                Volatile.Write(ref gaugesOfSecond[i], gaugeValues[i]);
            }
        }

        var countersOfSecond = counters.AsSpan(index * CountersPerSecond, CountersPerSecond);
        for (var i = 0; i < countersOfSecond.Length; i++) {
            Volatile.Write(ref countersOfSecond[i], counterValues[i]);
        }

        Volatile.Write(ref sampledSeconds[index], second);
    }

    /// <summary>
    /// The course over the requested seconds, into spans of the caller of exactly <see cref="GaugesPerSecond"/> and
    /// <see cref="CountersPerSecond"/> values, so that collecting allocates nothing here; the layout is slot times
    /// count plus number, as when sampling. False when no sample was taken in that window; the spans then hold nothing
    /// meaningful.
    /// The gauges yield their highest level in the window; seconds without a sample are skipped, because those say
    /// nothing about the level and could, as zero, only lower the peak.
    /// The counters yield their increase over the window: that is the level at the end of the last sampled second
    /// minus the level from just before the window.
    /// </summary>
    public bool TryCollect(int firstSecond, int lastSecond, Span<long> gaugePeaks, Span<long> counterIncreases)
    {
        if (gaugePeaks.Length != GaugesPerSecond || counterIncreases.Length != CountersPerSecond) {
            throw new ArgumentException(
                $"Expected {GaugesPerSecond} gauge peaks and {CountersPerSecond} counter increases, but got {gaugePeaks.Length} and {counterIncreases.Length}."
            );
        }

        // The slots are bounded by ConnectionPoolSlots.MaxCount, so this fits on the stack.
        Span<long> gaugeBuffer = stackalloc long[GaugesPerSecond];
        Span<long> counterBuffer = stackalloc long[CountersPerSecond];
        Span<long> firstCounters = stackalloc long[CountersPerSecond];
        Span<long> lastCounters = stackalloc long[CountersPerSecond];
        gaugePeaks.Clear();
        var sawSample = false;

        for (var second = firstSecond; second <= lastSecond; second++) {
            if (!TryRead(second, gaugeBuffer, counterBuffer)) {
                continue;
            }

            for (var i = 0; i < gaugePeaks.Length; i++) {
                gaugePeaks[i] = Math.Max(gaugePeaks[i], gaugeBuffer[i]);
            }

            counterBuffer.CopyTo(lastCounters);
            if (!sawSample) {
                counterBuffer.CopyTo(firstCounters);
                sawSample = true;
            }
        }

        if (!sawSample) {
            return false;
        }

        // Without a starting level from just before the window the increase within the first sampled second does
        // not count; that second then has no earlier level to calculate from.
        var baseCounters = TryReadLastBefore(firstSecond, gaugeBuffer, counterBuffer) ? counterBuffer : firstCounters;
        for (var i = 0; i < counterIncreases.Length; i++) {
            counterIncreases[i] = lastCounters[i] - baseCounters[i];
        }

        return true;
    }

    /// <summary>
    /// The last levels from before <paramref name="second"/> that the ring still holds. The second before it is
    /// usually not sampled, so we search for the nearest one that is.
    /// The buffers are scratch space and hold the levels of that second afterwards.
    /// </summary>
    bool TryReadLastBefore(int second, Span<long> gaugeBuffer, Span<long> counterBuffer)
    {
        for (var earlier = second - 1; earlier > second - sampledSeconds.Length; earlier--) {
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
    bool TryRead(int second, Span<long> gaugesOut, Span<long> countersOut)
    {
        var index = IndexOf(second);
        if (Volatile.Read(ref sampledSeconds[index]) != second) {
            return false;
        }

        var gaugesOfSecond = gauges.AsSpan(index * GaugesPerSecond, GaugesPerSecond);
        for (var i = 0; i < gaugesOfSecond.Length; i++) {
            gaugesOut[i] = Volatile.Read(ref gaugesOfSecond[i]);
        }

        var countersOfSecond = counters.AsSpan(index * CountersPerSecond, CountersPerSecond);
        for (var i = 0; i < countersOfSecond.Length; i++) {
            countersOut[i] = Volatile.Read(ref countersOfSecond[i]);
        }

        // An index that was reused while reading describes a different second by now.
        return Volatile.Read(ref sampledSeconds[index]) == second;
    }
}
