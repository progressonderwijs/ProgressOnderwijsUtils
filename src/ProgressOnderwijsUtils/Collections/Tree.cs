﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        TTree TypedThis { get; }

        IReadOnlyList<TTree> Children { get; }
    }

    public interface IRecursiveStructure<out TTree, T> : IRecursiveStructure<TTree>, IHasNodeValue<T>
        where TTree : IRecursiveStructure<TTree>
    {
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
            => CachedTreeBuilder<T, T>.Resolve(root, kidLookup, Node);

        [Pure]
        public static Tree<T> BuildRecursively<T>(T root, IReadOnlyDictionary<T, IReadOnlyList<T>> kidLookup)
            where T : notnull
            => CachedTreeBuilder<T, T>.Resolve(root, arg => kidLookup.GetOrDefaultR(arg)?.AsEnumerable(), Node);

        [Pure]
        public static IEqualityComparer<Tree<T>?> EqualityComparer<T>(IEqualityComparer<T> valueComparer)
            => new Tree<T>.Comparer(valueComparer);

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
                => ReferenceEquals(pair.A, pair.B)
                    || pair.A.Children.Count == pair.B.Children.Count
                    && ValueComparer.Equals(pair.A.NodeValue, pair.B.NodeValue);

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
            return indent
                + (nodeValue?.ToString() ?? "<NULL>").Replace("\n", "\n" + indent)
                + " "
                + (Children.Count == 0
                    ? "."
                    : ":\n" + Children.Select(t => t.ToString(indent + "    ")).JoinStrings("\n")
                );
        }

        Tree<T> IRecursiveStructure<Tree<T>>.TypedThis
            => this;

        Tree<T> IRecursiveStructure<Tree<T>, T>.UnrootedSubTree()
            => this;
    }
}
