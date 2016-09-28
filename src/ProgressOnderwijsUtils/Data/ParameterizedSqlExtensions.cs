using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using NUnit.Framework;
using static ProgressOnderwijsUtils.SafeSql;

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

        /// <summary>
        /// Concatenate a sequence of sql expressions with a space separator.
        /// e.g.  concatenating 'a' and 'b' results in 'a b'
        /// </summary>
        public static ParameterizedSql ConcatenateSql(this IEnumerable<ParameterizedSql> sqlExpressions) => ConcatenateSql(sqlExpressions, ParameterizedSql.Empty);

        /// <summary>
        /// Concatenate a sequence of sql expressions with a separator (surrounded by space).  A sequence of N items includes the separator N-1 times.
        /// e.g.  concatenating 'a' and 'b' with separator 'X' results in 'a X b'
        /// </summary>
        public static ParameterizedSql ConcatenateSql(this IEnumerable<ParameterizedSql> sqlExpressions, ParameterizedSql separator) => sqlExpressions.Aggregate((a, b) => a.Append(separator).Append(b));
    }

    public sealed class ConcatenateSqlTest
    {
        [Test]
        public void ConcatenateWithEmptySeparatorIsStillSpaced()
        {
            PAssert.That(() => new[] { SQL($"een"), SQL($"twee"), SQL($"drie") }.ConcatenateSql() == SQL($"een twee drie"));
        }

        [Test]
        public void EmptyConcatenateFails()
        {
            Assert.Catch(() => new ParameterizedSql[] { }.ConcatenateSql(SQL($"bla")));
        }

        [Test]
        public void ConcatenateIsFastEnoughForLargeSequences()
        {
            var someSqls = Enumerable.Range(0, 20000).Select(i => ParameterizedSql.CreateDynamic(i.ToStringInvariant())).ToArray();
            var time = BenchTimer.BestTime(() => {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                someSqls.ConcatenateSql().CommandText();
            }, 5); 
            //At 1ns per op (equiv to approx 4 clock cycles), a quadratic implementation would use some multiple of 400 ms.  Even with an extremely low 
            //scaling factor, if it's faster than 5ms, it's almost certainly better than quadratic, and in any case fast enough.
            PAssert.That(() => time.TotalMilliseconds < 5.0);
        }

        [Test]
        public void ConcatenateWithSeparatorUsesSeparatorSpaced()
        {
            PAssert.That(() => new[] { SQL($"een"), SQL($"twee"), SQL($"drie") }.ConcatenateSql(SQL($"!")) == SQL($"een ! twee ! drie"));
        }
    }
}
