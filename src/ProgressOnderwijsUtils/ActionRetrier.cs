using System;
using System.Diagnostics;
using System.Threading;

namespace ProgressOnderwijsUtils
{
    public sealed record ActionRetrier(Func<Exception, int, TimeSpan?> retryDelayChooser)
    {
        //Indien hier ooit meer complexiteit in komt, evt. https://github.com/App-vNext/Polly overwegen
        //de polly api ziet er best redelijk uit, en het heeft handige HttpClient(Factory) integratie

        public ActionRetrier(RetryDelayChooser retryDelayChooser, int maxNumberOfAttempts, Func<Exception, bool> shouldRetryThisException)
            : this((ex, attempt) => attempt < maxNumberOfAttempts && shouldRetryThisException(ex) ? retryDelayChooser.GetDelayForErrorThatJustHappened_ThreadSafe() : null)
        {
            if (maxNumberOfAttempts < 2) {
                throw new ArgumentOutOfRangeException(nameof(maxNumberOfAttempts), "must be at least 2");
            }
        }

        public Action<Exception, int, TimeSpan>? RetriableFailureLogger { get; init; }

        public T ExecuteWithRetries<T>(Func<T> action, CancellationToken retryCancellation)
        {
            var attempt = 0;
            var sw = Stopwatch.StartNew();
            while (true) {
                try {
                    return action();
                } catch (Exception ex) when (!retryCancellation.IsCancellationRequested) {
                    if (retryDelayChooser(ex, ++attempt) is { } retryAfter) {
                        RetriableFailureLogger?.Invoke(ex, attempt, retryAfter);
                        _ = retryCancellation.WaitHandle.WaitOne(retryAfter);
                    } else {
                        throw new($"Failed {attempt} attempts.\r\nGiving up after {sw.Elapsed} elapsed time.\r\nLast exception included as inner exception", ex);
                    }
                }
            }
        }
    }
}
