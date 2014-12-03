using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils.Collections
{
    public interface IRecursiveStructure<out TTree>
        where TTree : IRecursiveStructure<TTree>
    {
        IReadOnlyList<TTree> Children { get; }
    }

    public static class Tree
    {
        public static Tree<T> Node<T>(T value, IEnumerable<Tree<T>> children) { return new Tree<T>(value, children); }
        public static Tree<T> Node<T>(T value, Tree<T> a) { return new Tree<T>(value, new[] { a, }); }
        public static Tree<T> Node<T>(T value, Tree<T> a, Tree<T> b) { return new Tree<T>(value, new[] { a, b }); }
        public static Tree<T> Node<T>(T value, Tree<T> a, Tree<T> b, Tree<T> c) { return new Tree<T>(value, new[] { a, b, c }); }
        // ReSharper disable MethodOverloadWithOptionalParameter
        public static Tree<T> Node<T>(T value, params Tree<T>[] kids) { return new Tree<T>(value, kids); }
        // ReSharper restore MethodOverloadWithOptionalParameter
        public static Tree<T> Node<T>(T value) { return new Tree<T>(value, null); }
        public static Tree<T> Build<T>(T root, Func<T, IEnumerable<T>> kidLookup) { return new CachedTreeBuilder<T>(kidLookup).Resolve(root); }
        public static Tree<T> Build<T>(T root, IReadOnlyDictionary<T, IReadOnlyList<T>> kidLookup) { return Build(root, id => kidLookup.GetOrDefaultR(id)); }
        public static Tree<T> Build<T>(T root, ILookup<T, T> kidLookup) { return Build(root, id => kidLookup[id]); }
        public static IEqualityComparer<Tree<T>> EqualityComparer<T>(IEqualityComparer<T> valueComparer) { return new Tree<T>.Comparer(valueComparer); }

        public static Tree<TR> Select<T, TR>(this Tree<T> tree, Func<T, TR> mapper)
        {
            var todo = new Stack<Tree<T>>(16);
            var reconstruct = new Stack<Tree<T>>(16);
            var output = new Stack<Tree<TR>>(16);
            todo.Push(tree);
            while (todo.Count > 0) {
                var next = todo.Pop();
                reconstruct.Push(next);
                foreach (var kid in next.Children)
                    todo.Push(kid);
            }
            while (reconstruct.Count > 0) {
                var next = reconstruct.Pop();
                var mappedChildren = new Tree<TR>[next.Children.Count];
                for (int i = mappedChildren.Length - 1; i >= 0; i--)
                    mappedChildren[i] = output.Pop();
                output.Push(Node(mapper(next.NodeValue), mappedChildren));
            }
            return output.Pop();
        }
    }

    public sealed class Tree<T> : IEquatable<Tree<T>>, IRecursiveStructure<Tree<T>>
    {
        static readonly IReadOnlyList<Tree<T>> EmptyArray = new Tree<T>[0]; // cache this since it will be used very commonly.
        readonly T nodeValue;
        readonly IReadOnlyList<Tree<T>> kidArray;
        public T NodeValue { get { return nodeValue; } }
        public IReadOnlyList<Tree<T>> Children { get { return kidArray; } }

        /// <summary>
        /// Creates a Tree with specified child nodes.  The child node enumeration is materialized using ToArray() before usage.
        /// </summary>
        /// <param name="value">The value of this node.</param>
        /// <param name="children">The children of this node, (null is allowed and means none).</param>
        public Tree(T value, IEnumerable<Tree<T>> children)
            : this(value, children == null ? null : children.ToArray()) { }

        /// <summary>
        /// Creates a Tree with specified child nodes.  The child node array is used directly. Do not mutate the array after passing it into the tree; doing so
        /// results in undefined behavior.
        /// </summary>
        /// <param name="value">The value of this node.</param>
        /// <param name="children">The children of this node, (null is allowed and means none).</param>
        public Tree(T value, Tree<T>[] children)
        {
            nodeValue = value;
            kidArray = children ?? EmptyArray;
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
                foreach (var kid in obj.Children) {
                    hash += offset * InternalHash(kid);
                    offset += 2;
                }
                return hash;
            }

            public int GetHashCode(Tree<T> obj)
            {
                if (obj == null) {
                    return typeHash;
                }
                var hash = InternalHash(obj);
                return (int)((uint)(hash >> 32) + (uint)hash);
            }
        }

        public override bool Equals(object obj) { return DefaultComparer.Equals(this, obj as Tree<T>); }
        public bool Equals(Tree<T> other) { return DefaultComparer.Equals(this, other); }
        public override int GetHashCode() { return DefaultComparer.GetHashCode(this); }
        public override string ToString() { return "TREE:\n" + ToString(""); }

        string ToString(string indent)
        {
            return indent + nodeValue.ToString().Replace("\n", "\n" + indent) + " "
                + (Children.Count == 0
                    ? "."
                    : ":\n" + Children.Select(t => t.ToString(indent + "    ")).JoinStrings("\n")
                    );
        }

        public int Height() { return Children.Count > 0 ? Children.Max(sub => sub.Height()) + 1 : 1; }
    }
}
