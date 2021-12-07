using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Collections
{
    static class CachedTreeBuilder<TInput, TNodeValue>
    {
        sealed class TreeNodeBuilder
        {
            public readonly TInput value;
            public TreeNodeBuilder? parent;
            public int idxInParent;
            public Tree<TNodeValue>[]? kids;

            public TreeNodeBuilder(TInput value)
                => this.value = value;
        }

        [Pure]
        public static Tree<TNodeValue> Resolve(TInput rootNodeValue, Func<TInput, IEnumerable<TInput>?> kidLookup, Func<TInput, Tree<TNodeValue>[], Tree<TNodeValue>> map)
        {
            var needsKids = new Stack<TreeNodeBuilder>();

            var generatedNodes = 0;
            var rootBuilder = new TreeNodeBuilder(rootNodeValue);
            generatedNodes++;

            needsKids.Push(rootBuilder);

            while (true) {
                var nodeBuilderThatWantsKids = needsKids.Pop();
                var kids = kidLookup(nodeBuilderThatWantsKids.value);
                if (kids != null) { //allow null to represent absence of kids
                    var kidIdx = 0;
                    foreach (var kid in kids) {
                        var builderForKid = new TreeNodeBuilder(kid) { idxInParent = kidIdx++, parent = nodeBuilderThatWantsKids };
                        generatedNodes++;

                        if (generatedNodes >= 10_000_000) {
                            throw new InvalidOperationException("Tree too large (possibly a cycle?)");
                        }
                        needsKids.Push(builderForKid);
                    }
                    if (kidIdx > 0) {
                        nodeBuilderThatWantsKids.kids = new Tree<TNodeValue>[kidIdx];
                        continue;
                    }
                }

                var toGenerate = nodeBuilderThatWantsKids;
                while (true) {
                    var finishedNode = map(toGenerate.value, toGenerate.kids ?? EmptyKids());
                    if (toGenerate.idxInParent == 0) {
                        if (toGenerate.parent == null) {
                            return finishedNode;
                        }
                        Debug.Assert(toGenerate.parent.kids?[toGenerate.idxInParent] == null, "has already been generated");
                        toGenerate.parent.kids![toGenerate.idxInParent] = finishedNode;
                        toGenerate = toGenerate.parent;
                    } else {
                        Debug.Assert(toGenerate.parent?.kids?[toGenerate.idxInParent] == null, "has already been generated");
                        toGenerate.parent!.kids![toGenerate.idxInParent] = finishedNode;
                        break;
                    }
                }
            }
        }

        static Tree<TNodeValue>[] EmptyKids()
            => Array.Empty<Tree<TNodeValue>>();
    }
}
