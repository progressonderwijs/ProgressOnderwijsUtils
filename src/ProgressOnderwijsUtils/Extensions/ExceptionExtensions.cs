using System.Runtime.InteropServices;

namespace ProgressOnderwijsUtils;

public static class ExceptionExtensions
{
    extension(Exception? exception)
    {
        /// <summary>
        /// Tests whether an exception is non-null and matches a predicate, or any of its inner exceptions do.  For AggregateExceptions, tests whether *all* children match.
        /// </summary>
        public bool AnyNestingLevelMatches(Func<Exception, bool> predicate)
        {
            if (exception is null) {
                return false;
            } else if (predicate(exception)) {
                return true;
            } else if (exception is AggregateException aggEx) {
                return aggEx.InnerExceptions.Count > 0 && aggEx.InnerExceptions.All(child => child.AnyNestingLevelMatches(predicate));
            } else {
                return exception.InnerException.AnyNestingLevelMatches(predicate);
            }
        }

        public bool IsSqlTimeoutException()
            => exception.AnyNestingLevelMatches(sqlTimeoutPredicate);

        /// <summary>
        /// Detects a cancellation that was propagated to SQL Server (typically via <see cref="System.Data.Common.DbCommand.Cancel"/>
        /// triggered by a <see cref="CancellationToken"/>). Such cancellations surface as a <see cref="SqlException"/> — not an
        /// <see cref="OperationCanceledException"/> — so <see cref="IsCancellationExceptionOfToken"/> will not catch them.
        /// Matched by error number (3617 "Operation cancelled by user." and 3980) because messages are localized.
        /// </summary>
        public bool IsSqlCancelledException()
            => exception.AnyNestingLevelMatches(sqlCancelledPredicate);

        public bool IsRetriableConnectionFailure()
            => exception.AnyNestingLevelMatches(retriableConnFailurePredicate);

        /// <summary>
        /// Do not catch fatal-exceptions to log them for example.
        /// Unlike AnyNestingLevelMatches, this returns true if *any* child of an AggregateException is fatal.
        /// </summary>
        public bool IsFatalException()
            => exception is OutOfMemoryException or AccessViolationException or SEHException or BadImageFormatException or InvalidProgramException
                || exception is AggregateException aggregated && aggregated.InnerExceptions.Any(inner => inner.IsFatalException())
                || exception?.InnerException.IsFatalException() == true;

        /// <summary>
        /// Check whether the exception is or contains a cancellation of the specified token.
        /// Also returns true when <paramref name="token"/> has been cancelled and the exception is a SQL-side
        /// cancellation (see <see cref="IsSqlCancelledException"/>), because a token-triggered
        /// <see cref="System.Data.Common.DbCommand.Cancel"/> surfaces as a <see cref="SqlException"/> rather than
        /// an <see cref="OperationCanceledException"/>.
        /// </summary>
        public bool IsCancellationExceptionOfToken(CancellationToken token)
            => exception is OperationCanceledException ex && ex.CancellationToken == token
                || exception is AggregateException aggregated && aggregated.InnerExceptions.Any(child => child.IsCancellationExceptionOfToken(token))
                || exception?.InnerException.IsCancellationExceptionOfToken(token) == true
                || token.IsCancellationRequested && exception.IsSqlCancelledException();
    }

    static readonly Func<Exception, bool> retriableConnFailurePredicate = ex =>
        ex is SqlException sqlE && IsRetriableSqlException(sqlE)
        || ex is DBConcurrencyException && ex.Message.StartsWith("Concurrency violation:", StringComparison.Ordinal)
        || ex is DataException && ex.Message == "The underlying provider failed on Open.";

    static readonly Func<Exception, bool> sqlTimeoutPredicate = ex => ex is SqlException { Number: -2, };

    static readonly Func<Exception, bool> sqlCancelledPredicate = ex => ex is SqlException { Number: 3617 or 3980, };

    static bool IsRetriableSqlException(SqlException sqlException)
    { //sqlE.Number docs at https://msdn.microsoft.com/en-us/library/cc645611.aspx
        //see also system error codes: https://msdn.microsoft.com/en-us/library/windows/desktop/ms681382
        const int timeoutExpired = -2;
        const int failedToEstablishConnection = 53;
        const int deadlockVictim = 1205;
        return sqlException.Number == timeoutExpired
            || sqlException.Number == failedToEstablishConnection
            || sqlException.Number == deadlockVictim
            || sqlException.Message.StartsWith("A transport-level error has occurred when receiving results from the server.", StringComparison.Ordinal) //number 121 and possibly others
            || sqlException.Message.StartsWith("A transport-level error has occurred when sending the request to the server.", StringComparison.Ordinal); //number 121 and possibly others
    }
}
