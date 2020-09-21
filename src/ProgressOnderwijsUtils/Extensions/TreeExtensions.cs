using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
    public static class TreeExtensions
    {
        public static TTree[] AsSingletonArray<TTree>(this TTree tree)
            where TTree : IRecursiveStructure<TTree> //to avoid namepsace pollution
            => new[] { tree };

        [Pure]
        public static int Height<TTree, T>(this IRecursiveStructure<TTree, T> tree)
            where TTree : IRecursiveStructure<TTree>
        {
            var maxHeight = 0;
            var todo = new Stack<(Tree<T>, int)>(16);
            todo.Push((tree.UnrootedSubTree(), 1));

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

        /// <summary>
        /// Builds a copy of this tree with the same structure, but with different node values, as computed by the mapper argument.
        /// mapper is called bottom-up, in reverse preorder traversal (i.e. children before the node, and the last child first before the first).
        /// </summary>
        [Pure]
        public static Tree<TR> Select<TTree, T, TR>(this IRecursiveStructure<TTree, T> tree, Func<T, TR> mapper)
            where TTree : IRecursiveStructure<TTree, T>
            => tree.TypedThis.Select(node => mapper(node.NodeValue));

        /// <summary>
        /// Builds a copy of this tree with the same structure, but with different node values, as computed by the mapper argument.
        /// mapper is called bottom-up, in reverse preorder traversal (i.e. children before the node, and the last child first before the first).
        /// </summary>
        [Pure]
        public static Tree<TR> Select<TTree, TR>(this TTree tree, Func<TTree, TR> mapper)
            where TTree : IRecursiveStructure<TTree>
            => CachedTreeBuilder<TTree, TR>.Resolve(tree, o => o.Children, (o, kids) => Tree.Node(mapper(o), kids));

        /// <summary>
        /// Recreates a copy of this tree with both structure and node-values altered, as computed by the mapper arguments.
        /// mapValue and mapStructure are called exactly once for each node in the tree.
        /// mapValue is passed the old node and should return its new value.
        /// mapStructure is passed the old node, its new value, and the already-mapped children and should return the nodes that should replace the old one in the tree.
        /// </summary>
        [Pure]
        public static Tree<TR>[] Rebuild<TTree, TR>(this TTree tree, Func<TTree, TR> mapValue, Func<TTree, TR, Tree<TR>[], Tree<TR>[]?> mapStructure)
            where TTree : IRecursiveStructure<TTree>
        {
            var collectionSelector = Utils.F((Tree<Tree<TR>[]> o) => o.NodeValue);
            return CachedTreeBuilder<TTree, Tree<TR>[]>.Resolve(tree, n => n.Children, (n, kids) => Tree.Node(mapStructure(n, mapValue(n), kids.SelectMany(collectionSelector)).EmptyIfNull())).NodeValue;
        }

        /// <summary>
        /// Recreates a copy of this tree with both structure and node-values altered, as computed by the mapper arguments.
        /// mapValue and mapStructure are called exactly once for each node in the tree.
        /// mapValue is passed the old node and should return its new value.
        /// mapStructure is passed the old node, its new value, and the already-mapped children and should return the nodes that should replace the old one in the tree.
        /// </summary>
        [Pure]
        public static Tree<TR>[] Rebuild<TTree, TR>(this TTree tree, Func<TTree, TR> mapValue, Func<TTree, TR, Tree<TR>[], IEnumerable<Tree<TR>>?> mapStructure)
            where TTree : IRecursiveStructure<TTree>
        {
            var collectionSelector = Utils.F((Tree<Tree<TR>[]> o) => o.NodeValue);
            return CachedTreeBuilder<TTree, Tree<TR>[]>.Resolve(tree, n => n.Children, (n, kids) => Tree.Node(mapStructure(n, mapValue(n), kids.SelectMany(collectionSelector)).EmptyIfNull().ToArray())).NodeValue;
        }

        /// <summary>
        /// Builds a copy of this tree with the same vales, but with some subtrees optionally removed.
        /// The filter function is called for children before parents, and is passed the *output* subtree that may or may not be retained;
        /// i.e. it will be called for the root node last, and that root node may differ from the initial root node as subtrees have already been pruned.
        /// </summary>
        [Pure]
        public static Tree<T>? Where<TTree, T>(this IRecursiveStructure<TTree, T> tree, Func<TTree, bool> retainSubTree)
            where TTree : IRecursiveStructure<TTree, T>
            => tree.TypedThis is var treeTyped && retainSubTree(treeTyped)
                ? CachedTreeBuilder<TTree, T>.Resolve(treeTyped, o => o.Children.Where(retainSubTree), (o, kids) => Tree.Node(o.NodeValue, kids))
                : null;

        /// <summary>
        /// Builds a copy of this tree with the same vales, but with some subtrees optionally removed.
        /// The filter function is called for children before parents, and is passed the *output* subtree that may or may not be retained;
        /// i.e. it will be called for the root node last, and that root node may differ from the initial root node as subtrees have already been pruned.
        /// </summary>
        [Pure]
        public static Tree<T>? Where<TTree, T>(this IRecursiveStructure<TTree, T> tree, Func<T, bool> retainSubTree)
            where TTree : IRecursiveStructure<TTree, T>
            => Where(tree, o => retainSubTree(o.NodeValue));
    }
}
