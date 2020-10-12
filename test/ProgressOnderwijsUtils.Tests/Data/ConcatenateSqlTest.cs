using System;
using System.Linq;
using ExpressionToCodeLib;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests.Data
{
    public sealed class ConcatenateSqlTest
    {
        [Fact]
        public void ConcatenatationOfEmptySequenceIsEmpty()
        {
            PAssert.That(() => Array.Empty<ParameterizedSql>().ConcatenateSql() == ParameterizedSql.Empty);
            PAssert.That(() => Array.Empty<ParameterizedSql>().ConcatenateSql(SQL($"bla")) == ParameterizedSql.Empty);
        }

        [Fact]
        public void ConcatenateWithEmptySeparatorIsStillSpaced()
            => PAssert.That(() => new[] { SQL($"een"), SQL($"twee"), SQL($"drie") }.ConcatenateSql() == SQL($"een twee drie"));

        [Fact]
        public void ConcatenateWithSeparatorUsesSeparatorSpaced()
            => PAssert.That(() => new[] { SQL($"een"), SQL($"twee"), SQL($"drie") }.ConcatenateSql(SQL($"!")) == SQL($"een ! twee ! drie"));

        [Fact]
        public void ConcatenateIsFastEnoughForLargeSequences()
        {
            var someSqls = Enumerable.Range(0, 10000).Select(i => ParameterizedSql.CreateDynamic(i.ToStringInvariant())).ToArray();
            var time = BenchTimer.BestTime(() => {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                someSqls.ConcatenateSql().CommandText();
            }, 5);
            //At 1ns per op (equiv to approx 4 clock cycles), a quadratic implementation would use some multiple of 100 ms.  Even with an extremely low
            //scaling factor, if it's faster than 25ms, it's almost certainly better than quadratic, and in any case fast enough.
            PAssert.That(() => time.TotalMilliseconds < 25);
        }
    }
}
