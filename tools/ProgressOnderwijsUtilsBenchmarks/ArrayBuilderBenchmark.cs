using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using MoreLinq;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtilsBenchmarks
{
    public interface IFactory<out T>
    {
        T Init(int value);
    }

    public class IntArrayBuilderBenchmark : ArrayBuilderBenchmark<int, IntArrayBuilderBenchmark.Factory>
    {
        public struct Factory : IFactory<int>
        {
            public int Init(int value) => value;
        }
    }

    public class ByteArrayBuilderBenchmark : ArrayBuilderBenchmark<byte, ByteArrayBuilderBenchmark.Factory>
    {
        public struct Factory : IFactory<byte>
        {
            public byte Init(int value) => (byte)value;
        }
    }

    public class SmallStructArrayBuilderBenchmark : ArrayBuilderBenchmark<(int,int), SmallStructArrayBuilderBenchmark.Factory>
    {
        public struct Factory : IFactory<(int,int)>
        {
            public (int,int) Init(int value) => (value,value);
        }
    }

    public class ReferenceTypeArrayBuilderBenchmark : ArrayBuilderBenchmark<object, ReferenceTypeArrayBuilderBenchmark.Factory>
    {
        public struct Factory : IFactory<object>
        {
            static readonly object[] Values = { "test", null, Tuple.Create(1, 2, 3), "lala", new List<int>(), new object(), new object(), new object(), };
            public object Init(int value) => Values[value & 7];
        }
    }

    public class BigStructArrayBuilderBenchmark : ArrayBuilderBenchmark<BigStructArrayBuilderBenchmark.BigStruct, BigStructArrayBuilderBenchmark.Factory>
    {
        public struct BigStruct
        {
            public long A, B, C, D;
            public string X, Y;
            public int E, F, G, H;
        }

        public struct Factory : IFactory<BigStruct>
        {
            public BigStruct Init(int value) => new BigStruct {
                A = value,
                B = value,
                C = value,
                D = value,
                X = "X",
                Y = "Y",
                E = value,
                F = value,
                G = value,
                H = value,
            };
        }
    }

    //[ClrJob]
    //[CoreJob]
    [RankColumn]
    public abstract class ArrayBuilderBenchmark<T, TFactory>
        where TFactory : struct, IFactory<T>
    {
        [Params(3, 21, 1000, 1000)] //, 10_000, 100_000, 1000_000
        public int MaxSize;

        [Params(3)]
        public int Threads;

        public int[] Sizes;
        public T[] Values;

        [GlobalSetup]
        public void Setup()
        {
            var count = 100_000 / (MaxSize + 4);
            Sizes = Enumerable.Range(0, count + 1).Select(i => (int)(i / (double)count * MaxSize + 0.5)).RandomSubset(count + 1, new Random(42)).ToArray();
            Task.WaitAll(Enumerable.Range(0, Threads).Select(__ => Task.Factory.StartNew(() => { Thread.Yield(); }, TaskCreationOptions.LongRunning)).ToArray()); //I don't want to benchmark thread-pool startup.
        }

        [Benchmark]
        public void List()
        {
            Task.WaitAll(Enumerable.Range(0, Threads).Select(__ => Task.Factory.StartNew(() => {
                foreach (var size in Sizes) {
                    var builder = new List<T>();
                    for (int i = 0; i < size; i++) {
                        builder.Add(default(TFactory).Init(i));
                    }
                    GC.KeepAlive(builder.ToArray());
                }
            }, TaskCreationOptions.LongRunning)).ToArray());
        }

        [Benchmark]
        public void FastArrayBuilder()
        {
            Task.WaitAll(Enumerable.Range(0, Threads).Select(__ => Task.Factory.StartNew(() => {
                foreach (var size in Sizes) {
                    var builder = ArrayBuilder_WithArraySegments<T>.Create();
                    for (int i = 0; i < size; i++) {
                        builder.Add(default(TFactory).Init(i));
                    }
                    GC.KeepAlive(builder.ToArray());
                }
            }, TaskCreationOptions.LongRunning)).ToArray());
        }

        [Benchmark]
        public void FastArrayBuilder2()
        {
            Task.WaitAll(Enumerable.Range(0, Threads).Select(__ => Task.Factory.StartNew(() => {
                foreach (var size in Sizes) {
                    var builder = new ArrayBuilder_Inline63ValuesAndSegments<T>();
                    for (int i = 0; i < size; i++) {
                        builder.Add(default(TFactory).Init(i));
                    }
                    GC.KeepAlive(builder.ToArray());
                }
            }, TaskCreationOptions.LongRunning)).ToArray());
        }

        [Benchmark]
        public void FastArrayBuilder2b()
        {
            Task.WaitAll(Enumerable.Range(0, Threads).Select(__ => Task.Factory.StartNew(() => {
                foreach (var size in Sizes) {
                    var builder = new ArrayBuilder_Inline16ValuesAndSegments<T>();
                    for (int i = 0; i < size; i++) {
                        builder.Add(default(TFactory).Init(i));
                    }
                    GC.KeepAlive(builder.ToArray());
                }
            }, TaskCreationOptions.LongRunning)).ToArray());
        }

        [Benchmark]
        public void FastArrayBuilder2c()
        {
            Task.WaitAll(Enumerable.Range(0, Threads).Select(__ => Task.Factory.StartNew(() => {
                foreach (var size in Sizes) {
                    var builder = new ArrayBuilder<T>();
                    for (int i = 0; i < size; i++) {
                        builder.Add(default(TFactory).Init(i));
                    }
                    GC.KeepAlive(builder.ToArray());
                }
            }, TaskCreationOptions.LongRunning)).ToArray());
        }

        [Benchmark]
        public void FastArrayBuilder3()
        {
            Task.WaitAll(Enumerable.Range(0, Threads).Select(__ => Task.Factory.StartNew(() => {
                foreach (var size in Sizes) {
                    var builder = new ArrayBuilder_Inline32ValuesAndSegments<T>();
                    for (int i = 0; i < size; i++) {
                        builder.Add(default(TFactory).Init(i));
                    }
                    GC.KeepAlive(builder.ToArray());
                }
            }, TaskCreationOptions.LongRunning)).ToArray());
        }
    }
}
