using System.Linq;
using System.Collections.Generic;
using System;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils.Collections
{
    public struct RootedTree<T> : IEquatable<RootedTree<T>>, IRecursiveStructure<RootedTree<T>>
    {
        public static RootedTree<T> RootTree(Tree<T> rootNode) => new RootedTree<T>(SList.SingleElement(new TreePathSegment(0, rootNode)));
        public IEnumerable<RootedTree<T>> PathSelfToRoot() => PathSegments.NonEmpySuffixes.Select(path => new RootedTree<T>(path));
        public int IndexInParent() => PathSegments.Head.Index;
        public Tree<T> UnrootedSubTree() => PathSegments.Head.ThisSubTree;

        public IReadOnlyList<RootedTree<T>> Children
        {
            get {
                var treePathSegments = PathSegments;
                return UnrootedSubTree().Children.SelectIndexable((kid, i) => new RootedTree<T>(treePathSegments.Prepend(new TreePathSegment(i, kid))));
            }
        }

        public T NodeValue => UnrootedSubTree().NodeValue;
        public bool IsRoot => PathSegments.Tail.IsEmpty;
        public bool HasValue => !PathSegments.IsEmpty;
        public RootedTree<T> Parent => new RootedTree<T>(PathSegments.Tail);

        public bool Equals(RootedTree<T> other)
        {
            //two rooted trees are identical when their underlying trees are identical and their paths within that tree are identical.
            return PathSegments.Last().ThisSubTree.Equals(other.PathSegments.Last().ThisSubTree)
                && PathSegments.SelectEager(segment => segment.Index).SequenceEqual(other.PathSegments.SelectEager(segment => segment.Index));
        }

        public override int GetHashCode()
        {
            return PathSegments.Last().ThisSubTree.GetHashCode() + EnumerableExtensions.GetSequenceHashCode(PathSegments.SelectEager(segment => segment.Index));
        }

        public override bool Equals(object obj) => obj is RootedTree<T> && Equals((RootedTree<T>)obj);

        // internal details:
        RootedTree(SList<TreePathSegment> pathSegments)
        {
            PathSegments = pathSegments;
        }

        readonly SList<TreePathSegment> PathSegments;

        struct TreePathSegment
        {
            public readonly int Index;
            public readonly Tree<T> ThisSubTree;

            public TreePathSegment(int index, Tree<T> node)
            {
                Index = index;
                ThisSubTree = node;
            }
        }
    }
}
