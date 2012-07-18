using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils.Collections
{
	public interface ITree<out TNode> where TNode : ITree<TNode>
	{
		IArrayView<TNode> Children { get; }
	}

	public sealed class Tree<T> : IEquatable<Tree<T>>, ITree<Tree<T>>
	{
		static readonly IArrayView<Tree<T>> EmptyArray = new Tree<T>[0].AsReadView(); // cache this since it will be used very commonly.

		readonly T nodeValue;
		readonly IArrayView<Tree<T>> kidArray;
		public T NodeValue { get { return nodeValue; } }
		public IArrayView<Tree<T>> Children { get { return kidArray; } }

		/// <summary>
		/// Creates a Tree with specified child nodes.  The child node enumeration is materialized using ToArray() before usage.
		/// </summary>
		/// <param name="value">The value of this node.</param>
		/// <param name="children">The children of this node, (null is allowed and means none).</param>
		public Tree(T value, IEnumerable<Tree<T>> children) : this(value, children == null ? null : children.ToArray()) { }

		/// <summary>
		/// Creates a Tree with specified child nodes.  The child node array is used directly. Do not mutate the array after passing it into the tree; doing so
		/// results in undefined behavior.
		/// </summary>
		/// <param name="value">The value of this node.</param>
		/// <param name="children">The children of this node, (null is allowed and means none).</param>
		public Tree(T value, Tree<T>[] children)
		{
			nodeValue = value;
			kidArray = children == null || children.Length == 0 ? EmptyArray : new ArrayView<Tree<T>>(children);
		}
		#region Equality implementation
		public static readonly Comparer DefaultComparer = new Comparer(EqualityComparer<T>.Default);
		public sealed class Comparer : IEqualityComparer<Tree<T>>
		{
			static readonly int typeHash = typeof(Tree<T>).GetHashCode();
			readonly IEqualityComparer<T> ValueComparer;
			public Comparer(IEqualityComparer<T> valueComparer) { ValueComparer = valueComparer; }

			public bool Equals(Tree<T> x, Tree<T> y)
			{
				return
					ReferenceEquals(x, y) ||
					!ReferenceEquals(x, null) && !ReferenceEquals(y, null)
					&& x.Children.Count == y.Children.Count
					&& ValueComparer.Equals(x.NodeValue, y.NodeValue)
					&& x.Children.SequenceEqual(y.Children, this);
			}

			ulong InternalHash(Tree<T> obj)
			{
				ulong hash = (uint)ValueComparer.GetHashCode(obj.NodeValue);
				ulong offset = 1;
				foreach (var kid in obj.Children)
				{
					hash += offset * InternalHash(kid);
					offset += 2;
				}
				return hash;
			}

			public int GetHashCode(Tree<T> obj)
			{
				if (obj == null)
					return typeHash;
				var hash = InternalHash(obj);
				return (int)((uint)(hash >> 32) + (uint)hash);
			}
		}
		public override bool Equals(object obj) { return DefaultComparer.Equals(this, obj as Tree<T>); }
		public bool Equals(Tree<T> other) { return DefaultComparer.Equals(this, other); }
		public override int GetHashCode() { return DefaultComparer.GetHashCode(this); }
		#endregion
	}

	public static class Tree
	{
		public static Tree<T> Node<T>(T value, IEnumerable<Tree<T>> children) { return new Tree<T>(value, children); }
		public static Tree<T> Node<T>(T value, Tree<T> a) { return new Tree<T>(value, new[] { a, }); }
		public static Tree<T> Node<T>(T value, Tree<T> a, Tree<T> b) { return new Tree<T>(value, new[] { a, b }); }
		public static Tree<T> Node<T>(T value, Tree<T> a, Tree<T> b, Tree<T> c) { return new Tree<T>(value, new[] { a, b, c }); }
		public static Tree<T> Node<T>(T value, params  Tree<T>[] kids) { return new Tree<T>(value, kids); }
		public static Tree<T> Node<T>(T value) { return new Tree<T>(value, null); }

		public static IEqualityComparer<Tree<T>> EqualityComparer<T>(IEqualityComparer<T> valueComparer) { return new Tree<T>.Comparer(valueComparer); }
	}

	public struct RootedTreeView<T> : IEquatable<RootedTreeView<T>>, ITree<RootedTreeView<T>>
	{
		public readonly SList<TreePathSegment> PathSegments;

		public RootedTreeView(SList<TreePathSegment> pathSegments) { PathSegments = pathSegments; }

		public int Index { get { return PathSegments.Head.Index; } }
		public Tree<T> ThisSubTree { get { return PathSegments.Head.ThisSubTree; } }

		public IArrayView<RootedTreeView<T>> Children
		{
			get
			{
				var treePathSegments = PathSegments;
				return ThisSubTree.Children.Select((kid, i) => new RootedTreeView<T>(treePathSegments.Prepend(new TreePathSegment(i, kid))));
			}
		}

		public T NodeValue { get { return ThisSubTree.NodeValue; } }
		public bool IsRoot { get { return PathSegments.Tail.IsEmpty; } }
		public bool Exists { get { return !PathSegments.IsEmpty; } }
		public RootedTreeView<T> Parent { get { return new RootedTreeView<T>(PathSegments.Tail); } }

		public struct TreePathSegment
		{
			public readonly int Index;
			public readonly Tree<T> ThisSubTree;
			public T NodeValue { get { return ThisSubTree.NodeValue; } }
			public IArrayView<Tree<T>> Children { get { return ThisSubTree.Children; } }

			public TreePathSegment(int index, Tree<T> node) { Index = index; ThisSubTree = node; }
		}
		#region Equality implementation
		public bool Equals(RootedTreeView<T> other)
		{
			//two rooted trees are identical when their underlying trees are identical and their paths within that tree are identical.
			return PathSegments.Last().Equals(other.PathSegments.Last())
				&& PathSegments.Select(segment => segment.Index).SequenceEqual(other.PathSegments.Select(segment => segment.Index));
		}
		public override int GetHashCode()
		{
			return PathSegments.Last().GetHashCode() + PathSegments.Select(segment => segment.Index).GetSequenceHashCode();
		}
		public override bool Equals(object obj) { return obj is RootedTreeView<T> && Equals((RootedTreeView<T>)obj); }
		#endregion
	}

	public static class TreeExtensions
	{
		public static RootedTreeView<T> RootHere<T>(this Tree<T> tree) { return new RootedTreeView<T>(SList.SingleElement(new RootedTreeView<T>.TreePathSegment(0, tree))); }

		public static IEnumerable<T> PreorderTraversal<T>(this T tree) where T : ITree<T>
		{
			yield return tree;

			Stack<IEnumerator<T>> todo = new Stack<IEnumerator<T>>(16);

			try
			{
				todo.Push(tree.Children.GetEnumerator());

				while (todo.Count > 0)
				{
					var current = todo.Peek();
					if (current.MoveNext())
					{
						yield return current.Current;
						todo.Push(current.Current.Children.GetEnumerator());
					}
					else
					{
						current.Dispose();
						todo.Pop();
					}
				}
			}
			finally
			{
				while (todo.Count > 0)
					todo.Pop().Dispose();
			}
		}
	}

}
