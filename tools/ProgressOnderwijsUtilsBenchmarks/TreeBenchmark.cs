using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtilsBenchmarks
{
    [MemoryDiagnoser]
    public class TreeBenchmark
    {
        [Params(3, 33, 400)]
        public int MaxSize;

        public int iters;
        Tree<int> tree = null!;

        [GlobalSetup]
        public void Setup()
        {
            iters = 1200 / MaxSize;
            var used = new HashSet<int>();
            tree = Tree.BuildRecursively(MaxSize, i => new[] { i - 37, i - 42, i - 3, i + 1 }.Where(n => 0 <= n && n < MaxSize && used.Add(n)));
        }

        [Benchmark]
        public void BuildRecursivelyA()
        {
            var used = new HashSet<int>();
            Func<int, bool> predicate = n => 0 <= n && n < MaxSize && used.Add(n);
            for (var iter = 0; iter < iters; iter++) {
                var tree = Tree.BuildRecursively(MaxSize, i => new[] { i - 37, i - 42, i - 3, i + 1 }.Where(predicate));
                used.Clear();
                GC.KeepAlive(tree);
            }
        }

        [Benchmark]
        public int Where()
        {
            var x = 0;
            Func<Tree<int>, bool> predicate = node => node.NodeValue is var n && n % 3 != 0 && n < MaxSize * 2 / 3;
            for (var iter = 0; iter < iters; iter++) {
                var output = tree.Where(predicate);
                x += output?.NodeValue ?? 0;
            }
            return x;
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
        }
    }
}
