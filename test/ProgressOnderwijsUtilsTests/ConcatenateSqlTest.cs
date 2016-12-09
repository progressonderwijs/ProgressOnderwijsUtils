using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business.Test;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    [PullRequestTest]
    public sealed class ConcatenateSqlTest
    {
        [Test]
        public void ConcatenateWithEmptySeparatorIsStillSpaced()
        {
            PAssert.That(() => new[] { SafeSql.SQL($"een"), SafeSql.SQL($"twee"), SafeSql.SQL($"drie") }.ConcatenateSql() == SafeSql.SQL($"een twee drie"));
        }

        [Test]
        public void EmptyConcatenateFails()
        {
            Assert.Catch(() => new ParameterizedSql[] { }.ConcatenateSql(SafeSql.SQL($"bla")));
        }

        [Test]
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

        [Test]
        public void ConcatenateWithSeparatorUsesSeparatorSpaced()
        {
            PAssert.That(() => new[] { SafeSql.SQL($"een"), SafeSql.SQL($"twee"), SafeSql.SQL($"drie") }.ConcatenateSql(SafeSql.SQL($"!")) == SafeSql.SQL($"een ! twee ! drie"));
        }
    }
}
