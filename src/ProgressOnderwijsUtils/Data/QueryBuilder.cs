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
        readonly IQueryComponent impl;

        internal QueryBuilder(IQueryComponent impl)
        {
            this.impl = impl;
        }

        internal void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory
            => impl?.AppendTo(ref factory);

        public ReusableCommand CreateSqlCommand(SqlCommandCreationContext conn)
        {
            var factory = CommandFactory.Create();
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
        public string DebugText() => DebugCommandFactory.DebugTextFor(impl);

        public string CommandText()
        {
            using (var cmd = CreateSqlCommand(new SqlCommandCreationContext(null, 0, null)))
                return cmd.Command.CommandText;
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

    interface IQueryComponent
    {
        void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory;
    }

    class StringSqlFragment : IQueryComponent
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

    class SingleParameterSqlFragment : IQueryComponent
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
        void ToSqlParameter(ref SqlParamArgs paramArgs);
        object EquatableValue { get; }
    }

    public static class SafeSql
    {
        [Pure]
        public static QueryBuilder SQL(FormattableString interpolatedQuery) => SqlFactory.InterpolationToQuery(interpolatedQuery);
    }

    static class SqlFactory
    {
        public static QueryBuilder BuildableToQuery(this IQueryComponent q) => new QueryBuilder(q);
        public static QueryBuilder InterpolationToQuery(FormattableString interpolatedQuery) => new InterpolatedSqlFragment(interpolatedQuery).BuildableToQuery();

        public static void AppendSql<TCommandFactory>(ref TCommandFactory factory, string sql)
            where TCommandFactory : struct, ICommandFactory
            => factory.AppendSql(sql, 0, sql.Length);
    }

    class TwoSqlFragments : IQueryComponent
    {
        readonly IQueryComponent a, b;

        public TwoSqlFragments(IQueryComponent a, IQueryComponent b)
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

    class InterpolatedSqlFragment : IQueryComponent
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
            var strLen = str.Length;

            var pos = 0;
            while (true) {
                var paramRefMatch = ParamRefNextMatch(str, pos, strLen);
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
            factory.AppendSql(str, pos, strLen - pos);
        }

        static ParamRefSubString ParamRefNextMatch(string query, int pos, int length)
        {
            while (pos < length) {
                char c = query[pos];
                if (c == '{') {
                    var startPos = pos;
                    int num = 0;
                    for (pos++; pos < length; pos++) {
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
