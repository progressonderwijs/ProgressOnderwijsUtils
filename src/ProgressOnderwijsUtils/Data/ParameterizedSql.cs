using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
    /// <summary>
    /// Represents a string of SQL including parameter values.
    /// </summary>
    public struct ParameterizedSql
    {
        internal readonly ISqlComponent impl;

        internal ParameterizedSql(ISqlComponent impl)
        {
            this.impl = impl;
        }

        internal void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory
            => impl?.AppendTo(ref factory);

        /// <summary>
        /// Converts this parameterized sql statement into an sql command.
        /// The underlying SqlCommand is pooled for performance; if the provided ReusableCommand is disposed, then the SqlCommand may be reused.
        /// </summary>
        public ReusableCommand CreateSqlCommand([NotNull] SqlCommandCreationContext conn)
        {
            var factory = CommandFactory.Create();
            impl?.AppendTo(ref factory);
            return factory.FinishBuilding(conn);
        }

        /// <summary>
        /// The empty sql string.
        /// </summary>
        public static ParameterizedSql Empty => default;

        public static readonly ParameterizedSql TruthyEmpty = new ParameterizedSql(new StringSqlFragment(""));
        public bool IsEmpty => impl == TruthyEmpty.impl || this == Empty;
        public static implicit operator ParameterizedSql(bool present) => present ? TruthyEmpty : Empty;

        /// <summary>
        /// Returns the provided sql only when the condition is true; empty otherwise.
        /// </summary>
        public static ParameterizedSql operator &(ParameterizedSql a, ParameterizedSql b) => a.impl != null ? b : a;

        /// <summary>
        /// Returns the provided sql only when the condition is true; empty otherwise.
        /// </summary>
        public static ParameterizedSql operator |(ParameterizedSql a, ParameterizedSql b) => a.impl != null ? a : b;

        /// <summary>
        /// Whether this sql fragment is not Empty (i.e. is non-empty or is TruthyEmpty)
        /// </summary>
        public static bool operator true(ParameterizedSql a) => a.impl != null;

        /// <summary>
        /// Whether this sql fragment is Empty (i.e. contains no content as is not TruthyEmpty)
        /// </summary>
        public static bool operator false(ParameterizedSql a) => a.impl == null;

        /// <summary>
        /// Concatenates two sql fragments.
        /// </summary>
        [Pure]
        public static ParameterizedSql operator +(ParameterizedSql a, ParameterizedSql b)
            => (a.impl == null || b.impl == null ? a.impl ?? b.impl : new TwoSqlFragments(a.impl, b.impl)).BuildableToQuery();

        public static ParameterizedSql CreateDynamic([NotNull] string rawSqlString)
        {
            if (rawSqlString == null) {
                throw new ArgumentNullException(nameof(rawSqlString));
            }
            if (rawSqlString == "") {
                return Empty;
            }
            return new StringSqlFragment(rawSqlString).BuildableToQuery();
        }

        [Pure]
        public override bool Equals(object obj)
            => obj is ParameterizedSql && (ParameterizedSql)obj == this;

        [Pure]
        public static bool operator ==(ParameterizedSql a, ParameterizedSql b)
            => a.impl == b.impl
                || a.impl != TruthyEmpty.impl && b.impl != TruthyEmpty.impl && EqualityKeyCommandFactory.EqualityKey(a.impl).Equals(EqualityKeyCommandFactory.EqualityKey(b.impl));

        [Pure]
        public bool Equals(ParameterizedSql other) => this == other;

        [Pure]
        public static bool operator !=(ParameterizedSql a, ParameterizedSql b) => !(a == b);

        [Pure]
        public override int GetHashCode() => EqualityKeyCommandFactory.EqualityKey(impl).GetHashCode();

        //ToString is constructed to be invalid sql, so that accidental string-concat doesn't result in something that looks reasonable to execute.
        public override string ToString() => "*/Pseudo-sql (with parameter values inlined!):/*\r\n" + DebugText();

        [NotNull]
        public string DebugText() => DebugCommandFactory.DebugTextFor(impl);

        [NotNull]
        [Pure]
        public string CommandText()
        {
            var factory = CommandFactory.Create();
            impl?.AppendTo(ref factory);
            return factory.FinishBuilding_CommandTextOnly();
        }

        public static ParameterizedSql Param(object paramVal) => new SingleParameterSqlFragment(paramVal).BuildableToQuery();

        [Pure]
        public static ParameterizedSql TableParamDynamic([NotNull] Array o) => SqlParameterComponent.ToTableValuedParameterFromPlainValues(o).BuildableToQuery();

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
            => SqlParameterComponent.ToTableValuedParameter(typeName, objects, o => (T[])o).BuildableToQuery();

        public static IReadOnlyDictionary<Type, string> BuiltInTabledValueTypes => SqlParameterComponent.CustomTableType.SqlTableTypeNameByDotnetType;
        public static ParameterizedSql TableValuedTypeDefinitionScripts => SqlParameterComponent.CustomTableType.DefinitionScripts;
    }

    interface ISqlComponent
    {
        void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory;
    }

    class StringSqlFragment : ISqlComponent
    {
        readonly string rawSqlString;

        public StringSqlFragment(string rawSqlString)
        {
            this.rawSqlString = rawSqlString;
        }

        public void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory
            => ParameterizedSqlFactory.AppendSql(ref factory, rawSqlString);
    }

    class SingleParameterSqlFragment : ISqlComponent
    {
        readonly object paramVal;

        public SingleParameterSqlFragment(object paramVal)
        {
            this.paramVal = paramVal;
        }

        public void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory
            => SqlParameterComponent.AppendParamTo(ref factory, paramVal);
    }

    interface IQueryParameter
    {
        void ToSqlParameter(ref SqlParamArgs paramArgs);
        object EquatableValue { get; }
    }

    public static class SafeSql
    {
        [Pure]
        public static ParameterizedSql SQL([NotNull] FormattableString interpolatedQuery) => ParameterizedSqlFactory.InterpolationToQuery(interpolatedQuery);
    }

    static class ParameterizedSqlFactory
    {
        public static ParameterizedSql BuildableToQuery(this ISqlComponent q) => new ParameterizedSql(q);

        public static ParameterizedSql InterpolationToQuery([NotNull] FormattableString interpolatedQuery) =>
            interpolatedQuery.Format == "" ? ParameterizedSql.Empty :
                new InterpolatedSqlFragment(interpolatedQuery).BuildableToQuery();

        public static void AppendSql<TCommandFactory>(ref TCommandFactory factory, [NotNull] string sql)
            where TCommandFactory : struct, ICommandFactory
            => factory.AppendSql(sql, 0, sql.Length);
    }

    class TwoSqlFragments : ISqlComponent
    {
        readonly ISqlComponent a, b;

        public TwoSqlFragments(ISqlComponent a, ISqlComponent b)
        {
            this.a = a;
            this.b = b;
        }

        public void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory
        {
            a.AppendTo(ref factory);
            factory.AppendSql(" ", 0, 1);
            b.AppendTo(ref factory);
        }
    }

    class SeveralSqlFragments : ISqlComponent
    {
        readonly ISqlComponent[] kids;

        public SeveralSqlFragments(ISqlComponent[] kids)
        {
            this.kids = kids;
        }

        public void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory
        {
            if (kids == null || kids.Length == 0)
                return;
            kids[0].AppendTo(ref factory);
            for (var index = 1; index < kids.Length; index++) {
                factory.AppendSql(" ", 0, 1);
                kids[index].AppendTo(ref factory);
            }
        }
    }


    class InterpolatedSqlFragment : ISqlComponent
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
                    SqlParameterComponent.AppendParamTo(ref factory, argument);
                }
                pos = paramRefMatch.EndIndex;
            }
            factory.AppendSql(str, pos, str.Length - pos);
        }

        static readonly ConcurrentDictionary<string, ParamRefSubString[]> parsedFormatStrings
            = new ConcurrentDictionary<string, ParamRefSubString[]>(new ReferenceEqualityComparer<string>());

        static ParamRefSubString[] GetFormatStringParamRefs([NotNull] string formatstring) => parsedFormatStrings.GetOrAdd(formatstring, ParseFormatString_Delegate);
        static readonly Func<string, ParamRefSubString[]> ParseFormatString_Delegate = ParseFormatString;

        static ParamRefSubString[] ParseFormatString([NotNull] string formatstring)
        {
            var arrayBuilder = new ArrayBuilder<ParamRefSubString>();
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
                var c = query[pos];
                if (c == '{') {
                    var startPos = pos;
                    var num = 0;
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
