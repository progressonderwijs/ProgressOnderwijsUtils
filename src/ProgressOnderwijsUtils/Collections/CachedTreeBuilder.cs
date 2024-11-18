namespace ProgressOnderwijsUtils.Collections;

static class TreeBuilder<TInput, TOutput>
{
    sealed class TreeNodeBuilder
    {
        public required TInput value;
        public TreeNodeBuilder? parent;
        public required int idxInParent;
        public TOutput[] kids = [];
    }

    [Pure]
    public static TOutput Build(TInput rootNodeValue, Func<TInput, IEnumerable<TInput>?> kidLookup, Func<TInput, TOutput[], TOutput> map)
    {
        var needsKids = new Stack<TreeNodeBuilder>();

        var generatedNodes = 0;
        var rootBuilder = new TreeNodeBuilder { value = rootNodeValue, idxInParent = 0, };
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
                    nodeBuilderThatWantsKids.kids = new TOutput[kidIdx];
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
