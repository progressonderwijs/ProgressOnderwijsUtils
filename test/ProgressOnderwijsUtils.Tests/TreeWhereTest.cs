using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using Xunit;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class TreeWhereTest
    {
        [Fact]
        public void SingleNodeWhereWorks()
        {
            var tree = Tree.Node("abc");
            PAssert.That(() => tree.Where(n => n.NodeValue == "x") == null);
            PAssert.That(() => tree.Where(n => n.NodeValue.Length == 3) != null);
        }

        [Fact]
        public void WhereFiltersOutDescenantsToo()
        {
            var tree = Tree.Node("1", Tree.Node("2", Tree.Node("x")), Tree.Node("3"), Tree.Node("y", Tree.Node("4")));
            var whereNumeric = tree.Where(n => int.TryParse(n.NodeValue, out _));
            var expected = Tree.Node("1", Tree.Node("2"), Tree.Node("3"));
            PAssert.That(() => expected.Equals(whereNumeric));
        }

        [Fact]
        public void NastySideEffectsHappenInConsistentOrder()
        {
            var tree = Tree.Node("1", Tree.Node("2", Tree.Node("x")), Tree.Node("3"), Tree.Node("y", Tree.Node("4")));
            var preorder = tree.PreorderTraversal().Select(n => n.NodeValue).ToArray();
            var expectedOrder = "4x23y1".Select(c => c.ToString());
            var orderOfWhereTrueCalls = new List<string>();
            var orderOfWhereFalseCalls = new List<string>();

            var unused = tree.Where(n => {
                orderOfWhereTrueCalls.Add(n.NodeValue);
                return true;
            });
            unused = tree.Where(n => {
                orderOfWhereFalseCalls.Add(n.NodeValue);
                return false;
            });

            PAssert.That(() => preorder.SetEqual(orderOfWhereFalseCalls));
            PAssert.That(() => preorder.SetEqual(orderOfWhereTrueCalls));
            PAssert.That(() => expectedOrder.SequenceEqual(orderOfWhereFalseCalls));
            PAssert.That(() => expectedOrder.SequenceEqual(orderOfWhereTrueCalls));
        }
    }
}
