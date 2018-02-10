using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Progress.Business.Tools
{
    public interface IOrdering<in T>
    {
        bool LessThan(T a, T b);
        bool Equal(T a, T b);
    }

    public struct SortedSet<T, TOrder> : IEquatable<SortedSet<T, TOrder>>
        where TOrder : struct, IOrdering<T>
    {
        readonly T[] sortedDistinctValues;
        static readonly T[] empty = Array.Empty<T>();
        static TOrder Ordering => default(TOrder);
        SortedSet(T[] sortedDistinctValues) => this.sortedDistinctValues = sortedDistinctValues;
        public static SortedSet<T, TOrder> Empty => FromSortedDistinctValues(Array.Empty<T>());
        public static SortedSet<T, TOrder> FromValues(IEnumerable<T> values) => FromValues(values as T[] ?? values.ToArray());

        public static SortedSet<T, TOrder> FromValues(T[] values)
        {
            for (int i = 1; i < values.Length; i++) {
                if (!Ordering.LessThan(values[i - 1], values[i])) {
                    return FromUnsortedValues(values);
                }
            }
            return FromSortedDistinctValues(values);
        }

        public static SortedSet<T, TOrder> FromSortedDistinctValues(T[] sortedDistinctRechten)
            => new SortedSet<T, TOrder>(sortedDistinctRechten);

        public static SortedSet<T, TOrder> FromUnsortedValues(T[] values)
        {
            var originalLength = values.Length;
            if (originalLength < 2) {
                return new SortedSet<T, TOrder>(values);
            }
            var tmpArray = GetCachedAccumulator(originalLength);
            for (var i = 0; i < originalLength; i++) {
                tmpArray[i] = values[i];
            }

            Algorithms.QuickSort(tmpArray, 0, originalLength - 1);
            var newLength = Algorithms.CountAndMoveDistinctValuesToFront(tmpArray, originalLength);
            var output = new T[newLength];
            for (var i = 0; i < output.Length; i++) {
                output[i] = tmpArray[i];
            }
            return new SortedSet<T, TOrder>(output);
        }

        public T[] ValuesInOrder => sortedDistinctValues ?? empty;

        [Pure]
        public bool Contains(T value)
        {
            var idxAfterLastLtNode = Algorithms.IdxAfterLastLtNode(sortedDistinctValues, value);
            return idxAfterLastLtNode < sortedDistinctValues.Length && Ordering.Equal(sortedDistinctValues[idxAfterLastLtNode], value);
        }

        [ThreadStatic]
        static T[] Accumulator;

        static T[] GetCachedAccumulator(int maxSize)
        {
            var outputValues = Accumulator ?? (Accumulator = new T[16]);
            if (outputValues.Length < maxSize) {
                var newSize = (int)Math.Max(maxSize, outputValues.Length * 3L / 2L);
                Accumulator = outputValues = new T[newSize];
            }
            return outputValues;
        }

        [Pure]
        public static SortedSet<T, TOrder> MergeSets(IEnumerable<SortedSet<T, TOrder>> inputSets)
        {
            var inputSetCursors = new Cursor[4];
            var inputSetCount = 0;
            int maxSize = 0;

            foreach (var set in inputSets) {
                if (inputSetCount == inputSetCursors.Length) {
                    Array.Resize(ref inputSetCursors, inputSetCount * 2);
                }

                var setValues = set.ValuesInOrder;
                var setSize = setValues.Length;
                if (setSize > 0) {
                    inputSetCursors[inputSetCount++] = new Cursor(0, setValues);
                    maxSize += setSize;
                }
            }
            if (inputSetCount < 2) {
                if (inputSetCount == 1) {
                    return FromSortedDistinctValues(inputSetCursors[0].Arr);
                } else {
                    return Empty;
                }
            }
            var outputValues = GetCachedAccumulator(maxSize);

            var outputLength = 0;
            while (inputSetCount > 0) {
                int minIdx = 0;
                var minValue = inputSetCursors[0].Value;
                for (var i = 1; i < inputSetCount; i++) {
                    var value = inputSetCursors[i].Value;
                    if (Ordering.LessThan(value, minValue)) {
                        minIdx = i;
                        minValue = value;
                    }
                }
                outputValues[outputLength++] = minValue;
                var writeIdx = minIdx;
                if (inputSetCursors[minIdx].MoveNext()) {
                    writeIdx++;
                }

                for (minIdx++; minIdx < inputSetCount; minIdx++) {
                    if (Ordering.Equal(minValue, inputSetCursors[minIdx].Value)) {
                        if (!inputSetCursors[minIdx].MoveNext()) {
                            continue;
                        }
                    }
                    if (minIdx != writeIdx) {
                        inputSetCursors[writeIdx] = inputSetCursors[minIdx];
                    }
                    writeIdx++;
                }
                inputSetCount = writeIdx;
            }

            var output = new T[outputLength];
            for (var i = 0; i < output.Length; i++) {
                output[i] = outputValues[i];
            }
            return FromSortedDistinctValues(output);
        }

        struct Cursor
        {
            public int Pos; //, Value;
            public readonly T[] Arr;

            public Cursor(int pos, T[] arr)
            {
                Pos = pos;
                Arr = arr;
            }

            public bool MoveNext()
            {
                Pos++;
                return Pos < Arr.Length;
            }

            public T Value => Arr[Pos];
        }

        public static class Algorithms
        {
            public static void Sort(T[] array) => QuickSort(array);

            public static void MergeSort(T[] array)
                => TopDownMergeSort(array, GetCachedAccumulator(array.Length), array.Length);

            public static void QuickSort(T[] array)
                => QuickSort(array, 0, array.Length - 1);

            public static void QuickSort(T[] array, int firstIdx, int lastIdx)
            {
                if (firstIdx < lastIdx) {
                    var pivot = Partition(array, firstIdx, lastIdx);
                    QuickSort(array, firstIdx, pivot);
                    QuickSort(array, pivot + 1, lastIdx);
                }
            }

            static int Partition(T[] array, int firstIdx, int lastIdx)
            {
                var pivotValue = array[(firstIdx + lastIdx) / 2];
                while (true) {
                    while (Ordering.LessThan(array[firstIdx], pivotValue)) {
                        firstIdx++;
                    }
                    while (Ordering.LessThan(pivotValue, array[lastIdx])) {
                        lastIdx--;
                    }
                    if (lastIdx <= firstIdx) {
                        return lastIdx;
                    }
                    var tmp = array[firstIdx];
                    array[firstIdx] = array[lastIdx];
                    array[lastIdx] = tmp;
                    firstIdx++;
                    lastIdx--;
                }
            }


            static void TopDownMergeSort(T[] items, T[] scratchSpace, int n)
            {
                CopyArray(items, 0, n, scratchSpace);
                TopDownSplitMerge(scratchSpace, 0, n, items);
            }

            static void TopDownSplitMerge(T[] source, int iBegin, int iEnd, T[] target)
            {
                if (iEnd - iBegin < 2) { // if run size == 1
                    return; //   consider it sorted
                }
                int iMiddle = (iEnd + iBegin) / 2; // iMiddle = mid point
                // recursively sort both runs from array T[] A into T[] B
                TopDownSplitMerge(target, iBegin, iMiddle, source); // sort the left  run
                TopDownSplitMerge(target, iMiddle, iEnd, source); // sort the right run
                // merge the resulting runs from array T[] B into T[] A
                TopDownMerge(source, iBegin, iMiddle, iEnd, target);
            }

            //  Left source half is A[ iBegin:iMiddle-1].
            // Right source half is A[iMiddle:iEnd-1   ].
            // Result is            B[ iBegin:iEnd-1   ].
            static void TopDownMerge(T[] source, int iBegin, int iMiddle, int iEnd, T[] target)
            {
                int i = iBegin, j = iMiddle;

                // While there are elements in the left or right runs...
                for (int k = iBegin; k < iEnd; k++) {
                    // If left run head exists and is <= existing right run head.
                    if (i < iMiddle && (j >= iEnd || !Ordering.LessThan(source[j], source[i]))) {
                        target[k] = source[i];
                        i++;
                    } else {
                        target[k] = source[j];
                        j++;
                    }
                }
            }

            static void CopyArray(T[] source, int iBegin, int iEnd, T[] target)
            {
                for (int k = iBegin; k < iEnd; k++) {
                    target[k] = source[k];
                }
            }

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

            public static int CountAndMoveDistinctValuesToFront(T[] arr, int len)
            {
                if (len < 2) {
                    return len;
                }
                int distinctUptoIdx = 0;
                int readIdx = 1;
                do {
                    if (!Ordering.Equal(arr[distinctUptoIdx], arr[readIdx])) {
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
            var a = ValuesInOrder;
            var b = other.ValuesInOrder;
            if (a.Length != b.Length) {
                return false;
            }
            for (int i = 0; i < a.Length; i++) {
                if (!Ordering.Equal(a[i], b[i])) {
                    return false;
                }
            }
            return true;
        }
    }

    public static class SortedSetHelpers
    {
        public static SortedSet<T, TOrder> MergeSets<T, TOrder>(this IEnumerable<SortedSet<T, TOrder>> inputSets)
            where TOrder : struct, IOrdering<T>
            => SortedSet<T, TOrder>.MergeSets(inputSets);
    }
}
