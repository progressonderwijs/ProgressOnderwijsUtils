#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Collections
{
    static class CachedTreeBuilder<T>
    {
        sealed class TreeNodeBuilder
        {
            public T value;
            public TreeNodeBuilder firstChildOfParent;
            public TreeNodeBuilder[] tempKids;
            public Tree<T> finishedNode;
        }

        [Pure]
        public static Tree<T> Resolve(T rootNodeValue, Func<T, IEnumerable<T>> kidLookup)
        {
            var needsKids = new Stack<TreeNodeBuilder>();

            var generatedNodes = 0;
            var rootBuilder = new TreeNodeBuilder { value = rootNodeValue, };
            generatedNodes++;

            needsKids.Push(rootBuilder);

            var tempKidBuilders = new List<TreeNodeBuilder>();

            while (needsKids.Count > 0) {
                var nodeBuilderThatWantsKids = needsKids.Pop();
                var kids = kidLookup(nodeBuilderThatWantsKids.value);
                if (kids != null) { //allow null to represent absence of kids
                    foreach (var kid in kids) {
                        var builderForKid = new TreeNodeBuilder { value = kid, };
                        generatedNodes++;

                        if (generatedNodes >= 10_000_000) {
                            throw new InvalidOperationException("Tree too large (possibly a cycle?)");
                        }
                        needsKids.Push(builderForKid);
                        tempKidBuilders.Add(builderForKid);
                    }
                    if (tempKidBuilders.Count > 0) {
                        nodeBuilderThatWantsKids.tempKids = tempKidBuilders.ToArray();
                        nodeBuilderThatWantsKids.tempKids[0].firstChildOfParent = nodeBuilderThatWantsKids;
                        tempKidBuilders.Clear();
                        continue;
                    }
                }
                nodeBuilderThatWantsKids.finishedNode = Tree.Node(nodeBuilderThatWantsKids.value);
                var toGenerate = nodeBuilderThatWantsKids.firstChildOfParent;
                nodeBuilderThatWantsKids.firstChildOfParent = null;
                while (toGenerate != null) {
                    Debug.Assert(toGenerate.finishedNode == null, "has already been generated");
                    Debug.Assert(toGenerate.tempKids.Length > 0, "leaf nodes should not need this method");
                    var finishedKidsNodes = new Tree<T>[toGenerate.tempKids.Length];
                    for (var i = 0; i < finishedKidsNodes.Length; i++) {
                        finishedKidsNodes[i] = toGenerate.tempKids[i].finishedNode;
                        Debug.Assert(finishedKidsNodes[i] != null, "this should have been been done already");
                    }
                    var nextToGenerate = toGenerate.firstChildOfParent;

                    toGenerate.finishedNode = Tree.Node(toGenerate.value, finishedKidsNodes);
                    toGenerate.tempKids = null;
                    toGenerate.firstChildOfParent = null;
                    toGenerate.value = default;
                    toGenerate = nextToGenerate;
                }
            }

            return rootBuilder.finishedNode ?? throw new InvalidOperationException("Internal error detected!");
        }
    }
}
