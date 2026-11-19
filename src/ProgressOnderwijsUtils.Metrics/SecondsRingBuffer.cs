using System.Numerics;

namespace ProgressOnderwijsUtils.Metrics;

sealed class SecondsRingBuffer
{
    /// <summary>
    /// The stamp of a slot that has never held a second. The timeline starts at second 0, so the default of the array
    /// would make every slot look as if it already holds that second, and its first writer would skip the reset.
    /// </summary>
    const int NoSecond = -1;

    readonly SecondsSinceStart clock;

    /// <summary>
    /// Per slot the second it holds, <see cref="NoSecond"/>, or <see cref="BeingResetFor"/> a second: the first
    /// writer of a second marks the slot that way while it resets it, and only after that stamps the second itself.
    /// </summary>
    readonly int[] stamps;

    public SecondsRingBuffer(SecondsSinceStart clock, int length)
    {
        if (!BitOperations.IsPow2(length)) {
            throw new ArgumentOutOfRangeException(nameof(length), length, "Expected a power of two, so that the slot of a second can be found with a mask.");
        }

        this.clock = clock;
        stamps = new int[length];
        Array.Fill(stamps, NoSecond);
    }

    public int Length
        => stamps.Length;

    /// <summary>
    /// Every slot is reused every <see cref="Length"/> seconds, so the index alone does not tell which second it
    /// holds now; that is what the stamp of the slot is for.
    /// </summary>
    public int IndexOf(int second)
        => second & (stamps.Length - 1);

    /// <summary>
    /// The stamp of a slot while it is being reset for <paramref name="second"/>: below <see cref="NoSecond"/>, so
    /// it is never mistaken for a second of the timeline.
    /// </summary>
    static int BeingResetFor(int second)
        => -second - 2;

    /// <summary>
    /// The second a stamp is about, whether the slot already holds it or is still being reset for it.
    /// </summary>
    static int SecondOf(int stamp)
        => stamp < NoSecond ? -stamp - 2 : stamp;

    /// <summary>
    /// Enters the slot of the current second. The first writer of a second resets the slot through
    /// <paramref name="resetSlot"/> and only stamps the second after that; other writers of that second wait for
    /// that stamp so that their measurement is not wiped after all.
    /// Returns false when the measurement belongs to a second the ring has already overwritten.
    /// A slot that is still being reset for an older second is taken over: its writer stalled for a whole rotation,
    /// and waiting for it would hold up every writer of the current second. Its late reset may then clear the newer
    /// measurements; that costs at most one second.
    /// The result does not hold on to ownership of this generation either: a writer that is scheduled out for longer
    /// than <see cref="Length"/> seconds between this return and its own mutation, writes into a slot that has been
    /// reused and cleared in the meantime and so attributes its measurement to the wrong second. That costs at most
    /// that one measurement, whereas the alternative (a reservation per slot that clearing has to wait for) would
    /// slow down the measuring path of every request; that trade is not worth it.
    /// </summary>
    /// <param name="resetSlot">The action that resets the slot to its initial state.</param>
    /// <param name="index">The index of the entered slot.</param>
    public bool TryEnterSlot(Action<int>? resetSlot, out int index)
    {
        var current = clock.Second;
        index = IndexOf(current);

        var spin = new SpinWait();
        while (true) {
            // In by far the most cases the stamp is already right and this single read suffices.
            var stamp = Volatile.Read(ref stamps[index]);
            if (stamp == current) {
                return true;
            }

            var held = SecondOf(stamp);
            if (held > current) {
                return false;
            } else if (held == current) {
                spin.SpinOnce();
            } else if (Interlocked.CompareExchange(ref stamps[index], BeingResetFor(current), stamp) == stamp) {
                resetSlot?.Invoke(index);
                // Fails when a newer second took the slot over during our reset; that second then holds the slot.
                _ = Interlocked.CompareExchange(ref stamps[index], current, BeingResetFor(current));
                return true;
            }
        }
    }

    /// <summary>
    /// Whether the slot holds the requested second right now. Readers call this before and after reading: only when
    /// the stamp is right both times does the data read really belong to that second and has no writer cleared the
    /// slot for a new second in the meantime. A slot that is being reset holds no second at all, so a reader never
    /// takes its half cleared data for a measurement.
    /// The stamps below zero are no seconds of the timeline, so whoever looks back before the start (second -1)
    /// must not take such a slot for a recorded second.
    /// </summary>
    public bool HoldsSecond(int second, int index)
        => second >= 0 && Volatile.Read(ref stamps[index]) == second;
}
