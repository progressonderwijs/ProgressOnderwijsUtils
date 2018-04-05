using System;
using System.Data.SqlClient;
using System.Linq;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class SqlTimeoutDetection
    {
        public static bool IsTimeoutException([CanBeNull] Exception e)
        {
            if (e is AggregateException aggregateException) {
                return aggregateException.InnerExceptions.All(IsTimeoutException);
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
