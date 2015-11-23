using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public struct QueryBuilder
    {
        readonly IBuildableQuery impl;
        internal QueryBuilder(IBuildableQuery impl)
        {
            this.impl = impl;
        }

        internal void AppendTo(ref CommandFactory factory) => impl?.AppendTo(ref factory);

        public SqlCommand CreateSqlCommand(SqlCommandCreationContext conn)
        {
            var factory = new CommandFactory(impl?.EstimateLength() ?? 0);
            impl?.AppendTo(ref factory);
            return factory.CreateCommand(conn.Connection, conn.CommandTimeoutInS);
        }


        [Pure]
        public static QueryBuilder operator +(QueryBuilder a, QueryBuilder b) 
            => (a.impl==null||b.impl==null ? (a.impl ?? b.impl) : new TwoSqlFragments(a.impl, b.impl)).ToQuery();

        [Pure, Obsolete("Implicitly converts to SQL", true)]
        public static QueryBuilder operator +(QueryBuilder a, string b)
        {
            throw new InvalidOperationException("Cannot concatenate sql with strings");
        }

        [Pure, Obsolete("Implicitly converts to SQL", true)]
        public static QueryBuilder operator +(string a, QueryBuilder b)
        {
            throw new InvalidOperationException("Cannot concatenate sql with strings");
        }

        internal static QueryBuilder CreateDynamic(string rawSqlString)
        {
            return new StringSqlFragment(rawSqlString).ToQuery();
        }

        [Pure]
        public override bool Equals(object obj) => obj is QueryBuilder && Equals((QueryBuilder)this);

        [Pure]
        public override int GetHashCode() => (impl?.GetHashCode() ?? 12345678) + 4567;

        [Pure]
        public static bool operator ==(QueryBuilder a, QueryBuilder b) => ReferenceEquals(a.impl, b.impl) || !ReferenceEquals(a.impl, null) && a.Equals(b);

        [Pure]
        public bool Equals(QueryBuilder other) => this == other;

        [Pure]
        public static bool operator !=(QueryBuilder a, QueryBuilder b) => !(a == b);

        public static QueryBuilder Empty => new QueryBuilder(null);

        static readonly QueryBuilder[] AllColumns = { SqlFactory.SQL($"*") };
        static readonly QueryBuilder Comma_ColumnSeperator = SqlFactory.SQL($", ");

        static QueryBuilder SubQueryHelper(
            QueryBuilder subquery,
            IEnumerable<QueryBuilder> projectedColumns,
            QueryBuilder filterClause,
            OrderByColumns sortOrder,
            QueryBuilder topRowsOrNull)
        {
            projectedColumns = projectedColumns ?? AllColumns;

            var topClause = topRowsOrNull != Empty ? SqlFactory.SQL($"top ({topRowsOrNull}) ") : Empty;
            var projectedColumnsClause = CreateProjectedColumnsClause(projectedColumns);
            return
                SqlFactory.SQL($"select {topClause}{projectedColumnsClause} from (\r\n{subquery}\r\n) as _g1 where {filterClause}\r\n")
                    + CreateFromSortOrder(sortOrder);
        }

        static QueryBuilder CreateProjectedColumnsClause(IEnumerable<QueryBuilder> projectedColumns)
            => projectedColumns.Aggregate((a, b) => a.Append(Comma_ColumnSeperator).Append(b));

        [Pure]
        static QueryBuilder CreateFromSortOrder(OrderByColumns sortOrder)
        {
            return !sortOrder.Columns.Any()
                ? Empty
                : CreateDynamic("order by " + sortOrder.Columns.Select(sc => sc.SqlSortString()).JoinStrings(", "));
        }

        [Pure]
        public static QueryBuilder CreatePagedSubQuery(
            QueryBuilder subQuery,
            IEnumerable<QueryBuilder> projectedColumns,
            QueryBuilder filterClause,
            OrderByColumns sortOrder,
            int skipNrows,
            int takeNrows)
        {
            projectedColumns = projectedColumns ?? AllColumns;
            if (!projectedColumns.Any()) {
                throw new InvalidOperationException(
                    "Cannot create subquery without any projected columns: at least one column must be projected (are your columns all virtual?)\nQuery:\n"
                        + subQuery.DebugText());
            }

            var takeRowsParam = SqlFactory.SQL($@"{(long)takeNrows}");
            var skipNrowsParam = SqlFactory.SQL($@"{(long)skipNrows}");

            var sortorder = sortOrder;
            var orderClause = sortorder == OrderByColumns.Empty ? SqlFactory.SQL($"order by (select 1)") : CreateFromSortOrder(sortorder);
            var projectedColumnsClause = CreateProjectedColumnsClause(projectedColumns);

            var topNSubQuery = SubQueryHelper(subQuery, projectedColumns, filterClause, sortOrder, takeRowsParam + SqlFactory.SQL($"+") + skipNrowsParam);
            return SqlFactory.SQL($@"
select top ({takeRowsParam}) {projectedColumnsClause}
from (select _row=row_number() over ({orderClause}),
      _g2.*
from (

{topNSubQuery}

) as _g2) t
where _row > {skipNrowsParam}
order by _row");
        }

        public string DebugText()
        {
            using (var cmd = CreateSqlCommand(new SqlCommandCreationContext(null, 0, null))) {
                return QueryTracer.DebugFriendlyCommandText(cmd, QueryTracerParameterValues.Included);
            }
        }

        [Pure]
        public static QueryBuilder CreateSubQuery(QueryBuilder subQuery, IEnumerable<QueryBuilder> projectedColumns, QueryBuilder filterClause, OrderByColumns sortOrder)
            => SubQueryHelper(subQuery, projectedColumns, filterClause, sortOrder, Empty);

        public static QueryBuilder Param(object paramVal) => new SingleParameterSqlFragment(paramVal).ToQuery();

        [Pure]
        public static QueryBuilder TableParamDynamic(Array o) => new SingleTableParameterSqlFragment(o).ToQuery();
    }

    internal class StringSqlFragment : IBuildableQuery
    {
        readonly string rawSqlString;

        public StringSqlFragment(string rawSqlString)
        {
            this.rawSqlString = rawSqlString;
        }

        public void AppendTo(ref CommandFactory factory) => factory.AppendSql(rawSqlString, 0, rawSqlString.Length);

        public int EstimateLength() => rawSqlString.Length;
    }

    internal class SingleParameterSqlFragment : IBuildableQuery
    {
        readonly object paramVal;

        public SingleParameterSqlFragment(object paramVal)
        {
            this.paramVal = paramVal;
        }

        public void AppendTo(ref CommandFactory factory) 
            => QueryComponent.AppendParamTo(ref factory, paramVal);

        public int EstimateLength() => 5;
    }

    internal class SingleTableParameterSqlFragment : IBuildableQuery
    {
        readonly Array paramVal;

        public SingleTableParameterSqlFragment(Array paramVal)
        {
            this.paramVal = paramVal;
        }

        public void AppendTo(ref CommandFactory factory)
            => QueryComponent.ToTableParameter(paramVal).AppendTo(ref factory);

        public int EstimateLength() => 5;
    }


    interface IBuildableQuery
    {
        void AppendTo(ref CommandFactory factory);
        int EstimateLength();
    }

    public static class SqlFactory
    {
        internal static QueryBuilder ToQuery(this IBuildableQuery q) => new QueryBuilder(q);
        public static QueryBuilder SQL(FormattableString interpolatedQuery) => new InterpolatedSqlFragment(interpolatedQuery).ToQuery();
    }

    class TwoSqlFragments : IBuildableQuery
    {
        readonly IBuildableQuery a, b;

        public TwoSqlFragments(IBuildableQuery a, IBuildableQuery b)
        {
            this.a = a;
            this.b = b;
        }

        public void AppendTo(ref CommandFactory factory)
        {
            a.AppendTo(ref factory);
            b.AppendTo(ref factory);
        }

        public int EstimateLength() => a.EstimateLength() + b.EstimateLength();
    }

    class InterpolatedSqlFragment : IBuildableQuery
    {
        readonly FormattableString interpolatedQuery;

        public InterpolatedSqlFragment(FormattableString interpolatedQuery)
        {
            this.interpolatedQuery = interpolatedQuery;
        }

        public void AppendTo(ref CommandFactory factory)
        {

            if (interpolatedQuery == null) {
                throw new ArgumentNullException(nameof(interpolatedQuery));
            }

            var str = interpolatedQuery.Format;

            var pos = 0;
            while (true) {
                var paramRefMatch = ParamRefNextMatch(str, pos);
                if (paramRefMatch.WasNotFound())
                    break;
                factory.AppendSql(str, pos, paramRefMatch.StartIndex - pos);
                var argument = interpolatedQuery.GetArgument(paramRefMatch.ReferencedParameterIndex);
                if (argument is QueryBuilder) {
                    ((QueryBuilder)argument).AppendTo(ref factory);
                } else {
                    QueryComponent.AppendParamTo(ref factory, argument);
                }
                pos = paramRefMatch.EndIndex;
            }
            factory.AppendSql(str, pos, str.Length - pos);
        }

        static ParamRefSubString ParamRefNextMatch(string query, int pos)
        {
            while (pos < query.Length) {
                char c = query[pos];
                if (c == '{') {
                    var startPos = pos;
                    int num = 0;
                    for (pos++; pos < query.Length; pos++) {
                        c = query[pos];
                        if (c >= '0' && c <= '9') {
                            num = num * 10 + (c - '0');
                            continue;
                        } else if (c == '}') {
                            return new ParamRefSubString {
                                StartIndex = startPos,
                                EndIndex = pos + 1,
                                ReferencedParameterIndex = num
                            };
                        } else {
                            break;
                        }
                    }
                }
                pos++;
            }
            return ParamRefSubString.NotFound;
        }

        public int EstimateLength()
        {
            return interpolatedQuery.Format.Length + interpolatedQuery.ArgumentCount * 2;
            // converting {0} into @par0 adds 2 to length
        }

        struct ParamRefSubString
        {
            public int StartIndex, EndIndex, ReferencedParameterIndex;
            public bool WasNotFound() => ReferencedParameterIndex < 0;
            public static readonly ParamRefSubString NotFound = new ParamRefSubString { ReferencedParameterIndex = -1 };
        }
    }
}
