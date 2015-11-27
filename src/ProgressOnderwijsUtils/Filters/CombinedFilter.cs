using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils
{
    [Serializable]
    public sealed class CombinedFilter : FilterBase, IEquatable<CombinedFilter>
    {
        readonly BooleanOperator andor;
        readonly FilterBase[] filterLijst;
        public IEnumerable<FilterBase> FilterLijst => filterLijst;
        public BooleanOperator AndOr => andor;

        internal CombinedFilter(BooleanOperator andor, FilterBase[] condities)
        {
            Debug.Assert(condities != null && condities.Length > 0);
            this.andor = andor;
            filterLijst = condities;
        }

        protected internal override bool IsFilterValid(Func<string, Type> colTypeLookup) => FilterLijst.All(f => f.IsFilterValid(colTypeLookup));

        protected internal override Expression ToMetaObjectFilterExpr<T>(
            Expression objParExpr,
            Expression dateTimeNowTokenValue,
            Func<int, Func<int, bool>> getStaticGroupContainmentVerifier)
        {
            return
                filterLijst.Select(filter => filter.ToMetaObjectFilterExpr<T>(objParExpr, dateTimeNowTokenValue, getStaticGroupContainmentVerifier))
                    .Aggregate(andor == BooleanOperator.And ? Expression.AndAlso : (Func<Expression, Expression, Expression>)Expression.OrElse);
        }

        static readonly QueryBuilder sql_and_keyword = SQL($" and ");
        static readonly QueryBuilder sql_or_keyword = SQL($" or ");
        QueryBuilder OperatorToSql() => andor == BooleanOperator.Or ? sql_or_keyword : sql_and_keyword;

        protected internal override QueryBuilder ToQueryBuilderImpl()
        {
            var andorQ = OperatorToSql();
            return SQL($"(") + filterLijst.Aggregate(default(QueryBuilder), (q, f) => QueryBuilder.Empty == q ? f.ToQueryBuilder() : q + andorQ + f.ToQueryBuilder()) + SQL($")");
        }

        protected internal override FilterBase ReplaceImpl(FilterBase toReplace, FilterBase replaceWith)
        {
            return ReferenceEquals(this, toReplace) ? replaceWith : Filter.CreateCombined(AndOr, filterLijst.Select(child => child.ReplaceImpl(toReplace, replaceWith)));
        }

        protected internal override FilterBase AddToImpl(FilterBase filterInEditMode, BooleanOperator booleanOperator, FilterBase c)
        {
            return ReferenceEquals(filterInEditMode, this)
                ? Filter.CreateCombined(booleanOperator, this, c)
                : Filter.CreateCombined(AndOr, FilterLijst.Select(f => f.AddToImpl(filterInEditMode, booleanOperator, c)));
        }

        public override string SerializeToString()
        {
            return SerializeOp() + string.Join(",", filterLijst.Select(filter => filter.SerializeToString())) + ";";
        }

        public static Tuple<FilterBase, string> Parse(string serialized)
        {
            if (serialized.Length == 0) {
                return null;
            }
            BooleanOperator? op =
                serialized[0] == '&'
                    ? BooleanOperator.And
                    : serialized[0] == '|'
                        ? BooleanOperator.Or
                        : default(BooleanOperator?);
            if (!op.HasValue) {
                return null;
            }
            var filters = new List<FilterBase>();
            serialized = serialized.Substring(1);
            while (true) {
                var subExpression = Filter.TryParseSerializedFilterWithLeftovers(serialized);
                if (subExpression != null) {
                    filters.Add(subExpression.Item1);
                    if (subExpression.Item2.Length == 0 || subExpression.Item2[0] != ';' && subExpression.Item2[0] != ',') {
                        return null;
                    } else {
                        serialized = subExpression.Item2[0] == ';' ? subExpression.Item2 : subExpression.Item2.Substring(1);
                    }
                } else if (serialized.StartsWith(";")) {
                    serialized = serialized.Substring(1);
                    return Tuple.Create(Filter.CreateCombined(op.Value, filters), serialized);
                } else {
                    return null;
                }
            }
        }

        string SerializeOp()
        {
            switch (andor) {
                case BooleanOperator.And:
                    return "&";
                case BooleanOperator.Or:
                    return "|";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override int GetHashCode()
        {
            long val = andor.GetHashCode() + filterLijst.Select((f, i) => f.GetHashCode() * ((long)2 * i + 1)).Sum();
            return (int)((uint)val ^ ((uint)val >> 32));
        }

        public override bool Equals(FilterBase other) => Equals(other as CombinedFilter);
        public override bool Equals(object obj) => Equals(obj as CombinedFilter);
        public bool Equals(CombinedFilter obj) => obj != null && andor == obj.andor && FilterLijst.SequenceEqual(obj.FilterLijst);
    }
}
