using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using MoreLinq;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtilsBenchmarks.OrderingBenchmarks
{
    struct IntOrdering : IOrdering<int>
    {
        public bool LessThan(int a, int b) => a < b;
        public bool Equal(int a, int b) => a == b;
    }

    public class SortedSetBench
    {
        const int MaximumIndividualSetSize = 10;
        const int MaximumValue = 1234567890;
        static readonly int NumberOfSets = (int)(2_000_000 / (Math.Log(MaximumIndividualSetSize) * MaximumIndividualSetSize));
        static readonly IReadOnlyList<int>[] arrays;
        static readonly int[][] sortedArrays;

        static SortedSetBench()
        {
            var r = new Random(42);

            arrays =
                MoreEnumerable.GenerateByIndex(_ => r.Next(MaximumIndividualSetSize + 1)).Take(NumberOfSets)
                    .Select(len => (IReadOnlyList<int>)MoreEnumerable.GenerateByIndex(_ => r.Next(MaximumValue)).Take(len).ToArray())
                    .ToArray();
            sortedArrays = arrays.ArraySelect(arr => arr.OrderBy(n => n).ToArray());
        }

        [Benchmark]
        public void JustCopy()
        {
            foreach (var arr in sortedArrays) {
                var copy = arr.ToArray();
            }
        }

        [Benchmark]
        public void QuickSort()
        {
            foreach (var arr in arrays) {
                var copy = arr.ToArray();
                SortedSet<int, IntOrdering>.Algorithms.QuickSort(copy);
            }
        }

        [Benchmark]
        public void QuickSort_Sorted()
        {
            foreach (var arr in sortedArrays) {
                var copy = arr.ToArray();
                SortedSet<int, IntOrdering>.Algorithms.QuickSort(copy);
            }
        }

        [Benchmark]
        public void MergeSort()
        {
            foreach (var arr in arrays) {
                var copy = arr.ToArray();
                SortedSet<int, IntOrdering>.Algorithms.MergeSort(copy);
            }
        }

        [Benchmark]
        public void MergeSort_Sorted()
        {
            foreach (var arr in sortedArrays) {
                var copy = arr.ToArray();
                SortedSet<int, IntOrdering>.Algorithms.MergeSort(copy);
            }
        }

        [Benchmark]
        public void SystemArraySort()
        {
            foreach (var arr in arrays) {
                var copy = arr.ToArray();
                Array.Sort(copy);
            }
        }

        [Benchmark]
        public void SystemArraySort_Sorted()
        {
            foreach (var arr in sortedArrays) {
                var copy = arr.ToArray();
                Array.Sort(copy);
            }
        }

        [Benchmark]
        public void LinqOrderBy()
        {
            foreach (var arr in arrays) {
                var copy = arr.OrderBy(i => i).ToArray();
            }
        }

        [Benchmark]
        public void LinqOrderBy_Sorted()
        {
            foreach (var arr in sortedArrays) {
                var copy = arr.OrderBy(i => i).ToArray();
            }
        }
    }
}
