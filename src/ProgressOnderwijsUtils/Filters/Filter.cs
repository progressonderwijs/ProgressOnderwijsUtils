using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ProgressOnderwijsUtils.Collections;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils
{
    public interface IFilterFactory<out T> { }

    public static class Filter<TMetaObject>
    {
        public static FilterBase CreateFilter<T>(Expression<Func<TMetaObject, T>> columnToFilter, BooleanComparer comparer, T waarde)
        {
            var pi = MetaObject.GetMemberInfo(columnToFilter);
            return Filter.CreateCriterium(pi.Name, comparer, waarde);
        }
    }

    public static class Filter
    {
        public sealed class CurrentTimeToken
        {
            CurrentTimeToken() { }
            public static readonly CurrentTimeToken Instance = new CurrentTimeToken();

            public static CurrentTimeToken Parse(string s)
            {
                if (s != "") {
                    throw new ArgumentException("Can only parse empty string as current time token!");
                }
                return Instance;
            }
        }

        public static QueryBuilder ToQueryBuilder(this FilterBase filter) => filter == null ? SqlQuery($"1=1") : filter.ToQueryBuilderImpl();

        public static Func<T, bool> ToMetaObjectFilter<T>(this FilterBase filter, Func<int, Func<int, bool>> getStaticGroupContainmentVerifier) //where T : IMetaObject
        {
            if (filter == null) {
                return _ => true;
            }
            return ToMetaObjectFilterExpr<T>(filter, getStaticGroupContainmentVerifier).Compile();
        }

        public static Expression<Func<T, bool>> ToMetaObjectFilterExpr<T>(FilterBase filter, Func<int, Func<int, bool>> getStaticGroupContainmentVerifier)
        {
            var metaObjParam = Expression.Parameter(typeof(T));
            var nowTokenValue = Expression.Constant(DateTime.Now, typeof(DateTime));
            //rather than re-determine "now" several times, we pre-determine it to ensure one consistent moment in all filter evaluations.

            var filterBodyExpr = filter == null ? Expression.Constant(true) : filter.ToMetaObjectFilterExpr<T>(metaObjParam, nowTokenValue, getStaticGroupContainmentVerifier);
            return Expression.Lambda<Func<T, bool>>(filterBodyExpr, metaObjParam);
        }

        public static FilterBase Replace(this FilterBase filter, FilterBase toReplace, FilterBase replaceWith)
        {
            return filter == null
                ? (toReplace == null ? replaceWith : null)
                : filter.ReplaceImpl(toReplace, replaceWith);
        }

        public static FilterBase AddTo(this FilterBase filter, FilterBase filterInEditMode, BooleanOperator booleanOperator, FilterBase c)
        {
            return filter == null
                ? (filterInEditMode == null ? c : null)
                : filter.AddToImpl(filterInEditMode, booleanOperator, c);
        }

        public static FilterBase Remove(this FilterBase filter, FilterBase filterToRemove) => filter.ReplaceImpl(filterToRemove, null);

        /// <summary>
        /// Maakt een filter definitie aan.  Om twee kolommen onderling te vergelijken, moet de waarde van type ColumnReference zijn.
        /// </summary>
        public static FilterBase CreateCriterium(string kolomnaam, BooleanComparer comparer, object waarde)
        {
            return new CriteriumFilter(kolomnaam, comparer, waarde);
        }

        public static FilterBase CreateCriterium<TMetaObject, T>(Expression<Func<TMetaObject, T>> columnToFilter, BooleanComparer comparer, object waarde)
        {
            var kolomnaam = MetaObject.GetMemberInfo(columnToFilter).Name;
            return new CriteriumFilter(kolomnaam, comparer, waarde);
        }

        // ReSharper disable once UnusedParameter.Global
        public static FilterBase CreateFilter<TMetaObject, T>(
            this IFilterFactory<TMetaObject> target,
            Expression<Func<TMetaObject, T>> columnToFilter,
            BooleanComparer comparer,
            T waarde)
        {
            return Filter<TMetaObject>.CreateFilter(columnToFilter, comparer, waarde);
        }

        public static FilterBase CreateCombined(BooleanOperator andor, FilterBase a, FilterBase b, params FilterBase[] extra)
        {
            return CreateCombined(andor, new[] { a, b }.Concat(extra));
        }

        public static FilterBase CreateCombined(BooleanOperator andor, IEnumerable<FilterBase> condities)
        {
            var list = FastArrayBuilder<FilterBase>.Create();
            foreach (var f in condities) {
                if (f != null) {
                    list.Add(f);
                }
            }
            var conditiesArr = list.ToArray();

            if (conditiesArr.Length == 0) {
                return null;
            } else if (conditiesArr.Length == 1) {
                return conditiesArr[0];
            } else {
                var list1 = FastArrayBuilder<FilterBase>.Create();
                foreach (var conditie in conditiesArr) {
                    if (conditie is CombinedFilter && ((CombinedFilter)conditie).AndOr == andor) {
                        foreach (var f in ((CombinedFilter)conditie).FilterLijst) {
                            list1.Add(f);
                        }
                    } else {
                        list1.Add(conditie);
                    }
                }

                return new CombinedFilter(andor, list1.ToArray());
            }
        }

        public static IEnumerable<Tuple<string, object>> ExtractInsertWaarden(this FilterBase filter)
        {
            var crit = filter as CriteriumFilter;
            var combined = filter as CombinedFilter;
            if (crit != null && crit.Comparer == BooleanComparer.Equal) {
                yield return Tuple.Create(crit.KolomNaam, crit.Waarde);
            } else if (combined != null && combined.AndOr == BooleanOperator.And) {
                foreach (var f1 in combined.FilterLijst) {
                    if (!(f1 is CriteriumFilter) || ((CriteriumFilter)f1).Comparer != BooleanComparer.Equal) {
                        yield break;
                    }
                }

                foreach (CriteriumFilter f1 in combined.FilterLijst) {
                    yield return Tuple.Create(f1.KolomNaam, f1.Waarde);
                }
            }
        }

        public static bool CanReferenceColumn(this BooleanComparer comparer)
        {
            return comparer.In(
                BooleanComparer.Equal,
                BooleanComparer.GreaterThan,
                BooleanComparer.GreaterThanOrEqual,
                BooleanComparer.LessThan,
                BooleanComparer.LessThanOrEqual,
                BooleanComparer.NotEqual);
        }

        public static string NiceString(this BooleanComparer comparer)
        {
            switch (comparer) {
                case BooleanComparer.LessThan:
                    return "<";
                case BooleanComparer.LessThanOrEqual:
                    return "<=";
                case BooleanComparer.Equal:
                    return "=";
                case BooleanComparer.GreaterThanOrEqual:
                    return ">=";
                case BooleanComparer.GreaterThan:
                    return ">";
                case BooleanComparer.NotEqual:
                    return "!=";
                case BooleanComparer.In:
                    return "in";
                case BooleanComparer.NotIn:
                    return "!in";
                case BooleanComparer.StartsWith:
                    return "starts with";
                case BooleanComparer.EndsWith:
                    return "ends with";
                case BooleanComparer.Contains:
                    return "contains";
                case BooleanComparer.IsNull:
                    return "is null";
                case BooleanComparer.IsNotNull:
                    return "is not null";
                case BooleanComparer.HasFlag:
                    return "has flag";
                default:
                    throw new InvalidOperationException("Geen geldige operator");
            }
        }

        static class ComparerLookup
        {
            public static readonly Dictionary<string, BooleanComparer> ComparerByString = EnumHelpers.GetValues<BooleanComparer>()
                .ToDictionary(NiceString, StringComparer.Ordinal);
        }

        public static BooleanComparer? ParseComparerNiceString(string s) => ComparerLookup.ComparerByString.GetOrDefaultR(s, default(BooleanComparer?));

        public static FilterBase ClearFilterWhenItContainsInvalidColumns(this FilterBase filter, Func<string, Type> typeIfPresent)
        {
            return filter != null && filter.IsFilterValid(typeIfPresent) ? filter : null;
        }

        public static FilterBase ClearFilterWhenItContainsInvalidColumns<T>(this FilterBase filter)
            where T : IMetaObject
        {
            var byName = MetaObject.GetMetaProperties<T>().ToDictionary(mp => mp.Name, mp => mp.DataType, StringComparer.OrdinalIgnoreCase);
            return ClearFilterWhenItContainsInvalidColumns(filter, str => byName.GetOrDefault(str));
        }

        public static Tuple<FilterBase, string> TryParseSerializedFilterWithLeftovers(string serialized)
        {
            return CombinedFilter.Parse(serialized) ?? CriteriumFilter.Parse(serialized);
        }

        public static FilterBase TryParseSerializedFilter(string serialized)
        {
            var parsed = TryParseSerializedFilterWithLeftovers(serialized);
            return parsed != null && parsed.Item2 == "" ? parsed.Item1 : null;
        }

        public static QueryBuilder ToFilterClause(this IEnumerable<FilterBase> filters) => CreateCombined(BooleanOperator.And, filters.EmptyIfNull()).ToQueryBuilder();
    }
}
