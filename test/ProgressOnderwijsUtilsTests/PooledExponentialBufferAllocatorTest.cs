using System;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    class PooledExponentialBufferAllocatorTest
    {
        [Test]
        public void AllocatesArray()
        {
            var array = PooledExponentialBufferAllocator<int>.GetByLength(3);
            PAssert.That(() => array.SequenceEqual(Enumerable.Repeat(default(int), 4)));
        }

        [Test]
        public void TwoAllocationsWithDifferentLengthsWork()
        {
            var array3 = PooledExponentialBufferAllocator<int>.GetByLength(3);
            var array4 = PooledExponentialBufferAllocator<int>.GetByLength(240);
            PAssert.That(() => array3.Length == 4 && array4.Length == 256);
        }

        [Test]
        public void AllocatingThenReleasingAnArrayDoesNotCrash()
        {
            var array5 = PooledExponentialBufferAllocator<int>.GetByLength(5);
            PooledExponentialBufferAllocator<int>.ReturnToPool(array5);
        }

        [Test]
        public void AReleasedArrayIsEventuallyReused()
        {
            var array600 = PooledExponentialBufferAllocator<int>.GetByLength(600);
            PooledExponentialBufferAllocator<int>.ReturnToPool(array600);
            for (int i = 0; i < 10000; i++) {
                var secondArray = PooledExponentialBufferAllocator<int>.GetByLength(600);
                if (secondArray == array600) {
                    return;
                }
            }
            throw new Exception("The released array was not reused even after 10000 attempts!");
        }
        [Test]
        public void ANonReleasedArrayIsNotReused()
        {
            var array6 = PooledExponentialBufferAllocator<int>.GetByLength(6);
            for (int i = 0; i < 10000; i++) {
                var secondArray = PooledExponentialBufferAllocator<int>.GetByLength(6);
                if (secondArray == array6) {
                    throw new Exception("The non-released array was reused!");
                }
            }
        }
    }
}
