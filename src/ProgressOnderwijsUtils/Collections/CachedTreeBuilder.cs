using System.Collections.Generic;
using System;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Collections
{
    static class CachedTreeBuilder<T>
    {
        static readonly TreeNodeBuilder[] Empty = new TreeNodeBuilder[0];

        sealed class TreeNodeBuilder
        {
            public T value;
            public TreeNodeBuilder[] tempKids;
            public Tree<T> finishedNode;

            public void GenerateOutput()
            {
                var finishedKidsNodes = new Tree<T>[tempKids.Length];
                for (int i = 0; i < finishedKidsNodes.Length; i++) {
                    finishedKidsNodes[i] = tempKids[i].finishedNode;
                    if (finishedKidsNodes[i] == null) {
                        throw new InvalidOperationException("Cycle detected!");
                    }
                }
                finishedNode = Tree.Node(value, finishedKidsNodes);

                tempKids = Empty;
            }
        }

        [Pure]
        public static Tree<T> Resolve(T rootNodeValue, Func<T, IEnumerable<T>> kidLookup)
        {
            var needsGenerateOutput = new Stack<TreeNodeBuilder>(); //in order of creation; so junctions always before their kids.

            var rootBuilder = new TreeNodeBuilder { value = rootNodeValue, };
            needsGenerateOutput.Push(rootBuilder);
            var branchesWithBuilders = new Dictionary<T, TreeNodeBuilder> { { rootNodeValue, rootBuilder } };

            var needsKids = new Stack<TreeNodeBuilder>();
            needsKids.Push(rootBuilder);
            var tempKidBuilder = new TreeNodeBuilder[15];

            while (needsKids.Count > 0) {
                var nodeBuilderThatWantsKids = needsKids.Pop();
                if (nodeBuilderThatWantsKids.tempKids == null) {
                    var kids = kidLookup(nodeBuilderThatWantsKids.value);
                    if (kids == null) {
                        nodeBuilderThatWantsKids.tempKids = Empty;
                    } else {
                        int kidCount = 0;
                        foreach (var kid in kids) {
                            TreeNodeBuilder builderForKid;
                            if (!branchesWithBuilders.TryGetValue(kid, out builderForKid)) {
                                builderForKid = new TreeNodeBuilder { value = kid, };
                                needsGenerateOutput.Push(builderForKid);
                                branchesWithBuilders.Add(kid, builderForKid);
                                needsKids.Push(builderForKid);
                            } else if (builderForKid.tempKids == null) {
                                needsKids.Push(builderForKid);
                            }
                            if (tempKidBuilder.Length == kidCount + 1) {
                                Array.Resize(ref tempKidBuilder, tempKidBuilder.Length * 2 + 1); //will eventually precisely reach int.MaxValue.
                            }
                            tempKidBuilder[kidCount++] = builderForKid;
                        }
                        nodeBuilderThatWantsKids.tempKids = tempKidBuilder;
                        Array.Resize(ref nodeBuilderThatWantsKids.tempKids, kidCount);
                    }
                }
            }

            while (needsGenerateOutput.Count > 0) {
                needsGenerateOutput.Pop().GenerateOutput();
            }

            return rootBuilder.finishedNode;
        }
    }
}
