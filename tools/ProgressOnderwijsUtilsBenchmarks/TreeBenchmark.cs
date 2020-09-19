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
        [Params(3, 33, 400)]
        public int MaxSize;

        [Params(3)]
        public int Threads;

        public int iters;
        Tree<int> tree;

        [GlobalSetup]
        public void Setup()
        {
            iters = 1200 / MaxSize;
            Task.WaitAll(Enumerable.Range(0, Threads).Select(__ => Task.Factory.StartNew(() => { Thread.Yield(); }, TaskCreationOptions.LongRunning)).ToArray()); //I don't want to benchmark thread-pool startup.
            var used = new HashSet<int>();
            Func<int, bool> predicate = n => 0 <= n && n < MaxSize && used.Add(n);
            tree = Tree.BuildRecursively(MaxSize, i => new[] { i - 37, i - 42, i - 3, i + 1 }.Where(predicate));
        }

        //[Benchmark]
        public void BuildRecursivelyA()
        {
            //Task.WaitAll(Enumerable.Range(0, Threads).Select(__ => Task.Factory.StartNew(() => {
            var used = new HashSet<int>();
            Func<int, bool> predicate = n => 0 <= n && n < MaxSize && used.Add(n);
            for (var iter = 0; iter < iters; iter++) {
                var tree = Tree.BuildRecursively(MaxSize, i => new[] { i - 37, i - 42, i - 3, i + 1 }.Where(predicate));
                used.Clear();
                GC.KeepAlive(tree);
            }
            //}, TaskCreationOptions.LongRunning)).ToArray());
        }

        //[Benchmark]
        public int Where()
        {
            var x = 0;
            Func<Tree<int>, bool> predicate = node => node.NodeValue is var n && n % 3 != 0 && n < MaxSize * 2 / 3;
            for (var iter = 0; iter < iters; iter++) {
                var output = tree.Where(predicate);
                x += output?.NodeValue ?? 0;
            }
            return x;
            //}, TaskCreationOptions.LongRunning)).ToArray());
        }

        //[Benchmark]
        public int Where2()
        {
            var x = 0;
            Func<Tree<int>, bool> predicate = node => node.NodeValue is var n && n % 3 != 0 && n < MaxSize * 2 / 3;
            for (var iter = 0; iter < iters; iter++) {
                var output = tree.Where2(predicate);
                x += output?.NodeValue ?? 0;
            }
            return x;
            //}, TaskCreationOptions.LongRunning)).ToArray());
        }

        //[Benchmark]
        public int Where3()
        {
            var x = 0;
            Func<int, bool> predicate = node => node is var n && n % 3 != 0 && n < MaxSize * 2 / 3;
            for (var iter = 0; iter < iters; iter++) {
                var output = tree.Where3(predicate);
                x += output?.NodeValue ?? 0;
            }
            return x;
            //}, TaskCreationOptions.LongRunning)).ToArray());
        }

        [Benchmark]
        public int Select()
        {
            var x = 0;
            Func<Tree<int>, bool> predicate = node => node.NodeValue is var n && n % 3 != 0 && n < MaxSize * 2 / 3;
            for (var iter = 0; iter < iters; iter++) {
                var output = tree.Select(predicate);
                x += output.NodeValue ? 1 : 0;
            }
            return x;
            //}, TaskCreationOptions.LongRunning)).ToArray());
        }

        [Benchmark]
        public int Select2()
        {
            var x = 0;
            Func<Tree<int>, bool> predicate = node => node.NodeValue is var n && n % 3 != 0 && n < MaxSize * 2 / 3;
            for (var iter = 0; iter < iters; iter++) {
                var output = tree.Select2(predicate);
                x += output.NodeValue ? 1 : 0;
            }
            return x;
            //}, TaskCreationOptions.LongRunning)).ToArray());
        }

        [Benchmark]
        public int SelectB()
        {
            var x = 0;
            Func<int, bool> predicate = node => node is var n && n % 3 != 0 && n < MaxSize * 2 / 3;
            for (var iter = 0; iter < iters; iter++) {
                var output = tree.Select(predicate);
                x += output.NodeValue ? 1 : 0;
            }
            return x;
            //}, TaskCreationOptions.LongRunning)).ToArray());
        }

        [Benchmark]
        public int Select3()
        {
            var x = 0;
            Func<Tree<int>, bool> predicate = node => node.NodeValue is var n && n % 3 != 0 && n < MaxSize * 2 / 3;
            for (var iter = 0; iter < iters; iter++) {
                var output = tree.Select3(predicate);
                x += output.NodeValue ? 1 : 0;
            }
            return x;
            //}, TaskCreationOptions.LongRunning)).ToArray());
        }
    }
}
