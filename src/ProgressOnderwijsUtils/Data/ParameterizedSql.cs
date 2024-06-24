namespace ProgressOnderwijsUtils;

// ReSharper disable once StructCanBeMadeReadOnly - less efficient and pointless, because there's just 1 member!
/// <summary>
/// Represents a string of SQL including parameter values.
/// </summary>
[DebuggerDisplay("{DebugText()}")]
public struct ParameterizedSql
{
    internal readonly ISqlComponent? impl;

    internal ParameterizedSql(ISqlComponent? impl)
        => this.impl = impl;

    internal void AppendTo<TCommandFactory>(ref TCommandFactory factory)
        where TCommandFactory : struct, ICommandFactory
        => impl?.AppendTo(ref factory);

    /// <summary>
    /// Converts this parameterized sql statement into an sql command.
    /// The underlying SqlCommand is pooled for performance; if the provided ReusableCommand is disposed, then the SqlCommand may be reused.
    /// </summary>
    public ReusableCommand CreateSqlCommand(SqlConnection conn, CommandTimeout timeout)
    {
        var factory = CommandFactory.Create();
        impl?.AppendTo(ref factory);
        return factory.FinishBuilding(conn, timeout);
    }

    public static readonly ParameterizedSql TruthyEmpty = new(new StringSqlFragment(""));

    public bool IsEmpty
        => impl == TruthyEmpty.impl || this == EmptySql;

    public static implicit operator ParameterizedSql(bool present)
        => present ? TruthyEmpty : EmptySql;

    /// <summary>
    /// Returns the provided sql only when the condition is true; empty otherwise.
    /// </summary>
    public static ParameterizedSql operator &(ParameterizedSql a, ParameterizedSql b)
        => a.impl != null ? b : a;

    /// <summary>
    /// Returns the provided sql only when the condition is true; empty otherwise.
    /// </summary>
    public static ParameterizedSql operator |(ParameterizedSql a, ParameterizedSql b)
        => a.impl != null ? a : b;

    /// <summary>
    /// Whether this sql fragment is not Empty (i.e. is non-empty or is TruthyEmpty)
    /// </summary>
    public static bool operator true(ParameterizedSql a)
        => a.impl != null;

    /// <summary>
    /// Whether this sql fragment is Empty (i.e. contains no content as is not TruthyEmpty)
    /// </summary>
    public static bool operator false(ParameterizedSql a)
        => a.impl == null;

    /// <summary>
    /// Concatenates two sql fragments.
    /// </summary>
    [Pure]
    public static ParameterizedSql operator +(ParameterizedSql a, ParameterizedSql b)
        => (a.impl == null || b.impl == null ? a.impl ?? b.impl : new TwoSqlFragments(a.impl, b.impl)).BuildableToQuery();

    [Obsolete("Avoid implicitly converting ParameterizedSql to string; this does not result in valid SQL")]
    public static string operator +(string a, ParameterizedSql b)
        => a + (object)b;

    [Obsolete("Avoid implicitly converting ParameterizedSql to string; this does not result in valid SQL")]
    public static string operator +(ParameterizedSql a, string b)
        => (object)a + b;

    public static ParameterizedSql RawSql_PotentialForSqlInjection(string rawSqlString)
        => rawSqlString switch {
            null => throw new ArgumentNullException(nameof(rawSqlString)),
            "" => EmptySql,
            _ => new StringSqlFragment(rawSqlString).BuildableToQuery()
        };

    static bool ValidInitialIdentifierChar(char c) //https://learn.microsoft.com/en-us/sql/relational-databases/databases/database-identifiers
        => c
            is >= 'a' and <= 'z'
            or >= 'A' and <= 'Z'
            or '_' or '#' or '@';

    static bool ValidSubsequentIdentifierChar(char c) //https://learn.microsoft.com/en-us/sql/relational-databases/databases/database-identifiers
        => ValidInitialIdentifierChar(c)
            || c is >= '0' and <= '9' or '$';

    /// <summary>
    /// raw sql string, throws exception if the identifier does not follow https://learn.microsoft.com/en-us/sql/relational-databases/databases/database-identifiers rules.
    /// </summary>
    public static ParameterizedSql UnescapedSqlIdentifier(string rawSqlString)
    {
        ArgumentNullException.ThrowIfNull(nameof(rawSqlString));
        AssertIsIdentifier(rawSqlString, 0);
        return new StringSqlFragment(rawSqlString).BuildableToQuery();
    }
    /// <summary>
    /// raw sql string, throws exception if the identifier does not follow https://learn.microsoft.com/en-us/sql/relational-databases/databases/database-identifiers rules.
    /// </summary>
    public static ParameterizedSql AssertQualifiedSqlIdentifier(string rawSqlString)
    {
        if (rawSqlString == null) {
            throw new ArgumentNullException(nameof(rawSqlString));
        }
        if (rawSqlString.Length < 3) {
            throw new("A qualified SQL identifier must have a qualifier, a dot, and a name");
        }

        var index = 0;
        AssertIsInitialChar(rawSqlString, index);

        while (index < rawSqlString.Length) {
            if (rawSqlString[index] == '.') {
                index++;
                break;
            }
            AssertIsSubsequentChar(rawSqlString, index);
            index++;
        }

        if (index == rawSqlString.Length) {
            throw new("Missing '.' in qualified name");
        }

        AssertIsIdentifier(rawSqlString, index);
        return new StringSqlFragment(rawSqlString).BuildableToQuery();
    }

    static void AssertIsIdentifier(string rawSqlString, int index)
    {
        if (rawSqlString.Length == 0) {
            throw new("A qualified SQL identifier must have at least one char");
        }

        AssertIsInitialChar(rawSqlString, index);

        index++;
        for (; index < rawSqlString.Length; index++) {
            AssertIsSubsequentChar(rawSqlString, index);
        }
    }

    static void AssertIsSubsequentChar(string rawSqlString, int index)
    {
        if (!ValidSubsequentIdentifierChar(rawSqlString[index])) {
            ThrowInvalidIdentifierChar(rawSqlString, index);
        }
    }

    static void AssertIsInitialChar(string rawSqlString, int index)
    {
        if (!ValidInitialIdentifierChar(rawSqlString[index])) {
            ThrowInvalidIdentifierChar(rawSqlString, index);
        }
    }

    static void ThrowInvalidIdentifierChar(string rawSqlString, int index)
        => throw new($"Invalid SQL identifier @ index {index} ({rawSqlString[index]}): {rawSqlString}");



    public static ParameterizedSql EscapedSqlObjectName(string objectName)
        => RawSql_PotentialForSqlInjection("[" + objectName.Replace("]", "]]") + "]");

    public static ParameterizedSql EscapedLiteralString(string literalStringValue) // Escapen van quotes lijkt voldoende: http://stackoverflow.com/questions/10476252.
        => RawSql_PotentialForSqlInjection("'" + literalStringValue.Replace("'", "''") + "'");

    public static ParameterizedSql LiteralSqlNumericString(int value)
        => RawSql_PotentialForSqlInjection(value.ToStringInvariant());

    [Pure]
    public static ParameterizedSql FromSqlInterpolated(FormattableString interpolatedQuery)
        => interpolatedQuery.Format == "" ? EmptySql : new FormattableStringSqlComponent(interpolatedQuery).BuildableToQuery();

    [Pure]
    public override bool Equals(object? obj)
        => obj is ParameterizedSql parameterizedSql && parameterizedSql == this;

    [Pure]
    public static bool operator ==(ParameterizedSql a, ParameterizedSql b)
        => a.impl == b.impl
            || a.impl != TruthyEmpty.impl && b.impl != TruthyEmpty.impl && EqualityKeyCommandFactory.EqualityKey(a.impl).Equals(EqualityKeyCommandFactory.EqualityKey(b.impl));

    [Pure]
    public bool Equals(ParameterizedSql other)
        => this == other;

    [Pure]
    public static bool operator !=(ParameterizedSql a, ParameterizedSql b)
        => !(a == b);

    [Pure]
    public override int GetHashCode()
        => EqualityKeyCommandFactory.EqualityKey(impl).GetHashCode();

    //ToString is constructed to be invalid sql, so that accidental string-concat doesn't result in something that looks reasonable to execute.
    public override string ToString()
        => $"*/Pseudo-sql (with parameter values inlined!):/*\n{DebugText()}";

    public string DebugText()
        => DebugCommandFactory.DebugTextFor(impl);

    [Pure]
    public string CommandText()
    {
        var factory = CommandFactory.Create();
        impl?.AppendTo(ref factory);
        return factory.FinishBuilding_CommandTextOnly();
    }

    public static ParameterizedSql Param(object? paramVal)
        => new SingleParameterSqlFragment(paramVal).BuildableToQuery();

    [Pure]
    public static ParameterizedSql TableParamDynamic(Array o)
        => SqlParameterComponent.ToTableValuedParameterFromPlainValues(o).BuildableToQuery();

    /// <summary>
    /// Adds a parameter to the query with a table-value.
    /// 
    /// You need to define a corresponding user-defined-table-type in the database (see QueryComponent.ToTableParameter for details).
    /// </summary>
    /// <param name="typeName">name of the db-type e.g. IntValues</param>
    /// <param name="objects">the list of pocos with shape corresponding to the DB type</param>
    /// <returns>a composable query-component</returns>
    [Pure]
    public static ParameterizedSql TableParam<T>(string typeName, T[] objects)
        where T : IReadImplicitly, new()
        => objects.Length == 1
            ? new SingletonQueryTableValuedParameterComponent<T>(objects[0]).BuildableToQuery()
            : new QueryTableValuedParameterComponent<T, T>(typeName, objects, arr => (T[])arr).BuildableToQuery();

    public static IReadOnlyDictionary<Type, string> BuiltInTabledValueTypes
        => SqlParameterComponent.CustomTableType.SqlTableTypeNameByDotnetType;

    public static ParameterizedSql TableValuedTypeDefinitionScripts
        => SqlParameterComponent.CustomTableType.DefinitionScripts;
}

interface ISqlComponent
{
    void AppendTo<TCommandFactory>(ref TCommandFactory factory)
        where TCommandFactory : struct, ICommandFactory;
}

sealed class StringSqlFragment : ISqlComponent
{
    readonly string rawSqlString;

    public StringSqlFragment(string rawSqlString)
        => this.rawSqlString = rawSqlString;

    public void AppendTo<TCommandFactory>(ref TCommandFactory factory)
        where TCommandFactory : struct, ICommandFactory
        => ParameterizedSqlFactory.AppendSql(ref factory, rawSqlString);
}

sealed class SingleParameterSqlFragment : ISqlComponent
{
    readonly object? paramVal;

    public SingleParameterSqlFragment(object? paramVal)
        => this.paramVal = paramVal;

    public void AppendTo<TCommandFactory>(ref TCommandFactory factory)
        where TCommandFactory : struct, ICommandFactory
        => SqlParameterComponent.AppendParamOrFragment(ref factory, paramVal);
}

interface IQueryParameter
{
    void ToSqlParameter(ref SqlParamArgs paramArgs);
    object EquatableValue { get; }
}

public static class SafeSql
{
    [Pure]
    public static ParameterizedSql SQL(InterpolatedSqlFragment interpolatedQuery)
        => interpolatedQuery.ToComponent();

    [Pure]
    public static SqlParam AsSqlParam(object? Value)
        => new(Value);

    /// <summary>
    /// The empty sql string.
    /// </summary>
    public static ParameterizedSql EmptySql
        => new();
}

public readonly record struct SqlParam(object? Value);

static class ParameterizedSqlFactory
{
    public static ParameterizedSql BuildableToQuery(this ISqlComponent? q)
        => new(q);

    public static void AppendSql<TCommandFactory>(ref TCommandFactory factory, ReadOnlySpan<char> sql)
        where TCommandFactory : struct, ICommandFactory
        => factory.AppendSql(sql);
}

sealed class TwoSqlFragments : ISqlComponent
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
        factory.AppendSql(" ");
        b.AppendTo(ref factory);
    }
}

sealed class SeveralSqlFragments : ISqlComponent
{
    readonly ISqlComponent[]? kids;

    public SeveralSqlFragments(ISqlComponent[]? kids)
        => this.kids = kids;

    public void AppendTo<TCommandFactory>(ref TCommandFactory factory)
        where TCommandFactory : struct, ICommandFactory
    {
        if (kids == null || kids.Length == 0) {
            return;
        }
        kids[0].AppendTo(ref factory);
        for (var index = 1; index < kids.Length; index++) {
            factory.AppendSql(" ");
            kids[index].AppendTo(ref factory);
        }
    }
}

sealed class FormattableStringSqlComponent : ISqlComponent
{
    readonly FormattableString interpolatedQuery;

    public FormattableStringSqlComponent(FormattableString interpolatedQuery)
        => this.interpolatedQuery = interpolatedQuery;

    public void AppendTo<TCommandFactory>(ref TCommandFactory factory)
        where TCommandFactory : struct, ICommandFactory
    {
        if (interpolatedQuery == null) {
            throw new InvalidOperationException($"{nameof(interpolatedQuery)} is null");
        }

        var str = interpolatedQuery.Format;

#if DEBUG
        if (string.IsInterned(str) == null) {
            throw new("Interpolated SQL statements must be compile time constants (e.g. do not use FormattableStringFactory!)");
        }
#endif

        var formatStringTokenization = GetFormatStringParamRefs(str);
        var pos = 0;
        foreach (var paramRefMatch in formatStringTokenization) {
            factory.AppendSql(str.AsSpan(pos, paramRefMatch.StartIndex - pos));
            var argument = interpolatedQuery.GetArgument(paramRefMatch.ReferencedParameterIndex);
            SqlParameterComponent.AppendParamOrFragment(ref factory, argument);
            pos = paramRefMatch.EndIndex;
        }
        factory.AppendSql(str.AsSpan(pos, str.Length - pos));
    }

    static readonly ConcurrentDictionary<string, ParamRefSubString[]> parsedFormatStrings = new(new ReferenceEqualityComparer<string>());

    static ParamRefSubString[] GetFormatStringParamRefs(string formatstring)
        => parsedFormatStrings.GetOrAdd(formatstring, ParseFormatString_Delegate);

    static readonly Func<string, ParamRefSubString[]> ParseFormatString_Delegate = ParseFormatString;

    static ParamRefSubString[] ParseFormatString(string formatstring)
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
                    if (c is >= '0' and <= '9') {
                        num = num * 10 + (c - '0');
                    } else if (c == '}') {
                        return new() {
                            StartIndex = startPos,
                            EndIndex = pos + 1,
                            ReferencedParameterIndex = num,
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

        public bool WasNotFound()
            => ReferencedParameterIndex < 0;

        public static readonly ParamRefSubString NotFound = new() { ReferencedParameterIndex = -1, };
    }
}

[InterpolatedStringHandler]
public ref struct InterpolatedSqlFragment
{
    readonly (string prefix, object? arg)[] sqlArgs;
    bool justAppendedSql;
    int idx = 0;

    public InterpolatedSqlFragment(int literalLength, int formattedCount)
    {
        sqlArgs = new (string prefix, object?)[formattedCount + 1];
    }

    public void AppendLiteral(string s)
    {
        if (justAppendedSql) {
            throw new("appending literals twice in succession makes no sense!");
        }
        justAppendedSql = true;
        sqlArgs[idx].prefix = s;
    }

    public void AppendFormatted(ParameterizedSql t)
        => AppendParam(t);

    public void AppendFormatted(INestableSql t)
        => AppendParam(t.Sql);

    public void AppendFormatted(IHasValueConverter? t)
        => AppendParam(t);

    public void AppendFormatted(Enum? t)
        => AppendParam(t);

    public void AppendFormatted<T>(IEnumerable<T> t)
        where T : Enum
        => AppendParam(t);

    public void AppendFormatted(IEnumerable<Guid> t)
        => AppendParam(t);

    public void AppendFormatted(IEnumerable<int> t)
        => AppendParam(t);

    public void AppendFormatted(IEnumerable<IHasValueConverter> t)
        => AppendParam(t);

    public void AppendFormatted(IEnumerable<string> t)
        => AppendParam(t);

    public void AppendFormatted(IEnumerable<DateTime> t)
        => AppendParam(t);

    public void AppendFormatted(IEnumerable<double> t)
        => AppendParam(t);

    public void AppendFormatted(IEnumerable<byte[]> t)
        => AppendParam(t);

    public void AppendFormatted(bool t)
        => AppendParam(t);

    public void AppendFormatted(SqlParam t)
        => AppendParam(t.Value);

    public void AppendFormatted(byte[]? t)
        => AppendParam(t);

    public void AppendFormatted(Guid? t)
        => AppendParam(t);

    public void AppendFormatted(char? t)
        => AppendParam(t);

    public void AppendFormatted(int? t)
        => AppendParam(t);

    public void AppendFormatted(DateTime? t)
        => AppendParam(t);

    public void AppendFormatted(DateOnly? t)
        => AppendParam(t);

    public void AppendFormatted(decimal? t)
        => AppendParam(t);

    public void AppendFormatted(string? t)
        => AppendParam(t);

    public void AppendFormatted(double? t)
        => AppendParam(t);

    public void AppendFormatted(long? t)
        => AppendParam(t);

    public void AppendFormatted(ulong? t)
        => AppendParam(t);

    public void AppendFormatted(uint? t)
        => AppendParam(t);

    public void AppendFormatted(CurrentTimeToken t)
        => AppendParam(t);

    void AppendParam(object? t)
    {
        if (!justAppendedSql) {
            sqlArgs[idx].prefix = "";
        }
        sqlArgs[idx].arg = t;
        idx++;
        justAppendedSql = false;
    }

    internal ParameterizedSql ToComponent()
        => idx == 0 && !justAppendedSql ? EmptySql : new InterpolatedSqlComponent(sqlArgs, justAppendedSql).BuildableToQuery();

    sealed class InterpolatedSqlComponent : ISqlComponent
    {
        readonly (string prefix, object? arg)[] sqlArgs;
        readonly bool endsWithSql;

        public InterpolatedSqlComponent((string prefix, object? arg)[] sqlArgs, bool endsWithSql)
        {
            this.sqlArgs = sqlArgs;
            this.endsWithSql = endsWithSql;
        }

        public void AppendTo<TCommandFactory>(ref TCommandFactory factory)
            where TCommandFactory : struct, ICommandFactory
        {
            var segmentIdx = 0;
            var realLen = endsWithSql ? sqlArgs.Length : sqlArgs.Length - 1;
            while (true) {
                if (segmentIdx >= realLen) {
                    break;
                }
                var sqlLiteral = sqlArgs[segmentIdx].prefix;
                if (sqlLiteral is not "") {
                    factory.AppendSql(sqlLiteral);
                }
                if (segmentIdx + 1 >= sqlArgs.Length && endsWithSql) {
                    break;
                }

                SqlParameterComponent.AppendParamOrFragment(ref factory, sqlArgs[segmentIdx].arg);
                segmentIdx++;
            }
        }
    }
}
