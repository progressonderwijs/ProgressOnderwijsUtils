using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class PnetNullableExtensions
    {
        // ReSharper disable AvoidFirstOrDefault
        // ReSharper disable AvoidSingleOrDefault
        public static TVal? FirstOrNull<TVal>(this IEnumerable<TVal?> values)
            where TVal : struct
            => values.FirstOrDefault();

        public static TVal? FirstOrNull<TVal>(this IEnumerable<TVal?> values, Func<TVal?, bool> test)
            where TVal : struct
            => values.FirstOrDefault(test);

        public static TVal FirstOrNull<TVal>(this IEnumerable<TVal> values)
            where TVal : class?
            => values.FirstOrDefault();

        public static TVal FirstOrNull<TVal>(this IEnumerable<TVal> values, Func<TVal, bool> test)
            where TVal : class?
            => values.FirstOrDefault(test);

        public static TVal? FirstOrNullable<TVal>(this IEnumerable<TVal> values)
            where TVal : struct
            => values.Select(o => (TVal?)o).FirstOrNull();

        public static TVal? FirstOrNullable<TVal>(this IEnumerable<TVal> values, Func<TVal, bool> test)
            where TVal : struct
            => values.Where(test).FirstOrNullable();

        public static TVal? SingleOrNullable<TVal>(this IEnumerable<TVal> values)
            where TVal : struct
            => values.Select(o => (TVal?)o).SingleOrNull();

        [UsefulToKeep("library function")]
        public static TVal? SingleOrNullable<TVal>(this IEnumerable<TVal> values, Func<TVal, bool> test)
            where TVal : struct
            => values.Where(test).SingleOrNullable();

        public static TVal? SingleOrNull<TVal>(this IEnumerable<TVal?> values)
            where TVal : struct
            => values.SingleOrDefault();

        [CanBeNull]
        public static TVal SingleOrNull<TVal>(this IEnumerable<TVal> values)
            where TVal : class?
            => values.SingleOrDefault();

        [UsefulToKeep("library function")]
        public static TVal? SingleOrNull<TVal>(this IEnumerable<TVal?> values, Func<TVal?, bool> test)
            where TVal : struct
            => values.SingleOrDefault(test);

        public static TVal SingleOrNull<TVal>(this IEnumerable<TVal> values, Func<TVal, bool> test)
            where TVal : class?
            => values.SingleOrDefault(test);

        public static TVal? AsNullable<TVal>(this TVal value)
            where TVal : struct
            => value;

        public static IEnumerable<TVal?> NullIfEmpty<TVal>(this IEnumerable<TVal?> values)
            where TVal : struct
            => values.DefaultIfEmpty();

        public static IEnumerable<TVal?> NullableIfEmpty<TVal>(this IEnumerable<TVal> values)
            where TVal : struct
            => values.Select(o => (TVal?)o).NullIfEmpty();

        public static IEnumerable<TVal> NullIfEmpty<TVal>(this IEnumerable<TVal> values)
            where TVal : class?
            => values.DefaultIfEmpty();

        public static TVal? FirstOrNull<TVal>(this IQueryable<TVal?> values)
            where TVal : struct
            => values.FirstOrDefault();

        [UsefulToKeep("library function")]
        public static TVal? FirstOrNull<TVal>(this IQueryable<TVal?> values, Expression<Func<TVal?, bool>> test)
            where TVal : struct
            => values.FirstOrDefault(test);

        [UsefulToKeep("library function")]
        public static TVal FirstOrNull<TVal>(this IQueryable<TVal> values)
            where TVal : class?
            => values.FirstOrDefault();

        [UsefulToKeep("library function")]
        public static TVal FirstOrNull<TVal>(this IQueryable<TVal> values, Expression<Func<TVal, bool>> test)
            where TVal : class?
            => values.FirstOrDefault(test);

        public static TVal? FirstOrNullable<TVal>(this IQueryable<TVal> values)
            where TVal : struct
            => values.Select(o => (TVal?)o).FirstOrNull();

        [UsefulToKeep("library function")]
        public static TVal? FirstOrNullable<TVal>(this IQueryable<TVal> values, Expression<Func<TVal, bool>> test)
            where TVal : struct
            => values.Where(test).FirstOrNullable();

        public static TVal? SingleOrNullable<TVal>(this IQueryable<TVal> values)
            where TVal : struct
            => values.Select(o => (TVal?)o).SingleOrNull();

        [UsefulToKeep("library function")]
        public static TVal? SingleOrNullable<TVal>(this IQueryable<TVal> values, Expression<Func<TVal, bool>> test)
            where TVal : struct
            => values.Where(test).SingleOrNullable();

        public static TVal? SingleOrNull<TVal>(this IQueryable<TVal?> values)
            where TVal : struct
            => values.SingleOrDefault();

        [UsefulToKeep("library function")]
        public static TVal SingleOrNull<TVal>(this IQueryable<TVal> values)
            where TVal : class?
            => values.SingleOrDefault();

        [UsefulToKeep("library function")]
        public static TVal? SingleOrNull<TVal>(this IQueryable<TVal?> values, Expression<Func<TVal?, bool>> test)
            where TVal : struct
            => values.SingleOrDefault(test);

        [UsefulToKeep("library function")]
        public static TVal SingleOrNull<TVal>(this IQueryable<TVal> values, Expression<Func<TVal, bool>> test)
            where TVal : class?
            => values.SingleOrDefault(test);

        public static IQueryable<TVal?> NullIfEmpty<TVal>(this IQueryable<TVal?> values)
            where TVal : struct
            => values.DefaultIfEmpty();

        [UsefulToKeep("library function")]
        public static IQueryable<TVal?> NullableIfEmpty<TVal>(this IQueryable<TVal> values)
            where TVal : struct
            => values.Select(o => (TVal?)o).NullIfEmpty();

        [UsefulToKeep("library function")]
        public static IQueryable<TVal> NullIfEmpty<TVal>(this IQueryable<TVal> values)
            where TVal : class?
            => values.DefaultIfEmpty();
    }
}
