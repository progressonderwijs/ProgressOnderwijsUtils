﻿using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Collections;
using ProgressOnderwijsUtils.Test;

namespace Progress.Business.Test.Tools
{
    public sealed class TreeTest
    {
        [Test, Continuous]
        public void TreeStoresInfo()
        {
            var tree = Tree.Node("abc", Tree.Node("def"), Tree.Node("xyz"));
            PAssert.That(() => tree.NodeValue == "abc");
            PAssert.That(() => tree.Children.Count == 2);
            PAssert.That(() => tree.Children.All(kid => kid.Children.Count == 0));
            PAssert.That(() => tree.Children[0].NodeValue == "def");
            PAssert.That(() => tree.Children[1].NodeValue == "xyz");
        }

        [Test, Continuous]
        public void TreeWithManyLeaves()
        {
            var tree = Tree.Node(100, Tree.Node(1), Tree.Node(2), Tree.Node(3), Tree.Node(4));
            PAssert.That(() => tree.Children.Count == 4);
            PAssert.That(() => tree.Children.Select((kid, index) => kid.NodeValue == index + 1).All(b => b));
        }

        [Test, Continuous]
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
            PAssert.That(() => !Equals(tree2, null));
            PAssert.That(() => !Equals(null, tree2));
            PAssert.That(() => !Equals(tree2, leaf2));
        }

        [Test, Continuous]
        public void CustomizableComparerWorks()
        {
            var tree1 = Tree.Node("a", Tree.Node("x"), Tree.Node("b"), Tree.Node(default(string)), Tree.Node(""));
            var tree2 = Tree.Node("a", Tree.Node("x"), Tree.Node("B"), Tree.Node(default(string)), Tree.Node(""));
            var tree3 = Tree.Node("a", Tree.Node("x"), Tree.Node("b"), Tree.Node(default(string)), Tree.Node(""));
            var tree4 = Tree.Node("a", Tree.Node("y"), Tree.Node("b"), Tree.Node(default(string)), Tree.Node(""));

            PAssert.That(() => !tree1.Equals(tree2));
            PAssert.That(() => tree1.Equals(tree3));
            PAssert.That(() => !tree2.Equals(tree3));

            PAssert.That(() => Tree.EqualityComparer(StringComparer.OrdinalIgnoreCase).Equals(tree1, tree2));
            PAssert.That(() => !Tree.EqualityComparer(StringComparer.OrdinalIgnoreCase).Equals(tree1, tree4));
        }

        [Test, Continuous]
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
            PAssert.That(() => equalityComparer.GetHashCode(tree1) != equalityComparer.GetHashCode(null));
        }

        [Test, Continuous]
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

            PAssert.That(() => comparer.GetHashCode(tree1) == comparer.GetHashCode(tree2));
            PAssert.That(() => comparer.GetHashCode(tree1) != comparer.GetHashCode(tree4));
        }

        [Test, Continuous]
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
                        .SequenceEqual(new[] { "a", "ba", "ca", "dca", "ea" }));
        }

        [Test, Continuous]
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

        [Test]
        public void BuildDetectsCycles()
        {
            Tree<int> ignore;
            Assert.Throws<InvalidOperationException>(() => ignore = Tree.BuildRecursively(0, i => new[] { (i + 1) % 10, (i + 2) % 13 }));
        }

        [Test, Continuous]
        public void TreeSelectSingleNode()
        {
            AssertTreeSelectMapsInputAsExpected(
                Tree.Node(1u),
                Tree.Node(1L)
                );
        }

        [Test, Continuous]
        public void TreeSelectOneChild()
        {
            AssertTreeSelectMapsInputAsExpected(
                Tree.Node(1u, Tree.Node(2u)),
                Tree.Node(1L, Tree.Node(2L))
                );
        }

        [Test, Continuous]
        public void TreeSelectTwoChildren()
        {
            AssertTreeSelectMapsInputAsExpected(
                Tree.Node(1u, Tree.Node(2u), Tree.Node(3u)),
                Tree.Node(1L, Tree.Node(2L), Tree.Node(3L))
                );
        }

        [Test, Continuous]
        public void TreeSelectTwoChildrenWithChild()
        {
            AssertTreeSelectMapsInputAsExpected(
                Tree.Node(1u, Tree.Node(2u, Tree.Node(4u)), Tree.Node(3u, Tree.Node(5u))),
                Tree.Node(1L, Tree.Node(2L, Tree.Node(4L)), Tree.Node(3L, Tree.Node(5L)))
                );
        }

        [Test, Continuous]
        public void ComplexTreeSelectTwoChildrenWithChild()
        {
            AssertTreeSelectMapsInputAsExpected(
                Tree.Node(
                    1u,
                    Tree.Node(
                        6u),
                    Tree.Node(
                        2u,
                        Tree.Node(
                            4u),
                        Tree.Node(
                            7u)),
                    Tree.Node(
                        3u,
                        Tree.Node(
                            5u,
                            Tree.Node(
                                8u)))),
                Tree.Node(
                    1L,
                    Tree.Node(
                        6L),
                    Tree.Node(
                        2L,
                        Tree.Node(
                            4L),
                        Tree.Node(
                            7L)),
                    Tree.Node(
                        3L,
                        Tree.Node(
                            5L,
                            Tree.Node(
                                8L))))
                );
        }

        [Test, Continuous]
        public void TreeSelectVisitsInReversePreorderTraversal()
        {
            //Tree.Select calls the mapping function for each tree node, but that might have side effects.
            //therefore, we want to ensure that the mapping function is called in some specific, reproducible order.
            //In particular, this happens currently in the idGenerator for Cijferlijsten.

            var tree = Tree.Node("", Tree.Node(""), Tree.Node("", Tree.Node(""), Tree.Node("")), Tree.Node("", Tree.Node("", Tree.Node(""))));
            var count = tree.PreorderTraversal().Count();
            Assert.That(count, Is.EqualTo(8));
            var counter = 1;
            var mappedTree = tree.Select(_ => counter++);
            var nodeValuesInPreorder = mappedTree.PreorderTraversal().Select(n => n.NodeValue);
            PAssert.That(() => nodeValuesInPreorder.SequenceEqual(Enumerable.Range(1, 8).Reverse()));
        }

        [Test, Continuous]
        public void CanWorkWithDeepTreesWithoutStackoverflow()
        {
            var input = Tree.BuildRecursively(0u, i => i < 100000 ? new[] { i + 1 } : new uint[0]);
            var output = Tree.BuildRecursively(0L, i => i < 100000 ? new[] { i + 1 } : new long[0]);
            var mappedInput = input.Select(i => (long)i);
            bool areEqual = mappedInput.Equals(output);
            Assert.That(areEqual, "Deep trees should be selectable and comparable too");
            Assert.That(input.Height(), Is.EqualTo(100001));
        }

        [Test, Continuous]
        public void SingleNodeHasHeight1()
        {
            PAssert.That(() => Tree.Node(0).Height() == 1);
        }

        [Test, Continuous]
        public void RightLeaningTreeComputesHeight4()
        {
            PAssert.That(() => Tree.Node(1u, Tree.Node(6u), Tree.Node(2u, Tree.Node(4u), Tree.Node(7u)), Tree.Node(3u, Tree.Node(5u, Tree.Node(8u)))).Height() == 4);
        }

        [Test, Continuous]
        public void MessyTreeComputesHeight4()
        {
            PAssert.That(() => Tree.Node(1u, Tree.Node(6u), Tree.Node(3u, Tree.Node(5u, Tree.Node(8u))), Tree.Node(2u, Tree.Node(4u), Tree.Node(7u))).Height() == 4);
        }

        static void AssertTreeSelectMapsInputAsExpected(Tree<uint> input, Tree<long> expected)
        {
            var output = input.Select(i => (long)i);
            Assert.That(output, Is.EqualTo(expected));
        }

        [Test, Continuous]
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
