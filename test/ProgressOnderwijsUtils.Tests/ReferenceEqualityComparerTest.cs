namespace ProgressOnderwijsUtils.Tests;

public sealed class ReferenceEqualityComparerTest
{
    record struct TestType(int value);

    static readonly TestType t1 = new(1);
    static readonly TestType t2 = new(1);

    [Fact]
    public void TestValue()
    {
        var sut = new HashSet<TestType>();
        Assert.True(sut.Add(t1));
        Assert.True(!sut.Add(t2));
    }

    [Fact]
    public void TestReference()
    {
        var sut = new HashSet<TestType>(new ReferenceEqualityComparer<TestType>());
        Assert.True(sut.Add(t1));
        Assert.True(sut.Add(t2));
    }
}
