﻿using System.Collections.Generic;
using System;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Collections
{
    static class CachedTreeBuilder<T>
    {
        sealed class TreeNodeBuilder
        {
            public T value;
            public List<TreeNodeBuilder> tempKids;
            public Tree<T> finishedNode;

            public void GenerateOutput()
            {
                if (finishedNode != null) {
                    return;
                }
                var finishedKidsNodes = new Tree<T>[tempKids.Count];
                for (int i = 0; i < finishedKidsNodes.Length; i++) {
                    finishedKidsNodes[i] = tempKids[i].finishedNode;
                    if (finishedKidsNodes[i] == null) {
                        throw new InvalidOperationException("Cycle detected!");
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
            var branchesWithBuilders = new Dictionary<T, TreeNodeBuilder> { { rootNodeValue, rootBuilder } };

            var needsKids = new Stack<TreeNodeBuilder>();
            needsKids.Push(rootBuilder);

            while (needsKids.Count > 0) {
                var nodeBuilderThatWantsKids = needsKids.Pop();
                if (nodeBuilderThatWantsKids.tempKids == null) {
                    var kids = kidLookup(nodeBuilderThatWantsKids.value);
                    var tempKidBuilders = new List<TreeNodeBuilder>();
                    foreach (var kid in kids.EmptyIfNull()) {
                        TreeNodeBuilder builderForKid;
                        if (!branchesWithBuilders.TryGetValue(kid, out builderForKid)) {
                            builderForKid = new TreeNodeBuilder { value = kid, };
                            needsGenerateOutput.Push(builderForKid);
                            branchesWithBuilders.Add(kid, builderForKid);
                            needsKids.Push(builderForKid);
                        } else if (builderForKid.tempKids == null) {
                            needsKids.Push(builderForKid);
                            needsGenerateOutput.Push(builderForKid);
                        }
                        tempKidBuilders.Add(builderForKid);
                    }
                    nodeBuilderThatWantsKids.tempKids = tempKidBuilders;
                }
            }

            while (needsGenerateOutput.Count > 0) {
                needsGenerateOutput.Pop().GenerateOutput();
            }

            return rootBuilder.finishedNode;
        }
    }
}
