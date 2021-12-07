using System.Collections.Generic;
using JetBrains.Annotations;
using Xunit;

namespace ProgressOnderwijsUtils.Tests;

public sealed class ReferenceEqualityComparerTest
{
    struct TestType
    {
        [UsedImplicitly] //for equality
        int value;

        public TestType(int value)
        {
            this.value = value;
        }
    }

    static readonly TestType t1 = new TestType(1);
    static readonly TestType t2 = new TestType(1);

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