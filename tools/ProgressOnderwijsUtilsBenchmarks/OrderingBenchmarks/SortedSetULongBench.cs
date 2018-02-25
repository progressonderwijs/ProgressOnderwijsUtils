using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ProgressOnderwijsUtils.Collections;
using static ProgressOnderwijsUtils.Collections.SortedSet<ulong, ProgressOnderwijsUtilsBenchmarks.OrderingBenchmarks.ULongOrdering>;

namespace ProgressOnderwijsUtilsBenchmarks.OrderingBenchmarks
{
    struct ULongOrdering : IOrdering<ulong>
    {
        public bool LessThan(ulong a, ulong b) => a < b;
        public bool Equal(ulong a, ulong b) => a == b;
    }

    //[InProcess]
    public class SortedSetULongBench
    {
        const int ArrSize = 1 << 24;
        static readonly ulong[] array;
        static readonly ulong[] copy;

        static SortedSetULongBench()
        {
            array = new ulong[ArrSize];
            copy = new ulong[ArrSize];
            var random = new Random(42);
            for (int j = 0; j < ArrSize; j++) {
                array[j] = ((ulong)(uint)random.Next() << 32) + (uint)random.Next();
            }
        }

        public static void RunBenchmarks()
        {
            BenchmarkRunner.Run<SortedSetULongBench>();
        }

        //[Benchmark]
        //public void Insertion_Copy()
        //{
        //    var dummy = new int[64];
        //    var arr = array;{
        //        SortedSet<ulong, ULongOrdering>.Algorithms.InsertionSort_Copy(arr,0,Math.Min(64,arr.Length),dummy);
        //    }
        //}

        //[Benchmark]
        //public void Insertion_InPlace()
        //{
        //    var dummy = new int[64];
        //    var arr = array;{
        //        int n = Math.Min(64, arr.Length);
        //        SortedSet<ulong, ULongOrdering>.Algorithms.CopyArray(arr, 0, n, dummy);
        //        SortedSet<ulong, ULongOrdering>.Algorithms.InsertionSort_InPlace(dummy, 0, n);
        //    }
        //}
        [IterationSetup]
        public void JustCopy()
        {
            Array.Copy(array, copy, array.Length);
        }

        [Benchmark]
        public void TopDownMergeSort()
        {
            for (int len = 3; len < 20000; len = len + 1 + (len >> 4)) {
                Algorithms.CopyArray(array, 0, len, copy);

                Algorithms.TopDownMergeSort(copy, len);
            }
        }

        [Benchmark]
        public void TopDownMergeSort2()
        {
            for (int len = 3; len < 20000; len = len + 1 + (len >> 4)) {
                Algorithms.CopyArray(array, 0, len, copy);

                Algorithms.TopDownMergeSort2(copy, len);
            }
        }

        /*
        [Benchmark]
        public void BottomUpMergeSort()
        {
            SortedSet<ulong, ULongOrdering>.Algorithms.BottomUpMergeSort(copy);
        }

        [Benchmark]
        public void QuickSort()
        {
            SortedSet<ulong, ULongOrdering>.Algorithms.QuickSort(copy);
        }

        [Benchmark]
        public void AltTopDownMergeSort()
        {
            SortedSet<ulong, ULongOrdering>.Algorithms.AltTopDownMergeSort(copy);
        }

        [Benchmark]
        public void SystemArraySort()
        {
            Array.Sort(copy);
        }

        /**/
        /*
        [Benchmark]
        public void QuickSort2()
        {
            var arr = array;{
                copy = new ulong[arr.Length];
                SortedSet<ulong, ULongOrdering>.Algorithms.CopyArray(arr, 0, arr.Length, copy);
                SortedSet<ulong, ULongOrdering>.Algorithms.QuickSort(copy);
            }
        }

        [Benchmark]
        public void MergeSort2()
        {
            var arr = array;{
                copy = new ulong[arr.Length];
                SortedSet<ulong, ULongOrdering>.Algorithms.CopyArray(arr, 0, arr.Length, copy);
                SortedSet<ulong, ULongOrdering>.Algorithms.TopDownMergeSort(copy);
            }
        }

        [Benchmark]
        public void BottomUpMergeSort2()
        {
            var arr = array;{
                copy = new ulong[arr.Length];
                SortedSet<ulong, ULongOrdering>.Algorithms.CopyArray(arr, 0, arr.Length, copy);
                SortedSet<ulong, ULongOrdering>.Algorithms.BottomUpMergeSort(copy);
            }
        }

        [Benchmark]
        public void AltMergeSort2()
        {
            var arr = array;{
                copy = new ulong[arr.Length];
                SortedSet<ulong, ULongOrdering>.Algorithms.CopyArray(arr, 0, arr.Length, copy);
                SortedSet<ulong, ULongOrdering>.Algorithms.AltTopDownMergeSort(copy);
            }
        }

        [Benchmark]
        public void CopyingMergeSort()
        {
            var arr = array;{
                copy = SortedSet<ulong, ULongOrdering>.Algorithms.TopDownMergeSort_Copy(arr);
            }
        }

        [Benchmark]
        public void QuickSort_Sorted()
        {
            foreach (var arr in sortedArrays) {
                //arr = arr.ToArray();
                SortedSet<ulong, ULongOrdering>.Algorithms.QuickSort(arr);
            }
        }

        [Benchmark]
        public void MergeSort_Sorted()
        {
            foreach (var arr in sortedArrays) {
                //arr = arr.ToArray();
                SortedSet<ulong, ULongOrdering>.Algorithms.TopDownMergeSort(arr);
            }
        }

        [Benchmark]
        public void AltMergeSort_Sorted()
        {
            foreach (var arr in sortedArrays) {
                //arr = arr.ToArray();
                SortedSet<ulong, ULongOrdering>.Algorithms.AltTopDownMergeSort(arr);
            }
        }

        [Benchmark]
        public void BottomUpMergeSort_Sorted()
        {
            foreach (var arr in sortedArrays) {
                //arr = arr.ToArray();
                SortedSet<ulong, ULongOrdering>.Algorithms.BottomUpMergeSort(arr);
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
        /**/
    }
}
