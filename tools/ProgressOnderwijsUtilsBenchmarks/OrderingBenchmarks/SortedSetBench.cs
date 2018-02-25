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

    //[InProcess]
    public class SortedSetBench
    {
        const int MaximumIndividualSetSize = 300_000_000;
        const int MaximumValue = 1234567890;
        const int NumberOfSets = 2;
        static readonly int[][] arrays;
        static readonly int[][] sortedArrays;
        int[] _copy;

        static SortedSetBench()
        {
            var r = new Random(42);

            arrays =
                MoreEnumerable.GenerateByIndex(_ => (int)(Math.Exp(r.NextDouble() * Math.Log(MaximumIndividualSetSize)) + 0.5)).Take(NumberOfSets)
                    .Select(len => MoreEnumerable.GenerateByIndex(_ => r.Next(MaximumValue)).Take(len).ToArray())
                    .ToArray();
            sortedArrays = arrays.ArraySelect(arr => {
                var copy = arr.ToArray();
                Array.Sort(copy);
                return copy;
            });
        }

        public static void ReportDistributionAndRun()
        {
            var lengths = arrays.ArraySelect(arr => (double)arr.Length);
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


        //[Benchmark]
        //public void Insertion_Copy()
        //{
        //    var dummy = new int[64];
        //    foreach (var arr in arrays) {
        //        SortedSet<int, IntOrdering>.Algorithms.InsertionSort_Copy(arr,0,Math.Min(64,arr.Length),dummy);
        //    }
        //}

        //[Benchmark]
        //public void Insertion_InPlace()
        //{
        //    var dummy = new int[64];
        //    foreach (var arr in arrays) {
        //        int n = Math.Min(64, arr.Length);
        //        SortedSet<int, IntOrdering>.Algorithms.CopyArray(arr, 0, n, dummy);
        //        SortedSet<int, IntOrdering>.Algorithms.InsertionSort_InPlace(dummy, 0, n);
        //    }
        //}


#if !smallset
        [Benchmark]
        public void JustCopy()
        {
            foreach (var arr in arrays) {
                _copy = arr.ToArray();
            }
        }

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
                SortedSet<int, IntOrdering>.Algorithms.TopDownMergeSort(copy);
            }
        }
        [Benchmark]
        public void QuickSort2()
        {
            foreach (var arr in arrays) {
                var copy = new int[arr.Length];
                SortedSet<int, IntOrdering>.Algorithms.CopyArray(arr,0,arr.Length,copy);
                SortedSet<int, IntOrdering>.Algorithms.QuickSort(copy);
            }
        }

        [Benchmark]
        public void MergeSort2()
        {
            foreach (var arr in arrays) {
                var copy = new int[arr.Length];
                SortedSet<int, IntOrdering>.Algorithms.CopyArray(arr,0,arr.Length,copy);
                SortedSet<int, IntOrdering>.Algorithms.TopDownMergeSort(copy);
            }
        }
        [Benchmark]
        public void BottomUpMergeSort2()
        {
            foreach (var arr in arrays) {
                var copy = new int[arr.Length];
                SortedSet<int, IntOrdering>.Algorithms.CopyArray(arr,0,arr.Length,copy);
                SortedSet<int, IntOrdering>.Algorithms.BottomUpMergeSort(copy);
            }
        }

        
        [Benchmark]
        public void SystemArraySort()
        {
            foreach (var arr in arrays) {
                var copy = new int[arr.Length];
                SortedSet<int, IntOrdering>.Algorithms.CopyArray(arr,0,arr.Length,copy);
                Array.Sort(copy);
            }
        }
        [Benchmark]
        public void AltMergeSort2()
        {
            foreach (var arr in arrays) {
                var copy = new int[arr.Length];
                SortedSet<int, IntOrdering>.Algorithms.CopyArray(arr,0,arr.Length,copy);
                SortedSet<int, IntOrdering>.Algorithms.AltTopDownMergeSort(copy);
            }
        }

        [Benchmark]
        public void CopyingMergeSort()
        {
            foreach (var arr in arrays) {
                var copy = SortedSet<int, IntOrdering>.Algorithms.TopDownMergeSort_Copy(arr);
            }
        }

        [Benchmark]
        public void LinqOrderBy()
        {
            foreach (var arr in arrays) {
                var copy = arr.OrderBy(i => i).ToArray();
            }
        }
#endif

        [Benchmark]
        public void QuickSort_Sorted()
        {
            foreach (var arr in sortedArrays) {
                //arr = arr.ToArray();
                SortedSet<int, IntOrdering>.Algorithms.QuickSort(arr);
            }
        }

        [Benchmark]
        public void MergeSort_Sorted()
        {
            foreach (var arr in sortedArrays) {
                //arr = arr.ToArray();
                SortedSet<int, IntOrdering>.Algorithms.TopDownMergeSort(arr);
            }
        }
        [Benchmark]
        public void AltMergeSort_Sorted()
        {
            foreach (var arr in sortedArrays) {
                //arr = arr.ToArray();
                SortedSet<int, IntOrdering>.Algorithms.AltTopDownMergeSort(arr);
            }
        }

        [Benchmark]
        public void BottomUpMergeSort_Sorted()
        {
            foreach (var arr in sortedArrays) {
                //arr = arr.ToArray();
                SortedSet<int, IntOrdering>.Algorithms.BottomUpMergeSort(arr);
            }
        }

        [Benchmark]
        public void SystemArraySort_Sorted()
        {
            foreach (var arr in sortedArrays) {
                //arr = arr.ToArray();
                Array.Sort(arr);
            }
        }
#if !smallset



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
