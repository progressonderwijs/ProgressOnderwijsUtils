using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
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

        internal void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory
            => impl?.AppendTo(ref factory);

        public SqlCommand CreateSqlCommand(SqlCommandCreationContext conn)
        {
            var factory = new CommandFactory(impl?.EstimateLength() ?? 0);
            impl?.AppendTo(ref factory);
            return factory.CreateCommand(conn.Connection, conn.CommandTimeoutInS);
        }

        public static readonly QueryBuilder Empty = new QueryBuilder(null);

        [Pure]
        public static QueryBuilder operator +(QueryBuilder a, QueryBuilder b)
            => (a.impl == null || b.impl == null ? (a.impl ?? b.impl) : new TwoSqlFragments(a.impl, b.impl)).ToQuery();

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

        public static QueryBuilder CreateDynamic(string rawSqlString)
        {
            if (rawSqlString == null) {
                throw new ArgumentNullException(nameof(rawSqlString));
            }

            return new StringSqlFragment(rawSqlString).ToQuery();
        }

        [Pure]
        public override bool Equals(object obj)
            => obj is QueryBuilder && (QueryBuilder)obj == this;

        [Pure]
        public static bool operator ==(QueryBuilder a, QueryBuilder b)
            => ReferenceEquals(a.impl, b.impl)
                || EqualityKeyCommandFactory.EqualityKey(a.impl).Equals(EqualityKeyCommandFactory.EqualityKey(b.impl));

        [Pure]
        public bool Equals(QueryBuilder other) => this == other;

        [Pure]
        public static bool operator !=(QueryBuilder a, QueryBuilder b) => !(a == b);

        [Pure]
        public override int GetHashCode() => EqualityKeyCommandFactory.EqualityKey(impl).GetHashCode();

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

        public override string ToString() => DebugText();
        public string DebugText() => DebugCommandFactory.Create(impl?.EstimateLength() ?? 0).DebugTextFor(impl);

        public string CommandText()
        {
            using (var cmd = CreateSqlCommand(new SqlCommandCreationContext(null, 0, null)))
                return cmd.CommandText;
        }

        [Pure]
        public static QueryBuilder CreateSubQuery(QueryBuilder subQuery, IEnumerable<QueryBuilder> projectedColumns, QueryBuilder filterClause, OrderByColumns sortOrder)
            => SubQueryHelper(subQuery, projectedColumns, filterClause, sortOrder, Empty);

        public static QueryBuilder Param(object paramVal) => new SingleParameterSqlFragment(paramVal).ToQuery();

        [Pure]
        public static QueryBuilder TableParamDynamic(Array o) => QueryComponent.ToTableParameter(o).ToQuery();

        /// <summary>
        /// Adds a parameter to the query with a table-value.  Parameters must be an enumerable of meta-object type.
        /// 
        ///   You need to define a corresponding type in the database (see QueryComponent.ToTableParameter for details).
        /// </summary>
        /// <param name="typeName">name of the db-type e.g. IntValues</param>
        /// <param name="o">the list of meta-objects with shape corresponding to the DB type</param>
        /// <returns>a composable query-component</returns>
        [Pure]
        public static QueryBuilder TableParam<T>(string typeName, IEnumerable<T> o)
            where T : IMetaObject, new()
            => QueryComponent.ToTableParameter(typeName, o).ToQuery();

        //TODO: dit aanzetten voor datasource tests
        // ReSharper disable once UnusedMember.Global
        public void AssertNoVariableColumns()
        {
            var commandText = CommandText();
            var commandTextWithoutComments = Regex.Replace(
                commandText,
                @"/\*.*?\*/|--.*?$",
                "",
                RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Multiline);
            if (Regex.IsMatch(commandTextWithoutComments, @"(?<!count\()\*")) {
                throw new InvalidOperationException(
                    GetType().FullName + ": Query may not use * as that might cause runtime exceptions in productie when DB changes:\n" + commandText);
            }
        }
    }

    class StringSqlFragment : IBuildableQuery
    {
        readonly string rawSqlString;

        public StringSqlFragment(string rawSqlString)
        {
            this.rawSqlString = rawSqlString;
        }

        public void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory
            => factory.AppendSql(rawSqlString);

        public int EstimateLength() => rawSqlString.Length;
    }

    class SingleParameterSqlFragment : IBuildableQuery
    {
        readonly object paramVal;

        public SingleParameterSqlFragment(object paramVal)
        {
            this.paramVal = paramVal;
        }

        public void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory
            => QueryComponent.AppendParamTo(ref factory, paramVal);

        public int EstimateLength() => 5;
    }

    interface IBuildableQuery
    {
        void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory;

        int EstimateLength();
    }

    interface IQueryParameter : IBuildableQuery
    {
        SqlParameter ToSqlParameter(string paramName);
        object EquatableValue { get; }
    }

    public static class SafeSql
    {
        [Pure]
        public static QueryBuilder SQL(FormattableString interpolatedQuery) => SqlFactory.SQL(interpolatedQuery);
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

        public void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory
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

        public void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory
        {
            if (interpolatedQuery == null) {
                throw new ArgumentNullException(nameof(interpolatedQuery));
            }

            var str = interpolatedQuery.Format;

            var pos = 0;
            while (true) {
                var paramRefMatch = ParamRefNextMatch(str, pos);
                if (paramRefMatch.WasNotFound()) {
                    break;
                }
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
                        } else if (c == '}') {
                            return new ParamRefSubString {
                                StartIndex = startPos,
                                EndIndex = pos + 1,
                                ReferencedParameterIndex = num
                            };
                        } else {
                            throw new ArgumentException("format string invalid: an opening brace must be followed by one or more decimal digits which must be followed by a closing brace", nameof(query));
                        }
                    }
                }
                pos++;
            }
            return ParamRefSubString.NotFound;
        }

        static readonly int EstimatedPlaceholderLength = "{0}".Length;

        public int EstimateLength()
            => interpolatedQuery.Format.Length + interpolatedQuery.ArgumentCount * (CommandFactory.EstimatedParameterLength - EstimatedPlaceholderLength);

        //we ignore TVP and subqueries here - any query using those will thus incur a slight perf overhead, which seems acceptable to me.
        struct ParamRefSubString
        {
            public int StartIndex, EndIndex, ReferencedParameterIndex;
            public bool WasNotFound() => ReferencedParameterIndex < 0;
            public static readonly ParamRefSubString NotFound = new ParamRefSubString { ReferencedParameterIndex = -1 };
        }
    }
}
