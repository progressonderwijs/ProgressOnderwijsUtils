using System;
using System.Diagnostics;

namespace ProgressOnderwijsUtils
{
    /// <summary>
    /// This class simulates an exponentially decaying value. The value can be increased at a given time.
    /// The value can be queried for any moment - at any time before the last increase the value is fixed;
    /// at any time after the last increase the value decays exponentially.
    /// 
    /// See Progress.Business.Test.Taken.ExponentialDecayEstimatorTest for example scenarios.
    /// </summary>
    public sealed class ExponentialDecayEstimator
    {
        static readonly double logOfHalf = Math.Log(0.5);
        public readonly TimeSpan halflife;
        DateTime timestampOfValue = default(DateTime).ToUniversalTime();
        double currentValue;

        public ExponentialDecayEstimator(TimeSpan halflife)
        {
            this.halflife = halflife;
        }

        /// <summary>
        /// This is the raw value; i.e. when you add some value X to the empty estimator at some moment, then the raw value at that moment is X, and is X/2 one half-life later.
        /// However, if you continually add values at moments based on some stochastic process, and wish to estimate what the average rate of added values now is, then this value is too large; use EstimatedRateOfChangePerHalflife instead.
        /// </summary>
        public double RawValueAt(DateTime moment)
        {
            Debug.Assert(moment.Kind == DateTimeKind.Utc, "Error:non-UTC DateTime detected; all moments should be in UTC to make reasoning about exponential decays simpler.");
            var halflives = (moment - timestampOfValue).TotalSeconds / halflife.TotalSeconds;

            return currentValue * Math.Exp(logOfHalf * Math.Max(0.0, halflives));
        }

        /// <summary>
        /// Returns the estimated average rate of value adding of some stochastic process (per half-life).
        /// This converges to the true average when you register events over many halflives and when the time between events is typically significantly smaller than the halflife.
        /// When you register values for a short period of time (e.g. just for a single half-life or even less), this will under-estimate the true value.
        /// When you register values too infrequently (i.e. the half-life is too short), then this will over-estimate the true value.
        /// 
        /// This simply returns RawValueAt(last-added-amount-timestamp) * ln(2)
        /// </summary>
        public double EstimatedRateOfChangePerHalflife()
            => RawValueAt(timestampOfValue) * -logOfHalf;

        /// <summary>
        /// Returns the estimated average rate of value adding of some stochastic process (per half-life).
        /// This converges to the true average when you register events over many halflives and when the time between events is typically significantly smaller than the halflife.
        /// When you register values for a short period of time (e.g. just for a single half-life or even less), this will under-estimate the true value.
        /// When you register values too infrequently (i.e. the half-life is too short), then this will over-estimate the true value.
        /// 
        /// This simply returns RawValueAt(moment) * ln(2)
        /// </summary>
        public double EstimatedRateOfChangePerHalflife(DateTime moment)
            => RawValueAt(moment) * -logOfHalf;

        /// <summary>
        /// Adding amounts with a little timestamp jitter doesn't cause huge accuracy issues, but if the timestamps are (relative to the halflife) significantly in the past, you
        /// </summary>
        public void AddAmount(DateTime timestamp, double amount)
        {
            currentValue = RawValueAt(timestamp) + amount;
            timestampOfValue = timestamp;
        }

        public override string ToString()
            => $"{currentValue} at {timestampOfValue} with halflife {halflife}";
    }

    /// <summary>
    /// This class pick a retry delay based on the current error rate (see ErrorRateToRetryDelay for the formula)
    /// The error rate is estimated using exponential decay, so
    /// </summary>
    public sealed class RetryDelayChooser
    {
        readonly ExponentialDecayEstimator errorRateEstimator;
        readonly double halflivesPerDay;
        readonly double scaleFactor;

        public RetryDelayChooser(TimeSpan constantFailureDelayTarget)
        {
            errorRateEstimator = new ExponentialDecayEstimator(TimeSpan.FromHours(12));
            halflivesPerDay = 1.0 / errorRateEstimator.halflife.TotalDays;

            var targetConvergedDelaysPerDay = TimeSpan.FromDays(1).TotalSeconds / constantFailureDelayTarget.TotalSeconds;
            //choose scaleFactor such that: delayConvergenceTarget.TotalSeconds == targetConvergedDelaysPerDay * targetConvergedDelaysPerDay * scaleFactor
            scaleFactor = constantFailureDelayTarget.TotalSeconds / (targetConvergedDelaysPerDay * targetConvergedDelaysPerDay);
        }

        public void RegisterErrorAt(DateTime errorMoment)
            => errorRateEstimator.AddAmount(errorMoment, 1.0);

        public TimeSpan RetryDelayAt(DateTime moment)
            => ErrorsPerDayToRetryDelay(errorRateEstimator.EstimatedRateOfChangePerHalflife(moment) * halflivesPerDay);

        public TimeSpan RegisterErrorAndGetDelay(DateTime errorMoment)
        {
            RegisterErrorAt(errorMoment);
            return ErrorsPerDayToRetryDelay(errorRateEstimator.EstimatedRateOfChangePerHalflife() * halflivesPerDay);
        }

        public TimeSpan ErrorsPerDayToRetryDelay(double approximateErrorsPerDay)
            => TimeSpan.FromSeconds(approximateErrorsPerDay * approximateErrorsPerDay * scaleFactor);
    }
}
