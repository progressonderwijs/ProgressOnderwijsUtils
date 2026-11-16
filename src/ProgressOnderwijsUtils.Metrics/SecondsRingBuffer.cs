using System.Numerics;

namespace ProgressOnderwijsUtils.Metrics;

sealed class SecondsRingBuffer
{
    /// <summary>
    /// The stamp of a slot that has never held a second. The timeline starts at second 0, so the default of the array
    /// would make every slot look as if it already holds that second, and its first writer would skip the reset.
    /// </summary>
    const long NoSecond = -1;

    readonly SecondsSinceStart clock;
    readonly long[] slots;
    readonly long[] readySlots;

    public SecondsRingBuffer(SecondsSinceStart clock, int length)
    {
        if (!BitOperations.IsPow2(length)) {
            throw new ArgumentOutOfRangeException(nameof(length), length, "Expected a power of two, so that the slot of a second can be found with a mask.");
        }

        this.clock = clock;
        slots = new long[length];
        readySlots = new long[length];
        Array.Fill(slots, NoSecond);
        Array.Fill(readySlots, NoSecond);
    }

    public int Length
        => slots.Length;

    /// <summary>
    /// Every slot is reused every <see cref="Length"/> seconds, so the index alone does not tell which second it
    /// holds now; that is what the stamp in the slot itself is for.
    /// </summary>
    public int IndexOf(long second)
        => (int)(second & (slots.Length - 1));

    /// <summary>
    /// Claims the slot for the current second. The first writer of a second resets the slot through
    /// <paramref name="resetSlot"/> and only releases it after that; other writers wait for that release so that
    /// their measurement is not wiped after all.
    /// Returns false when the measurement belongs to a second the ring has already overwritten.
    /// The result does not hold on to ownership of this generation: a writer that is scheduled out for longer than
    /// <see cref="Length"/> seconds between this return and its own mutation, writes into a slot that has been reused
    /// and cleared in the meantime and so attributes its measurement to the wrong second. That costs at most that one
    /// measurement, whereas the alternative (a reservation per slot that clearing has to wait for) would slow down
    /// the measuring path of every request; that trade is not worth it.
    /// </summary>
    public bool TryEnterSlot(Action<int>? resetSlot, out int index)
        => TryEnterSlot(resetSlot, out index, out _);

    /// <inheritdoc cref="TryEnterSlot(Action{int}?,out int)"/>
    /// <param name="index">The index of the claimed slot.</param>
    /// <param name="current">The second the slot was claimed for.</param>
    /// <param name="resetSlot">The action that resets the slot to its initial state.</param>
    public bool TryEnterSlot(Action<int>? resetSlot, out int index, out long current)
    {
        current = clock.Second;
        index = IndexOf(current);

        // In by far the most cases the stamp is already right and a plain read suffices.
        var previous = Volatile.Read(ref slots[index]);
        if (previous != current) {
            if (previous > current) {
                return false;
            }

            if (Interlocked.CompareExchange(ref slots[index], current, previous) == previous) {
                resetSlot?.Invoke(index);
                Volatile.Write(ref readySlots[index], current);
                return true;
            }
        }

        var spin = new SpinWait();
        while (Volatile.Read(ref readySlots[index]) is var ready && ready != current) {
            if (ready > current) {
                return false;
            }

            spin.SpinOnce();
        }

        return true;
    }

    /// <summary>
    /// Whether the slot holds the requested second right now. Readers call this before and after reading: only when
    /// the stamp is right both times does the data read really belong to that second and has no writer cleared the
    /// slot for a new second in the meantime.
    /// Both stamps count: a writer claims the slot in <see cref="slots"/> before clearing it and only releases it
    /// after that in <see cref="readySlots"/>. Whoever only looks at the release, still sees the old second in that
    /// window while the data is already wiped, and would take an empty slot for a valid measurement.
    /// A slot that has never been used carries <see cref="NoSecond"/>; that is not a second of the timeline, so
    /// whoever looks back before the start (second -1) must not take such an empty slot for a recorded second.
    /// </summary>
    public bool HoldsSecond(long second, int index)
        => second >= 0 && Volatile.Read(ref slots[index]) == second && Volatile.Read(ref readySlots[index]) == second;
}
