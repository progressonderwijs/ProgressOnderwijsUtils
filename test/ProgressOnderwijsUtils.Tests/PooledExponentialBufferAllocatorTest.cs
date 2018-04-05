using System;
using System.Linq;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    
    public class PooledExponentialBufferAllocatorTest
    {
        [Fact]
        public void AllocatesArray()
        {
            var array = PooledExponentialBufferAllocator<int>.GetByLength(3);
            PAssert.That(() => array.SequenceEqual(Enumerable.Repeat(default(int), 4)));
        }

        [Fact]
        public void TwoAllocationsWithDifferentLengthsWork()
        {
            var array3 = PooledExponentialBufferAllocator<int>.GetByLength(3);
            var array4 = PooledExponentialBufferAllocator<int>.GetByLength(240);
            PAssert.That(() => array3.Length == 4 && array4.Length == 256);
        }

        [Fact]
        public void AllocatingThenReleasingAnArrayDoesNotCrash()
        {
            var array5 = PooledExponentialBufferAllocator<int>.GetByLength(5);
            PooledExponentialBufferAllocator<int>.ReturnToPool(array5);
        }

        [Fact]
        public void AReleasedArrayIsEventuallyReused()
        {
            var array600 = PooledExponentialBufferAllocator<int>.GetByLength(600);
            PooledExponentialBufferAllocator<int>.ReturnToPool(array600);
            for (var i = 0; i < 10000; i++) {
                var secondArray = PooledExponentialBufferAllocator<int>.GetByLength(600);
                if (secondArray == array600) {
                    return;
                }
            }
            throw new Exception("The released array was not reused even after 10000 attempts!");
        }

        [Fact]
        public void ANonReleasedArrayIsNotReused()
        {
            var array6 = PooledExponentialBufferAllocator<int>.GetByLength(6);
            for (var i = 0; i < 10000; i++) {
                var secondArray = PooledExponentialBufferAllocator<int>.GetByLength(6);
                if (secondArray == array6) {
                    throw new Exception("The non-released array was reused!");
                }
            }
        }
    }
}
