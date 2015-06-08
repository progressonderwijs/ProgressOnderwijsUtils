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
        public static Tree<T> BuildRecursively<T>(T root, Func<T, IEnumerable<T>> kidLookup) { return new CachedTreeBuilder<T>(kidLookup).Resolve(root); }

        public static Tree<T> BuildRecursively<T>(T root, IReadOnlyDictionary<T, IReadOnlyList<T>> kidLookup)
        {
            return BuildRecursively(root, id => kidLookup.GetOrDefaultR(id));
        }

        public static Tree<T> BuildRecursively<T>(T root, ILookup<T, T> kidLookup) { return BuildRecursively(root, id => kidLookup[id]); }
        public static IEqualityComparer<Tree<T>> EqualityComparer<T>(IEqualityComparer<T> valueComparer) { return new Tree<T>.Comparer(valueComparer); }

        /// <summary>
        /// Builds a copy of this tree with the same structure, but with different node values, as computed by the mapper argument.
        /// mapper is called in a preorder traversal (i.e. a node before its children, and the descendents of the first child before the second).
        /// </summary>
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
                for (int i = 0; i < mappedChildren.Length; i++) {
                    mappedChildren[i] = output.Pop();
                }
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
        public T NodeValue => nodeValue;
        public IReadOnlyList<Tree<T>> Children => kidArray;

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

            struct NodePair
            {
                public Tree<T> A, B;
            }

            public bool Equals(Tree<T> a, Tree<T> b)
            {
                var todo = new Stack<NodePair>(16);
                todo.Push(new NodePair { A = a, B = b });
                while (todo.Count > 0) {
                    var pair = todo.Pop();
                    if (ShallowEquals(pair)) {
                        for (int i = 0; i < pair.A.Children.Count; i++) {
                            todo.Push(new NodePair { A = pair.A.Children[i], B = pair.B.Children[i] });
                        }
                    } else {
                        return false;
                    }
                }
                return true;
            }

            bool ShallowEquals(NodePair pair)
            {
                // ReSharper disable RedundantCast
                //workaround resharper issue: object comparison is by reference, and faster than ReferenceEquals
                return (object)pair.A == (object)pair.B ||
                    (object)pair.A != null && (object)pair.B != null
                        && pair.A.Children.Count == pair.B.Children.Count
                        && ValueComparer.Equals(pair.A.NodeValue, pair.B.NodeValue);
                // ReSharper restore RedundantCast
            }

            public int GetHashCode(Tree<T> obj)
            {
                if (obj == null) {
                    return typeHash;
                }
                ulong hash = (uint)ValueComparer.GetHashCode(obj.NodeValue);
                ulong offset = 1;//keep offset odd to ensure no bits are lost in scaling.
                foreach (var node in obj.PreorderTraversal()) {
                    hash += offset * ((uint)ValueComparer.GetHashCode(node.NodeValue) + ((ulong)node.Children.Count << 32));
                    offset += 2;
                }
                return (int)((uint)(hash >> 32) + (uint)hash);
            }
        }

        public override bool Equals(object obj) { return DefaultComparer.Equals(this, obj as Tree<T>); }
        public bool Equals(Tree<T> other) { return DefaultComparer.Equals(this, other); }
        public override int GetHashCode() => DefaultComparer.GetHashCode(this);
        public override string ToString() => "TREE:\n" + ToString("");

        string ToString(string indent)
        {
            if (indent.Length > 80) {
                return "<<TOO DEEP>>";
            }
            return indent + nodeValue.ToString().Replace("\n", "\n" + indent) + " "
                + (Children.Count == 0
                    ? "."
                    : ":\n" + Children.Select(t => t.ToString(indent + "    ")).JoinStrings("\n")
                    );
        }

        public int Height()
        {
            int height = 1;
            var nextSet = Children.ToArray();
            while (nextSet.Length > 0) {
                height++;
                nextSet = nextSet.SelectMany(o => o.Children).ToArray();
            }
            return height;
        }
    }
}
