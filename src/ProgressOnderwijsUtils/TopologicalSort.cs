using System;
using System.Collections.Generic;

namespace ProgressOnderwijsUtils;

public static class TopologicalSort
{
    /// <summary>
    /// Orders a DAG (directed acyclic graph), children before parents.  If two nodes are not related, they remain in the same order as in the seeds.
    /// All DAG nodes reachable via listChildrenOf are returned, not just the seeds - the output sequence can thus be larger than the input.
    /// </summary>
    public static (bool cycleDetected, T[] ordered) OrderByTopology<T>(this IEnumerable<T> seeds, Func<T, IEnumerable<T>> listChildrenOf)
        where T : notnull
        => seeds.OrderByTopology(listChildrenOf, EqualityComparer<T>.Default);

    sealed class NodeVisitState
    {
        public bool OnStack = true;
    }

    /// <summary>
    /// Orders a DAG (directed acyclic graph), children before parents.  If two nodes are not related, they remain in the same order as in the seeds.
    /// All DAG nodes reachable via listChildrenOf are returned, not just the seeds - the output sequence can thus be larger than the input.
    /// </summary>
    public static (bool cycleDetected, T[] ordered) OrderByTopology<T>(this IEnumerable<T> seeds, Func<T, IEnumerable<T>> listChildrenOf, IEqualityComparer<T> comparer)
        where T : notnull
    {
        var output = new List<T>();
        var visitState = new Dictionary<T, NodeVisitState>(comparer);
        var nextState = new NodeVisitState();
        var hasCycle = false;
        foreach (var seed in seeds) {
            TopologicalSortVisit(seed);
        }
        return (hasCycle, output.ToArray());

        void TopologicalSortVisit(T r)
        {
            if (visitState.TryAdd(r, nextState)) {
                var thisState = nextState;
                nextState = new NodeVisitState();
                foreach (var kid in listChildrenOf(r)) {
                    TopologicalSortVisit(kid);
                }
                output.Add(r);
                thisState.OnStack = false;
            } else if (visitState[r].OnStack) {
                hasCycle = true;
            }
        }
    }
}