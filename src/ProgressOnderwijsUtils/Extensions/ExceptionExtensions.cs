using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class ExceptionExtensions
    {
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
        {
            if (e is AggregateException aggregateException) {
                return aggregateException.InnerExceptions.All(IsSqlTimeoutException);
            }

            for (var current = e; current != null; current = current.InnerException) {
                if (current is SqlException sqlE && sqlE.Number == -2) {
                    return true;
                }
            }
            return false;
        }
    }
}
