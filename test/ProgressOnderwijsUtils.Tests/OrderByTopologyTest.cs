namespace ProgressOnderwijsUtils.Tests;

readonly struct DagNode
{
    public readonly string Name;
    readonly IReadOnlyList<string> Dependencies;

    public IEnumerable<DagNode> Children(Dictionary<string, DagNode> lookup)
        => Dependencies.Select(name => lookup.GetValueOrDefault(name, new(name)));

    public DagNode(string name, params string[] dependencies)
    {
        Name = name;
        Dependencies = dependencies;
    }
}

public sealed class OrderByTopologyTest
{
    [Fact]
    public void NodesWithoutDependenciesAreNotSorted()
    {
        var example = new[] { "a", "b", "d", "c", };
        var (cycleDetected, ordered) = example.OrderByTopology(_ => Array.Empty<string>());
        PAssert.That(() => cycleDetected == false && ordered.SequenceEqual(example));
    }

    [Fact]
    public void SimpleDependencyComesFirst()
    {
        var nodes = new[] {
            new DagNode("a", "b"),
            new DagNode("b", "c"),
            new DagNode("c"),
        };
        var lookup = nodes.ToDictionary(s => s.Name);
        var (cycleDetected, ordered) = nodes.OrderByTopology(node => node.Children(lookup));
        PAssert.That(() => cycleDetected == false && ordered.Select(s => s.Name).SequenceEqual(new[] { "c", "b", "a", }));
    }

    [Fact]
    public void DoesDetectCircularDependencies()
    {
        var nodes = new[] {
            new DagNode("a", "b"),
            new DagNode("b", "c"),
            new DagNode("c", "a"),
        };
        var lookup = nodes.ToDictionary(s => s.Name);
        var (cycleDetected, ordered) = nodes.OrderByTopology(node => node.Children(lookup));
        PAssert.That(() => cycleDetected && ordered.Select(s => s.Name).SequenceEqual(new[] { "c", "b", "a", }));
    }

    [Fact]
    public void SupportsMulitpleDependancies()
    {
        var nodes = new[] {
            new DagNode("c", "b"),
            new DagNode("b", "a"),
            new DagNode("a"),
            new DagNode("z", "y", "b"),
            new DagNode("y", "x"),
            new DagNode("x", "a"),
        };
        var lookup = nodes.ToDictionary(s => s.Name);
        var expected = new[] { "a", "b", "c", "x", "y", "z", };
        var (cycleDetected, ordered) = nodes.OrderByTopology(node => node.Children(lookup));
        PAssert.That(() => cycleDetected == false && ordered.Select(s => s.Name).SequenceEqual(expected));
    }
}
