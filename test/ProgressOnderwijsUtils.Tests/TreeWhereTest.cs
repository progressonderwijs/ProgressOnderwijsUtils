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
        public void WhereJaggyStructureSurvives()
        {
            var tree = Tree.Node("1", Tree.Node("2", Tree.Node("x", Tree.Node("deeper"))), Tree.Node("3"), Tree.Node("y", Tree.Node("4")));
            var whereTrue = tree.WhereNodeValue(_ => true);
            PAssert.That(() => tree.Equals(whereTrue));
        }

        [Fact]
        public void NastySideEffectsHappenInConsistentOrder()
        {
            var tree = Tree.Node("1", Tree.Node("2", Tree.Node("x")), Tree.Node("3"), Tree.Node("y", Tree.Node("4")));
            var preorder = tree.PreorderTraversal().Select(n => n.NodeValue).ToArray();
            var expectedOrder = "123y4x".Select(c => c.ToString());
            var orderOfWhereTrueCalls = new List<string>();
            var orderOfWhereFalseCalls = new List<string>();

            var unused1 = tree.Where(n => {
                orderOfWhereTrueCalls.Add(n.NodeValue);
                return true;
            });
            var unused2 = tree.Where(n => {
                orderOfWhereFalseCalls.Add(n.NodeValue);
                return false;
            });

            PAssert.That(() => preorder.Take(1).SetEqual(orderOfWhereFalseCalls));
            PAssert.That(() => preorder.SetEqual(orderOfWhereTrueCalls));
            PAssert.That(() => expectedOrder.Take(1).SequenceEqual(orderOfWhereFalseCalls));
            PAssert.That(() => expectedOrder.SequenceEqual(orderOfWhereTrueCalls));
        }
    }
}
