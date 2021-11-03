using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Collections
{
    public interface IOrdering<in T>
    {
        bool LessThan(T a, T b);
    }

    public readonly struct SortedSet<T, TOrder> : IEquatable<SortedSet<T, TOrder>>, IReadOnlyList<T>
        where TOrder : struct, IOrdering<T>
    {
        readonly T[]? sortedDistinctValues;
        static readonly T[] empty = Array.Empty<T>();

        static TOrder Ordering
            => new();

        SortedSet(T[] sortedDistinctValues)
            => this.sortedDistinctValues = sortedDistinctValues;

        [CodeThatsOnlyUsedForTests]
        public static SortedSet<T, TOrder> Empty
            => new(Array.Empty<T>());

        public static SortedSet<T, TOrder> FromValues(IEnumerable<T> values)
            => FromValues(values as T[] ?? values.ToArray());

        public static SortedSet<T, TOrder> FromValues(T[] values)
        {
            for (var i = 1; i < values.Length; i++) {
                if (!Ordering.LessThan(values[i - 1], values[i])) {
                    return FromUnsortedValues(values);
                }
            }
            return new(values);
        }

        static SortedSet<T, TOrder> FromUnsortedValues(T[] values)
        {
            if (values.Length < 2) {
                return new(values);
            }
            var mutableCopy = values.ToArray();
            return FromMutableUnsortedTmpArray(mutableCopy, values.Length);
        }

        static SortedSet<T, TOrder> FromMutableUnsortedTmpArray(T[] privateMutableArray, int valuesLength)
        {
            Array.Sort(privateMutableArray, 0, valuesLength);
            var newLength = Algorithms.CountAndMoveDistinctValuesToFront(privateMutableArray, valuesLength);
            Array.Resize(ref privateMutableArray, newLength);
            return new(privateMutableArray);
        }

        public T[] ValuesInOrder
            => sortedDistinctValues ?? empty;

        [Pure]
        public bool Contains(T value)
            => sortedDistinctValues is not null
                && BinarySearchAlgorithm.IdxAfterLastLtNode(sortedDistinctValues, value) is var idxAfterLastLtNode
                && idxAfterLastLtNode < sortedDistinctValues.Length
                && !Ordering.GreaterThan(sortedDistinctValues[idxAfterLastLtNode], value);

        [Pure]
        public static SortedSet<T, TOrder> MergeSets(IEnumerable<SortedSet<T, TOrder>> inputSets)
        {
            var output = new List<T>();
            foreach (var inputSet in inputSets) {
                output.AddRange(inputSet.ValuesInOrder);
            }
            return FromMutableUnsortedTmpArray(output.ToArray(), output.Count);
        }

        public static class BinarySearchAlgorithm
        {
            public static int IdxAfterLastLtNode(T[] sortedArray, T needle)
            {
                int start = 0, end = sortedArray.Length;
                //invariant: only LT nodes before start
                //invariant: only GTE nodes at or past end

                while (end != start) {
                    var midpoint = end + start >> 1;
                    // start <= midpoint < end
                    if (Ordering.LessThan(sortedArray[midpoint], needle)) {
                        start = midpoint + 1; //i.e.  midpoint < start1 so start0 < start1
                    } else {
                        end = midpoint; //i.e end1 = midpoint so end1 < end0
                    }
                }
                return end;
            }
        }

        public static class Algorithms
        {
            public static int CountAndMoveDistinctValuesToFront(T[] arr, int len)
            {
                if (len < 2) {
                    return len;
                }
                var distinctUptoIdx = 0;
                var readIdx = 1;
                do {
                    if (Ordering.LessThan(arr[distinctUptoIdx], arr[readIdx])) {
                        distinctUptoIdx++;
                        arr[distinctUptoIdx] = arr[readIdx];
                    }
                    readIdx++;
                } while (readIdx < len);
                return distinctUptoIdx + 1;
            }
        }

        public bool Equals(SortedSet<T, TOrder> other)
        {
            if (ValuesInOrder.Length != other.ValuesInOrder.Length) {
                return false;
            }
            for (var i = 0; i < ValuesInOrder.Length; i++) {
                if (Ordering.LessThan(ValuesInOrder[i], other.ValuesInOrder[i]) || Ordering.GreaterThan(ValuesInOrder[i], other.ValuesInOrder[i])) {
                    return false;
                }
            }
            return true;
        }

        public IEnumerator<T> GetEnumerator()
            => ((IEnumerable<T>)ValuesInOrder).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public int Count
            => ValuesInOrder.Length;

        public T this[int index]
            => ValuesInOrder[index];
    }

    public static class SortedSetHelpers
    {
        public static SortedSet<T, TOrder> MergeSets<T, TOrder>(this IEnumerable<SortedSet<T, TOrder>> inputSets)
            where TOrder : struct, IOrdering<T>
            => SortedSet<T, TOrder>.MergeSets(inputSets);

        public static bool GreaterThan<T>(this IOrdering<T> ordering, T a, T b)
            => ordering.LessThan(b, a);
    }
}
