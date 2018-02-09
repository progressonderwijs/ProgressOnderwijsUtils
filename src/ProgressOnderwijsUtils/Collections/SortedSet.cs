﻿using System;
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

    public struct SortedSet<T, TOrder>
        where TOrder : struct, IOrdering<T>
    {
        readonly T[] sortedDistinctValues;
        static readonly T[] empty = Array.Empty<T>();
        static TOrder Ordering => default(TOrder);
        SortedSet(T[] sortedDistinctValues) => this.sortedDistinctValues = sortedDistinctValues;

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
            var tmpArray = GetCachedAccumulator(originalLength);
            for (var i = 0; i < originalLength; i++) {
                tmpArray[i] = values[i];
            }

            Algorithms.QuickSort(tmpArray, 0, originalLength - 1);
            var newLength = RemoveDuplicates(tmpArray, originalLength);
            var output = new T[newLength];
            for (var i = 0; i < output.Length; i++) {
                output[i] = tmpArray[i];
            }
            return new SortedSet<T, TOrder>(output);
        }

        public T[] Values => sortedDistinctValues ?? empty;

        [Pure]
        public bool Contains(T value)
        {
            var idxAfterLastLtNode = Algorithms.IdxAfterLastLtNode(sortedDistinctValues, value);
            return idxAfterLastLtNode < sortedDistinctValues.Length && Ordering.Equal(sortedDistinctValues[idxAfterLastLtNode], value);
        }

        [ThreadStatic]
        static T[] Accumulator;

        static int RemoveDuplicates(T[] arr, int len)
        {
            if (len < 2) {
                return len;
            }
            int j = 0;

            for (int i = 1; i < len; i++) {
                if (!Ordering.Equal(arr[i - 1], arr[i])) {
                    arr[j++] = arr[i - 1];
                }
            }

            arr[j++] = arr[len - 1];

            return j;
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

                inputSetCursors[inputSetCount++] = new Cursor(0, set.Values);
                maxSize += set.Values.Length;
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

        static T[] GetCachedAccumulator(int maxSize)
        {
            var outputValues = Accumulator ?? (Accumulator = new T[16]);
            if (outputValues.Length < maxSize) {
                var newSize = (int)Math.Max(maxSize, outputValues.Length * 3L / 2L);
                Accumulator = outputValues = new T[newSize];
            }
            return outputValues;
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
            public static void QuickSort(T[] A) => QuickSort(A, 0, A.Length - 1);

            public static void QuickSort(T[] A, int lo, int hi)
            {
                if (lo < hi) {
                    var pivot = Partition(A, lo, hi);
                    QuickSort(A, lo, pivot);
                    QuickSort(A, pivot + 1, hi);
                }
            }

            static int Partition(T[] A, int lo, int hi)
            {
                var pivotValue = A[(lo + hi) / 2];
                var i = lo;
                var j = hi;
                while (true) {
                    while (Ordering.LessThan(A[i], pivotValue)) {
                        i++;
                    }
                    while (Ordering.LessThan(pivotValue, A[j])) {
                        j--;
                    }
                    //so pivot <= A[i]  &  A[j] <= pivot
                    //and A[lo,i) < pivot  & pivot < A(j,hi]
                    if (j <= i) {
                        return j;
                    }
                    var tmp = A[i];
                    A[i] = A[j];
                    A[j] = tmp;
                    i++;
                    j--;
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
        }
    }

    public static class SortedSetHelpers
    {
        public static SortedSet<T, TOrder> MergeSets<T, TOrder>(this IEnumerable<SortedSet<T, TOrder>> inputSets)
            where TOrder : struct, IOrdering<T>
            => SortedSet<T, TOrder>.MergeSets(inputSets);
    }
}
