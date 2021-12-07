using System;
using System.Linq;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests;

public sealed class PooledSmallBufferAllocatorTest
{
    [Fact]
    public void AllocatesArray()
    {
        var array = PooledSmallBufferAllocator<object>.GetByLength(3);
        PAssert.That(() => array.SequenceEqual(Enumerable.Repeat(default(object), 3)));
    }

    [Fact]
    public void TwoAllocationsWithDifferentLengthsWork()
    {
        var array3 = PooledSmallBufferAllocator<object>.GetByLength(3);
        var array4 = PooledSmallBufferAllocator<object>.GetByLength(4);
        PAssert.That(() => array3.Length == 3 && array4.Length == 4);
    }

    [Fact]
    public void AllocatingThenReleasingAnArrayDoesNotCrash()
    {
        var array5 = PooledSmallBufferAllocator<object>.GetByLength(5);
        PooledSmallBufferAllocator<object>.ReturnToPool(array5);
    }

    [Fact]
    public void AReleasedArrayIsEventuallyReused()
    {
        var array6 = PooledSmallBufferAllocator<object>.GetByLength(6);
        PooledSmallBufferAllocator<object>.ReturnToPool(array6);
        for (var i = 0; i < 10000; i++) {
            var secondArray = PooledSmallBufferAllocator<object>.GetByLength(6);
            if (secondArray == array6) {
                return;
            }
        }
        throw new Exception("The released array was not reused even after 10000 attempts!");
    }

    [Fact]
    public void ANonReleasedArrayIsNotReused()
    {
        var array6 = PooledSmallBufferAllocator<object>.GetByLength(6);
        for (var i = 0; i < 10000; i++) {
            var secondArray = PooledSmallBufferAllocator<object>.GetByLength(6);
            if (secondArray == array6) {
                throw new Exception("The non-released array was reused!");
            }
        }
    }
}