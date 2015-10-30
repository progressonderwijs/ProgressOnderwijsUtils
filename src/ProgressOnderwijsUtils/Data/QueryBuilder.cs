using System;
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
        {
            return QueryBuilder.CreateDynamic(interpolatedQuery.Format, interpolatedQuery.GetArguments());
        }
    }

    public abstract class QueryBuilder : IEquatable<QueryBuilder>
    {
        QueryBuilder() { } // only inner classes may inherit
        protected virtual QueryBuilder PrefixOrNull => null;
        protected virtual QueryBuilder SuffixOrNull => null;
        internal virtual IQueryComponent ValueOrNull => null;

        sealed class EmptyComponent : QueryBuilder
        {
            EmptyComponent() { }
            public static readonly EmptyComponent Instance = new EmptyComponent();
        }

        sealed class SingleComponent : QueryBuilder
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

        sealed class PrefixAndComponent : QueryBuilder
        {
            readonly QueryBuilder precedingComponents;
            readonly IQueryComponent value;
            protected override QueryBuilder PrefixOrNull => precedingComponents;
            internal override IQueryComponent ValueOrNull => value;

            public PrefixAndComponent(QueryBuilder prefix, IQueryComponent singleComponent)
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

        sealed class PrefixAndSuffix : QueryBuilder
        {
            readonly QueryBuilder precedingComponents, next;
            protected override QueryBuilder PrefixOrNull => precedingComponents;
            protected override QueryBuilder SuffixOrNull => next;

            public PrefixAndSuffix(QueryBuilder prefix, QueryBuilder continuation)
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
        public static readonly QueryBuilder Empty = EmptyComponent.Instance;
        bool IsEmpty => this is EmptyComponent;
        bool IsSingleNonNullElement => this is SingleComponent;

        [Pure]
        public static QueryBuilder operator +(QueryBuilder a, QueryBuilder b) => Concat(a, b);

        [Pure, Obsolete("Implicitly converts to SQL", true)]
        public static QueryBuilder operator +(QueryBuilder a, string b) => Concat(a, QueryComponent.CreateString(b));

        [Pure, Obsolete("Implicitly converts to SQL", true)]
        public static QueryBuilder operator +(string a, QueryBuilder b) => Concat(CreateDynamic(a), b);

        static QueryBuilder Concat(QueryBuilder query, IQueryComponent part) => null == part ? query : new PrefixAndComponent(query, part);

        static QueryBuilder Concat(QueryBuilder first, QueryBuilder second)
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
        public static QueryBuilder Param(object o) => new SingleComponent(QueryComponent.CreateParam(o));

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
        public static QueryBuilder TableParam<T>(string typeName, IEnumerable<T> o)
            where T : IMetaObject, new()
            => new SingleComponent(QueryComponent.ToTableParameter(typeName, o));

        [Pure]
        public static QueryBuilder TableParam(IEnumerable<int> o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        [Pure]
        public static QueryBuilder TableParam(IEnumerable<string> o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        [Pure]
        public static QueryBuilder TableParam(IEnumerable<DateTime> o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        [Pure]
        public static QueryBuilder TableParam(IEnumerable<TimeSpan> o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        [Pure]
        public static QueryBuilder TableParam(IEnumerable<decimal> o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        [Pure]
        public static QueryBuilder TableParam(IEnumerable<char> o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        [Pure]
        public static QueryBuilder TableParam(IEnumerable<bool> o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        [Pure]
        public static QueryBuilder TableParam(IEnumerable<byte> o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        [Pure]
        public static QueryBuilder TableParam(IEnumerable<short> o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        [Pure]
        public static QueryBuilder TableParam(IEnumerable<long> o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        [Pure]
        public static QueryBuilder TableParam(IEnumerable<double> o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        [Pure]
        public static QueryBuilder TableParamDynamic(Array o) => new SingleComponent(QueryComponent.ToTableParameter(o));

        // ReSharper restore UnusedMember.Global
        public static QueryBuilder CreateDynamic(string str) 
            => new SingleComponent(QueryComponent.CreateString(str));

        [Pure]
        public static QueryBuilder CreateDynamic(string str, params object[] arguments)
        {
            if (str == null) {
                throw new ArgumentNullException(nameof(str));
            }

            //null if argument is already a QueryBuilder and no new component needs to be created
            var queryComponents = new IQueryComponent[arguments.Length];

            for (var i = 0; i < arguments.Length; i++) {
                if (!(arguments[i] is QueryBuilder)) {
                    queryComponents[i] = QueryComponent.CreateParam(arguments[i]);
                }
            }
            var query = Empty;

            var pos = 0;
            foreach (var paramRefMatch in ParamRefMatches(str)) {
                query = Concat(query, QueryComponent.CreateString(str.Substring(pos, paramRefMatch.Index - pos)));
                var componentIndex = int.Parse(str.Substring(paramRefMatch.Index + 1, paramRefMatch.Length - 2), NumberStyles.None, CultureInfo.InvariantCulture);
                var queryComponent = queryComponents[componentIndex];
                if (queryComponent == null) {
                    query = Concat(query, (QueryBuilder)arguments[componentIndex]);
                } else {
                    query = Concat(query, queryComponent);
                }
                pos = paramRefMatch.Index + paramRefMatch.Length;
            }
            query = Concat(query, QueryComponent.CreateString(str.Substring(pos, str.Length - pos)));

            return query;
        }

        struct SubstringPosition
        {
            public int Index, Length;
        }

        static IEnumerable<SubstringPosition> ParamRefMatches(string query)
        {
            for (int pos = 0; pos < query.Length; pos++) {
                char c = query[pos];
                if (c == '{') {
                    for (int pI = pos + 1; pI < query.Length; pI++) {
                        if (query[pI] >= '0' && query[pI] <= '9') {
                            continue;
                        } else if (query[pI] == '}' && pI >= pos + 2) //{} testen
                        {
                            yield return new SubstringPosition { Index = pos, Length = pI - pos + 1 };
                            pos = pI;
                            break;
                        } else {
                            break;
                        }
                    }
                }
            }
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
                var Continuation = new Stack<QueryBuilder>();
                QueryBuilder current = this;
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
        public override bool Equals(object obj) => Equals(obj as QueryBuilder);

        [Pure]
        public static bool operator ==(QueryBuilder a, QueryBuilder b) => ReferenceEquals(a, b) || !ReferenceEquals(a, null) && a.Equals(b);

        [Pure]
        public bool Equals(QueryBuilder other) => !ReferenceEquals(other, null) && CanonicalReverseComponents.SequenceEqual(other.CanonicalReverseComponents);

        [Pure]
        public static bool operator !=(QueryBuilder a, QueryBuilder b) => !(a == b);

        [Pure]
        public override int GetHashCode() => HashCodeHelper.ComputeHash(CanonicalReverseComponents.ToArray()) + 123;

        [Pure]
        public override string ToString() => DebugText(null);

        static readonly QueryBuilder[] AllColumns = { SQL($"*") };
        static readonly QueryBuilder Comma_ColumnSeperator = SQL($", ");

        static QueryBuilder SubQueryHelper(
            QueryBuilder subquery,
            IEnumerable<QueryBuilder> projectedColumns,
            QueryBuilder filterClause,
            OrderByColumns sortOrder,
            QueryBuilder topRowsOrNull)
        {
            projectedColumns = projectedColumns ?? AllColumns;

            var topClause = topRowsOrNull != null ? SQL($"top ({topRowsOrNull}) ") : Empty;
            var projectedColumnsClause = CreateProjectedColumnsClause(projectedColumns);
            return
                SQL($"select {topClause}{projectedColumnsClause} from (\r\n{subquery}\r\n) as _g1 where {filterClause}\r\n")
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
        public static QueryBuilder CreateSubQuery(QueryBuilder subQuery, IEnumerable<QueryBuilder> projectedColumns, QueryBuilder filterClause, OrderByColumns sortOrder)
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
