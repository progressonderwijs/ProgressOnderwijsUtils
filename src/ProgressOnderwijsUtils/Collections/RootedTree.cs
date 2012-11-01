using System.Linq;
using System.Collections.Generic;
using System;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils.Collections
{
	public struct RootedTree<T> : IEquatable<RootedTree<T>>, IRecursiveStructure<RootedTree<T>>
	{
		public static RootedTree<T> RootTree(Tree<T> rootNode) { return new RootedTree<T>(SList.SingleElement(new TreePathSegment(0, rootNode))); }

		public IEnumerable<RootedTree<T>> PathSelfToRoot() { return PathSegments.NonEmpySuffixes.Select(path => new RootedTree<T>(path)); }

		public int IndexInParent() { return PathSegments.Head.Index; }
		public Tree<T> UnrootedSubTree() { return PathSegments.Head.ThisSubTree; }

		public IReadOnlyList<RootedTree<T>> Children
		{
			get
			{
				var treePathSegments = PathSegments;
				return UnrootedSubTree().Children.SelectIndexable((kid, i) => new RootedTree<T>(treePathSegments.Prepend(new TreePathSegment(i, kid))));
			}
		}

		public T NodeValue { get { return UnrootedSubTree().NodeValue; } }
		public bool IsRoot { get { return PathSegments.Tail.IsEmpty; } }
		public bool HasValue { get { return !PathSegments.IsEmpty; } }
		public RootedTree<T> Parent { get { return new RootedTree<T>(PathSegments.Tail); } }
		#region Equality implementation
		public bool Equals(RootedTree<T> other)
		{
			//two rooted trees are identical when their underlying trees are identical and their paths within that tree are identical.
			return PathSegments.Last().ThisSubTree.Equals(other.PathSegments.Last().ThisSubTree)
				&& PathSegments.Select(segment => segment.Index).SequenceEqual(other.PathSegments.Select(segment => segment.Index));
		}
		public override int GetHashCode()
		{
			return PathSegments.Last().ThisSubTree.GetHashCode() + PathSegments.Select(segment => segment.Index).GetSequenceHashCode();
		}
		public override bool Equals(object obj) { return obj is RootedTree<T> && Equals((RootedTree<T>)obj); }
		#endregion

		#region internal details
		RootedTree(SList<TreePathSegment> pathSegments) { PathSegments = pathSegments; }
		readonly SList<TreePathSegment> PathSegments;
		struct TreePathSegment
		{
			public readonly int Index;
			public readonly Tree<T> ThisSubTree;
			public T NodeValue { get { return ThisSubTree.NodeValue; } }
			public IReadOnlyList<Tree<T>> Children { get { return ThisSubTree.Children; } }

			public TreePathSegment(int index, Tree<T> node) { Index = index; ThisSubTree = node; }
		}
		#endregion
	}
}