using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExpressionToCodeLib;
using NUnit.Framework;
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

			PAssert.That(() => Tree.EqualityComparer(StringComparer.OrdinalIgnoreCase).Equals(tree1,tree2));
			PAssert.That(() => !Tree.EqualityComparer(StringComparer.OrdinalIgnoreCase).Equals(tree1, tree4));
		}

	}
}
