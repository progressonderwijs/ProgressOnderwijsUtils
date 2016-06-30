﻿using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business.Test;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    struct DagNode
    {
        public readonly string Name;
        readonly IReadOnlyList<string> Dependencies;
        public IEnumerable<DagNode> Children(Dictionary<string, DagNode> lookup) => Dependencies.Select(name => lookup.GetOrDefault(name, new DagNode(name)));

        public DagNode(string name, params string[] dependencies)
        {
            Name = name;
            Dependencies = dependencies;
        }
    }

    public class OrderByTopologyTest
    {
        [Test, Continuous]
        public void NodesWithoutDependenciesAreNotSorted()
        {
            var example = new[] { "a", "b", "d", "c" };
            PAssert.That(() => example.OrderByTopology(s => new string[0]).SequenceEqual(example));
        }

        [Test]
        public void SimpleDependencyComesFirst()
        {
            var nodes = new[] {
                new DagNode("a", "b"),
                new DagNode("b", "c"),
                new DagNode("c"),
            };
            var lookup = nodes.ToDictionary(s => s.Name);
            PAssert.That(() => nodes.OrderByTopology(node => node.Children(lookup)).Select(s => s.Name).SequenceEqual(new[] { "c", "b", "a" }));
        }

        [Test]
        public void DoesNotDetectCircularDependencies()
        {
            var nodes = new[] {
                new DagNode("a", "b"),
                new DagNode("b", "c"),
                new DagNode("c", "a"),
            };
            var lookup = nodes.ToDictionary(s => s.Name);
            PAssert.That(() => nodes.OrderByTopology(node => node.Children(lookup)).Select(s => s.Name).SequenceEqual(new[] { "c", "b", "a" }));
        }

        [Test]
        public void SupportsMulitpleDependancies()
        {
            var nodes = new[] {
                new DagNode("c", "b"),
                new DagNode("b", "a"),
                new DagNode("a"),
                new DagNode("z", "y", "b"),
                new DagNode("y", "x"),
                new DagNode("x", "a"),
            };
            var lookup = nodes.ToDictionary(s => s.Name);
            var expected = new[] { "a", "b", "c", "x", "y", "z" };
            PAssert.That(() => nodes.OrderByTopology(node => node.Children(lookup)).Select(s => s.Name).SequenceEqual(expected));
        }
    }
}
