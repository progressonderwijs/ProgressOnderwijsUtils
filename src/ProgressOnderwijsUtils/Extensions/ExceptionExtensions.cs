using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Tests whether an exception is non-null and matches a predicate, or any of its inner exceptions do.  For AggregateExceptions, tests wether *all* children match.
        /// </summary>
        public static bool AnyNestingLevelMatches([CanBeNull] this Exception exception, [NotNull] Func<Exception, bool> predicate)
        {
            if (exception == null) {
                return false;
            } else if (predicate(exception)) {
                return true;
            } else if (exception is AggregateException aggEx) {
                return aggEx.InnerExceptions.Count > 0 && aggEx.InnerExceptions.All(child => AnyNestingLevelMatches(child, predicate));
            } else {
                return AnyNestingLevelMatches(exception.InnerException, predicate);
            }
        }

        public static bool IsSqlTimeoutException([CanBeNull] this Exception e)
            => e.AnyNestingLevelMatches(ex => ex is SqlException sqlE && sqlE.Number == -2);

        public static bool IsRetriableConnectionFailure([CanBeNull] this Exception e)
            => e.AnyNestingLevelMatches(ex =>
                e is SqlException sqlE && IsRetriableSqlException(sqlE)
                || e is DBConcurrencyException && e.Message.StartsWith("Concurrency violation:", StringComparison.Ordinal)
                || e is DataException && e.Message == "The underlying provider failed on Open."
            );

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
}
