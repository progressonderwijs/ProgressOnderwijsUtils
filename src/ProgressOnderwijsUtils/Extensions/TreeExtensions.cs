using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
    public static class TreeExtensions
    {
        [Pure]
        public static int Height<TTree>(this IRecursiveStructure<TTree> tree)
            where TTree : IRecursiveStructure<TTree>
        {
            var maxHeight = 0;
            var todo = new Stack<(IRecursiveStructure<TTree> tree, int height)>(16);
            todo.Push((tree, 1));

            while (todo.Count > 0) {
                var (next, height) = todo.Pop();
                foreach (var kid in next.Children) {
                    todo.Push((kid, height + 1));
                }
                if (height > maxHeight) {
                    maxHeight = height;
                }
            }
            return maxHeight;
        }

        [Pure]
        public static RootedTree<T> RootHere<T>(this Tree<T> tree)
            => RootedTree<T>.RootTree(tree);

        [Pure]
        public static IEnumerable<T> PreorderTraversal<T>(this T tree)
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
