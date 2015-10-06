using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public interface IFilterFactory<out T> { }

    public struct FilterFactory<TObj>
    {
        [Pure]
        public FilterCreator<T> FilterOn<T>(Expression<Func<TObj, T>> columnToFilter) => new FilterCreator<T>(columnToFilter);

        public struct FilterCreator<T>
        {
            readonly Expression<Func<TObj, T>> ColumnToFilter;

            public FilterCreator(Expression<Func<TObj, T>> columnToFilter)
            {
                this.ColumnToFilter = columnToFilter;
            }

            // ReSharper disable UnusedMember.Global
            // Library methods
            public FilterBase In(IEnumerable<T> waarde) => CreateCriterium(BooleanComparer.In, waarde);
            public FilterBase NotIn(IEnumerable<T> waarde) => CreateCriterium(BooleanComparer.NotIn, waarde);
            public FilterBase IsNull() => CreateCriterium(BooleanComparer.IsNull, null);
            public FilterBase IsNotNull() => CreateCriterium(BooleanComparer.IsNotNull, null);
            public FilterBase LessThan(T waarde) => CreateCriterium(BooleanComparer.LessThan, waarde);
            public FilterBase LessThanOrEqual(T waarde) => CreateCriterium(BooleanComparer.LessThanOrEqual, waarde);
            public FilterBase Equal(T waarde) => CreateCriterium(BooleanComparer.Equal, waarde);
            public FilterBase GreaterThanOrEqual(T waarde) => CreateCriterium(BooleanComparer.GreaterThanOrEqual, waarde);
            public FilterBase GreaterThan(T waarde) => CreateCriterium(BooleanComparer.GreaterThan, waarde);
            public FilterBase NotEqual(T waarde) => CreateCriterium(BooleanComparer.NotEqual, waarde);
            public FilterBase HasFlag(T waarde) => CreateCriterium(BooleanComparer.HasFlag, waarde);
            // ReSharper restore UnusedMember.Global
            FilterBase CreateCriterium(BooleanComparer booleanComparer, object waarde) => Filter.CreateCriterium(ColumnToFilter, booleanComparer, waarde);

            //this is static so it doesn't pollute intellisense since it's only used for extension methods.
            public static Expression<Func<TObj, T>> ExtractColumn(FilterCreator<T> filterCreator) => filterCreator.ColumnToFilter;
        }
    }

    public static class FilterFactoryExtensions
    {
        static Expression<Func<TRow, T>> ExtractColumn<TRow, T>(this FilterFactory<TRow>.FilterCreator<T> x) => FilterFactory<TRow>.FilterCreator<T>.ExtractColumn(x);
        static FilterBase CreateCriterium<TRow, T>(this FilterFactory<TRow>.FilterCreator<T> x, BooleanComparer booleanComparer, object waarde) => Filter.CreateCriterium(x.ExtractColumn(), booleanComparer, waarde);

        // ReSharper disable once UnusedParameter.Global
        //parameter *is* used (just not at runtime) - for type inference.
        public static FilterFactory<TRow>.FilterCreator<T> FilterOn<TRow, T>(this IFilterFactory<TRow> factory, Expression<Func<TRow, T>> columnToFilter) =>
            new FilterFactory<TRow>().FilterOn(columnToFilter);

        // ReSharper disable UnusedMember.Global
        // Library methods
        public static FilterBase StartsWith<TRow>(this FilterFactory<TRow>.FilterCreator<string> x, string waarde)
            => x.CreateCriterium(BooleanComparer.StartsWith, waarde);

        public static FilterBase EndsWith<TRow>(this FilterFactory<TRow>.FilterCreator<string> x, string waarde)
            => x.CreateCriterium(BooleanComparer.EndsWith, waarde);

        public static FilterBase Equal<TRow, T>(this FilterFactory<TRow>.FilterCreator<T> x, T? waarde) where T : struct
            => x.CreateCriterium(BooleanComparer.Equal, waarde);

        public static FilterBase LessThan_CurrentTime<TRow>(this FilterFactory<TRow>.FilterCreator<DateTime> x)
            => x.CreateCriterium(BooleanComparer.LessThan, Filter.CurrentTimeToken.Instance);
        public static FilterBase LessThan_CurrentTime<TRow>(this FilterFactory<TRow>.FilterCreator<DateTime?> x)
            => x.CreateCriterium(BooleanComparer.LessThan, Filter.CurrentTimeToken.Instance);

        public static FilterBase GreaterThan_CurrentTime<TRow>(this FilterFactory<TRow>.FilterCreator<DateTime> x)
            => x.CreateCriterium(BooleanComparer.GreaterThan, Filter.CurrentTimeToken.Instance);
        public static FilterBase GreaterThan_CurrentTime<TRow>(this FilterFactory<TRow>.FilterCreator<DateTime?> x)
            => x.CreateCriterium(BooleanComparer.GreaterThan, Filter.CurrentTimeToken.Instance);

        public static FilterBase Contains<TRow>(this FilterFactory<TRow>.FilterCreator<string> x, string waarde)
            => x.CreateCriterium(BooleanComparer.Contains, waarde);

        static readonly char[] starTrimFilter = { '*' };

        /// <summary>
        /// Allows prefixing and suffixing a search term with '*' which are interpreted as globs.
        /// i.e., a prefixed or suffixed star is equivalent to the regex .*
        /// </summary>
        public static FilterBase WildcardSearch<TRow>(this FilterFactory<TRow>.FilterCreator<string> x, string waarde)
        {
            var realNeedle = waarde.Trim(starTrimFilter);
            bool startsWithWildcard = waarde.StartsWith("*");
            bool endWithWildcard = waarde.EndsWith("*");

            return startsWithWildcard && endWithWildcard
                ? x.Contains(realNeedle)
                : startsWithWildcard
                    ? x.EndsWith(realNeedle)
                    : endWithWildcard
                        ? x.StartsWith(realNeedle)
                        : x.Equal(realNeedle);
        }

        public static FilterBase In<TRow, T>(this FilterFactory<TRow>.FilterCreator<T?> x, IEnumerable<T> waarde) where T : struct
            => x.CreateCriterium(BooleanComparer.In, waarde);

        public static FilterBase NotIn<TRow, T>(this FilterFactory<TRow>.FilterCreator<T?> x, IEnumerable<T> waarde) where T : struct
            => x.CreateCriterium(BooleanComparer.NotIn, waarde);

        // ReSharper restore UnusedMember.Global
    }
}
