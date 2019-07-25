#nullable disable
using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Collections
{
    /// <summary>
    /// A fast way to build arrays.  Store the first 16 values inline (no heap allocation); then grows the scratch-space by less than 10% each time so that not too much memory is wasted during building.
    /// </summary>
    public struct ArrayBuilder<T>
    {
        const int InitSize2Pow = 4;
        const int InitSize = 1 << InitSize2Pow;
        const int InlineArrays = 12;
        const int SubArraysNeeded = 217;
        int idx, sI;
        T[] current;
#pragma warning disable 169
        //InitSize total:
        T v00, v01, v02, v03, v04, v05, v06, v07, v08, v09, v10, v11, v12, v13, v14, v15;

        //31 - InitSize2Pow total:
        T[] a00, a01, a02, a03, a04, a05, a06, a07, a08, a09, a10, a11;
#pragma warning restore 169
        T[][] LongTail;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            if (current == null) {
                if (idx < InitSize) {
                    Unsafe.Add(ref v00, idx++) = item;
                } else {
                    current = new T[InitSize];
                    current[0] = item;
                    idx = 1;
                }
            } else if (idx < current.Length) {
                current[idx++] = item;
            } else {
                if (sI < InlineArrays) {
                    Unsafe.Add(ref a00, sI++) = current;
                } else {
                    if (sI == InlineArrays) {
                        LongTail = new T[SubArraysNeeded - InlineArrays][];
                    }
                    LongTail[sI++ - InlineArrays] = current;
                }
                var nextArr = new T[current.Length + 16 + (current.Length >> 4)];
                nextArr[0] = item;
                idx = 1;
                current = nextArr;
            }
        }

        [NotNull]
        public T[] ToArray()
        {
            if (current == null) {
                if (idx == 0) {
                    return Array.Empty<T>();
                }
                var retval = new T[idx];
                for (var j = 0; j < retval.Length; j++) {
                    retval[j] = Unsafe.Add(ref v00, j);
                }
                return retval;
            } else {
                var sumlength = InitSize;
                var inlineArrCount = sI > InlineArrays ? InlineArrays : sI;
                for (var sJ = 0; sJ < inlineArrCount; sJ++) {
                    sumlength += Unsafe.Add(ref a00, sJ).Length;
                }

                for (var sJ = InlineArrays; sJ < sI; sJ++) {
                    sumlength += LongTail[sJ - InlineArrays].Length;
                }
                sumlength += idx;
                var retval = new T[sumlength];
                var j = 0;
                for (; j < InitSize; j++) {
                    retval[j] = Unsafe.Add(ref v00, j);
                }
                for (var sJ = 0; sJ < inlineArrCount; sJ++) {
                    var subarr = Unsafe.Add(ref a00, sJ);
                    subarr.CopyTo(retval, j);
                    j += subarr.Length;
                }
                for (var sJ = InlineArrays; sJ < sI; sJ++) {
                    var subarr = LongTail[sJ - InlineArrays];
                    subarr.CopyTo(retval, j);
                    j += subarr.Length;
                }

                Array.Copy(current, 0, retval, j, idx);
                return retval;
            }
        }
    }
}
