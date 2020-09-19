using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Collections
{
    public interface IHasNodeValue<out T>
    {
        T NodeValue { get; }
    }

    public interface IRecursiveStructure<out TTree>
        where TTree : IRecursiveStructure<TTree>
    {
        IReadOnlyList<TTree> Children { get; }
    }

    public interface IRecursiveStructure<out TTree, T> : IRecursiveStructure<TTree>, IHasNodeValue<T>
        where TTree : IRecursiveStructure<TTree>
    {
        TTree TypedThis { get; }
        Tree<T> UnrootedSubTree();
    }

    public static class Tree
    {
        [Pure]
        public static Tree<T> Node<T>(T value, IEnumerable<Tree<T>> children)
            => new Tree<T>(value, children);

        // ReSharper disable MethodOverloadWithOptionalParameter
        [Pure]
        public static Tree<T> Node<T>(T value, params Tree<T>[]? kids)
            => new Tree<T>(value, kids);

        // ReSharper restore MethodOverloadWithOptionalParameter

        [Pure]
        public static Tree<T> Node<T>(T value)
            => new Tree<T>(value, null);
        [Pure]
        public static Tree<T> BuildRecursively<T>(T root, Func<T, IEnumerable<T>?> kidLookup)
            => CachedTreeBuilder<T,T>.Resolve(root, kidLookup, Node);

        [Pure]
        public static Tree<T> BuildRecursively<T>(T root, IReadOnlyDictionary<T, IReadOnlyList<T>> kidLookup)
            where T : notnull
            => BuildRecursively(root, arg => kidLookup.GetOrDefaultR(arg)?.AsEnumerable());

        [Pure]
        public static IEqualityComparer<Tree<T>?> EqualityComparer<T>(IEqualityComparer<T> valueComparer)
            => new Tree<T>.Comparer(valueComparer);

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
        {
            var todo = new Stack<TTree>(16);
            var reconstruct = new Stack<TTree>(16);
            todo.Push(tree);
            while (todo.Count > 0) {
                var next = todo.Pop();
                reconstruct.Push(next);
                var children = next.Children;
                for (var i = children.Count - 1; i >= 0; i--) {
                    todo.Push(children[i]);
                }
            }

            var output = new Stack<Tree<TR>>(16);
            while (reconstruct.Count > 0) {
                var next = reconstruct.Pop();
                var mappedChildren = new Tree<TR>[next.Children.Count];
                for (var i = 0; i < mappedChildren.Length; i++) {
                    mappedChildren[i] = output.Pop();
                }
                output.Push(Node(mapper(next), mappedChildren));
            }
            return output.Pop();
        }

        [Pure]
        public static Tree<TR> Select2<TTree, TR>(this TTree tree, Func<TTree, TR> mapper)
            where TTree : IRecursiveStructure<TTree>
            => CachedTreeBuilder<TTree,TR>.Resolve(tree, o=>o.Children, (o, kids) => Node(mapper(o), kids));


        [Pure]
        public static Tree<TR> Select3<TTree, TR>(this TTree tree, Func<TTree, TR> mapper)
            where TTree : IRecursiveStructure<TTree>
            => Rebuild(tree, mapper, (o,v, kids) => new []{ Node(v, kids) }).Single();


        /// <summary>
        /// Recreates a copy of this tree with both structure and node-values altered, as computed by the mapper arguments.
        /// mapValue and mapStructure are called exactly once for each node in the tree.
        /// mapValue is passed the old node and should return its new value.
        /// mapStructure is passed the old node, its new value, and the already-mapped children and should return the nodes that should replace the old one in the tree.
        ///
        /// mapValue and mapStructure are called bottom-up, in reverse preorder traversal (i.e. children before the node, and the last child first before the first).
        /// For a given node, mapValue is called first (and its return value passed to mapStructure).
        /// </summary>
        [Pure]
        public static Tree<TR>[] Rebuild<TTree, TR>(this TTree tree, Func<TTree, TR> mapValue, Func<TTree, TR, IReadOnlyList<Tree<TR>>, IEnumerable<Tree<TR>?>?> mapStructure)
            where TTree : IRecursiveStructure<TTree>
        {
            var todo = new Stack<TTree>(16);
            var reconstruct = new Stack<TTree>(16);
            todo.Push(tree);
            while (todo.Count > 0) {
                var next = todo.Pop();
                reconstruct.Push(next);
                var children = next.Children;
                for (var i = children.Count - 1; i >= 0; i--) {
                    todo.Push(children[i]);
                }
            }

            var output = new List<Tree<TR>[]>(16);
            var childBuilder = new List<Tree<TR>>(16);
            while (reconstruct.Count > 0) {
                var next = reconstruct.Pop();
                var oldChildren = next.Children;
                childBuilder.Clear();
                for (var i = 0; i < oldChildren.Count; i++) {
                    childBuilder.AddRange(output[^(1 + i)]);
                }
                output.RemoveRange(output.Count - oldChildren.Count, oldChildren.Count);
                var newNodes = mapStructure(next, mapValue(next), childBuilder);
                var newNodesArray = newNodes as Tree<TR>[] ?? newNodes?.ToArray() ?? Array.Empty<Tree<TR>>();
                var writeIdx = 0;
                for (var i = 0; i < newNodesArray.Length; i++) {
                    if (newNodesArray[i] != null) {
                        newNodesArray[writeIdx++] = newNodesArray[i];
                    }
                }
                if (writeIdx != newNodesArray.Length) {
                    if (writeIdx == 0) {
                        newNodesArray = Array.Empty<Tree<TR>>();
                    } else {
                        Array.Resize(ref newNodesArray, writeIdx);
                    }
                }
                output.Add(newNodesArray!);
            }
            return output[0];
        }

        /// <summary>
        /// Builds a copy of this tree with the same vales, but with some subtrees optionally removed.
        /// The filter function is called for children before parents, and is passed the *output* subtree that may or may not be retained;
        /// i.e. it will be called for the root node last, and that root node may differ from the initial root node as subtrees have already been pruned.
        /// </summary>
        [Pure]
        public static Tree<T>? Where2<TTree, T>(this IRecursiveStructure<TTree, T> tree, Func<TTree, bool> retainSubTree)
            where TTree : IRecursiveStructure<TTree, T>
            => CachedTreeBuilder<TTree, T>.Resolve(tree.TypedThis, o => o.Children.Where(retainSubTree), (o, kids) => Node(o.NodeValue, kids));

        /// <summary>
        /// Builds a copy of this tree with the same vales, but with some subtrees optionally removed.
        /// The filter function is called for children before parents, and is passed the *output* subtree that may or may not be retained;
        /// i.e. it will be called for the root node last, and that root node may differ from the initial root node as subtrees have already been pruned.
        /// </summary>
        [Pure]
        public static Tree<T>? Where3<TTree, T>(this IRecursiveStructure<TTree, T> tree, Func<T, bool> retainSubTree)
            where TTree : IRecursiveStructure<TTree, T>
            => CachedTreeBuilder<TTree, T>.Resolve(tree.TypedThis, o => o.Children.Where(o => retainSubTree(o.NodeValue)), (o, kids) => Node(o.NodeValue, kids));



        /// <summary>
        /// Builds a copy of this tree with the same vales, but with some subtrees optionally removed.
        /// The filter function is called for children before parents, and is passed the *output* subtree that may or may not be retained;
        /// i.e. it will be called for the root node last, and that root node may differ from the initial root node as subtrees have already been pruned.
        /// </summary>
        [Pure]
        public static Tree<T>? Where<T>(this Tree<T> tree, Func<Tree<T>, bool> retainSubTree)
        {
            var reconstruct = new Stack<Tree<T>>(16);
            var todo = new Stack<Tree<T>>(16);
            todo.Push(tree);
            while (todo.Count > 0) {
                var next = todo.Pop();
                reconstruct.Push(next);
                var children = next.Children;
                for (var i = children.Count - 1; i >= 0; i--) {
                    todo.Push(children[i]);
                }
            }
            var tmp = new List<Tree<T>>();
            while (reconstruct.Count > 0) {
                var next = reconstruct.Pop();
                var kidCount = next.Children.Count;
                for (var i = 0; i < kidCount; i++) {
                    var maybeKid = todo.Pop();
                    if (retainSubTree(maybeKid)) {
                        tmp.Add(maybeKid);
                    }
                }
                todo.Push(Node(next.NodeValue, tmp.ToArray()));
                tmp.Clear();
            }
            return todo.Pop() is var finalTree && retainSubTree(finalTree) ? finalTree : null;
        }
    }

    public sealed class Tree<T> : IEquatable<Tree<T>>, IRecursiveStructure<Tree<T>, T>
    {
        readonly T nodeValue;
        readonly Tree<T>[] kidArray;

        public T NodeValue
            => nodeValue;

        public IReadOnlyList<Tree<T>> Children
            => kidArray;

        /// <summary>
        /// Creates a Tree with specified child nodes.  The child node enumeration is materialized using ToArray() before usage.
        /// </summary>
        /// <param name="value">The value of this node.</param>
        /// <param name="children">The children of this node, (null is allowed and means none).</param>
        public Tree(T value, IEnumerable<Tree<T>>? children)
            : this(value, children?.ToArray()) { }

        /// <summary>
        /// Creates a Tree with specified child nodes.  The child node array is used directly. Do not mutate the array after passing it into the tree; doing so
        /// results in undefined behavior.
        /// </summary>
        /// <param name="value">The value of this node.</param>
        /// <param name="children">The children of this node, (null is allowed and means none).</param>
        public Tree(T value, Tree<T>[]? children)
        {
            nodeValue = value;
            kidArray = children ?? Array.Empty<Tree<T>>();
        }

        public static readonly Comparer DefaultComparer = new Comparer(EqualityComparer<T>.Default);

        public sealed class Comparer : IEqualityComparer<Tree<T>?>
        {
            static readonly int typeHash = typeof(Tree<T>).GetHashCode();
            readonly IEqualityComparer<T> ValueComparer;

            public Comparer(IEqualityComparer<T> valueComparer)
            {
                ValueComparer = valueComparer;
            }

            struct NodePair
            {
                public Tree<T> A, B;
            }

            [Pure]
            public bool Equals(Tree<T>? a, Tree<T>? b)
            {
                if (a == null || b == null) {
                    return ReferenceEquals(a, b);
                }

                var todo = new Stack<NodePair>(16);
                todo.Push(new NodePair { A = a, B = b });
                while (todo.Count > 0) {
                    var pair = todo.Pop();
                    if (ShallowEquals(pair)) {
                        for (var i = 0; i < pair.A.Children.Count; i++) {
                            todo.Push(new NodePair { A = pair.A.Children[i], B = pair.B.Children[i] });
                        }
                    } else {
                        return false;
                    }
                }
                return true;
            }

            [Pure]
            bool ShallowEquals(NodePair pair)
                // ReSharper disable RedundantCast
                //workaround resharper issue: object comparison is by reference, and faster than ReferenceEquals
                => ReferenceEquals(pair.A, pair.B) ||
                    pair.A.Children.Count == pair.B.Children.Count
                    && ValueComparer.Equals(pair.A.NodeValue, pair.B.NodeValue);
            // ReSharper restore RedundantCast

            [Pure]
            public int GetHashCode(Tree<T>? obj)
            {
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                // ReSharper disable HeuristicUnreachableCode
                if (obj == null) {
                    return typeHash;
                }
                // ReSharper restore HeuristicUnreachableCode
                // ReSharper restore ConditionIsAlwaysTrueOrFalse
                ulong hash = obj.NodeValue is null ? 0 : (uint)ValueComparer.GetHashCode(obj.NodeValue);
                ulong offset = 1; //keep offset odd to ensure no bits are lost in scaling.
                foreach (var node in obj.PreorderTraversal()) {
                    hash += offset * ((uint)(node.NodeValue is null ? 0 : ValueComparer.GetHashCode(node.NodeValue!)) + ((ulong)node.Children.Count << 32));
                    offset += 2;
                }
                return (int)((uint)(hash >> 32) + (uint)hash);
            }
        }

        [Pure]
        public override bool Equals(object? obj)
            => DefaultComparer.Equals(this, obj as Tree<T>);

        [Pure]
        public bool Equals(Tree<T>? other)
            => DefaultComparer.Equals(this, other);

        [Pure]
        public override int GetHashCode()
            => DefaultComparer.GetHashCode(this);

        [Pure]
        public override string ToString()
            => "TREE:\n" + ToString("");

        [Pure]
        string ToString(string indent)
        {
            if (indent.Length > 80) {
                return "<<TOO DEEP>>";
            }
            return indent + (nodeValue?.ToString() ?? "<NULL>").Replace("\n", "\n" + indent) + " "
                + (Children.Count == 0
                    ? "."
                    : ":\n" + Children.Select(t => t.ToString(indent + "    ")).JoinStrings("\n")
                );
        }

        Tree<T> IRecursiveStructure<Tree<T>, T>.TypedThis
            => this;

        Tree<T> IRecursiveStructure<Tree<T>, T>.UnrootedSubTree()
            => this;
    }
}
