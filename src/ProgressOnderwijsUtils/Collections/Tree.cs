using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils.Collections
{
	public sealed class Tree<T> : IEquatable<Tree<T>>
	{
		static readonly ReadOnlyCollection<Tree<T>> EmptyArray = new Tree<T>[0].AsReadOnlyView(); // cache this since it will be used very commonly.

		readonly T nodeValue;
		readonly ReadOnlyCollection<Tree<T>> kidArray;
		public T NodeValue { get { return nodeValue; } }
		public ReadOnlyCollection<Tree<T>> Children { get { return kidArray; } }

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
			kidArray = children == null || children.Length == 0 ? EmptyArray : children.AsReadOnlyView();
		}
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
	}

	public static class Tree
	{
		public static Tree<T> Node<T>(T value, IEnumerable<Tree<T>> children)
		{
			return new Tree<T>(value, children);
		}
		public static Tree<T> Node<T>(T value, Tree<T> a)
		{
			return new Tree<T>(value, new[] { a, });
		}
		public static Tree<T> Node<T>(T value, Tree<T> a, Tree<T> b)
		{
			return new Tree<T>(value, new[] { a, b });
		}
		public static Tree<T> Node<T>(T value, Tree<T> a, Tree<T> b, Tree<T> c)
		{
			return new Tree<T>(value, new[] { a, b, c });
		}
		public static Tree<T> Node<T>(T value, params  Tree<T>[] kids)
		{
			return new Tree<T>(value, kids);
		}
		public static Tree<T> Node<T>(T value)
		{
			return new Tree<T>(value, null);
		}

		public static IEqualityComparer<Tree<T>> EqualityComparer<T>(IEqualityComparer<T> valueComparer)
		{
			return new Tree<T>.Comparer(valueComparer);
		}
	}

	public static class TreeExtensions
	{
		public struct TreePathSegment<T>
		{
			public readonly int Index;
			public readonly Tree<T> SubTree;
			public TreePathSegment(int index, Tree<T> node)
			{
				Index = index;
				SubTree = node;
			}
		}

		public struct TreeNodeWithPath<T>
		{
			public readonly Tree<T> Tree;
			public readonly SList<TreePathSegment<T>> PathToRoot;
			public TreeNodeWithPath(Tree<T> tree, SList<TreePathSegment<T>> pathToRoot)
			{
				Tree = tree;
				PathToRoot = pathToRoot;
			}
		}

		public static IEnumerable<SList<TreePathSegment<T>>> PreorderTraversal<T>(this Tree<T> tree)
		{
			var path = SList<TreePathSegment<T>>.Empty.Prepend(new TreePathSegment<T>(0, tree));
			yield return path;

			while (!path.IsEmpty)
			{
				var current = path.Head.SubTree;
				if (current.Children.Count > 0)
				{ //iterate children.
					var node = current.Children[0];
					path = path.Prepend(new TreePathSegment<T>(0, node));
					yield return path;
				}
				else
				{
					//one of three scenarios:
					// scenario (1):  I have no parent and hence no siblings; cannot move to next node therefore: DONE!
					// scenario (2):  I have a parent and that parent has a child after me: go to that child.
					// scenario (3):  I have a parent but no subsequent siblings: move past parent (i.e. loop to parent.)

					int nextIndex = path.Head.Index + 1;
					path = path.Tail;
					while (!path.IsEmpty) // scenario (1) or (2)
					{
						var container = path.Head.SubTree;

						if (container.Children.Count > nextIndex)
						{ //scenario (2)
							path = path.Prepend(new TreePathSegment<T>(nextIndex, container.Children[nextIndex]));
							yield return path;
							break;
						}
						else
						{ //scenario (3)
							nextIndex = path.Head.Index + 1;
							path = path.Tail;
							if (path.IsEmpty)
								yield break;//scenario (1)
						}
					}
				}
			}
		}
	}
}
