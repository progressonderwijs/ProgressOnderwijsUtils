#nullable disable
using System;
using System.Linq;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class GenericExtensions
    {
        /// <summary>
        /// Pseudo 'in' operation (someObj.In([values])
        /// </summary>
        /// <returns>true/false</returns>
        /// <remarks>
        /// **voorbeelden
        /// 1.In(1,2,3,4); // true
        /// "cando".In("nocando", "cando") //true
        /// Enum Vandaag = weekdays.monday;
        /// Vandaag.In(weekdays.thursday,weekdays.friday); //false
        /// </remarks>
        [Pure]
        public static bool In<T>(this T obj, [NotNull] params T[] values)
            where T : struct, IConvertible, IComparable
            => values.Contains(obj);

        [Pure]
        public static bool In<T>(this T obj, T value)
            where T : struct, IConvertible, IComparable
            => value.Equals(obj);

        [Pure]
        public static bool In<T>(this T? obj, T? value)
            where T : struct, IConvertible, IComparable
            => value.Equals(obj);

        [Pure]
        public static bool In<T>(this T obj, T a, T b)
            where T : struct, IConvertible, IComparable
            => a.Equals(obj) || b.Equals(obj);

        [Pure]
        public static bool In<T>(this T? obj, T? a, T? b)
            where T : struct, IConvertible, IComparable
            => a.Equals(obj) || b.Equals(obj);

        [Pure]
        public static bool In<T>(this T obj, T a, T b, T c)
            where T : struct, IConvertible, IComparable
            => a.Equals(obj) || b.Equals(obj) || c.Equals(obj);

        [Pure]
        public static bool In<T>(this T? obj, T? a, T? b, T? c)
            where T : struct, IConvertible, IComparable
            => a.Equals(obj) || b.Equals(obj) || c.Equals(obj);

        [Pure]
        public static bool In(this string obj, [NotNull] params string[] values)
            => values.Contains(obj);

        [UsefulToKeep("overload")]
        [Pure]
        public static bool In<T>(this T? obj, [NotNull] params T?[] values)
            where T : struct, IConvertible, IComparable
            => values.Contains(obj);
    }
}
