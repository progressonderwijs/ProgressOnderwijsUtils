using System;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests;

public sealed class LogBase2Test
{
    [Fact(Skip = "Extremely slow")]
    public void LogBase2RoundedDown_exhaustive_test()
    {
        for (uint i = 1; i < uint.MaxValue; i++) {
            var res = Utils.LogBase2RoundedDown(i);
            if (!(i < 1ul << res + 1)) {
                throw new Exception($"i < (1ul << res + 1) FAILED for i:{i}; res: {res}");
            }
            if (!(1ul << res <= i)) {
                throw new Exception($"(1ul << res) <= i FAILED for i:{i}; res: {res}");
            }
        }
        PAssert.That(() => Utils.LogBase2RoundedDown(0) == 0);
        PAssert.That(() => Utils.LogBase2RoundedDown(uint.MaxValue) == 31);
    }

    [Fact(Skip = "Extremely slow")]
    public void LogBase2RoundedUp_exhaustive_test()
    {
        for (uint i = 2; i < uint.MaxValue; i++) {
            var res = Utils.LogBase2RoundedUp(i);
            if (!(i <= 1ul << res)) {
                throw new Exception($"i <= (1ul << res) FAILED for i:{i}; res: {res}");
            }
            if (!(1ul << res - 1 < i)) {
                throw new Exception($"(1ul << res - 1) < i FAILED for i:{i}; res: {res}");
            }
        }
        PAssert.That(() => Utils.LogBase2RoundedUp(0) == 0);
        PAssert.That(() => Utils.LogBase2RoundedUp(1) == 0);
        PAssert.That(() => Utils.LogBase2RoundedUp(uint.MaxValue) == 32);
    }
}