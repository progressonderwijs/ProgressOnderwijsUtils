using System.Collections.Generic;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
    public static class TreeExtensions
    {
        [Pure]
        public static RootedTree<T> RootHere<T>([NotNull] this Tree<T> tree)
        {
            return RootedTree<T>.RootTree(tree);
        }

        [ItemNotNull]
        [Pure]
        public static IEnumerable<T> PreorderTraversal<T>([NotNull] this T tree)
            where T : IRecursiveStructure<T>
        {
            yield return tree;

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
                        todo.Pop();
                    }
                }
            } finally {
                while (todo.Count > 0) {
                    todo.Pop().Dispose();
                }
            }
        }
    }
}
