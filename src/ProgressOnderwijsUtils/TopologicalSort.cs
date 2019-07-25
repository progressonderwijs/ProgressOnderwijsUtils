#nullable disable
using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class TopologicalSort
    {
        /// <summary>
        /// Orders a DAG (directed acyclic graph), children before parents.  If two nodes are not related, they remain in the same order as in the seeds.
        /// All DAG nodes reachable via listChildrenOf are returned, not just the seeds - the output sequence can thus be larger than the input.
        /// </summary>
        [NotNull]
        public static T[] OrderByTopology<T>([NotNull] this IEnumerable<T> seeds, Func<T, IEnumerable<T>> listChildrenOf)
            => seeds.OrderByTopology(listChildrenOf, EqualityComparer<T>.Default);

        /// <summary>
        /// Orders a DAG (directed acyclic graph), children before parents.  If two nodes are not related, they remain in the same order as in the seeds.
        /// All DAG nodes reachable via listChildrenOf are returned, not just the seeds - the output sequence can thus be larger than the input.
        /// </summary>
        [NotNull]
        public static T[] OrderByTopology<T>([NotNull] this IEnumerable<T> seeds, Func<T, IEnumerable<T>> listChildren, IEqualityComparer<T> comparer)
        {
            var list = new List<T>();
            var visited = new HashSet<T>(comparer);
            foreach (var seed in seeds) {
                TopologicalSortVisit(visited, list, seed, listChildren);
            }
            return list.ToArray();
        }

        static void TopologicalSortVisit<T>([NotNull] HashSet<T> visited, List<T> output, T r, Func<T, IEnumerable<T>> listChildrenOf)
        {
            if (visited.Add(r)) {
                foreach (var kid in listChildrenOf(r)) {
                    TopologicalSortVisit(visited, output, kid, listChildrenOf);
                }
                output.Add(r);
            }
        }
    }
}
