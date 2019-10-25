#nullable disable
using System;
using System.Collections.Generic;
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

            public void GenerateOutput()
            {
                if (finishedNode != null) {
                    return;
                }
                var finishedKidsNodes = new Tree<T>[tempKids.Length];
                for (var i = 0; i < finishedKidsNodes.Length; i++) {
                    finishedKidsNodes[i] = tempKids[i].finishedNode;
                    if (finishedKidsNodes[i] == null) {
                        throw new InvalidOperationException("Internal error detected!");
                    }
                }
                finishedNode = Tree.Node(value, finishedKidsNodes);
                tempKids = null;
            }
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
                while (toGenerate != null) {
                    toGenerate.GenerateOutput();
                    toGenerate = toGenerate.firstChildOfParent;
                }
            }

            return rootBuilder.finishedNode ?? throw new InvalidOperationException("Internal error detected!");
        }
    }
}
