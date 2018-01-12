using System;
using System.Linq;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    
    public sealed class ConcatenateSqlTest
    {
        [Fact]
        public void ConcatenateWithEmptySeparatorIsStillSpaced()
        {
            PAssert.That(() => new[] { SafeSql.SQL($"een"), SafeSql.SQL($"twee"), SafeSql.SQL($"drie") }.ConcatenateSql() == SafeSql.SQL($"een twee drie"));
        }

        [Fact]
        public void EmptyConcatenateFails()
        {
            Assert.ThrowsAny<Exception>(() => new ParameterizedSql[] { }.ConcatenateSql(SafeSql.SQL($"bla")));
        }

        [Fact]
        public void ConcatenateIsFastEnoughForLargeSequences()
        {
            var someSqls = Enumerable.Range(0, 10000).Select(i => ParameterizedSql.CreateDynamic(i.ToStringInvariant())).ToArray();
            var time = BenchTimer.BestTime(() => {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                someSqls.ConcatenateSql().CommandText();
            }, 5);
            //At 1ns per op (equiv to approx 4 clock cycles), a quadratic implementation would use some multiple of 100 ms.  Even with an extremely low 
            //scaling factor, if it's faster than 5ms, it's almost certainly better than quadratic, and in any case fast enough.
            PAssert.That(() => time.TotalMilliseconds < 5.0);
        }

        [Fact]
        public void ConcatenateWithSeparatorUsesSeparatorSpaced()
        {
            PAssert.That(() => new[] { SafeSql.SQL($"een"), SafeSql.SQL($"twee"), SafeSql.SQL($"drie") }.ConcatenateSql(SafeSql.SQL($"!")) == SafeSql.SQL($"een ! twee ! drie"));
        }
    }
}
