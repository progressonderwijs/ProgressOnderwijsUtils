namespace ProgressOnderwijsUtils.Metrics;

static class MetricsSampler
{
    /// <summary>
    /// An exception on a thread of our own brings down the whole process, and a measurement is not worth that. That
    /// is why sampling stops on an exception and reports it: the windows after that then have no sampled levels
    /// anymore, which shows as null. Carrying on would report the same exception four times per second.
    /// </summary>
    public static void SampleUntilStopped(ManualResetEventSlim stop, TimeSpan interval, Action sample, Action<Exception> samplingFailed)
    {
        try {
            while (!stop.Wait(interval)) {
                sample();
            }
        } catch (Exception exception) {
            samplingFailed(exception);
        }
    }
}
