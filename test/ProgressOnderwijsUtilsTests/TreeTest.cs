using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public sealed class TreeTest
	{
		[Test]
		public void TreeStoresInfo()
		{
			var tree = Tree.Node("abc", Tree.Node("def"), Tree.Node("xyz"));
			PAssert.That(() => tree.NodeValue == "abc");
			PAssert.That(() => tree.Children.Count == 2);
			PAssert.That(() => tree.Children.All(kid => kid.Children.Count == 0));
			PAssert.That(() => tree.Children[0].NodeValue == "def");
			PAssert.That(() => tree.Children[1].NodeValue == "xyz");
		}


		[Test]
		public void TreeWithManyLeaves()
		{
			var tree = Tree.Node(100, Tree.Node(1), Tree.Node(2), Tree.Node(3), Tree.Node(4));
			PAssert.That(() => tree.Children.Count == 4);
			PAssert.That(() => tree.Children.Select((kid, index) => kid.NodeValue == index + 1).All(b => b));
		}


		[Test]
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
		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
		public void PreorderTraversalNormalCase()
		{
			var e = Tree.Node("e");
			var d = Tree.Node("d");
			var cd = Tree.Node("c", d);
			var b = Tree.Node("b");
			var abcde = Tree.Node("a", b, cd, e);
			PAssert.That(() => abcde.PreorderTraversal().Select(path => path.Head.SubTree.NodeValue).SequenceEqual(new[] { "a", "b", "c", "d", "e" }));
			PAssert.That(() => abcde.PreorderTraversal().Select(path => path.Select(tree => tree.SubTree.NodeValue).JoinStrings()).SequenceEqual(new[] { "a", "ba", "ca", "dca", "ea" }));
		}
	}
}
