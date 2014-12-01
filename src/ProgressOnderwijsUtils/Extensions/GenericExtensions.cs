using System;
using System.Linq;
using System.Collections.Generic;

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
        public static bool In<T>(this T obj, params T[] values)
            where T : struct, IConvertible, IComparable { return values.Contains(obj); }

        public static bool In<T>(this T obj, T value)
            where T : struct, IConvertible, IComparable { return value.Equals(obj); }

        public static bool In<T>(this T? obj, T? value)
            where T : struct, IConvertible, IComparable { return value.Equals(obj); }

        public static bool In<T>(this T obj, T a, T b)
            where T : struct, IConvertible, IComparable { return a.Equals(obj) || b.Equals(obj); }

        public static bool In<T>(this T? obj, T? a, T? b)
            where T : struct, IConvertible, IComparable { return a.Equals(obj) || b.Equals(obj); }

        public static bool In<T>(this T obj, T a, T b, T c)
            where T : struct, IConvertible, IComparable { return a.Equals(obj) || b.Equals(obj) || c.Equals(obj); }

        public static bool In<T>(this T? obj, T? a, T? b, T? c)
            where T : struct, IConvertible, IComparable { return a.Equals(obj) || b.Equals(obj) || c.Equals(obj); }

        public static bool In(this string obj, params string[] values) { return values.Contains(obj); }

        public static bool In<T>(this T? obj, params T?[] values)
            where T : struct, IConvertible, IComparable { return values.Contains(obj); }
    }
}
