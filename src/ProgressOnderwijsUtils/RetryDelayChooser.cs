using System;

namespace ProgressOnderwijsUtils
{
    /// <summary>
    /// This class pick a retry delay based on the current error rate (see ErrorsPerDayToRetryDelay for the formula)
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