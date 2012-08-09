using System.Linq;
using System.Collections.Generic;
using System;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
	public static class TreeExtensions
	{
		public static RootedTree<T> RootHere<T>(this Tree<T> tree) { return RootedTree<T>.RootTree(tree); }

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