namespace ProgressOnderwijsUtils.Tests;

public sealed class TreeCursorTest
{
    [Fact]
    public void ReplaceSubTree_can_replace_single_root_node()
    {
        var tree = Tree.Node(42).CursorForThisRoot();
        var newTree = tree.ReplaceSubTree(Tree.Node(11));

        PAssert.That(() => !newTree.Equals(tree));
        PAssert.That(() => newTree.Equals(Tree.Node(11).CursorForThisRoot()));
    }

    [Fact]
    public void ReplaceSubTree_can_replace_nested_node()
    {
        var tree =
            Tree.Node(
                1,
                Tree.Node(2),
                Tree.Node(
                    3,
                    Tree.Node(4),
                    Tree.Node(5)
                )
            ).CursorForThisRoot();

        var toReplace = tree
            .PreorderTraversal()
            .Single(node => node.NodeValue == 3);

        var newTree = toReplace
            .ReplaceSubTree(Tree.Node(42, toReplace.ToSubTree().Children))
            .Root;

        PAssert.That(() => !newTree.Equals(tree));
        PAssert.That(
            () => newTree.Equals(
                Tree.Node(
                    1,
                    Tree.Node(2),
                    Tree.Node(
                        42,
                        Tree.Node(4),
                        Tree.Node(5)
                    )
                )
            )
        );
    }

    [Fact]
    public void RootedTree_Sibling_Implementation_Works_Correctly()
    {
        var tree =
            Tree.Node(
                1,
                Tree.Node(2),
                Tree.Node(
                    3,
                    Tree.Node(4),
                    Tree.Node(5)
                ),
                Tree.Node(6)
            ).CursorForThisRoot();

        PAssert.That(() => !tree.PreviousSibling().HasValue);
        PAssert.That(() => !tree.NextSibling().HasValue);
        PAssert.That(() => !tree.Parent.PreviousSibling().HasValue);
        PAssert.That(() => !tree.Parent.NextSibling().HasValue);
        PAssert.That(() => !tree.Children[0].PreviousSibling().HasValue);
        PAssert.That(() => tree.Children[0].NextSibling().HasValue);
        PAssert.That(() => tree.Children[0].NextSibling().NodeValue == 3);
        PAssert.That(() => tree.Children[0].NextSibling().NextSibling().HasValue);
        PAssert.That(() => tree.Children[0].NextSibling().NextSibling().NodeValue == 6);
        PAssert.That(() => !tree.Children[0].NextSibling().NextSibling().NextSibling().HasValue);
    }
}
