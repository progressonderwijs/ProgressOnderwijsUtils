using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess;
using ProgressOnderwijsUtils.Collections;
using AnInt = System.UInt32;
// ReSharper disable BuiltInTypeReferenceStyle

namespace ProgressOnderwijsUtilsBenchmarks.OrderingBenchmarks
{
    using static SortedSet<AnInt, AnIntOrdering>;

    struct AnIntOrdering : IOrdering<AnInt>
    {
        public bool LessThan(AnInt a, AnInt b) => a < b;
        public bool Equal(AnInt a, AnInt b) => a == b;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class QuickBenchAttribute : Attribute, IConfigSource
    {
        public QuickBenchAttribute()
        {
            var job = new Job();
            job.Env.Platform = Platform.X64;
            job.Run.RunStrategy = BenchmarkDotNet.Engines.RunStrategy.Throughput;
            job.Run.WarmupCount = 1;
            //job.Run.InvocationCount = 4;
            job.Run.UnrollFactor = 1;
            job.Infrastructure.Toolchain = InProcessToolchain.Instance;
            job.Accuracy.MaxRelativeError=0.001;

            Config = ManualConfig.CreateEmpty().With(job);
        }

        public IConfig Config { get; }
    }

    [QuickBench]
    public class SortedSetULongBench
    {
        const int ArrSize = 1 << 20;
        static readonly AnInt[] sourcedata;
        static readonly AnInt[] array;
        static Random random = new Random(42);

        static SortedSetULongBench()
        {
            array = new AnInt[ArrSize];
            sourcedata = new AnInt[array.Length*5];
            Randomize(random, sourcedata);
        }

        static void Randomize(Random r, AnInt[] arr)
        {
            for (int j = 0; j < arr.Length; j++) {
                var next = (AnInt)r.Next();
                arr[j] = (next << (int)(next & 31u)) + next;
            }
        }

        static void RefreshData()
        {
            var offset = random.Next(array.Length << 2);
            Array.Copy(sourcedata, offset, array, 0, array.Length);
        }

        public static void RunBenchmarks()
        {
            BenchmarkRunner.Run<SortedSetULongBench>();
        }


        [Benchmark]
        public void JustInit()
        {
            RefreshData();
        }
        
        [Benchmark]
        public void QuickSort()
        {
            RefreshData();
            Algorithms.QuickSort(array);
        }

        //[Benchmark]
        //public void Insertion_Copy()
        //{
        //    var dummy = new int[64];
        //    var arr = array;{
        //        Algorithms.InsertionSort_Copy(arr,0,Math.Min(64,arr.Length),dummy);
        //    }
        //}

        //[Benchmark]
        //public void Insertion_InPlace()
        //{
        //    var dummy = new int[64];
        //    var arr = array;{
        //        int n = Math.Min(64, arr.Length);
        //        Algorithms.CopyArray(arr, 0, n, dummy);
        //        Algorithms.InsertionSort_InPlace(dummy, 0, n);
        //    }
        //}


        /*
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
        [Benchmark]
        public void BottomUpMergeSort()
        {
            Algorithms.BottomUpMergeSort(copy);
        }
        */

        /*
        [Benchmark]
        public void AltTopDownMergeSort()
        {
            Algorithms.AltTopDownMergeSort(copy);
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
                copy = new AnInt[arr.Length];
                Algorithms.CopyArray(arr, 0, arr.Length, copy);
                Algorithms.QuickSort(copy);
            }
        }

        [Benchmark]
        public void MergeSort2()
        {
            var arr = array;{
                copy = new AnInt[arr.Length];
                Algorithms.CopyArray(arr, 0, arr.Length, copy);
                Algorithms.TopDownMergeSort(copy);
            }
        }

        [Benchmark]
        public void BottomUpMergeSort2()
        {
            var arr = array;{
                copy = new AnInt[arr.Length];
                Algorithms.CopyArray(arr, 0, arr.Length, copy);
                Algorithms.BottomUpMergeSort(copy);
            }
        }

        [Benchmark]
        public void AltMergeSort2()
        {
            var arr = array;{
                copy = new AnInt[arr.Length];
                Algorithms.CopyArray(arr, 0, arr.Length, copy);
                Algorithms.AltTopDownMergeSort(copy);
            }
        }

        [Benchmark]
        public void CopyingMergeSort()
        {
            var arr = array;{
                copy = Algorithms.TopDownMergeSort_Copy(arr);
            }
        }

        [Benchmark]
        public void QuickSort_Sorted()
        {
            foreach (var arr in sortedArrays) {
                //arr = arr.ToArray();
                Algorithms.QuickSort(arr);
            }
        }

        [Benchmark]
        public void MergeSort_Sorted()
        {
            foreach (var arr in sortedArrays) {
                //arr = arr.ToArray();
                Algorithms.TopDownMergeSort(arr);
            }
        }

        [Benchmark]
        public void AltMergeSort_Sorted()
        {
            foreach (var arr in sortedArrays) {
                //arr = arr.ToArray();
                Algorithms.AltTopDownMergeSort(arr);
            }
        }

        [Benchmark]
        public void BottomUpMergeSort_Sorted()
        {
            foreach (var arr in sortedArrays) {
                //arr = arr.ToArray();
                Algorithms.BottomUpMergeSort(arr);
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
