using System.Collections.Generic;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
    public static class TreeExtensions
    {
        [Pure]
        public static RootedTree<T> RootHere<T>(this Tree<T> tree)
        {
            return RootedTree<T>.RootTree(tree);
        }

        [Pure]
        public static IEnumerable<T> PreorderTraversal<T>(this T tree) where T : IRecursiveStructure<T>
        {
            yield return tree;

            var todo = new Stack<IEnumerator<T>>(16);

            try {
                todo.Push(tree.Children.GetEnumerator());

                while (todo.Count > 0) {
                    var children = todo.Peek();
                    if (children.MoveNext()) {
                        var currentNode = children.Current;
                        yield return currentNode;
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
