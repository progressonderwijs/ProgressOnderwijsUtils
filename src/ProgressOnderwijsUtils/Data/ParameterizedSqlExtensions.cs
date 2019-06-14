﻿using System.Collections.Generic;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
    public static class ParameterizedSqlExtensions
    {
        [Pure]
        public static TSelf WithoutTimeout<TSelf>(this IWithTimeout<TSelf> batch)
            where TSelf : IWithTimeout<TSelf>
            => batch.WithTimeout(CommandTimeout.WithoutTimeout);

        [Pure]
        public static TSelf WithTimeout<TSelf>(this IWithTimeout<TSelf> batch, int scaledTimeoutInS)
            where TSelf : IWithTimeout<TSelf>
            => batch.WithTimeout(CommandTimeout.ScaledSeconds(scaledTimeoutInS));


        [Pure]
        public static TSelf WithNonScaledTimeout<TSelf>(this IWithTimeout<TSelf> batch, int timeoutInAbsoluteS)
            where TSelf : IWithTimeout<TSelf>
            => batch.WithTimeout(CommandTimeout.AbsoluteSeconds(timeoutInAbsoluteS));

        [Pure]
        public static ParameterizedSql Append(this ParameterizedSql source, ParameterizedSql extra)
            => source + extra;

        /// <summary>
        /// Concatenate a sequence of sql expressions with a space separator.
        /// e.g.  concatenating 'a' and 'b' results in 'a b'
        /// </summary>
        public static ParameterizedSql ConcatenateSql([NotNull] this IEnumerable<ParameterizedSql> sqlExpressions)
            => ConcatenateSql(sqlExpressions, ParameterizedSql.Empty);

        /// <summary>
        /// Concatenate a sequence of sql expressions with a separator (surrounded by space).  A sequence of N items includes the separator N-1 times.
        /// e.g.  concatenating 'a' and 'b' with separator 'X' results in 'a X b'
        /// </summary>
        public static ParameterizedSql ConcatenateSql([NotNull] this IEnumerable<ParameterizedSql> sqlExpressions, ParameterizedSql separator)
        {
            var builder = new ArrayBuilder<ISqlComponent>();
            var isBuilderEmpty = true;
            foreach (var expr in sqlExpressions) {
                if (expr.IsEmpty) {
                    continue;
                }

                if (isBuilderEmpty) {
                    isBuilderEmpty = false;
                } else if (!separator.IsEmpty) {
                    builder.Add(separator.impl);
                }
                builder.Add(expr.impl);
            }
            return new SeveralSqlFragments(builder.ToArray()).BuildableToQuery();
        }
    }
}
