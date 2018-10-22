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
        readonly double halflivesPerDay;
        readonly double scaleFactor;

        public RetryDelayChooser(TimeSpan constantFailureDelayTarget)
        {
            errorRateEstimator = new ExponentialDecayEstimator(TimeSpan.FromHours(12));
            halflivesPerDay = 1.0 / errorRateEstimator.halflife.TotalDays;

            var seconds_per_targetConvergedDelays = constantFailureDelayTarget.TotalSeconds;
            
            scaleFactor = constantFailureDelayTarget.TotalSeconds * (seconds_per_targetConvergedDelays * seconds_per_targetConvergedDelays);
        }

        public void RegisterErrorAt(DateTime errorMoment)
            => errorRateEstimator.AddAmount(errorMoment, 1.0);

        public TimeSpan RetryDelayAt(DateTime moment)
            => ErrorsPerDayToRetryDelay(errorRateEstimator.EstimatedRateOfChangePerHalflife(moment) * halflivesPerDay);

        public TimeSpan RegisterErrorAndGetDelay(DateTime errorMoment) 
            => ErrorsPerDayToRetryDelay(errorRateEstimator.AddAmount(errorMoment, 1.0).EstimatedEventCountPerHalflife * halflivesPerDay);

        public TimeSpan ErrorsPerDayToRetryDelay(double approximateErrorsPerDay)
            => TimeSpan.FromSeconds(approximateErrorsPerDay /TimeSpan.FromDays(1).TotalSeconds * approximateErrorsPerDay /TimeSpan.FromDays(1).TotalSeconds * scaleFactor);
    }
}
