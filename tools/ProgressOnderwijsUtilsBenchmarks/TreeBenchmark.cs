using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtilsBenchmarks
{
    [MemoryDiagnoser]
    public class TreeBenchmark
    {
        [Params(3, 21, 1000, 10_000)]
        public int MaxSize;

        [Params(3)]
        public int Threads;

        public int iters;

        [GlobalSetup]
        public void Setup()
        {
            iters = 10_000 / MaxSize + 3;
            Task.WaitAll(Enumerable.Range(0, Threads).Select(__ => Task.Factory.StartNew(() => { Thread.Yield(); }, TaskCreationOptions.LongRunning)).ToArray()); //I don't want to benchmark thread-pool startup.
        }

        [Benchmark]
        public void BuildRecursivelyA()
        {
            Task.WaitAll(Enumerable.Range(0, Threads).Select(__ => Task.Factory.StartNew(() => {
                var used = new HashSet<int>();
                Func<int, bool> predicate = n => 0 <= n && n < MaxSize && used.Add(n);
                for (var iter = 0; iter < iters; iter++) {
                    var tree = Tree.BuildRecursively(MaxSize, i => new[] { i - 1, i - 50, i / 2, i - 100 }.Where(predicate));
                    used.Clear();
                    GC.KeepAlive(tree);
                }
            }, TaskCreationOptions.LongRunning)).ToArray());
        }

        [Benchmark]
        public void BuildRecursivelyB()
        {
            Task.WaitAll(Enumerable.Range(0, Threads).Select(__ => Task.Factory.StartNew(() => {
                var used = new HashSet<int>();
                Func<int, bool> predicate = n => 0 <= n && n < MaxSize && used.Add(n);
                for (var iter = 0; iter < iters; iter++) {
                    var tree = Tree.BuildRecursively(MaxSize, i => new[] { i - 37, i - 42, i - 3, i + 1 }.Where(predicate));
                    used.Clear();
                GC.KeepAlive(tree);
                }
            }, TaskCreationOptions.LongRunning)).ToArray());
        }

        [Benchmark]
        public void BuildRecursivelyC()
        {
            Task.WaitAll(Enumerable.Range(0, Threads).Select(__ => Task.Factory.StartNew(() => {
                var used = new HashSet<int>();
                Func<int, bool> predicate = n => 0 <= n && n < MaxSize && used.Add(n);
                for (var iter = 0; iter < iters; iter++) {
                    var tree = Tree.BuildRecursively(MaxSize, i => new[] { i - 1, }.Where(predicate));
                    used.Clear();
                    GC.KeepAlive(tree);
                }
            }, TaskCreationOptions.LongRunning)).ToArray());
        }

        [Benchmark]
        public void BuildRecursivelyD()
        {
            Task.WaitAll(Enumerable.Range(0, Threads).Select(__ => Task.Factory.StartNew(() => {
                var used = new HashSet<int>();
                Func<int, bool> predicate = n => 0 <= n && n < MaxSize && used.Add(n);
                for (var iter = 0; iter < iters; iter++) {
                    var tree = Tree.BuildRecursively(MaxSize, i => new[] { i - 1, i * 3 / 4 }.Where(predicate));
                    used.Clear();
                    GC.KeepAlive(tree);
                }
            }, TaskCreationOptions.LongRunning)).ToArray());
        }
    }
}
