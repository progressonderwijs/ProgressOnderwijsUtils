using System;
using System.Text;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests;

public sealed class NullableReferenceTypesHelpersTest
{
    [Fact]
    public void AssertNotNull_class_throws_when_argument_is_null()
    {
        var exception = Assert.ThrowsAny<Exception>(() => default(StringBuilder).AssertNotNull());
        PAssert.That(() => exception.Message.Contains(nameof(StringBuilder)));
    }

    [Fact]
    public void AssertNotNull_struct_throws_when_argument_is_null()
    {
        var exception = Assert.ThrowsAny<Exception>(() => default(DayOfWeek?).AssertNotNull());
        PAssert.That(() => exception.Message.Contains(nameof(DayOfWeek)));
    }

    [Fact]
    public void AssertNotNull_doesnt_throw_when_argument_is_not_null()
    {
        var unused = new object().AssertNotNull();
    }

    [Fact]
    public void PretendNullable_does_nothing()
    {
        var obj = new object();
        PAssert.That(() => obj.PretendNullable() == obj);
    }
}