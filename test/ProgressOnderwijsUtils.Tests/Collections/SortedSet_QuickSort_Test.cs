using System;
using System.Linq;
using ExpressionToCodeLib;
using Xunit;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils.Tests.Collections
{
    public sealed class SortedSet_QuickSort_Test
    {
        static bool ArrayEquals(int[] a, int[] b) => ArrayComparer<int>.Default.Equals(a, b);

        [Fact]
        public void SortedEmpyArrayIsEmtpy()
        {
            var array = new int[0];
            SortedSet<int, IntOrdering>.Algorithms.Sort(array);
            PAssert.That(() => ArrayEquals(array, Array.Empty<int>()));
        }

        [Fact]
        public void SingleElementArrayRetainsElement()
        {
            var array = new[] { 1337 };
            SortedSet<int, IntOrdering>.Algorithms.Sort(array);
            PAssert.That(() => ArrayEquals(array, new[] { 1337 }));
        }

        [Fact]
        public void TwoElementsOutOfOrderAreSorted()
        {
            var array = new[] { 1337, 37 };
            SortedSet<int, IntOrdering>.Algorithms.Sort(array);
            PAssert.That(() => ArrayEquals(array, new[] { 37, 1337 }));
        }

        [Fact]
        public void TwoElementsInOrderRemainSorted()
        {
            var array = new[] { 4, 5 };
            SortedSet<int, IntOrdering>.Algorithms.Sort(array);
            PAssert.That(() => ArrayEquals(array, new[] { 4, 5 }));
        }

        [Fact]
        public void CountDownBecomesCountUp()
        {
            var array = new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
            SortedSet<int, IntOrdering>.Algorithms.Sort(array);
            PAssert.That(() => ArrayEquals(array, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }));
        }

        [Fact]
        public void ExampleWithLargeNumbersSorts()
        {
            var array = new[] { 302596119, 269548474, 1122627734, 361709742, 563913476, 1555655117 };
            var expected = array.OrderBy(n => n).ToArray();
            SortedSet<int, IntOrdering>.Algorithms.Sort(array);
            PAssert.That(() => ArrayEquals(array, expected));
        }

        [Fact]
        public void ABunchOfCases()
        {
            var r = new Random(37);
            for (int i = 0; i < 1000; i++) {
                var array = MoreLinq.MoreEnumerable.GenerateByIndex(_ => r.Next(200) - r.Next(200)).Take(r.Next(100) + 1).ToArray();
                var correctOrdering = array.OrderBy(n => n).ToArray();
                SortedSet<int, IntOrdering>.Algorithms.Sort(array);
                PAssert.That(() => ArrayEquals(array, correctOrdering));
            }
        }
    }
}
