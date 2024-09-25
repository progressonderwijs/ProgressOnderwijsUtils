namespace ProgressOnderwijsUtils;

public static class TreeExtensions
{
    public static TTree[] AsSingletonArray<TTree>(this IRecursiveStructure<TTree> tree)
        where TTree : IRecursiveStructure<TTree> //to avoid namepsace pollution
        => new[] { tree.TypedThis, };

    [Pure]
    public static int Height<TTree, T>(this IRecursiveStructure<TTree, T> tree)
        where TTree : IRecursiveStructure<TTree>
    {
        var maxHeight = 0;
        var todo = new Stack<(Tree<T>, int)>(16);
        todo.Push((tree.ToSubTree(), 1));

        while (todo.Count > 0) {
            var (next, height) = todo.Pop();
            foreach (var kid in next.Children) {
                todo.Push((kid, height + 1));
            }
            if (height > maxHeight) {
                maxHeight = height;
            }
        }
        return maxHeight;
    }

    [Pure]
    public static TreeCursor<T> CursorForThisRoot<T>(this Tree<T> tree)
        => TreeCursor<T>.CreateAtRoot(tree);

    [Pure]
    public static IEnumerable<T> PreorderTraversal<T>(this IRecursiveStructure<T> tree)
        where T : IRecursiveStructure<T>
    {
        yield return tree.TypedThis;

        var todo = new Stack<IEnumerator<T>>(16);

        try {
            todo.Push(tree.Children.GetEnumerator());

            while (todo.Count > 0) {
                var children = todo.Peek();
                if (children.MoveNext()) {
                    var currentNode = children.Current;
                    // ReSharper disable once AssignNullToNotNullAttribute (todo only contains enumerators containing non-null trees; see IRecursiveStructure.Children)
                    yield return currentNode;
                    // ReSharper disable once PossibleNullReferenceException (todo only contains enumerators containing non-null trees)
                    todo.Push(currentNode.Children.GetEnumerator());
                } else {
                    children.Dispose();
                    _ = todo.Pop();
                }
            }
        } finally {
            while (todo.Count > 0) {
                todo.Pop().Dispose();
            }
        }
    }

    /// <summary>
    /// Builds a copy of this tree with the same structure, but with different node values, as computed by the mapper argument.
    /// mapper is called bottom-up, in reverse preorder traversal (i.e. children before the node, and the last child first before the first).
    /// </summary>
    [Pure]
    public static Tree<TR> SelectNodeValue<TTree, T, TR>(this IRecursiveStructure<TTree, T> tree, Func<T, TR> mapper)
        where TTree : IRecursiveStructure<TTree, T>
        => tree.Select(node => mapper(node.NodeValue));

    /// <summary>
    /// Builds a copy of this tree with the same structure, but with different node values, as computed by the mapper argument.
    /// mapper is called bottom-up, in reverse preorder traversal (i.e. children before the node, and the last child first before the first).
    /// </summary>
    [Pure]
    public static Tree<TR> Select<TTree, TR>(this IRecursiveStructure<TTree> tree, Func<TTree, TR> mapper)
        where TTree : IRecursiveStructure<TTree>
        => TreeBuilder<TTree, Tree<TR>>.Build(tree.TypedThis, o => o.Children, (o, kids) => Tree.Node(mapper(o), kids));

    /// <summary>
    /// Recreates a copy of this tree with both structure and node-values altered, as computed by the mapper arguments.
    /// mapValue and mapStructure are called exactly once for each node in the tree.
    /// mapValue is passed the old node and should return its new value.
    /// mapStructure is passed the old node, its new value, and the already-mapped children and should return the nodes that should replace the old one in the tree.
    /// </summary>
    [Pure]
    public static Tree<TR>[] Rebuild<TTree, TR>(this IRecursiveStructure<TTree> tree, Func<TTree, TR> mapValue, Func<TTree, TR, Tree<TR>[], Tree<TR>[]?> mapStructure)
        where TTree : IRecursiveStructure<TTree>
        => TreeBuilder<TTree, Tree<TR>[]>.Build(tree.TypedThis, n => n.Children, (n, kids) => mapStructure(n, mapValue(n), kids.ConcatArrays()).EmptyIfNull());

    /// <summary>
    /// Builds a copy of this tree with the same vales, but with some subtrees optionally removed.
    /// The filter function is called for parents before children, and is passed the *input* subtree that may or may not be retained,
    /// if a subtree is NOT retained, then retainSubTree is not called for any descendants.
    /// </summary>
    [Pure]
    public static Tree<T>? Where<TTree, T>(this IRecursiveStructure<TTree, T> tree, Func<TTree, bool> retainSubTree)
        where TTree : IRecursiveStructure<TTree, T>
        => tree.TypedThis is var treeTyped && retainSubTree(treeTyped)
            ? TreeBuilder<TTree, Tree<T>>.Build(treeTyped, o => o.Children.Where(retainSubTree), (o, kids) => Tree.Node(o.NodeValue, kids))
            : null;

    /// <summary>
    /// Builds a copy of this tree with the same vales, but with some subtrees optionally removed.
    /// The filter function is called for children before parents, and is passed BOTH the originalInput and output subtrees that may or may not be retained;
    /// i.e. it will be called for the root node last, and that root node may differ from the initial root node as subtrees have already been pruned.
    /// </summary>
    [Pure]
    public static Tree<T>? WherePostFilter<TTree, T>(this IRecursiveStructure<TTree, T> tree, Func<(TTree originalNode, Tree<T>[] filteredKids), bool> retainSubTree)
        where TTree : IRecursiveStructure<TTree, T>
        => TreeBuilder<TTree, Tree<T>?>.Build(
            tree.TypedThis,
            o => o.Children,
            (o, kids) => kids.WhereNotNull() is var newKids && retainSubTree((o, newKids)) ? Tree.Node(o.NodeValue, newKids) : null
        );

    /// <summary>
    /// Builds a copy of this tree with the same vales, but with some subtrees optionally removed.
    /// The filter function is called for children before parents, and is passed the *output* subtree that may or may not be retained;
    /// i.e. it will be called for the root node last, and that root node may differ from the initial root node as subtrees have already been pruned.
    /// </summary>
    [Pure]
    public static Tree<T>? WhereNodeValue<TTree, T>(this IRecursiveStructure<TTree, T> tree, Func<T, bool> retainSubTree)
        where TTree : IRecursiveStructure<TTree, T>
        => Where(tree, o => retainSubTree(o.NodeValue));
}
