using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Collections
{
    public interface IRecursiveStructure<out TTree>
        where TTree : IRecursiveStructure<TTree>
    {
        IReadOnlyList<TTree> Children { get; }
    }

    public static class Tree
    {
        [Pure]
        public static Tree<T> Node<T>(T value, IEnumerable<Tree<T>> children)
            => new Tree<T>(value, children);

        // ReSharper disable MethodOverloadWithOptionalParameter
        [Pure]
        public static Tree<T> Node<T>(T value, params Tree<T>[] kids)
            => new Tree<T>(value, kids);

        // ReSharper restore MethodOverloadWithOptionalParameter

        [Pure]
        public static Tree<T> Node<T>(T value)
            => new Tree<T>(value, null);

        [Pure]
        public static Tree<T> BuildRecursively<T>(T root, Func<T, IEnumerable<T>?> kidLookup)
            => CachedTreeBuilder<T>.Resolve(root, kidLookup);

        [Pure]
        public static Tree<T> BuildRecursively<T>(T root, IReadOnlyDictionary<T, IReadOnlyList<T>> kidLookup)
            where T : notnull
            => BuildRecursively(root, kidLookup.GetOrDefaultR);

        [Pure]
        public static IEqualityComparer<Tree<T>?> EqualityComparer<T>(IEqualityComparer<T> valueComparer)
            => new Tree<T>.Comparer(valueComparer);

        /// <summary>
        /// Builds a copy of this tree with the same structure, but with different node values, as computed by the mapper argument.
        /// mapper is called in a preorder traversal (i.e. a node before its children, and the descendents of the first child before the second).
        /// </summary>
        [Pure]
        public static Tree<TR> Select<T, TR>(this Tree<T> tree, Func<T, TR> mapper)
        {
            var todo = new Stack<Tree<T>>(16);
            var reconstruct = new Stack<Tree<T>>(16);
            var output = new Stack<Tree<TR>>(16);
            todo.Push(tree);
            while (todo.Count > 0) {
                var next = todo.Pop();
                reconstruct.Push(next);
                var children = next.Children;
                for (var i = children.Count - 1; i >= 0; i--) {
                    todo.Push(children[i]);
                }
            }
            while (reconstruct.Count > 0) {
                var next = reconstruct.Pop();
                var mappedChildren = new Tree<TR>[next.Children.Count];
                for (var i = 0; i < mappedChildren.Length; i++) {
                    mappedChildren[i] = output.Pop();
                }
                output.Push(Node(mapper(next.NodeValue), mappedChildren));
            }
            return output.Pop();
        }

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

    public sealed class Tree<T> : IEquatable<Tree<T>>, IRecursiveStructure<Tree<T>>
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
    }
}
