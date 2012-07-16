using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils.Collections
{
	public sealed class Tree<T>
	{
		static readonly Tree<T>[] EmptyArray = new Tree<T>[0];

		readonly T nodeValue;
		readonly Tree<T>[] kidArray;//children is either null (leaf node) or length>0.  This optimization matters since there are many leaf nodes.
		public T NodeValue { get { return nodeValue; } }
		public ReadOnlyCollection<Tree<T>> Children { get { return (kidArray ?? EmptyArray).AsReadOnlyView(); } }
		public Tree(T value, IEnumerable<Tree<T>> children)
		{
			nodeValue = value;

			if (children == null)
				kidArray = null;
			else
			{
				var array = children.ToArray();
				kidArray = array.Length == 0 ? null : array;
			}
		}
	}

	public static class Tree
	{
		public static Tree<T> CreateNode<T>(T value, IEnumerable<Tree<T>> children)
		{
			return new Tree<T>(value, children);
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

		public class TreePathEnumerator<T> : IEnumerator<SList<TreePathSegment<T>>>
		{
			Tree<T> current;
			SList<TreePathSegment<T>> path;

			public SList<TreePathSegment<T>> Current { get { return path; } }
			object IEnumerator.Current { get { return Current; } }

			void IDisposable.Dispose() { current = null; path = default(SList<TreePathSegment<T>>); }

			public bool MoveNext()
			{
				if (current.Children.Count > 0)
				{ //iterate children.
					var node = current.Children[0];
					path = path.Prepend(new TreePathSegment<T>(0, node));
					current = node;
					return true;
				}
				else
				{ // no children; go to element past current
					return MovePastCurrent();
				}
			}

			bool MovePastCurrent()
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
						return true;
					}
					else
					{ //scenario (3)
						nextIndex = path.Head.Index + 1;
						path = path.Tail;
					}
				}
				return false;//scenario (3)
			}

			public void Reset() { throw new NotSupportedException(); }
		}


		public static IEnumerable<SList<TreePathSegment<T>>> PreorderTraversal<T>(this Tree<T> tree)
		{
			var path = SList<TreePathSegment<T>>.Empty.Prepend(new TreePathSegment<T>(0, tree));
			yield return path;

			while (!path.IsEmpty) {
				var current = path.Head.SubTree;
				if (current.Children.Count > 0)
				{ //iterate children.
					var node = current.Children[0];
					path = path.Prepend(new TreePathSegment<T>(0, node));
					yield return path;
				}
				else {
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
								yield break;//scenario (3)
						}
					}
				}
			}
		}
	}
}
