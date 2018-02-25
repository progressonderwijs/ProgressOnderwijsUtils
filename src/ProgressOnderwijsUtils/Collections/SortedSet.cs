using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Collections
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
            public static void TopDownMergeSort(T[] array)
                => TopDownMergeSort(array, GetCachedAccumulator(array.Length), array.Length);

            public static T[] TopDownMergeSort_Copy(T[] array)
                => CopyingTopDownMergeSort(array, new T[array.Length], array.Length);

            public static void AltTopDownMergeSort(T[] array)
                => AltTopDownMergeSort(array, GetCachedAccumulator(array.Length), array.Length);

            public static void BottomUpMergeSort(T[] array)
                => BottomUpMergeSort(array, GetCachedAccumulator(array.Length), array.Length);

            public static void QuickSort(T[] array)
                => QuickSort(array, 0, array.Length);

            public static void QuickSort(T[] array, int firstIdx, int endIdx) {
                QuickSort_Inclusive(array, firstIdx, endIdx - 1);
            }

            static void QuickSort_Inclusive(T[] array, int firstIdx, int lastIdx)
            {
                while (true) {
                    if (lastIdx - firstIdx < InsertionSortBatchSize - 1) {
                        InsertionSort_InPlace(array, firstIdx, lastIdx + 1);
                        return;
                    } else {
                        var pivot = Partition(array, firstIdx, lastIdx);
                        if (pivot - firstIdx > lastIdx - pivot) {
                            QuickSort_Inclusive(array, pivot + 1, lastIdx);
                            lastIdx = pivot; //QuickSort(array, firstIdx, pivot);
                        } else {
                            QuickSort_Inclusive(array, firstIdx, pivot);
                            firstIdx = pivot + 1; //QuickSort(array, pivot + 1, lastIdx);
                        }
                    }
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

            public static void InsertionSort_InPlace(T[] array, int firstIdx, int idxEnd)
            {
                var writeIdx = firstIdx;
                var readIdx = writeIdx + 1;
                while (readIdx < idxEnd) {
                    var x = array[readIdx];
                    //writeIdx == readIdx -1;
                    while (writeIdx >= firstIdx && Ordering.LessThan(x, array[writeIdx])) {
                        array[writeIdx + 1] = array[writeIdx];
                        writeIdx--;
                    }

                    if (writeIdx + 1 != readIdx) {
                        array[writeIdx + 1] = x;
                    }
                    writeIdx = readIdx;
                    readIdx = readIdx + 1;
                }
            }

            public static void InsertionSort_Copy(T[] source, int firstIdx, int idxEnd, T[] target)
            {
                var readIdx = firstIdx;
                var writeIdx = firstIdx;

                while (readIdx < idxEnd) {
                    var x = source[readIdx];
                    //writeIdx == readIdx -1;
                    while (writeIdx > firstIdx && Ordering.LessThan(x, target[writeIdx - 1])) {
                        target[writeIdx] = target[writeIdx - 1];
                        writeIdx--;
                    }

                    target[writeIdx] = x;
                    readIdx = writeIdx = readIdx + 1;
                }
            }

            const int InsertionSortBatchSize = 32;

            static void AltTopDownMergeSort(T[] items, T[] scratch, int n)
            {
                CopyArray(items, 0, n, scratch);
                TopDownSplitMerge_Either(items, 0, n, scratch);
            }

            static void TopDownSplitMerge_Either(T[] items, int iBegin, int iEnd, T[] scratch)
            {
                if (iEnd - iBegin < InsertionSortBatchSize) {
                    InsertionSort_InPlace(items, iBegin, iEnd);
                    return;
                }
                int iMiddle = (iEnd + iBegin) / 2;
                TopDownSplitMerge_Either(scratch, iBegin, iMiddle, items);
                TopDownSplitMerge_Either(scratch, iMiddle, iEnd, items);
                Merge(scratch, iBegin, iMiddle, iEnd, items);
            }

            static T[] CopyingTopDownMergeSort(T[] items, T[] scratch, int n)
            {
                var retval = new T[n];
                CopyingTopDownSplitMerge(items, retval, scratch, 0, n);
                return retval;
            }

            static void CopyingTopDownSplitMerge(T[] src, T[] items, T[] scratch, int iBegin, int iEnd)
            {
                if (iEnd- iBegin < InsertionSortBatchSize) {
                    //CopyArray(src, iBegin, iEnd, items);
                    //InsertionSort_InPlace(items, iBegin, iEnd);
                    InsertionSort_Copy(src, iBegin, iEnd, items);
                    return;
                }
                int iMiddle = (iEnd + iBegin) / 2;
                CopyingTopDownSplitMerge(src, scratch, items, iBegin, iMiddle);
                CopyingTopDownSplitMerge(src, scratch, items, iMiddle, iEnd);
                Merge(scratch, iBegin, iMiddle, iEnd, items);
            }

            static void TopDownMergeSort(T[] items, T[] scratch, int n)
            {
                TopDownSplitMerge_toItems(items, 0, n, scratch);
            }

            static void TopDownSplitMerge_toItems(T[] items, int iBegin, int iEnd, T[] scratch)
            {
                if (iEnd - iBegin < InsertionSortBatchSize) {
                    InsertionSort_InPlace(items, iBegin, iEnd);
                    return;
                }
                int iMiddle = (iEnd + iBegin) / 2;
                TopDownSplitMerge_toScratch(items, iBegin, iMiddle, scratch);
                TopDownSplitMerge_toScratch(items, iMiddle, iEnd, scratch);
                Merge(scratch, iBegin, iMiddle, iEnd, items);
            }

            static void TopDownSplitMerge_toScratch(T[] items, int iBegin, int iEnd, T[] scratch)
            {
                if (iEnd - iBegin < InsertionSortBatchSize) {
                    InsertionSort_Copy(items, iBegin, iEnd, scratch);
                    return;
                }
                int iMiddle = (iEnd + iBegin) / 2;
                TopDownSplitMerge_toItems(items, iBegin, iMiddle, scratch);
                TopDownSplitMerge_toItems(items, iMiddle, iEnd, scratch);
                Merge(items, iBegin, iMiddle, iEnd, scratch);
            }

            static void Merge(T[] source, int iBegin, int iMiddle, int iEnd, T[] target)
            {
                int i = iBegin, j = iMiddle, k = iBegin;
                while (true) {
                    if (!Ordering.LessThan(source[j], source[i])) {
                        target[k++] = source[i++];
                        if (i == iMiddle) {
                            while (j < iEnd) {
                                target[k++] = source[j++];
                            }
                            return;
                        }
                    } else {
                        target[k++] = source[j++];
                        if (j == iEnd) {
                            while (i < iMiddle) {
                                target[k++] = source[i++];
                            }
                            return;
                        }
                    }
                }
            }

            static void BottomUpMergeSort(T[] target, T[] scratchSpace, int n)
            {
                const int insertionSortBatchSize = InsertionSortBatchSize;

                var batchesSortedUpto = 0;
                while (true) {
                    if (batchesSortedUpto + insertionSortBatchSize <= n) {
                        InsertionSort_InPlace(target, batchesSortedUpto, batchesSortedUpto + insertionSortBatchSize);
                        batchesSortedUpto += insertionSortBatchSize;
                    } else {
                        if (n - batchesSortedUpto >= 2) {
                            InsertionSort_InPlace(target, batchesSortedUpto, n);
                        }
                        break;
                    }
                }

                var A = target;
                var B = scratchSpace;

                for (var width = insertionSortBatchSize; width < n; width = width << 1) {
                    var i = 0;
                    while (i + width + width <= n) {
                        Merge(A, i, i + width, i + width + width, B);
                        i = i + width + width;
                    }
                    if (i + width < n) {
                        Merge(A, i, i + width, n, B);
                    } else {
                        CopyArray(A, i, n, B);
                    }
                    var tmp = A;
                    A = B;
                    B = tmp;
                }
                if (target != A) {
                    CopyArray(A, 0, n, target);
                }
            }

            public static void CopyArray(T[] source, int iBegin, int iEnd, T[] target)
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
