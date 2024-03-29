namespace ProgressOnderwijsUtils;

/// <summary>
/// This class simulates an exponentially decaying value. The value can be increased at a given time.
/// The value can be queried for any moment - at any time before the last increase the value is fixed;
/// at any time after the last increase the value decays exponentially.
/// 
/// See ExponentialDecayEstimatorTest for example scenarios.
/// </summary>
public sealed class ExponentialDecayEstimator
{
    public const double LogOfHalf = -0.69314718055994529; //Math.Log(0.5);
    public readonly TimeSpan halflife;
    DateTime timestampOfValue = default(DateTime).ToUniversalTime();
    double currentValue;

    public DateTime UtcTimestampOfValue
        => timestampOfValue;

    public ExponentialDecayEstimator(TimeSpan halflife)
        => this.halflife = halflife;

    /// <summary>
    /// Returns the estimated average rate of value adding of some stochastic process (per half-life).
    /// This converges to the true average when you register events over many halflives and when the time between events is typically significantly smaller than the half-life.
    /// When you register values for a short period of time (e.g. just for a single half-life or even less), this will under-estimate the true value.
    /// When you register values too infrequently (i.e. the half-life is too short), then this will over-estimate the true value after each addition.
    /// 
    /// This simply returns RawValueAt(moment) * ln(2)
    /// </summary>
    public double EstimatedRateOfChangePerHalflife(DateTime moment)
        => ValueAt(moment).EstimatedEventCountPerHalflife;

    /// <summary>
    /// Returns the estimated average rate of value adding of some stochastic process (per half-life).
    /// This converges to the true average when you register events over many halflives and when the time between events is typically significantly smaller than the half-life.
    /// When you register values for a short period of time (e.g. just for a single half-life or even less), this will under-estimate the true value.
    /// When you register values too infrequently (i.e. the half-life is too short), then this will over-estimate the true value after each addition.
    /// </summary>
    public ExponentialDecayEstimatorValue ValueAt(DateTime moment)
    {
        Debug.Assert(moment.Kind == DateTimeKind.Utc, "Error:non-UTC DateTime detected; all moments should be in UTC to make reasoning about exponential decays simpler.");
        var halflives = (moment - timestampOfValue).TotalSeconds / halflife.TotalSeconds;
        return new(currentValue * Math.Exp(LogOfHalf * Math.Max(0.0, halflives)));
    }

    /// <summary>
    /// Adding amounts with a little timestamp jitter doesn't cause huge accuracy issues, but if the timestamps are (relative to the halflife) significantly in the past, you
    /// </summary>
    public ExponentialDecayEstimatorValue AddAmount(DateTime timestamp, double amount)
    {
        currentValue = ValueAt(timestamp).RawValue + amount;
        timestampOfValue = timestamp;
        return new(currentValue);
    }

    public override string ToString()
        => $"{currentValue} at {timestampOfValue} with halflife {halflife}";
}

public readonly struct ExponentialDecayEstimatorValue
{
    /// <summary>
    /// The raw value of an exponential decay estimation.  This is NOT an estimation of the number of events per halflife; use EstimatedRateOfChangePerHalflife for that.
    /// </summary>
    public readonly double RawValue;

    /// <summary>
    /// The estimated number of events that a hidden stochastic process triggers on average per half-life.  (Numerically this is simply RawValue * ln(2)).
    /// </summary>
    public double EstimatedEventCountPerHalflife
        => RawValue * -ExponentialDecayEstimator.LogOfHalf;

    public ExponentialDecayEstimatorValue(double rawValue)
        => RawValue = rawValue;
}
