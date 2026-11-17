namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// The seconds on which the rings of the collector count: whole seconds since the start, on the monotonic timestamp of
/// the <see cref="TimeProvider"/>. Second 0 begins exactly at the start, so every second is observed completely, and the
/// timeline never runs backwards, whatever happens to the wall clock. Only when collecting is a second converted to real
/// time.
/// </summary>
sealed class SecondsSinceStart
{
    readonly TimeProvider time;
    readonly long startTimestamp;
    readonly long timestampsPerSecond;

    public SecondsSinceStart(TimeProvider time)
    {
        this.time = time;
        startTimestamp = time.GetTimestamp();
        timestampsPerSecond = time.TimestampFrequency;
    }

    /// <summary>
    /// The current second; every measurement reads this. An int lasts some 68 years since the start; the conversion is
    /// checked so that a process running even longer fails instead of wrapping to negative seconds.
    /// </summary>
    public int Second
        => checked((int)((time.GetTimestamp() - startTimestamp) / timestampsPerSecond));

    /// <summary>
    /// The real time at which <paramref name="second"/> ended: the current real time minus how long ago that was on the
    /// timeline. Both clocks are read at the same moment, so the drift between them since the start does not count; only
    /// the drift over the few seconds since <paramref name="second"/> ended does.
    /// </summary>
    public DateTime EndUtcOf(int second)
    {
        var utcNow = time.GetUtcNow().UtcDateTime;
        var now = time.GetTimestamp();
        return utcNow - time.GetElapsedTime(startTimestamp + (second + 1) * timestampsPerSecond, now);
    }
}
