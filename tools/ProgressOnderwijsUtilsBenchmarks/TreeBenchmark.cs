namespace ProgressOnderwijsUtilsBenchmarks;

[MemoryDiagnoser]
// ReSharper disable once ClassCanBeSealed.Global
public class TreeBenchmark
{
    [Params(3, 33, 400)]
    // ReSharper disable once UnassignedField.Global
    public int MaxSize;

    public int iters;
    Tree<int> tree = Tree.Node(0);

    [GlobalSetup]
    public void Setup()
    {
        iters = 1200 / MaxSize;
        var used = new HashSet<int>();
        tree = Tree.BuildRecursively(MaxSize, i => new[] { i - 37, i - 42, i - 3, i + 1, }.Where(n => 0 <= n && n < MaxSize && used.Add(n)));
    }

    [Benchmark]
    public int Rebuild()
    {
        var x = 0;
        for (var iter = 0; iter < iters; iter++) {
            var b = tree.Rebuild(
                node => node.Children.Count + node.NodeValue * 13,
                (_, val, kids) =>
                    val % 2 == 0
                        ? kids
                        : val % 3 == 0
                            ? null
                            : new[] { Tree.Node(val, kids), Tree.Node(val + 1, kids), }
            );

            x += b.Sum(o => o.PreorderTraversal().Select(n => n.NodeValue).Sum());
        }
        return x;
    }

    [Benchmark]
    public void BuildRecursivelyA()
    {
        var used = new HashSet<int>();
        Func<int, bool> predicate = n => 0 <= n && n < MaxSize && used.Add(n);
        for (var iter = 0; iter < iters; iter++) {
            var tree2 = Tree.BuildRecursively(MaxSize, i => new[] { i - 37, i - 42, i - 3, i + 1, }.Where(predicate));
            used.Clear();
            GC.KeepAlive(tree2);
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
            var output = tree.SelectNodeValue(predicate);
            x += output.NodeValue ? 1 : 0;
        }
        return x;
    }
}
