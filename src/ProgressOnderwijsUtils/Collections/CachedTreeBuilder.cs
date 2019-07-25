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
            var needsGenerateOutput = new Stack<TreeNodeBuilder>(); //in order of creation; so junctions always before their kids.

            var rootBuilder = new TreeNodeBuilder { value = rootNodeValue, };
            needsGenerateOutput.Push(rootBuilder);

            var needsKids = new Stack<TreeNodeBuilder>();
            needsKids.Push(rootBuilder);

            while (needsKids.Count > 0) {
                var nodeBuilderThatWantsKids = needsKids.Pop();
                var kids = kidLookup(nodeBuilderThatWantsKids.value);
                if (kids != null) { //allow null to represent absence of kids
                    var tempKidBuilders = new List<TreeNodeBuilder>();
                    foreach (var kid in kids) {
                        var builderForKid = new TreeNodeBuilder { value = kid, };
                        if (needsGenerateOutput.Count >= 100 * 1000 * 1000) {
                            throw new InvalidOperationException("Tree too large (possibly a cycle?)");
                        }
                        needsGenerateOutput.Push(builderForKid);
                        needsKids.Push(builderForKid);
                        tempKidBuilders.Add(builderForKid);
                    }
                    nodeBuilderThatWantsKids.tempKids = tempKidBuilders.ToArray();
                } else {
                    nodeBuilderThatWantsKids.tempKids = Array.Empty<TreeNodeBuilder>();
                }
            }

            while (needsGenerateOutput.Count > 0) {
                needsGenerateOutput.Pop().GenerateOutput();
            }

            return rootBuilder.finishedNode;
        }
    }
}
