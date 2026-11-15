namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// The clock on which the rings of the collector count their seconds. Every measurement reads it, so it has to be
/// cheap, and it may never run backwards. Real time is neither: reading it costs more than the measurement itself and
/// it gets adjusted. The rings therefore count on a timeline of their own that equals Unix time at the start and may
/// slowly drift after that; only when collecting does the collector convert a second of this timeline to real time.
/// </summary>
public abstract class SecondsClock
{
    /// <summary>
    /// Milliseconds on the own timeline; equal to Unix time in milliseconds at the start.
    /// </summary>
    public abstract long Milliseconds { get; }

    public long Second
        => Milliseconds / 1000;

    /// <summary>
    /// Follows the tick counter of the operating system from now on: reading it is a memory read, and it only ever
    /// increases.
    /// </summary>
    public static SecondsClock Ticking(TimeProvider wallClock)
        => new TickingClock(wallClock.GetUtcNow().ToUnixTimeMilliseconds() - Environment.TickCount64);

    /// <summary>
    /// Follows the given clock exactly, so that whoever moves that clock (a test) also decides the seconds of the rings.
    /// </summary>
    public static SecondsClock Following(TimeProvider clock)
        => new FollowingClock(clock);

    sealed class TickingClock(long unixMillisecondsAtTickZero) : SecondsClock
    {
        public override long Milliseconds
            => Environment.TickCount64 + unixMillisecondsAtTickZero;
    }

    sealed class FollowingClock(TimeProvider clock) : SecondsClock
    {
        public override long Milliseconds
            => clock.GetUtcNow().ToUnixTimeMilliseconds();
    }
}
