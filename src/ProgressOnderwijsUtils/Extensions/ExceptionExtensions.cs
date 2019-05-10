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
        /// Tests whether a exception is non-null and matches a predicate, or any of its inner exceptions are non-null and match a predicate.  For AggregateExceptions, tests wether *all* children match.
        /// </summary>
        public static bool AnyNestingLevelMatches(this Exception exception, Func<Exception, bool> predicate)
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
            => e.AnyNestingLevelMatches(ex => {
                if (e == null) {
                    return false;
                } else if (e is SqlException sqlE) {
                    //sqlE.Number docs at https://msdn.microsoft.com/en-us/library/cc645611.aspx
                    //see also system error codes: https://msdn.microsoft.com/en-us/library/windows/desktop/ms681382
                    const int timeoutExpired = -2;
                    const int failedToEstablishConnection = 53;
                    const int deadlockVictim = 1205;
                    return sqlE.Number == timeoutExpired
                        || sqlE.Number == failedToEstablishConnection
                        || sqlE.Number == deadlockVictim
                        || e.Message.StartsWith("A transport-level error has occurred when receiving results from the server.", StringComparison.Ordinal) //number 121 and possibly others
                        || e.Message.StartsWith("A transport-level error has occurred when sending the request to the server.", StringComparison.Ordinal); //number 121 and possibly others
                } else if (e is DBConcurrencyException) {
                    return e.Message.StartsWith("Concurrency violation:", StringComparison.Ordinal);
                } else if (e is DataException) {
                    return e.Message == "The underlying provider failed on Open.";
                } else {
                    return false;
                }
            });
    }
}
