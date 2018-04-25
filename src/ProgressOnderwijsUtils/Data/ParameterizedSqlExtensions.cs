using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class ParameterizedSqlExtensions
    {
        [Pure]
        public static ParameterizedSql Append(this ParameterizedSql source, ParameterizedSql extra)
            => source + extra;

        /// <summary>
        /// Concatenate a sequence of sql expressions with a space separator.
        /// e.g.  concatenating 'a' and 'b' results in 'a b'
        /// </summary>
        public static ParameterizedSql ConcatenateSql([NotNull] this IEnumerable<ParameterizedSql> sqlExpressions) => ConcatenateSql(sqlExpressions, ParameterizedSql.Empty);

        /// <summary>
        /// Concatenate a sequence of sql expressions with a separator (surrounded by space).  A sequence of N items includes the separator N-1 times.
        /// e.g.  concatenating 'a' and 'b' with separator 'X' results in 'a X b'
        /// </summary>
        public static ParameterizedSql ConcatenateSql([NotNull] this IEnumerable<ParameterizedSql> sqlExpressions, ParameterizedSql separator) => sqlExpressions.Aggregate((a, b) => a.Append(separator).Append(b));
    }
}
