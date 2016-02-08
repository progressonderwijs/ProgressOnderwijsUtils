using System;
using JetBrains.Annotations;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils
{
    public static class ParameterizedSqlExtensions
    {
        static readonly ParameterizedSql newline = SQL($"\r\n");

        [Pure]
        public static ParameterizedSql Append(this ParameterizedSql source, ParameterizedSql extra)
        {
            return source + newline + extra;
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
