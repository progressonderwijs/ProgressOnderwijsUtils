using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils.Extensions
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
    }
}
