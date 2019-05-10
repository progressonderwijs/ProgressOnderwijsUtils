using System;
using System.Data.SqlClient;
using System.Linq;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class SqlTimeoutDetection
    {
        [Obsolete("Use the extension method exception.IsSqlTimeoutException() instead")]
        public static bool IsTimeoutException([CanBeNull] Exception e)
            => e.IsSqlTimeoutException();

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
