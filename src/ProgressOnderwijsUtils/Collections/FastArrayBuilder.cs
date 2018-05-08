using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Collections
{
    
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
                for (int j = 0; j < retval.Length; j++) {
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


    public struct ArrayBuilder_WithArraySegments<T>
    {
        const int InitSize2Pow = 4;
        const int InitSize = (1 << InitSize2Pow) - 1;
        int idx, sI;
        T[] current;
        T[][] segments;
        public static ArrayBuilder_WithArraySegments<T> Create() => new ArrayBuilder_WithArraySegments<T> { current = new T[InitSize] };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            if (idx < current.Length) {
                current[idx++] = item;
            } else {
                if (segments == null) {
                    segments = new T[31 - InitSize2Pow][];
                }
                segments[sI++] = current;
                current = new T[current.Length << 1 & ~current.Length];
                current[0] = item;
                idx = 1;
            }
        }

        public T[] ToArray()
        {
            if (segments == null) {
                var retval = current;
                Array.Resize(ref retval, idx);
                return retval;
            } else {
                var sumlength = (1 << sI + InitSize2Pow - 1) + idx - 1;
                var retval = new T[sumlength];
                var j = 0;
                for (var sJ = 0; sJ < sI; sJ++) {
                    var subarr = segments[sJ];
                    subarr.CopyTo(retval, j);
                    j += subarr.Length;
                }
                Array.Copy(current, 0, retval, j, idx);
                return retval;
            }
        }
    }

    public struct ArrayBuilder_Inline63ValuesAndSegments<T>
    {
        const int InitSize2Pow = 6;
        const int InitSize = (1 << InitSize2Pow) - 1;
        int idx, sI;
        T[] current;
#pragma warning disable 169
        //InitSize total:
        T v00, v01, v02, v03, v04, v05, v06, v07, v08, v09, v10, v11, v12, v13, v14, v15, v16, v17, v18, v19, v20, v21, v22, v23, v24, v25, v26, v27, v28, v29, v30, v31, v32, v33, v34, v35, v36, v37, v38, v39, v40, v41, v42, v43, v44, v45, v46, v47, v48, v49, v50, v51, v52, v53, v54, v55, v56, v57, v58, v59, v60, v61, v62;
        //31 - InitSize2Pow total:
        T[] a00, a01, a02, a03, a04, a05, a06, a07, a08, a09, a10, a11, a12, a13, a14, a15, a16, a17, a18, a19, a20, a21, a22, a23, a24;
#pragma warning restore 169
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            if (current == null) {
                if (idx < InitSize) {
                    Unsafe.Add(ref v00, idx++) = item;
                } else {
                    a00 = current = new T[InitSize << 1 & ~InitSize];
                    current[0] = item;
                    sI = idx = 1;
                }
            } else if (idx < current.Length) {
                current[idx++] = item;
            } else {
                current = new T[current.Length << 1 & ~current.Length];
                current[0] = item;
                idx = 1;
                Unsafe.Add(ref a00, sI++) = current;
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
                for (int j = 0; j < retval.Length; j++) {
                    retval[j] = Unsafe.Add(ref v00, j);
                }
                return retval;
            } else {
                var sumlength = (1 << sI + InitSize2Pow - 1) + idx - 1;
                var retval = new T[sumlength];
                var j = 0;
                for (; j < InitSize; j++) {
                    retval[j] = Unsafe.Add(ref v00, j);
                }
                for (var sJ = 0; sJ < sI - 1; sJ++) {
                    var subarr = Unsafe.Add(ref a00, sJ);
                    subarr.CopyTo(retval, j);
                    j += subarr.Length;
                }
                Array.Copy(current, 0, retval, j, idx);
                return retval;
            }
        }
    }

    public struct ArrayBuilder_Inline16ValuesAndSegments<T>
    {
        const int InitSize2Pow = 4;
        const int InitSize = 1 << InitSize2Pow;
        int idx, sI;
        T[] current;
#pragma warning disable 169
        //InitSize total:
        T v00, v01, v02, v03, v04, v05, v06, v07, v08, v09, v10, v11, v12, v13, v14, v15;
        //31 - InitSize2Pow total:
        T[] a00, a01, a02, a03, a04, a05, a06, a07, a08, a09, a10, a11, a12, a13, a14, a15, a16, a17, a18, a19, a20, a21, a22, a23, a24, a25, a26;
#pragma warning restore 169
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
                Unsafe.Add(ref a00, sI++) = current;
                current = new T[current.Length << 1];
                current[0] = item;
                idx = 1;
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
                for (int j = 0; j < retval.Length; j++) {
                    retval[j] = Unsafe.Add(ref v00, j);
                }
                return retval;
            } else {
                var sumlength = (1 << sI + InitSize2Pow) + idx;
                var retval = new T[sumlength];
                var j = 0;
                for (; j < InitSize; j++) {
                    retval[j] = Unsafe.Add(ref v00, j);
                }
                for (var sJ = 0; sJ < sI; sJ++) {
                    var subarr = Unsafe.Add(ref a00, sJ);
                    subarr.CopyTo(retval, j);
                    j += subarr.Length;
                }
                Array.Copy(current, 0, retval, j, idx);
                return retval;
            }
        }
    }
    public struct ArrayBuilder_Inline32ValuesAndSegments<T>
    {
        const int InitSize = 32;
        int idx, sI;
        T[] current;
#pragma warning disable 169
        //InitSize total:
        T v00, v01, v02, v03, v04, v05, v06, v07, v08, v09, v10, v11, v12, v13, v14, v15, v16, v17, v18, v19, v20, v21, v22, v23, v24, v25, v26, v27, v28, v29, v30, v31; //, v32, v33, v34, v35, v36, v37, v38, v39, v40, v41, v42, v43, v44, v45, v46, v47, v48, v49, v50, v51, v52, v53, v54, v55, v56, v57, v58, v59, v60, v61, v62, v63;
        T[] a00, a01, a02, a03, a04, a05, a06, a07, a08, a09, a10, a11, a12, a13, a14, a15, a16, a17, a18, a19, a20, a21, a22, a23, a24, a25, a26, a27, a28, a29, a30, a31, a32, a33, a34, a35, a36, a37, a38, a39, a40, a41, a42; //, a43, a44, a45, a46, a47, a48, a49, a50, a51, a52, a53, a54, a55, a56, a57, a58, a59, a60, a61, a62, a63, a64, a65, a66, a67, a68, a69, a70, a71, a72, a73, a74, a75, a76;
#pragma warning restore 169
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
                Unsafe.Add(ref a00, sI++) = current;
                current = new T[current.Length + (current.Length >> 1)];
                current[0] = item;
                idx = 1;
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
                for (int j = 0; j < retval.Length; j++) {
                    retval[j] = Unsafe.Add(ref v00, j);
                }
                return retval;
            } else {
                var sumlength = InitSize + idx;
                for (var sJ = 0; sJ < sI; sJ++) {
                    sumlength += Unsafe.Add(ref a00, sJ).Length;
                }
                var retval = new T[sumlength];
                var j = 0;
                for (; j < InitSize; j++) {
                    retval[j] = Unsafe.Add(ref v00, j);
                }
                for (var sJ = 0; sJ < sI; sJ++) {
                    var subarr = Unsafe.Add(ref a00, sJ);
                    subarr.CopyTo(retval, j);
                    j += subarr.Length;
                }
                Array.Copy(current, 0, retval, j, idx);
                return retval;
            }
        }
    }
}
