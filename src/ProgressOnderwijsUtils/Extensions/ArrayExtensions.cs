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
        public static TValue GetOrDefault<TValue>(this TValue[] array, int index, TValue defaultValue)
        {
            return array != null && index < array.Length && index >= 0 ? array[index] : defaultValue;
        }

        static class Helper<T>
        {
            public static readonly T[] EmptyArray = new T[0];
        }

        [Pure]
        public static T[] Empty<T>() => Helper<T>.EmptyArray;

        /// <summary>
        /// Return an empty array if it's null
        /// </summary>
        [Pure]
        public static T[] EmptyIfNull<T>(this T[] array)
        {
            return array ?? Helper<T>.EmptyArray;
        }

        [Pure, UsefulToKeep("This is indeed a faster ToArray, which could be useful for optimizations")]
        public static T[] ToArrayFast<T>(this IReadOnlyList<T> list)
        {
            if (list is T[]) {
                return (T[])((T[])list).Clone();
            }
            var retval = new T[list.Count];
            for (int i = 0; i < retval.Length; i++) {
                retval[i] = list[i];
            }
            return retval;
        }

        /// <summary>
        /// Like Enumerable.Select, but faster due to specialization for arrays.
        /// </summary>
        [Pure]
        public static TR[] ArraySelect<T,TR>(this T[] array, Func<T, TR> mappingFunction)
        {
            var output = new TR[array.Length];
            for (int i = 0; i < array.Length; ++i)
                output[i] = mappingFunction(array[i]);
            return output;
        }
    }
}
