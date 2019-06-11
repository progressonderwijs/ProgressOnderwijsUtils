using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Collections
{
    public struct RootedTree<T> : IEquatable<RootedTree<T>>, IRecursiveStructure<RootedTree<T>>
    {
        public static RootedTree<T> RootTree([NotNull] Tree<T> rootNode)
            => new RootedTree<T>(SList.SingleElement(new TreePathSegment(0, rootNode)));

        [NotNull]
        public IEnumerable<RootedTree<T>> PathSelfToRoot()
            => PathSegments.NonEmptySuffixes.Select(path => new RootedTree<T>(path));

        public int IndexInParent()
            => PathSegments.Head.Index;

        [NotNull]
        public Tree<T> UnrootedSubTree()
            => PathSegments.Head.ThisSubTree;

        public IReadOnlyList<RootedTree<T>> Children
        {
            get {
                var treePathSegments = PathSegments;
                return UnrootedSubTree().Children.SelectIndexable((kid, i) => new RootedTree<T>(treePathSegments.Prepend(new TreePathSegment(i, kid))));
            }
        }

        public T NodeValue
            => UnrootedSubTree().NodeValue;

        public bool IsRoot
            => PathSegments.Tail.IsEmpty;

        public bool HasValue
            => !PathSegments.IsEmpty;

        public RootedTree<T> Parent
            => new RootedTree<T>(PathSegments.Tail);

        public RootedTree<T> Root
            => PathSegments.Last().ThisSubTree.RootHere();

        public bool Equals(RootedTree<T> other)
            //two rooted trees are identical when their underlying trees are identical and their paths within that tree are identical.
            => PathSegments.Last().ThisSubTree.Equals(other.PathSegments.Last().ThisSubTree)
                && PathSegments.SelectEager(segment => segment.Index).SequenceEqual(other.PathSegments.SelectEager(segment => segment.Index));

        public override int GetHashCode()
            => PathSegments.Last().ThisSubTree.GetHashCode() + EnumerableExtensions.GetSequenceHashCode(PathSegments.SelectEager(segment => segment.Index));

        public override bool Equals(object obj)
            => obj is RootedTree<T> rootedTree && Equals(rootedTree);

        // internal details:
        RootedTree(SList<TreePathSegment> pathSegments)
            => PathSegments = pathSegments;

        readonly SList<TreePathSegment> PathSegments;

        struct TreePathSegment
        {
            public readonly int Index;

            [NotNull]
            public readonly Tree<T> ThisSubTree;

            public TreePathSegment(int index, [NotNull] Tree<T> node)
            {
                Index = index;
                ThisSubTree = node;
            }
        }

        [Pure]
        public RootedTree<T> ReplaceSubTree([NotNull] Tree<T> newSubTree)
        {
            if (IsRoot) {
                return newSubTree.RootHere();
            } else {
                var parentSubTree = PathSegments.Tail.Head.ThisSubTree;
                var myIndex = PathSegments.Head.Index;
                var newSiblings = CopyArrayWithNewValueOnIndex(parentSubTree.Children, myIndex, newSubTree);
                var newParentSubTree = Tree.Node(parentSubTree.NodeValue, newSiblings);
                var newParent = Parent.ReplaceSubTree(newParentSubTree);
                return new RootedTree<T>(newParent.PathSegments.Prepend(new TreePathSegment(myIndex, newSubTree)));
            }
        }

        [NotNull]
        [Pure]
        static Tree<T>[] CopyArrayWithNewValueOnIndex([NotNull] IReadOnlyList<Tree<T>> oldArray, int index, Tree<T> newValue)
        {
            var copy = oldArray.ToArray();
            copy[index] = newValue;
            return copy;
        }
    }
}
