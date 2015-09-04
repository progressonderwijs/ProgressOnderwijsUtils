using System.Collections.Generic;
using System;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Collections
{
    sealed class CachedTreeBuilder<T>
    {
        readonly Dictionary<T, NodeContainer> completedBranches = new Dictionary<T, NodeContainer>();
        readonly Func<T, IEnumerable<T>> kidLookup;
        public CachedTreeBuilder(Func<T, IEnumerable<T>> kidLookup) { this.kidLookup = kidLookup; }
        static readonly NodeContainer[] Empty = new NodeContainer[0];

        sealed class NodeContainer
        {
            public T value;
            public NodeContainer[] tempKids;
            public Tree<T> output;

            public void GenerateOutput()
            {
                var branches = new Tree<T>[tempKids.Length];
                for (int i = 0; i < branches.Length; i++) {
                    branches[i] = tempKids[i].output;
                    if (branches[i] == null) {
                        throw new InvalidOperationException("Cycle detected!");
                    }
                }
                output = Tree.Node(value, branches);

                tempKids = Empty;
            }
        }

        [Pure]
        public Tree<T> Resolve(T rootNodeValue)
        {
            Stack<NodeContainer> todoGenerateOutput = new Stack<NodeContainer>(); //in order of creation; so junctions always before their kids.

            NodeContainer rootContainer;
            if (completedBranches.TryGetValue(rootNodeValue, out rootContainer)) {
                return rootContainer.output;
            }
            rootContainer = new NodeContainer { value = rootNodeValue, };
            todoGenerateOutput.Push(rootContainer);
            completedBranches.Add(rootNodeValue, rootContainer);

            var todo = new Stack<NodeContainer>();
            todo.Push(rootContainer);
            var tempKidBuilder = new NodeContainer[15];
            while (todo.Count > 0) {
                var nodeContainer = todo.Pop();
                if (nodeContainer.tempKids == null) {
                    var kids = kidLookup(nodeContainer.value);
                    if (kids != null) {
                        int kidCount = 0;
                        //var kidCons = FastArrayBuilder<NodeContainer>.Create();
                        foreach (var kid in kids) {
                            NodeContainer con;
                            if (!completedBranches.TryGetValue(kid, out con)) {
                                con = new NodeContainer { value = kid, };
                                todoGenerateOutput.Push(con);
                                completedBranches.Add(kid, con);
                                todo.Push(con);
                            } else if (con.tempKids == null) {
                                todo.Push(con);
                            }
                            if (tempKidBuilder.Length == kidCount + 1) {
                                Array.Resize(ref tempKidBuilder, tempKidBuilder.Length * 2 + 1); //will eventually precisely reach int.MaxValue.
                            }
                            tempKidBuilder[kidCount++] = con;
                        }
                        nodeContainer.tempKids = tempKidBuilder;
                        Array.Resize(ref nodeContainer.tempKids, kidCount);
                    } else {
                        nodeContainer.tempKids = Empty;
                    }
                }
            }

            while (todoGenerateOutput.Count > 0) {
                todoGenerateOutput.Pop().GenerateOutput();
            }

            return rootContainer.output;
        }
    }
}
