using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using JetBrains.Annotations;
using MoreLinq;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Collections;

// ReSharper disable ClassCanBeSealed.Global  - for Benchmark.NET
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
            public int Init(int value)
                => value;
        }
    }

    public class ByteArrayBuilderBenchmark : ArrayBuilderBenchmark<byte, ByteArrayBuilderBenchmark.Factory>
    {
        public struct Factory : IFactory<byte>
        {
            public byte Init(int value)
                => (byte)value;
        }
    }

    public class SmallStructArrayBuilderBenchmark : ArrayBuilderBenchmark<(int, int), SmallStructArrayBuilderBenchmark.Factory>
    {
        public struct Factory : IFactory<(int, int)>
        {
            public (int, int) Init(int value)
                => (value, value);
        }
    }

    public class ReferenceTypeArrayBuilderBenchmark : ArrayBuilderBenchmark<object?, ReferenceTypeArrayBuilderBenchmark.Factory>
    {
        public struct Factory : IFactory<object?>
        {
            static readonly object?[] Values = { "test", null, Tuple.Create(1, 2, 3), "lala", new List<int>(), new object(), new object(), new object(), };

            public object? Init(int value)
                => Values[value & 7];
        }
    }

    public class BigStructArrayBuilderBenchmark : ArrayBuilderBenchmark<BigStructArrayBuilderBenchmark.BigStruct, BigStructArrayBuilderBenchmark.Factory>
    {
        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        public struct BigStruct
        {
            public long A, B, C, D;
            public string X, Y;
            public int E, F, G, H;
        }

        public struct Factory : IFactory<BigStruct>
        {
            public BigStruct Init(int value)
                => new BigStruct {
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

    sealed class ArrBenchConfig : ManualConfig
    {
        public ArrBenchConfig()
        {
            AddJob(
                new Job {
                    Run = { UnrollFactor = 1, InvocationCount = 1, LaunchCount = 1, WarmupCount = 3, RunStrategy = BenchmarkDotNet.Engines.RunStrategy.Throughput, MaxIterationCount = 1000, },
                    Accuracy = { MaxRelativeError = 0.01, },
                }.WithGcForce(true));
        }
    }

    [MedianColumn]
    [MemoryDiagnoser]
    [Config(typeof(ArrBenchConfig))]
    public abstract class ArrayBuilderBenchmark<T, TFactory>
        where TFactory : struct, IFactory<T>
    {
        [ParamsSource(nameof(Configs))]
        // ReSharper disable once UnassignedField.Global
        public (int MaxSize, int Threads, int Count, double avgLength) Config;

        public static IEnumerable<(int MaxSize, int Threads, int Count, double avgLength)> Configs
            => from maxSize in new[] { 3, 17, 98, 561, 18_347, /*104_920,600_000*/ }
                from threads in new[] { 1, 8 }
                let objCost = typeof(T).IsValueType ? Unsafe.SizeOf<T>() : 8
                let count = (int)(0.5 + 300_000_000.0 / ((maxSize + 2) * (objCost + 2) + 30))
                select (maxSize, threads, count, Math.Round(GetSizes(count, maxSize).Average(), 2));

        public int[]? Sizes;

        [GlobalSetup]
        public void Setup()
        {
            Sizes = GetSizes(Config.Count, Config.MaxSize);
            Task.WaitAll(Enumerable.Range(0, Config.Threads).Select(_ => Task.Factory.StartNew(() => { Thread.Yield(); }, TaskCreationOptions.LongRunning)).ToArray()); //I don't want to benchmark thread-pool startup.
        }

        static int[] GetSizes(int count, int maxSize)
            => Enumerable.Range(0, count + 1).Select(i => (int)(i / (double)count * maxSize + 0.5)).Shuffle(new Random(42)).ToArray();

        [Benchmark]
        public void List()
            => Task.WaitAll(Enumerable.Range(0, Config.Threads).Select(
                _ => Task.Factory.StartNew(
                    () => {
                        foreach (var size in Sizes.AssertNotNull()) {
                            var builder = new List<T>();
                            for (var i = 0; i < size; i++) {
                                builder.Add(default(TFactory).Init(i));
                            }
                            GC.KeepAlive(builder.ToArray());
                        }
                    },
                    TaskCreationOptions.LongRunning)).ToArray());

        [Benchmark]
        public void ArrayBuilder()
            => Task.WaitAll(Enumerable.Range(0, Config.Threads).Select(_ => Task.Factory.StartNew(
                () => {
                    foreach (var size in Sizes.AssertNotNull()) {
                        var builder = new ArrayBuilder<T>();
                        for (var i = 0; i < size; i++) {
                            builder.Add(default(TFactory).Init(i));
                        }
                        GC.KeepAlive(builder.ToArray());
                    }
                },
                TaskCreationOptions.LongRunning)).ToArray());

        [Benchmark]
        public void WithoutInlineValues()
            => Task.WaitAll(
                Enumerable.Range(0, Config.Threads).Select(
                    _ => Task.Factory.StartNew(
                        () => {
                            foreach (var size in Sizes.AssertNotNull()) {
                                var builder = AlternativeArrayBuilders.WithoutInlineValues<T>.Allocate();
                                for (var i = 0; i < size; i++) {
                                    builder.Add(default(TFactory).Init(i));
                                }
                                GC.KeepAlive(builder.ToArray());
                            }
                        },
                        TaskCreationOptions.LongRunning)).ToArray());

        [Benchmark]
        public void WithSegmentsAsArray()
            => Task.WaitAll(
                Enumerable.Range(0, Config.Threads).Select(
                    _ => Task.Factory.StartNew(
                        () => {
                            foreach (var size in Sizes.AssertNotNull()) {
                                var builder = AlternativeArrayBuilders.WithSegmentsAsArray<T>.Create();
                                for (var i = 0; i < size; i++) {
                                    builder.Add(default(TFactory).Init(i));
                                }
                                GC.KeepAlive(builder.ToArray());
                            }
                        },
                        TaskCreationOptions.LongRunning)).ToArray());

        [Benchmark]
        public void Inline63ValuesAndSegments()
            => Task.WaitAll(
                Enumerable.Range(0, Config.Threads).Select(
                    _ => Task.Factory.StartNew(
                        () => {
                            foreach (var size in Sizes.AssertNotNull()) {
                                var builder = new AlternativeArrayBuilders.Inline63ValuesAndSegments<T>();
                                for (var i = 0; i < size; i++) {
                                    builder.Add(default(TFactory).Init(i));
                                }
                                GC.KeepAlive(builder.ToArray());
                            }
                        },
                        TaskCreationOptions.LongRunning)).ToArray());

        [Benchmark]
        public void Inline16ValuesAndSegments()
            => Task.WaitAll(
                Enumerable.Range(0, Config.Threads).Select(
                    _ => Task.Factory.StartNew(
                        () => {
                            foreach (var size in Sizes.AssertNotNull()) {
                                var builder = new AlternativeArrayBuilders.Inline16ValuesAndSegments<T>();
                                for (var i = 0; i < size; i++) {
                                    builder.Add(default(TFactory).Init(i));
                                }
                                GC.KeepAlive(builder.ToArray());
                            }
                        },
                        TaskCreationOptions.LongRunning)).ToArray());

        [Benchmark]
        public void Inline32ValuesAndSegments()
            => Task.WaitAll(
                Enumerable.Range(0, Config.Threads).Select(
                    _ => Task.Factory.StartNew(
                        () => {
                            foreach (var size in Sizes.AssertNotNull()) {
                                var builder = new AlternativeArrayBuilders.Inline32ValuesAndSegments<T>();
                                for (var i = 0; i < size; i++) {
                                    builder.Add(default(TFactory).Init(i));
                                }
                                GC.KeepAlive(builder.ToArray());
                            }
                        },
                        TaskCreationOptions.LongRunning)).ToArray());

        public static void SanityCheck(int maxLen)
        {
            var r = new Random(37);
            for (var j = 0; j < maxLen; j++) {
                var builder = AlternativeArrayBuilders.WithoutInlineValues<T>.Allocate();
                var len = r.Next(j + 1);
                for (var i = 0; i < len; i++) {
                    builder.Add(default(TFactory).Init(i));
                }
                var array = builder.ToArray();

                if (!SequenceEqualityComparer<T>.Default.Equals(array, Enumerable.Range(0, len).Select(default(TFactory).Init).ToArray())) {
                    Console.WriteLine(len + " buggy");
                }
            }
        }
    }
}
