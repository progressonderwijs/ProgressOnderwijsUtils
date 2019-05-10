using System;
using System.Data.SqlClient;
using System.Linq;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Extensions;

namespace ProgressOnderwijsUtils
{
    public static class SqlTimeoutDetection
    {
        [Obsolete("Use the extension method exception.IsSqlTimeoutException() instead")]
        public static bool IsTimeoutException([CanBeNull] Exception e)
            => e.IsSqlTimeoutException();
    }
}
