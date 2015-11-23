﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using JetBrains.Annotations;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils
{
    public static class SafeSql
    {
        [Pure]
        public static QueryBuilder SQL(FormattableString interpolatedQuery) 
            => SqlFactory.SQL(interpolatedQuery);
    }

    public abstract class QueryBuilder0 : IEquatable<QueryBuilder0>
    {
        QueryBuilder0() { } // only inner classes may inherit
        protected virtual QueryBuilder0 PrefixOrNull => null;
        protected virtual QueryBuilder0 SuffixOrNull => null;
        internal virtual IQueryComponent ValueOrNull => null;

        sealed class EmptyComponent : QueryBuilder0
        {
            EmptyComponent() { }
            public static readonly EmptyComponent Instance = new EmptyComponent();
        }

        sealed class SingleComponent : QueryBuilder0
        {
            readonly IQueryComponent value;
            internal override IQueryComponent ValueOrNull => value;

            public SingleComponent(IQueryComponent singleNode)
            {
                if (singleNode == null) {
                    throw new ArgumentNullException(nameof(singleNode));
                }
                value = singleNode;
            }
        }

        sealed class PrefixAndComponent : QueryBuilder0
        {
            readonly QueryBuilder0 precedingComponents;
            readonly IQueryComponent value;
            protected override QueryBuilder0 PrefixOrNull => precedingComponents;
            internal override IQueryComponent ValueOrNull => value;

            public PrefixAndComponent(QueryBuilder0 prefix, IQueryComponent singleComponent)
            {
                if (null == prefix) {
                    throw new ArgumentNullException(nameof(prefix));
                }
                if (null == singleComponent) {
                    throw new ArgumentNullException(nameof(singleComponent));
                }
                precedingComponents = prefix.IsEmpty ? null : prefix;
                value = singleComponent;
            }
        }

        sealed class PrefixAndSuffix : QueryBuilder0
        {
            readonly QueryBuilder0 precedingComponents, next;
            protected override QueryBuilder0 PrefixOrNull => precedingComponents;
            protected override QueryBuilder0 SuffixOrNull => next;

            public PrefixAndSuffix(QueryBuilder0 prefix, QueryBuilder0 continuation)
            {
                if (null == prefix) {
                    throw new ArgumentNullException(nameof(prefix));
                }
                if (null == continuation) {
                    throw new ArgumentNullException(nameof(continuation));
                }
                precedingComponents = prefix;
                next = continuation;
            }
        }

        //INVARIANT:
        // IF next != null THEN precedingComponents !=null; conversely IF precedingComponents == null THEN next == null 
        // !(value != null AND next !=null)
        public static readonly QueryBuilder0 Empty = EmptyComponent.Instance;
        bool IsEmpty => this is EmptyComponent;
        bool IsSingleNonNullElement => this is SingleComponent;

        [Pure]
        public static QueryBuilder0 operator +(QueryBuilder0 a, QueryBuilder0 b) => Concat(a, b);

        [Pure, Obsolete("Implicitly converts to SQL", true)]
        public static QueryBuilder0 operator +(QueryBuilder0 a, string b) => Concat(a, QueryComponent.CreateString(b));

        [Pure, Obsolete("Implicitly converts to SQL", true)]
        public static QueryBuilder0 operator +(string a, QueryBuilder0 b) => Concat(CreateDynamic(a), b);

        static QueryBuilder0 Concat(QueryBuilder0 query, IQueryComponent part) => null == part ? query : new PrefixAndComponent(query, part);

        static QueryBuilder0 Concat(QueryBuilder0 first, QueryBuilder0 second)
        {
            if (null == first) {
                throw new ArgumentNullException(nameof(first));
            } else if (null == second) {
                throw new ArgumentNullException(nameof(second));
            } else if (first.IsEmpty) {
                return second;
            } else if (second.IsEmpty) {
                return first;
            } else if (second.IsSingleNonNullElement) {
                return new PrefixAndComponent(first, second.ValueOrNull);
            } else {
                return new PrefixAndSuffix(first, second);
            }
        }

        [Pure]
        public static QueryBuilder0 Param(object o) => new SingleComponent(QueryComponent.CreateParam(o));

        /// <summary>
        /// Adds a parameter to the query with a table-value.  Parameters must be an enumerable of meta-object type.
        /// 
        ///   You need to define a corresponding type in the database (see QueryComponent.ToTableParameter for details).
        /// </summary>
        /// <param name="typeName">name of the db-type e.g. IntValues</param>
        /// <param name="o">the list of meta-objects with shape corresponding to the DB type</param>
        /// <returns>a composable query-component</returns>
        // ReSharper disable UnusedMember.Global
        [Pure]
        public static QueryBuilder0 TableParam<T>(string typeName, IEnumerable<T> o)
            where T : IMetaObject, new()
            => new SingleComponent(QueryComponent.ToTableParameter(typeName, o));

        [Pure]
        public static QueryBuilder0 TableParam(IEnumerable<int> o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        [Pure]
        public static QueryBuilder0 TableParam(IEnumerable<string> o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        [Pure]
        public static QueryBuilder0 TableParam(IEnumerable<DateTime> o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        [Pure]
        public static QueryBuilder0 TableParam(IEnumerable<TimeSpan> o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        [Pure]
        public static QueryBuilder0 TableParam(IEnumerable<decimal> o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        [Pure]
        public static QueryBuilder0 TableParam(IEnumerable<char> o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        [Pure]
        public static QueryBuilder0 TableParam(IEnumerable<bool> o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        [Pure]
        public static QueryBuilder0 TableParam(IEnumerable<byte> o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        [Pure]
        public static QueryBuilder0 TableParam(IEnumerable<short> o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        [Pure]
        public static QueryBuilder0 TableParam(IEnumerable<long> o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        [Pure]
        public static QueryBuilder0 TableParam(IEnumerable<double> o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        [Pure]
        public static QueryBuilder0 TableParamDynamic(Array o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        // ReSharper restore UnusedMember.Global
        public static QueryBuilder0 CreateDynamic(string str)
        {
            var stringComponent = QueryComponent.CreateString(str);
            return stringComponent == null ? Empty : new SingleComponent(stringComponent);
        }

        public static QueryBuilder0 SQL(FormattableString interpolatedQuery) => CreateFromInterpolation(interpolatedQuery);

        [Pure]
        public static QueryBuilder0 CreateFromInterpolation(FormattableString interpolatedQuery)
        {
            if (interpolatedQuery == null) {
                throw new ArgumentNullException(nameof(interpolatedQuery));
            }

            var str = interpolatedQuery.Format;
            var query = Empty;

            var pos = 0;
            while (true) {
                var paramRefMatchOrNull = ParamRefNextMatch(str, pos);
                if (paramRefMatchOrNull == null)
                    break;
                var paramRefMatch = paramRefMatchOrNull.Value;
                query = Concat(query, QueryComponent.CreateString(str.Substring(pos, paramRefMatch.StartIndex - pos)));
                var argumentIndex = int.Parse(str.Substring(paramRefMatch.StartIndex + 1, paramRefMatch.EndIndex - paramRefMatch.StartIndex - 2), NumberStyles.None, CultureInfo.InvariantCulture);
                var argument = interpolatedQuery.GetArgument(argumentIndex);
                if (argument is QueryBuilder0) {
                    query = Concat(query, (QueryBuilder0)argument);
                } else {
                    query = Concat(query, QueryComponent.CreateParam(argument));
                }
                pos = paramRefMatch.EndIndex;
            }
            query = Concat(query, QueryComponent.CreateString(str.Substring(pos, str.Length - pos)));

            return query;
        }

        struct SubstringPosition
        {
            public int StartIndex, EndIndex;
        }

        static SubstringPosition? ParamRefNextMatch(string query, int pos)
        {
            while(pos < query.Length) {
                char c = query[pos];
                if (c == '{') {
                    for (int pI = pos + 1; pI < query.Length; pI++) {
                        if (query[pI] >= '0' && query[pI] <= '9') {
                            continue;
                        } else if (query[pI] == '}' && pI >= pos + 2) { //{} testen
                            return new SubstringPosition { StartIndex = pos, EndIndex = pI + 1 };
                        } else {
                            break;
                        }
                    }
                }
                pos++;
            }
            return null;
        }


        [Pure]
        public SqlCommand CreateSqlCommand(SqlCommandCreationContext commandCreationContext)
        {
            var cmd = CommandFactory.BuildQuery(ComponentsInReverseOrder.Reverse(), commandCreationContext.Connection, commandCreationContext.CommandTimeoutInS);
            if (commandCreationContext.Tracer != null) {
                try {
                    var timer = commandCreationContext.Tracer.StartQueryTimer(cmd);
                    cmd.Disposed += (s, e) => timer.Dispose();
                } catch {
                    cmd.Dispose();
                    throw;
                }
            }
            return cmd;
        }

        [Pure]
        public string DebugText(Taal? taalOrNull) => ComponentsInReverseOrder.Reverse().Select(component => component.ToDebugText(taalOrNull)).JoinStrings();

        [Pure]
        public string CommandText() => CommandFactory.BuildQueryText(ComponentsInReverseOrder.Reverse());

        IEnumerable<IQueryComponent> ComponentsInReverseOrder
        {
            get
            {
                if (IsEmpty) {
                    yield break;
                }
                var Continuation = new Stack<QueryBuilder0>();
                QueryBuilder0 current = this;
                while (true) {
                    if (current.PrefixOrNull != null) {
                        Continuation.Push(current.PrefixOrNull); //deal with prefix if any later
                    }

                    if (current.SuffixOrNull != null) {
                        current = current.SuffixOrNull; //can't have a value, so deal with suffix
                    } else //no suffix: either empty or with value.
                    {
                        if (current.ValueOrNull != null) {
                            yield return current.ValueOrNull;
                        }
                        if (Continuation.Count == 0) {
                            yield break;
                        }

                        current = Continuation.Pop();
                    }
                }
            }
        }

        IEnumerable<IQueryComponent> CanonicalReverseComponents
        {
            get
            {
                var cached = new List<QueryStringComponent>();
                foreach (var comp in ComponentsInReverseOrder) {
                    if (comp is QueryStringComponent) {
                        cached.Add((QueryStringComponent)comp);
                    } else {
                        if (cached.Count > 0) {
                            cached.Reverse();
                            yield return QueryComponent.CreateString(cached.Select(c => c.val).JoinStrings());
                            cached.Clear();
                        }
                        yield return comp;
                    }
                }
                if (cached.Count > 0) {
                    cached.Reverse();
                    yield return QueryComponent.CreateString(cached.Select(c => c.val).JoinStrings());
                }
            }
        }

        [Pure]
        public override bool Equals(object obj) => Equals(obj as QueryBuilder0);

        [Pure]
        public static bool operator ==(QueryBuilder0 a, QueryBuilder0 b) => ReferenceEquals(a, b) || !ReferenceEquals(a, null) && a.Equals(b);

        [Pure]
        public bool Equals(QueryBuilder0 other) => !ReferenceEquals(other, null) && CanonicalReverseComponents.SequenceEqual(other.CanonicalReverseComponents);

        [Pure]
        public static bool operator !=(QueryBuilder0 a, QueryBuilder0 b) => !(a == b);

        [Pure]
        public override int GetHashCode() => HashCodeHelper.ComputeHash(CanonicalReverseComponents.ToArray()) + 123;

        [Pure]
        public override string ToString() => DebugText(null);

        static readonly QueryBuilder0[] AllColumns = { SQL($"*") };
        static readonly QueryBuilder0 Comma_ColumnSeperator = SQL($", ");

        static QueryBuilder0 SubQueryHelper(
            QueryBuilder0 subquery,
            IEnumerable<QueryBuilder0> projectedColumns,
            QueryBuilder0 filterClause,
            OrderByColumns sortOrder,
            QueryBuilder0 topRowsOrNull)
        {
            projectedColumns = projectedColumns ?? AllColumns;

            var topClause = topRowsOrNull != null ? SQL($"top ({topRowsOrNull}) ") : Empty;
            var projectedColumnsClause = CreateProjectedColumnsClause(projectedColumns);
            return
                SQL($"select {topClause}{projectedColumnsClause} from (\r\n{subquery}\r\n) as _g1 where {filterClause}\r\n")
                    + CreateFromSortOrder(sortOrder);
        }

        static QueryBuilder0 CreateProjectedColumnsClause(IEnumerable<QueryBuilder0> projectedColumns)
            => projectedColumns.Aggregate((a, b) => a.Append(Comma_ColumnSeperator).Append(b));

        [Pure]
        static QueryBuilder0 CreateFromSortOrder(OrderByColumns sortOrder)
        {
            return !sortOrder.Columns.Any()
                ? Empty
                : CreateDynamic("order by " + sortOrder.Columns.Select(sc => sc.SqlSortString()).JoinStrings(", "));
        }

        [Pure]
        public static QueryBuilder0 CreatePagedSubQuery(
            QueryBuilder0 subQuery,
            IEnumerable<QueryBuilder0> projectedColumns,
            QueryBuilder0 filterClause,
            OrderByColumns sortOrder,
            int skipNrows,
            int takeNrows)
        {
            projectedColumns = projectedColumns ?? AllColumns;
            if (!projectedColumns.Any()) {
                throw new InvalidOperationException(
                    "Cannot create subquery without any projected columns: at least one column must be projected (are your columns all virtual?)\nQuery:\n"
                        + subQuery.DebugText(null));
            }

            var takeRowsParam = Param((long)takeNrows);
            var skipNrowsParam = Param((long)skipNrows);

            var sortorder = sortOrder;
            var orderClause = sortorder == OrderByColumns.Empty ? SQL($"order by (select 1)") : CreateFromSortOrder(sortorder);
            var projectedColumnsClause = CreateProjectedColumnsClause(projectedColumns);

            var topNSubQuery = SubQueryHelper(subQuery, projectedColumns, filterClause, sortOrder, takeRowsParam + SQL($"+") + skipNrowsParam);
            return SQL($@"
select top ({takeRowsParam}) {projectedColumnsClause}
from (select _row=row_number() over ({orderClause}),
      _g2.*
from (

{topNSubQuery}

) as _g2) t
where _row > {skipNrowsParam}
order by _row");
        }

        [Pure]
        public static QueryBuilder0 CreateSubQuery(QueryBuilder0 subQuery, IEnumerable<QueryBuilder0> projectedColumns, QueryBuilder0 filterClause, OrderByColumns sortOrder)
            => SubQueryHelper(subQuery, projectedColumns, filterClause, sortOrder, null);

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

    public class SqlCommandCreationContext : IDisposable
    {
        public SqlConnection Connection { get; }
        public IQueryTracer Tracer { get; }
        public int CommandTimeoutInS { get; }
        // ReSharper disable UnusedMember.Global
        // Handige generieke functionaliteit, maar niet altijd gebruikt
        public SqlCommandCreationContext OverrideTimeout(int timeoutSeconds) => new SqlCommandCreationContext(Connection, timeoutSeconds, Tracer);
        // ReSharper restore UnusedMember.Global
        public SqlCommandCreationContext(SqlConnection conn, int defaultTimeoutInS, IQueryTracer tracer)
        {
            Connection = conn;
            CommandTimeoutInS = defaultTimeoutInS;
            Tracer = tracer;
        }

        public void Dispose()
        {
            Connection.Dispose();
        }

        public static implicit operator SqlCommandCreationContext(SqlConnection conn) => new SqlCommandCreationContext(conn, 0, null);
    }
}
