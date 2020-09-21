using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using Xunit;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class TreeTest
    {
        [Fact]
        public void TreeStoresInfo()
        {
            var tree = Tree.Node("abc", Tree.Node("def"), Tree.Node("xyz"));
            PAssert.That(() => tree.NodeValue == "abc");
            PAssert.That(() => tree.Children.Count == 2);
            PAssert.That(() => tree.Children.All(kid => kid.Children.Count == 0));
            PAssert.That(() => tree.Children[0].NodeValue == "def");
            PAssert.That(() => tree.Children[1].NodeValue == "xyz");
        }

        [Fact]
        public void TreeWithManyLeaves()
        {
            var tree = Tree.Node(100, Tree.Node(1), Tree.Node(2), Tree.Node(3), Tree.Node(4));
            PAssert.That(() => tree.Children.Count == 4);
            PAssert.That(() => tree.Children.Select((kid, index) => kid.NodeValue == index + 1).All(b => b));
        }

        [Fact]
        public void SaneDefaultComparer()
        {
            var leaf2 = Tree.Node(2);
            var tree1 = Tree.Node(100, Tree.Node(1), leaf2, Tree.Node(3), Tree.Node(4));
            var tree2 = Tree.Node(100, Tree.Node(1), leaf2, Tree.Node(3), Tree.Node(4));

            PAssert.That(() => !ReferenceEquals(tree1, tree2));
            PAssert.That(() => Equals(tree1, tree2));
            PAssert.That(() => Equals(tree2, tree1));
            PAssert.That(() => tree2.Equals(tree1));
            PAssert.That(() => tree2.Equals((object)tree1));
            PAssert.That(() => tree1.Equals(tree1));
            PAssert.That(() => !tree1.Equals(null));
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            PAssert.That(() => !Equals(tree2, null));
            PAssert.That(() => !Equals(null, tree2));
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            PAssert.That(() => !Equals(tree2, leaf2));
        }

        [Fact]
        public void CustomizableComparerWorks()
        {
            var tree1 = Tree.Node("a".PretendNullable(), Tree.Node("x".PretendNullable()), Tree.Node("b".PretendNullable()), Tree.Node(default(string)), Tree.Node("".PretendNullable()));
            var tree2 = Tree.Node("a".PretendNullable(), Tree.Node("x".PretendNullable()), Tree.Node("B".PretendNullable()), Tree.Node(default(string)), Tree.Node("".PretendNullable()));
            var tree3 = Tree.Node("a".PretendNullable(), Tree.Node("x".PretendNullable()), Tree.Node("b".PretendNullable()), Tree.Node(default(string)), Tree.Node("".PretendNullable()));
            var tree4 = Tree.Node("a".PretendNullable(), Tree.Node("y".PretendNullable()), Tree.Node("b".PretendNullable()), Tree.Node(default(string)), Tree.Node("".PretendNullable()));

            PAssert.That(() => !tree1.Equals(tree2));
            PAssert.That(() => tree1.Equals(tree3));
            PAssert.That(() => !tree2.Equals(tree3));

            PAssert.That(() => Tree.EqualityComparer(StringComparer.OrdinalIgnoreCase).Equals(tree1, tree2));
            PAssert.That(() => !Tree.EqualityComparer(StringComparer.OrdinalIgnoreCase).Equals(tree1, tree4));
        }

        [Fact]
        public void SaneDefaultHashCodes()
        {
            var leaf2 = Tree.Node(2);
            var tree1 = Tree.Node(100, Tree.Node(1), leaf2, Tree.Node(3), Tree.Node(4));
            var tree2 = Tree.Node(100, Tree.Node(1), leaf2, Tree.Node(3), Tree.Node(4));

            // ReSharper disable EqualExpressionComparison
            PAssert.That(() => tree1.GetHashCode() == tree1.GetHashCode(), "hashcode should be consistent");
            // ReSharper restore EqualExpressionComparison
            PAssert.That(() => tree1.GetHashCode() == tree2.GetHashCode());
            PAssert.That(() => tree1.GetHashCode() != leaf2.GetHashCode());

            var equalityComparer = EqualityComparer<Tree<int>>.Default;
            PAssert.That(() => equalityComparer.GetHashCode(tree2) == equalityComparer.GetHashCode(tree1));
            PAssert.That(() => equalityComparer.GetHashCode(tree2) != equalityComparer.GetHashCode(leaf2));
            PAssert.That(() => equalityComparer.GetHashCode(tree1) != equalityComparer.GetHashCode(null!));
        }

        [Fact]
        public void CustomizableGetHashCodeWorks()
        {
            var comparer = Tree.EqualityComparer(StringComparer.OrdinalIgnoreCase);

            var tree1 = Tree.Node("a", Tree.Node("x"), Tree.Node("b"), Tree.Node(123.ToStringInvariant()), Tree.Node(""));
            var tree2 = Tree.Node("a", Tree.Node("x"), Tree.Node("B"), Tree.Node(123.ToStringInvariant()), Tree.Node(""));
            var tree3 = Tree.Node("a", Tree.Node("x"), Tree.Node("b"), Tree.Node(123.ToStringInvariant()), Tree.Node(""));
            var tree4 = Tree.Node("a", Tree.Node("y"), Tree.Node("b"), Tree.Node(123.ToStringInvariant()), Tree.Node(""));

            PAssert.That(() => tree1.GetHashCode() != tree2.GetHashCode());
            PAssert.That(() => tree1.GetHashCode() == tree3.GetHashCode());
            PAssert.That(() => tree2.GetHashCode() != tree3.GetHashCode());

            PAssert.That(() => comparer.GetHashCode(tree1!) == comparer.GetHashCode(tree2!));
            PAssert.That(() => comparer.GetHashCode(tree1!) != comparer.GetHashCode(tree4!));
        }

        [Fact]
        public void PreorderTraversalNormalCase()
        {
            var e = Tree.Node("e");
            var d = Tree.Node("d");
            var cd = Tree.Node("c", d);
            var b = Tree.Node("b");
            var abcde = Tree.Node("a", b, cd, e);
            PAssert.That(() => abcde.PreorderTraversal().Select(rooted => rooted.NodeValue).SequenceEqual(new[] { "a", "b", "c", "d", "e" }));
            PAssert.That(
                () =>
                    abcde.RootHere()
                        .PreorderTraversal()
                        .Select(rooted => rooted.PathSelfToRoot().Select(node => node.NodeValue).JoinStrings())
                        .SequenceEqual(new[] { "a", "ba", "ca", "dca", "ea" })
            );
        }

        [Fact]
        public void BuildWorks()
        {
            var dict = new Dictionary<string, IReadOnlyList<string>> {
                { "root", new[] { "a", "b", "c" } },
                { "a", new[] { "b", "c" } },
                { "b", new[] { "c", "d" } },
                { "c", new[] { "e", "d" } },
            };

            var tree = Tree.BuildRecursively("root", id => dict.GetOrDefault(id));

            var e = Tree.Node("e");
            var d = Tree.Node("d");
            var c = Tree.Node("c", e, d);
            var b = Tree.Node("b", c, d);
            var a = Tree.Node("a", b, c);
            var root = Tree.Node("root", a, b, c);

            PAssert.That(() => root.Equals(tree), "Generating a tree from a kid lookup delegate");

            PAssert.That(() => root.Equals(Tree.BuildRecursively("root", dict)), "Generating a tree from a dictionary");
            PAssert.That(() => !ReferenceEquals(tree, root), "Test really did create the tree itself");

            var root_a_b = tree.Children[0].Children[0];
            var root_b = tree.Children[1];
            PAssert.That(() => root_a_b.NodeValue == "b" && root_b.NodeValue == "b", "Test should select 'b' branches correctly");
        }

        [Fact]
        public void BuildDetectsCycles()
        {
            // ReSharper disable once NotAccessedVariable
            Tree<int> ignore;
            Assert.Throws<InvalidOperationException>(() => ignore = Tree.BuildRecursively(0, i => new[] { (i + 1) % 10, (i + 2) % 13 }));
        }

        [Fact]
        public void TreeSelectSingleNode()
            => AssertTreeSelectMapsInputAsExpected(
                Tree.Node(1u),
                Tree.Node(1L)
            );

        [Fact]
        public void TreeRebuildSingleNode()
        {
            var output = Tree.Node(1u).Rebuild(node => (long)node.NodeValue, (oldNode, value, newKids) => Tree.Node(value, newKids).AsSingletonArray());
            var expected = Tree.Node(1L).AsSingletonArray();
            PAssert.That(() => output.SequenceEqual(expected));
        }

        [Fact]
        public void TreeRebuildSingleNodeToTwo()
        {
            var output = Tree.Node(1u).Rebuild(node => (long)node.NodeValue, (oldNode, value, newKids) => new[] { Tree.Node(value, newKids), Tree.Node(value, newKids) });
            var expected = Enumerable.Repeat(Tree.Node(1L), 2);
            PAssert.That(() => output.SequenceEqual(expected));
        }

        [Fact]
        public void TreeSelectOneChild()
            => AssertTreeSelectMapsInputAsExpected(
                Tree.Node(1u, Tree.Node(2u)),
                Tree.Node(1L, Tree.Node(2L))
            );

        [Fact]
        public void TreeRebuildOneChildDoubleEverything()
        {
            var input = Tree.Node(1u, Tree.Node(2u));
            var output = input.Rebuild(node => 2 * (long)node.NodeValue, (oldNode, value, newKids) => new[] { Tree.Node(value, newKids), Tree.Node(value + 1, newKids) });
            var expected = new[] {
                Tree.Node(2L, Tree.Node(4L), Tree.Node(5L)),
                Tree.Node(3L, Tree.Node(4L), Tree.Node(5L))
            };
            PAssert.That(() => output.SequenceEqual(expected));
        }

        [Fact]
        public void TreeRebuildCanBeEmptyEvenWhenThereAreDescendants()
        {
            var input = Tree.Node(1u, Tree.Node(2u));
            var output = input.Rebuild(node => 2 * (long)node.NodeValue, (oldNode, value, newKids) => new[] { Tree.Node(value, newKids), Tree.Node(value + 1, newKids) }.Where(_ => oldNode.NodeValue != 1).ToArray());
            var expected = new Tree<long>[] { };
            PAssert.That(() => output.SequenceEqual(expected));
        }

        [Fact]
        public void TreeSelectTwoChildren()
            => AssertTreeSelectMapsInputAsExpected(
                Tree.Node(1u, Tree.Node(2u), Tree.Node(3u)),
                Tree.Node(1L, Tree.Node(2L), Tree.Node(3L))
            );

        [Fact]
        public void TreeSelectTwoChildrenWithChild()
            => AssertTreeSelectMapsInputAsExpected(
                Tree.Node(1u, Tree.Node(2u, Tree.Node(4u)), Tree.Node(3u, Tree.Node(5u))),
                Tree.Node(1L, Tree.Node(2L, Tree.Node(4L)), Tree.Node(3L, Tree.Node(5L)))
            );

        [Fact]
        public void TreeRebuildSupportsNullSequences()
        {
            var input = Tree.Node(1u, Tree.Node(2u, Tree.Node(4u)), Tree.Node(3u, Tree.Node(5u)));
            var output = input.Rebuild(node => (long)node.NodeValue, (oldNode, value, newKids) => value == 2 ? null : Tree.Node(value, newKids).AsSingletonArray());
            var expected = new[] {
                Tree.Node(1L, Tree.Node(3L, Tree.Node(5L)))
            };
            PAssert.That(() => output.SequenceEqual(expected));
        }


        [Fact]
        public void TreeRebuildSupportsMessyGunkMixingNullsAndEmpty()
        {
            var input = Tree.Node(1u, Tree.Node(2u, Tree.Node(4u)), Tree.Node(3u, Tree.Node(6u), Tree.Node(4u), Tree.Node(5u)), Tree.Node(4u));
            var output = input.Rebuild(
                node => (long)node.NodeValue,
                (oldNode, value, newKids) => value % 2 == 0 ? Array.Empty<Tree<long>>() : Enumerable.Repeat(Tree.Node(value, newKids), 2).ToArray()
            );
            var n5 = Tree.Node(5L);
            var n3 = Tree.Node(3L, n5, n5);
            var n1 = Tree.Node(1L, n3, n3);
            var expected = new[] { n1, n1 };
            PAssert.That(() => output.SequenceEqual(expected));
        }

        [Fact]
        public void TreeRebuildSupportsEnumerableOverloadToo()
        {
            var input = Tree.Node(1u, Tree.Node(2u, Tree.Node(4u)), Tree.Node(3u, Tree.Node(6u), Tree.Node(4u), Tree.Node(5u)), Tree.Node(4u));
            var output = input.Rebuild(
                node => (long)node.NodeValue,
                (oldNode, value, newKids) => value % 2 == 0 ? Array.Empty<Tree<long>>() : Enumerable.Repeat(Tree.Node(value, newKids), 2)
            );
            var n5 = Tree.Node(5L);
            var n3 = Tree.Node(3L, n5, n5);
            var n1 = Tree.Node(1L, n3, n3);
            var expected = new[] { n1, n1 };
            PAssert.That(() => output.SequenceEqual(expected));
        }

        [Fact]
        public void ComplexTreeSelectTwoChildrenWithChild()
            => AssertTreeSelectMapsInputAsExpected(
                Tree.Node(
                    1u,
                    Tree.Node(6u),
                    Tree.Node(
                        2u,
                        Tree.Node(4u),
                        Tree.Node(7u)
                    ),
                    Tree.Node(
                        3u,
                        Tree.Node(
                            5u,
                            Tree.Node(8u)
                        )
                    )
                ),
                Tree.Node(
                    1L,
                    Tree.Node(6L),
                    Tree.Node(
                        2L,
                        Tree.Node(4L),
                        Tree.Node(7L)
                    ),
                    Tree.Node(
                        3L,
                        Tree.Node(
                            5L,
                            Tree.Node(8L)
                        )
                    )
                )
            );

        [Fact]
        public void TreeSelectVisitsInReversePreorderTraversal()
        {
            //Tree.Select calls the mapping function for each tree node, but that might have side effects.
            //therefore, we want to ensure that the mapping function is called in some specific, reproducible order.
            //In particular, this happens currently in the idGenerator for Cijferlijsten.

            var tree = Tree.Node("a", Tree.Node("a.b"), Tree.Node("a.c", Tree.Node("a.c.d"), Tree.Node("a.c.e")), Tree.Node("a.f", Tree.Node("a.f.g", Tree.Node("a.f.g.h"))));
            var count = tree.PreorderTraversal().Count();
            PAssert.That(() => count == 8);
            var counter = 1;
            var mappedTree = tree.Select(_ => counter++);
            var nodeValuesInPreorder = mappedTree.PreorderTraversal().Select(n => n.NodeValue);
            PAssert.That(() => nodeValuesInPreorder.SequenceEqual(Enumerable.Range(1, 8).Reverse()));
        }

        [Fact]
        public void CanWorkWithDeepTreesWithoutStackoverflow()
        {
            const int targetHeight = 1000_000;
            var input = Tree.BuildRecursively(1u, i => i < targetHeight ? new[] { i + 1 } : new uint[0]);
            var output = Tree.BuildRecursively(1L, i => i < targetHeight ? new[] { i + 1 } : new long[0]);
            var mappedInput = input.Select(i => (long)i);
            var areEqual = mappedInput.Equals(output);
            PAssert.That(() => areEqual, "Deep trees should be selectable and comparable too");
            PAssert.That(() => input.Height() == targetHeight);
        }

        [Fact]
        public void SingleNodeHasHeight1()
            => PAssert.That(() => Tree.Node(0).Height() == 1);

        [Fact]
        public void RightLeaningTreeComputesHeight4()
            => PAssert.That(() => Tree.Node(1u, Tree.Node(6u), Tree.Node(2u, Tree.Node(4u), Tree.Node(7u)), Tree.Node(3u, Tree.Node(5u, Tree.Node(8u)))).Height() == 4);

        [Fact]
        public void MessyTreeComputesHeight4()
            => PAssert.That(() => Tree.Node(1u, Tree.Node(6u), Tree.Node(3u, Tree.Node(5u, Tree.Node(8u))), Tree.Node(2u, Tree.Node(4u), Tree.Node(7u))).Height() == 4);

        static void AssertTreeSelectMapsInputAsExpected(Tree<uint> input, Tree<long> expected)
        {
            var output = input.Select(i => (long)i);
            PAssert.That(() => output.Equals(expected));
        }

        [Fact]
        public void BuildRecursivelyDoesntDetectCycleWhenDifferentParentsHaveIdenticalLeafChildren()
        {
            var tree = Tree.Node(
                1,
                Tree.Node(3),
                Tree.Node(
                    2,
                    Tree.Node(3)
                )
            );

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            Tree.BuildRecursively(tree, t => t.Children);
        }
    }
}
