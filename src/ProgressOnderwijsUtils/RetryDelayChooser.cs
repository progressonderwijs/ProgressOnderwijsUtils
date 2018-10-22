using System;

namespace ProgressOnderwijsUtils
{
    /// <summary>
    /// This class picks a retry delay appropriate for a possibly intermittently failing service or similar process.
    /// The aim is to have long retry delays during a real outage, yet very low retry delays during normal operations even when the occasional call fails.
    /// By delaying for a long time during an outage, you can avoid log spam, and avoid excessive load on a service that's possibly already overloaded.
    /// Simultaneously, the retry delay is bounded such that when the outage stops, the system automatically resumes normal operations without requiring human intervention.
    /// The algorithm is very simple; and not intended to find a truly optimal delay (which would depend on the circumstances); the intent
    /// is to a pick a delays that works reasonably even when the underlying error rate varies wildly.
    ///
    /// The algorithm accepts one parameter, the retry delay that the algorithm should converge to during a 100% outages;
    /// i.e. an infinite loop of Thread.Sleep(chooser.RegisterErrorAndGetDelay(DateTime.UtcNow)) will eventually converge to the value provided.
    /// </summary>
    public sealed class RetryDelayChooser
    {
        readonly ExponentialDecayEstimator errorRateEstimator;
        readonly double halflivesPerSecond;
        readonly double delayTargetInSecondsCubed;

        /// <param name="constantFailureDelayTarget">The target delay to converge to during a long service outage.  The error rate half-life will be 100 times this value.</param>
        public RetryDelayChooser(TimeSpan constantFailureDelayTarget)
            : this(constantFailureDelayTarget, 100) { }

        /// <param name="constantFailureDelayTarget">The target delay to converge to during a long service outage</param>
        /// <param name="halflifeFactor">The halflife (as a factor of the constantFailureDelayTarget) of the error rate estimate.  Larger values ramp and recover more slowly; smaller values ramp and recover more quickly.</param>
        public RetryDelayChooser(TimeSpan constantFailureDelayTarget, double halflifeFactor)
        {
            if (halflifeFactor < 1.0)
                throw new Exception("For reasonably convergence, the error-rate half-life must exceed the delay target; i.e. halflifeFactor must exceed 1.0.  A reasonable value might be 10.");
            var delaysPerHalflife = 1.0 / halflifeFactor; //so this is at *most* 1, and typically much smaller, therefore...
            var empiracalConvergenceOverestimateRatio = 1 + 0.230 * delaysPerHalflife + 0.09761662037037 * delaysPerHalflife * delaysPerHalflife; //...this is at *most* 1.33

            //The convergenceOverestimateRatio is irrelevant for long halflives, but it's a "nice" to compensate for unusually small halflives, and a compensation of 1.33 isn't too bad.

            var halfLife = TimeSpan.FromSeconds(constantFailureDelayTarget.TotalSeconds * halflifeFactor);
            errorRateEstimator = new ExponentialDecayEstimator(halfLife);
            halflivesPerSecond = 1.0 / errorRateEstimator.halflife.TotalSeconds;

            var compensatedTargetConvergedDelay = constantFailureDelayTarget.TotalSeconds / empiracalConvergenceOverestimateRatio;

            delayTargetInSecondsCubed = compensatedTargetConvergedDelay * compensatedTargetConvergedDelay * compensatedTargetConvergedDelay;
        }

        public void RegisterErrorAt(DateTime errorMoment)
            => errorRateEstimator.AddAmount(errorMoment, 1.0);

        public TimeSpan RetryDelayAt(DateTime moment)
            => ErrorsPerSecondToRetryDelay(errorRateEstimator.EstimatedRateOfChangePerHalflife(moment) * halflivesPerSecond);

        public TimeSpan RegisterErrorAndGetDelay(DateTime errorMoment)
            => ErrorsPerSecondToRetryDelay(errorRateEstimator.AddAmount(errorMoment, 1.0).EstimatedEventCountPerHalflife * halflivesPerSecond);

        public TimeSpan ErrorsPerDayToRetryDelay(double approximateErrorsPerDay)
            => ErrorsPerSecondToRetryDelay(approximateErrorsPerDay / TimeSpan.FromDays(1).TotalSeconds);

        public TimeSpan ErrorsPerSecondToRetryDelay(double approximateErrorsPerSecond)
            => TimeSpan.FromSeconds(approximateErrorsPerSecond * approximateErrorsPerSecond * delayTargetInSecondsCubed);
    }
}
