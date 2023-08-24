namespace ProgressOnderwijsUtils.Collections;

public readonly struct TreeCursor<T> : IEquatable<TreeCursor<T>>, IRecursiveStructure<TreeCursor<T>, T>
{
    public static TreeCursor<T> CreateAtRoot(Tree<T> rootNode)
        => new(SList.SingleElement(new TreePathSegment(0, rootNode)));

    public IEnumerable<TreeCursor<T>> PathSelfToRoot()
        => PathSegments.NonEmptySuffixes.Select(path => new TreeCursor<T>(path));

    public int IndexInParent()
        => PathSegments.Head.Index;

    public Tree<T> ToSubTree()
        => PathSegments.Head.ThisSubTree;

    public IReadOnlyList<TreeCursor<T>> Children
    {
        get {
            var treePathSegments = PathSegments;
            return ToSubTree().Children.SelectIndexable((kid, i) => new TreeCursor<T>(treePathSegments.Prepend(new TreePathSegment(i, kid))));
        }
    }

    public T NodeValue
        => ToSubTree().NodeValue;

    public bool IsRoot
        => PathSegments.Tail.IsEmpty;

    public bool HasValue
        => !PathSegments.IsEmpty;

    public TreeCursor<T> Parent
        => new(PathSegments.Tail);

    public Tree<T> Root
        => PathSegments.Last().ThisSubTree;

    public TreeCursor<T> PreviousSibling()
        => !HasValue || IsRoot || IndexInParent() == 0 || Parent.Children.Count <= 1 ? new() : Parent.Children[IndexInParent() - 1];

    public TreeCursor<T> NextSibling()
        => !HasValue || IsRoot || IndexInParent() + 1 >= Parent.Children.Count ? new() : Parent.Children[IndexInParent() + 1];

    public bool Equals(TreeCursor<T> other)
        //two rooted trees are identical when their underlying trees are identical and their paths within that tree are identical.
        => PathSegments.Last().ThisSubTree.Equals(other.PathSegments.Last().ThisSubTree)
            && PathSegments.SelectEager(segment => segment.Index).SequenceEqual(other.PathSegments.SelectEager(segment => segment.Index));

    public override int GetHashCode()
        => PathSegments.Last().ThisSubTree.GetHashCode() + EnumerableExtensions.GetSequenceHashCode(PathSegments.SelectEager(segment => segment.Index));

    public override bool Equals(object? obj)
        => obj is TreeCursor<T> rootedTree && Equals(rootedTree);

    // internal details:
    TreeCursor(SList<TreePathSegment> pathSegments)
        => PathSegments = pathSegments;

    readonly SList<TreePathSegment> PathSegments;

    readonly struct TreePathSegment
    {
        public readonly int Index;
        public readonly Tree<T> ThisSubTree;

        public TreePathSegment(int index, Tree<T> node)
        {
            Index = index;
            ThisSubTree = node;
        }
    }

    [Pure]
    public TreeCursor<T> ReplaceSubTree(Tree<T> newSubTree)
    {
        if (IsRoot) {
            return newSubTree.CursorForThisRoot();
        } else {
            var parentSubTree = PathSegments.Tail.Head.ThisSubTree;
            var myIndex = PathSegments.Head.Index;
            var newSiblings = CopyArrayWithNewValueOnIndex(parentSubTree.Children, myIndex, newSubTree);
            var newParentSubTree = Tree.Node(parentSubTree.NodeValue, newSiblings);
            var newParent = Parent.ReplaceSubTree(newParentSubTree);
            return new(newParent.PathSegments.Prepend(new TreePathSegment(myIndex, newSubTree)));
        }
    }

    [Pure]
    static Tree<T>[] CopyArrayWithNewValueOnIndex(IReadOnlyList<Tree<T>> oldArray, int index, Tree<T> newValue)
    {
        var copy = oldArray.ToArray();
        copy[index] = newValue;
        return copy;
    }

    TreeCursor<T> IRecursiveStructure<TreeCursor<T>>.TypedThis
        => this;

    public static bool operator ==(TreeCursor<T> left, TreeCursor<T> right)
        => left.Equals(right);

    public static bool operator !=(TreeCursor<T> left, TreeCursor<T> right)
        => !(left == right);
}
