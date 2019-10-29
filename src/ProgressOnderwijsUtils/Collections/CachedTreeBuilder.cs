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
            public TreeNodeBuilder parent;
            public int idxInParent;
            public Tree<T>[] kids;
        }

        [Pure]
        public static Tree<T> Resolve(T rootNodeValue, Func<T, IEnumerable<T>> kidLookup)
        {
            var needsKids = new Stack<TreeNodeBuilder>();

            var generatedNodes = 0;
            var rootBuilder = new TreeNodeBuilder { value = rootNodeValue, };
            generatedNodes++;

            needsKids.Push(rootBuilder);

            while (true) {
                var nodeBuilderThatWantsKids = needsKids.Pop();
                var kids = kidLookup(nodeBuilderThatWantsKids.value);
                if (kids != null) { //allow null to represent absence of kids
                    var kidIdx = 0;
                    foreach (var kid in kids) {
                        var builderForKid = new TreeNodeBuilder { value = kid, idxInParent = kidIdx++, parent = nodeBuilderThatWantsKids };
                        generatedNodes++;

                        if (generatedNodes >= 10_000_000) {
                            throw new InvalidOperationException("Tree too large (possibly a cycle?)");
                        }
                        needsKids.Push(builderForKid);
                    }
                    if (kidIdx > 0) {
                        nodeBuilderThatWantsKids.kids = new Tree<T>[kidIdx];
                        continue;
                    }
                }

                var toGenerate = nodeBuilderThatWantsKids;
                while (true) {
                    var finishedNode = Tree.Node(toGenerate.value, toGenerate.kids);
                    if (toGenerate.idxInParent == 0) {
                        if (toGenerate.parent == null) {
                            return finishedNode;
                        }
                        Debug.Assert(toGenerate.parent.kids[toGenerate.idxInParent] == null, "has already been generated");
                        toGenerate.parent.kids[toGenerate.idxInParent] = finishedNode;
                        toGenerate = toGenerate.parent;
                    } else {
                        Debug.Assert(toGenerate.parent.kids[toGenerate.idxInParent] == null, "has already been generated");
                        toGenerate.parent.kids[toGenerate.idxInParent] = finishedNode;
                        break;
                    }
                }
            }
        }
    }
}
