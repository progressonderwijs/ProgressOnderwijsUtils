﻿using System;
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
            => (a.impl == null || b.impl == null ? (a.impl ?? b.impl) : new TwoSqlFragments(a.impl, b.impl)).BuildableToQuery();

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

            return new StringSqlFragment(rawSqlString).BuildableToQuery();
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


        public override string ToString() => DebugText();
        public string DebugText() => DebugCommandFactory.Create(impl?.EstimateLength() ?? 0).DebugTextFor(impl);

        public string CommandText()
        {
            using (var cmd = CreateSqlCommand(new SqlCommandCreationContext(null, 0, null)))
                return cmd.CommandText;
        }

        public static QueryBuilder Param(object paramVal) => new SingleParameterSqlFragment(paramVal).BuildableToQuery();

        [Pure]
        public static QueryBuilder TableParamDynamic(Array o) => QueryComponent.ToTableParameter(o).BuildableToQuery();

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
            => QueryComponent.ToTableParameter(typeName, o).BuildableToQuery();
    }

    interface IBuildableQuery
    {
        void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory;
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
            => SqlFactory.AppendSql(ref factory, rawSqlString);
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
    }

    interface IQueryParameter
    {
        SqlParameter ToSqlParameter(string paramName);
        object EquatableValue { get; }
    }

    public static class SafeSql
    {
        [Pure]
        public static QueryBuilder SQL(FormattableString interpolatedQuery) => SqlFactory.InterpolationToQuery(interpolatedQuery);
    }

    static class SqlFactory
    {
        public static int EstimateLength(this IBuildableQuery q)
        {
            var lengthEstimator = new LengthEstimationCommandFactory();
            q.AppendTo(ref lengthEstimator);
            return lengthEstimator.QueryLength;
        }

        public static QueryBuilder BuildableToQuery(this IBuildableQuery q) => new QueryBuilder(q);
        public static QueryBuilder InterpolationToQuery(FormattableString interpolatedQuery) => new InterpolatedSqlFragment(interpolatedQuery).BuildableToQuery();

        public static void AppendSql<TCommandFactory>(ref TCommandFactory factory, string sql)
            where TCommandFactory : struct, ICommandFactory
            => factory.AppendSql(sql, 0, sql.Length);
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

        //we ignore TVP and subqueries here - any query using those will thus incur a slight perf overhead, which seems acceptable to me.
        struct ParamRefSubString
        {
            public int StartIndex, EndIndex, ReferencedParameterIndex;
            public bool WasNotFound() => ReferencedParameterIndex < 0;
            public static readonly ParamRefSubString NotFound = new ParamRefSubString { ReferencedParameterIndex = -1 };
        }
    }
}
