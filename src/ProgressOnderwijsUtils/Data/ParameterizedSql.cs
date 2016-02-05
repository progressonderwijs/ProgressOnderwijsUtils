using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
    public struct ParameterizedSql
    {
        readonly IQueryComponent impl;

        internal ParameterizedSql(IQueryComponent impl)
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
            var command = factory.CreateCommand(conn);
            factory.ReturnToPool();
            return command;
        }

        public static readonly ParameterizedSql Empty = new ParameterizedSql(null);

        [Pure]
        public static ParameterizedSql operator +(ParameterizedSql a, ParameterizedSql b)
            => (a.impl == null || b.impl == null ? (a.impl ?? b.impl) : new TwoSqlFragments(a.impl, b.impl)).BuildableToQuery();

        public static ParameterizedSql CreateDynamic(string rawSqlString)
        {
            if (rawSqlString == null) {
                throw new ArgumentNullException(nameof(rawSqlString));
            }

            return new StringSqlFragment(rawSqlString).BuildableToQuery();
        }

        [Pure]
        public override bool Equals(object obj)
            => obj is ParameterizedSql && (ParameterizedSql)obj == this;

        [Pure]
        public static bool operator ==(ParameterizedSql a, ParameterizedSql b)
            => ReferenceEquals(a.impl, b.impl)
                || EqualityKeyCommandFactory.EqualityKey(a.impl).Equals(EqualityKeyCommandFactory.EqualityKey(b.impl));

        [Pure]
        public bool Equals(ParameterizedSql other) => this == other;

        [Pure]
        public static bool operator !=(ParameterizedSql a, ParameterizedSql b) => !(a == b);

        [Pure]
        public override int GetHashCode() => EqualityKeyCommandFactory.EqualityKey(impl).GetHashCode();

        public override string ToString() => DebugText();
        public string DebugText() => DebugCommandFactory.DebugTextFor(impl);

        public string CommandText()
        {
            var factory = CommandFactory.Create();
            impl?.AppendTo(ref factory);
            var commandText = factory.CommandText;
            factory.ReturnToPool();
            return commandText;
        }

        public static ParameterizedSql Param(object paramVal) => new SingleParameterSqlFragment(paramVal).BuildableToQuery();

        [Pure]
        public static ParameterizedSql TableParamDynamic(Array o) => QueryComponent.ToTableValuedParameterFromPlainValues(o).BuildableToQuery();

        /// <summary>
        /// Adds a parameter to the query with a table-value.  Parameters must be an enumerable of meta-object type.
        /// 
        ///   You need to define a corresponding type in the database (see QueryComponent.ToTableParameter for details).
        /// </summary>
        /// <param name="typeName">name of the db-type e.g. IntValues</param>
        /// <param name="objects">the list of meta-objects with shape corresponding to the DB type</param>
        /// <returns>a composable query-component</returns>
        [Pure]
        public static ParameterizedSql TableParam<T>(string typeName, T[] objects)
            where T : IMetaObject, new()
            => QueryComponent.ToTableValuedParameter<T,T>(typeName, objects, o=>(T[])o).BuildableToQuery();
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
        public static ParameterizedSql SQL(FormattableString interpolatedQuery) => SqlFactory.InterpolationToQuery(interpolatedQuery);
    }

    static class SqlFactory
    {
        public static ParameterizedSql BuildableToQuery(this IQueryComponent q) => new ParameterizedSql(q);
        public static ParameterizedSql InterpolationToQuery(FormattableString interpolatedQuery) => new InterpolatedSqlFragment(interpolatedQuery).BuildableToQuery();

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

#if DEBUG
            if (string.IsInterned(str) == null) {
                throw new Exception("Interpolated SQL statements must be compile time constants (e.g. do not use FormattableStringFactory!)");
            }
#endif

            var formatStringTokenization = GetFormatStringParamRefs(str);
            var pos = 0;
            foreach (var paramRefMatch in formatStringTokenization) {
                factory.AppendSql(str, pos, paramRefMatch.StartIndex - pos);
                var argument = interpolatedQuery.GetArgument(paramRefMatch.ReferencedParameterIndex);
                if (argument is ParameterizedSql) {
                    ((ParameterizedSql)argument).AppendTo(ref factory);
                } else {
                    QueryComponent.AppendParamTo(ref factory, argument);
                }
                pos = paramRefMatch.EndIndex;
            }
            factory.AppendSql(str, pos, str.Length - pos);
        }

        static readonly ConcurrentDictionary<string, ParamRefSubString[]> parsedFormatStrings
            = new ConcurrentDictionary<string, ParamRefSubString[]>(new ReferenceEqualityComparer<string>());

        static ParamRefSubString[] GetFormatStringParamRefs(string formatstring) => parsedFormatStrings.GetOrAdd(formatstring, ParseFormatString_Delegate);
        static readonly Func<string, ParamRefSubString[]> ParseFormatString_Delegate = ParseFormatString;
        static ParamRefSubString[] ParseFormatString(string formatstring)
        {
            var arrayBuilder = FastArrayBuilder<ParamRefSubString>.Create();
            var pos = 0;
            var strLen = formatstring.Length;
            while (true) {
                var paramRefMatch = ParamRefNextMatch(formatstring, pos, strLen);
                if (paramRefMatch.WasNotFound()) {
                    return arrayBuilder.ToArray();
                }
                arrayBuilder.Add(paramRefMatch);
                pos = paramRefMatch.EndIndex;
            }
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
