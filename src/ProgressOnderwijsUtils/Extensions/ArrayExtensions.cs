using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Utility method to retrieve a value with a default from an array
        /// </summary>
        /// <param name="array">The array to extract  from</param>
        /// <param name="index">The index whose to get the array element of.</param>
        /// <param name="defaultValue">The value to get if the index is out of range.</param>
        /// <returns>The value at the index, or the default if the array does encompass that index.</returns>
        [Pure]
        public static TValue GetOrDefault<TValue>(this TValue[]? array, int index, TValue defaultValue)
            => array != null && index < array.Length && index >= 0 ? array[index] : defaultValue;

        /// <summary>
        /// Return an empty array if it's null
        /// </summary>
        [Pure]
        public static T[] EmptyIfNull<T>(this T[]? array)
            => array ?? Array.Empty<T>();

        /// <summary>
        /// Like Enumerable.Select, but faster due to specialization for arrays.
        /// </summary>
        [Pure]
        public static TR[] ArraySelect<T, TR>(this T[] array, Func<T, TR> mappingFunction)
        {
            var output = new TR[array.Length];
            for (var i = 0; i < array.Length; ++i) {
                output[i] = mappingFunction(array[i]);
            }
            return output;
        }

        /// <summary>
        /// Like Enumerable.Select, but faster due to specialization for arrays.
        /// </summary>
        [Pure]
        public static TR[] ArraySelect<T, TR>(this IReadOnlyList<T> array, Func<T, TR> mappingFunction)
        {
            var output = new TR[array.Count];
            for (var i = 0; i < output.Length; ++i) {
                output[i] = mappingFunction(array[i]);
            }
            return output;
        }

        /// <summary>
        /// Like Enumerable.Select, but faster due to specialization for arrays.
        /// </summary>
        [Pure]
        public static TR[] ArraySelect<T, TR>(this T[] array, Func<T, int, TR> mappingFunction)
        {
            var output = new TR[array.Length];
            for (var i = 0; i < array.Length; ++i) {
                output[i] = mappingFunction(array[i], i);
            }
            return output;
        }

        /// <summary>
        /// Like Enumerable.Select, but faster due to specialization for arrays.
        /// </summary>
        [Pure]
        public static TR[] ArraySelect<T, TR>(this IReadOnlyList<T> array, Func<T, int, TR> mappingFunction)
        {
            var output = new TR[array.Count];
            for (var i = 0; i < output.Length; ++i) {
                output[i] = mappingFunction(array[i], i);
            }
            return output;
        }

        /// <summary>
        /// Concatenates two arrays.  Null arrays are interpreted as the empty array. The returned array may be the same as one of the parameters.
        /// </summary>
        public static T[] ConcatArray<T>(this T[]? beginning, T[]? end)
        {
            if (end == null || end.Length == 0) {
                return beginning ?? Array.Empty<T>();
            } else if (beginning == null || beginning.Length == 0) {
                return end;
            }
            var newChildNodes = new T[beginning.Length + end.Length];
            Array.Copy(beginning, 0, newChildNodes, 0, beginning.Length);
            Array.Copy(end, 0, newChildNodes, beginning.Length, end.Length);
            return newChildNodes;
        }

        public static TO[] SelectMany<TI,TO>(this TI[] list, Func<TI, TO[]> map)
            => list.ArraySelect(map).ConcatArrays();

        public static T[] ConcatArrays<T>(this T[][] arrays)
        {
            var len = 0;
            foreach (var kid in arrays) {
                len += kid.Length;
            }
            var arr = Array.Empty<T>();
            if (len != 0) {
                arr = new T[len];
                var writeCursor = arr.AsSpan();
                foreach (var kid in arrays) {
                    var replacements = kid.AsSpan();
                    replacements.CopyTo(writeCursor);
                    writeCursor = writeCursor.Slice(replacements.Length);
                }
            }
            return arr;
        }
    }
}
