using System;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class ParameterizedSqlExtensions
    {
        [Pure]
        public static ParameterizedSql Append(this ParameterizedSql source, ParameterizedSql extra)
        {
            return source + extra;
        }

        [Pure]
        public static ParameterizedSql AppendIf(this ParameterizedSql source, bool condition, ParameterizedSql extra)
        {
            return condition ? source.Append(extra) : source;
        }

        [Pure, UsefulToKeep("Library function, other overloads used")]
        public static ParameterizedSql AppendIf(this ParameterizedSql source, bool condition, Func<ParameterizedSql> extra)
        {
            return condition ? source.Append(extra()) : source;
        }
    }
}
