using System;
using System.Data.SqlClient;
using System.Linq;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Tests wether an exception, or any of its inner exceptions match a predicate.  For AggregateExceptions, tests wether *all* children match.
        /// </summary>
        public static bool AnyNestingLevelMatches(this Exception exception, Func<Exception, bool> predicate)
        {
            if (predicate(exception)) {
                return true;
            } else if (exception is AggregateException aggEx) {
                return aggEx.InnerExceptions.Count > 0 && aggEx.InnerExceptions.All(child => AnyNestingLevelMatches(child, predicate));
            } else {
                return exception.InnerException is Exception child && AnyNestingLevelMatches(child, predicate);
            }
        }

        public static bool IsSqlTimeoutException([CanBeNull] this Exception e)
            => e.AnyNestingLevelMatches(ex => ex is SqlException sqlE && sqlE.Number == -2);
    }
}
