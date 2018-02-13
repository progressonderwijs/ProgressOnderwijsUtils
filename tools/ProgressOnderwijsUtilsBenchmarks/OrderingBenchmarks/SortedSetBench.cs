#define smallset

using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Running;
using IncrementalMeanVarianceAccumulator;
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

    [InProcess]
    public class SortedSetBench
    {
        const int MaximumIndividualSetSize = 1_000;
        const int MaximumValue = 1234567890;
        const int NumberOfSets = 1_000;
        static readonly IReadOnlyList<int>[] arrays;
        static readonly int[][] sortedArrays;

        static SortedSetBench()
        {
            var r = new Random(42);

            arrays =
                MoreEnumerable.GenerateByIndex(_ => (int)(Math.Exp(r.NextDouble() * Math.Log(MaximumIndividualSetSize)) + 0.5)).Take(NumberOfSets)
                    .Select(len => (IReadOnlyList<int>)MoreEnumerable.GenerateByIndex(_ => r.Next(MaximumValue)).Take(len).ToArray())
                    .ToArray();
            sortedArrays = arrays.ArraySelect(arr => arr.OrderBy(n => n).ToArray());
        }

        public static void ReportDistributionAndRun()
        {
            var lengths = arrays.ArraySelect(arr => (double)arr.Count);
            Array.Sort(lengths);
            var lengthDistribution = MeanVarianceAccumulator.FromSequence(lengths);
            var mean = lengthDistribution.Mean;
            var stddev = lengthDistribution.SampleStandardDeviation;
            var count = lengthDistribution.WeightSum;
            var sum = mean * count;
            var median = (lengths[lengths.Length / 2] + lengths[(lengths.Length + 1) / 2]) / 2;
            var halfOfBytesUptoLength = lengths.Scan((a, b) => a + b).TakeWhile(totalBytes => totalBytes <= sum / 2.0).Count();
            var arrsAboveMean = lengths.Count(len => len > mean);
            var distribSummary =
                lengths.GroupBy(len => (int)Math.Log(len + 1, 8.0))
                    .Select(g => $"    [{g.Min()} - {g.Max()}]: {g.Count()} arrays")
                    .JoinStrings("\r\n");

            Console.Error.WriteLine($"Array distribution statistics: ");
            Console.Error.WriteLine($"    {lengths.Length} arrays");
            Console.Error.WriteLine($"    {mean:f1} ~ {stddev:f1} bytes per array (mean/stddev)");
            Console.Error.WriteLine($"    {median} median bytes per array");
            Console.Error.WriteLine($"    {1 - halfOfBytesUptoLength / count:f3} of the arrays provide half the bytes");
            Console.Error.WriteLine($"    {arrsAboveMean / count:f3} of arrays are above average length");
            Console.Error.WriteLine(distribSummary);
            BenchmarkRunner.Run<SortedSetBench>();
        }

#if !smallset
        [Benchmark]
        public void JustCopy()
        {
            foreach (var arr in sortedArrays) {
                var copy = arr.ToArray();
            }
        }
#endif


        [Benchmark]
        public void BottomUpMergeSort()
        {
            foreach (var arr in arrays) {
                var copy = arr.ToArray();
                SortedSet<int, IntOrdering>.Algorithms.BottomUpMergeSort(copy);
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
        public void MergeSort()
        {
            foreach (var arr in arrays) {
                var copy = arr.ToArray();
                SortedSet<int, IntOrdering>.Algorithms.MergeSort(copy);
            }
        }

#if !smallset
        [Benchmark]
        public void SystemArraySort()
        {
            foreach (var arr in arrays) {
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
        public void QuickSort_Sorted()
        {
            foreach (var arr in sortedArrays) {
                var copy = arr.ToArray();
                SortedSet<int, IntOrdering>.Algorithms.QuickSort(copy);
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
        public void BottomUpMergeSort_Sorted()
        {
            foreach (var arr in sortedArrays) {
                var copy = arr.ToArray();
                SortedSet<int, IntOrdering>.Algorithms.BottomUpMergeSort(copy);
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
        public void LinqOrderBy_Sorted()
        {
            foreach (var arr in sortedArrays) {
                var copy = arr.OrderBy(i => i).ToArray();
            }
        }
#endif
    }
}
