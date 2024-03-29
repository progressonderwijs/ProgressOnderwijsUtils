namespace ProgressOnderwijsUtils.Collections;

static class CachedTreeBuilder<TInput, TNodeValue>
{
    sealed class TreeNodeBuilder
    {
        public required TInput value;
        public TreeNodeBuilder? parent;
        public int idxInParent;
        public Tree<TNodeValue>[] kids = [];
    }

    [Pure]
    public static Tree<TNodeValue> Resolve(TInput rootNodeValue, Func<TInput, IEnumerable<TInput>?> kidLookup, Func<TInput, Tree<TNodeValue>[], Tree<TNodeValue>> map)
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
                    var builderForKid = new TreeNodeBuilder { value = kid, idxInParent = kidIdx++, parent = nodeBuilderThatWantsKids, };
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
                var finishedNode = map(toGenerate.value, toGenerate.kids);
                var parent = toGenerate.parent;
                if (parent == null) {
                    return finishedNode;
                } else if (toGenerate.idxInParent == 0) {
                    Debug.Assert(parent.kids[toGenerate.idxInParent] == null, "has already been generated");
                    parent.kids[toGenerate.idxInParent] = finishedNode;
                    toGenerate = parent;
                } else {
                    Debug.Assert(parent.kids[toGenerate.idxInParent] == null, "has already been generated");
                    parent.kids[toGenerate.idxInParent] = finishedNode;
                    break;
                }
            }
        }
    }
}
