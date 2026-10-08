namespace ProgressOnderwijsUtils.Tests.Collections;

public sealed class SequenceEqualityComparerTest
{
    static readonly SequenceEqualityComparer<int> defaultEq = SequenceEqualityComparer<int>.Default;
    static readonly SequenceEqualityComparer<int> nullIsEmptyEq = defaultEq with { NullCountsAsEmpty = true, };

    static void AssertEquality(SequenceEqualityComparer<int> eq, int[]? a, int[]? b, bool shouldBeEqual)
    {
        var aSeq = a?.AsEnumerable();
        var bSeq = b?.AsEnumerable();
        PAssert.That(() => eq.Equals(a, b) == shouldBeEqual);
        PAssert.That(() => eq.Equals(aSeq, bSeq) == shouldBeEqual);
        PAssert.That(() => eq.GetHashCode(a) == eq.GetHashCode(b) == shouldBeEqual);
        PAssert.That(() => eq.GetHashCode(aSeq) == eq.GetHashCode(bSeq) == shouldBeEqual);
    }

    [Fact]
    public void Null_vs_Empty()
    {
        AssertEquality(defaultEq, null, [], false);
        AssertEquality(defaultEq, [], null, false);
        AssertEquality(defaultEq, null, null, true);
        AssertEquality(defaultEq, [], [], true);

        AssertEquality(nullIsEmptyEq, null, [], true);
        AssertEquality(nullIsEmptyEq, [], null, true);
        AssertEquality(nullIsEmptyEq, null, null, true);
        AssertEquality(nullIsEmptyEq, [], [], true);
    }

    [Fact]
    public void SingleElementArraysCompare()
    {
        AssertEquality(defaultEq, [1,], [2,], false);
        AssertEquality(defaultEq, [2,], [2,], true);
        var sharedRef = new[] { int.MinValue, };
        AssertEquality(defaultEq, sharedRef, sharedRef, true);
    }

    [Fact]
    public void AdditionalElementsArentRelevant()
    {
        AssertEquality(defaultEq, [1,], [1,], true);
        AssertEquality(defaultEq, [1, 100,], [1, 0,], false);
        AssertEquality(defaultEq, [1, 100,], [1, 100,], true);
        AssertEquality(defaultEq, [1, 100,], [0, 100,], false);
    }

    [Fact]
    public void LengthDifferencesMatter()
    {
        AssertEquality(defaultEq, [1, 2, 3,], [1, 2, 3, 4,], false);
        AssertEquality(defaultEq, [1, 2, 3, 4,], [1, 2, 3,], false);
    }
}
