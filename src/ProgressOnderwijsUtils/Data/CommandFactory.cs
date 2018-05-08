using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
    /// <summary>
    /// WARNING: all implementations are mutable value types, for QueryBuilder-internal use only!
    /// 
    /// It is an error to ever copy an ICommandFactory.
    /// </summary>
    interface ICommandFactory
    {
        string RegisterParameterAndGetName<T>(T o) where T : IQueryParameter;
        void AppendSql(string sql, int startIndex, int length);
    }

    struct SqlParamArgs
    {
        public object Value;
        public string TypeName;
    }

    public struct ReusableCommand : IDisposable
    {
        public SqlCommand Command;
        public IDisposable QueryTimer;

        public void Dispose()
        {
            QueryTimer?.Dispose();
            if (Command != null) {
                PooledSqlCommandAllocator.ReturnToPool(Command);
                Command = null;
            }
        }

        [NotNull]
        public ParameterizedSqlExecutionException CreateExceptionWithTextAndArguments([NotNull] string message, Exception innerException)
            => SqlCommandDebugStringifier.ExceptionWithTextAndArguments(message, Command, innerException);
    }

    /// <summary>
    /// Mutable value type - do not make copies!
    /// </summary>
    struct CommandFactory : ICommandFactory
    {
        static readonly ConcurrentQueue<Dictionary<object, string>> nameLookupBag = new ConcurrentQueue<Dictionary<object, string>>();

        static Dictionary<object, string> GetLookup()
        {
            if (nameLookupBag.TryDequeue(out var lookup)) {
                return lookup;
            }
            return new Dictionary<object, string>(8);
        }

        FastShortStringBuilder queryText;
        SqlParamArgs[] paramObjs;
        int paramCount;
        Dictionary<object, string> lookup;

        public static CommandFactory Create()
            =>
                new CommandFactory {
                    queryText = FastShortStringBuilder.Create(),
                    paramObjs = PooledExponentialBufferAllocator<SqlParamArgs>.GetByLength(16),
                    paramCount = 0,
                    lookup = GetLookup()
                };

        public ReusableCommand FinishBuilding([NotNull] SqlCommandCreationContext conn)
        {
            var command = PooledSqlCommandAllocator.GetByLength(paramCount);
            command.Connection = conn.Connection;
            command.CommandTimeout = conn.CommandTimeoutInS;
            command.CommandText = queryText.FinishBuilding();
            var cmdParams = command.Parameters;
            for (var i = 0; i < paramCount; i++) {
                if (paramObjs[i].TypeName != null) {
                    cmdParams[i].SqlDbType = SqlDbType.Structured;
                    cmdParams[i].TypeName = paramObjs[i].TypeName;
                } else {
                    cmdParams[i].ResetSqlDbType();
                }
                cmdParams[i].Value = paramObjs[i].Value;
                cmdParams[i].IsNullable = paramObjs[i].Value == DBNull.Value;
            }

            FreeParamsAndLookup();

            var timer = conn.Tracer?.StartCommandTimer(command);
            return new ReusableCommand { Command = command, QueryTimer = timer };
        }

        [NotNull]
        public string FinishBuilding_CommandTextOnly()
        {
            FreeParamsAndLookup();
            return queryText.FinishBuilding();
        }

        void FreeParamsAndLookup()
        {
            Array.Clear(paramObjs, 0, paramCount);
            PooledExponentialBufferAllocator<SqlParamArgs>.ReturnToPool(paramObjs);
            paramObjs = null;
            lookup.Clear();
            nameLookupBag.Enqueue(lookup);
            lookup = null;
        }

        const int ParameterNameCacheSize = 20;

        static readonly string[] CachedParameterNames =
            Enumerable.Range(0, ParameterNameCacheSize).Select(IndexToParameterName).ToArray();

        [NotNull]
        public static string IndexToParameterName(int parameterIndex) => "@par" + parameterIndex.ToStringInvariant();

        public string RegisterParameterAndGetName<T>([NotNull] T o)
            where T : IQueryParameter
        {
            if (!lookup.TryGetValue(o.EquatableValue, out var paramName)) {
                var parameterIndex = lookup.Count;
                paramName = parameterIndex < CachedParameterNames.Length
                    ? CachedParameterNames[parameterIndex]
                    : IndexToParameterName(parameterIndex);
                EnsureParamsArrayCanGrow();
                o.ToSqlParameter(ref paramObjs[paramCount]);
                paramCount++;
                lookup.Add(o.EquatableValue, paramName);
            }
            return paramName;
        }

        void EnsureParamsArrayCanGrow()
        {
            if (paramObjs.Length == paramCount) {
                var newArray = PooledExponentialBufferAllocator<SqlParamArgs>.GetByLength((uint)paramCount * 2);
                Array.Copy(paramObjs, newArray, paramCount);
                Array.Clear(paramObjs, 0, paramCount);
                PooledExponentialBufferAllocator<SqlParamArgs>.ReturnToPool(paramObjs);
                paramObjs = newArray;
            }
        }

        public void AppendSql([NotNull] string sql, int startIndex, int length) => queryText.AppendText(sql, startIndex, length);
    }

    /// <summary>
    /// faster than StringBuilder since we don't need insert-in-the-middle capability and can reuse this memory
    /// </summary>
    struct FastShortStringBuilder
    {
        char[] charBuffer;
        int queryLen;
        public static FastShortStringBuilder Create() => new FastShortStringBuilder { charBuffer = PooledExponentialBufferAllocator<char>.GetByLength(4096) };
        public static FastShortStringBuilder Create(uint length) => new FastShortStringBuilder { charBuffer = PooledExponentialBufferAllocator<char>.GetByLength(length) };
        public void AppendText([NotNull] string text) => AppendText(text, 0, text.Length);

        public void AppendText([NotNull] string text, int startIndex, int length)
        {
            if (charBuffer.Length < queryLen + length) {
                var newLen = (uint)Math.Max(charBuffer.Length * 2, queryLen + length);
                var newArray = PooledExponentialBufferAllocator<char>.GetByLength(newLen);
                Array.Copy(charBuffer, newArray, queryLen);
                PooledExponentialBufferAllocator<char>.ReturnToPool(charBuffer);
                charBuffer = newArray;
            }
            text.CopyTo(startIndex, charBuffer, queryLen, length);
            queryLen += length;
        }

        public int CurrentLength => queryLen;
        public char[] CurrentCharacterBuffer => charBuffer;

        public void DiscardBuilder()
        {
            PooledExponentialBufferAllocator<char>.ReturnToPool(charBuffer);
            charBuffer = null;
        }

        [NotNull]
        public string FinishBuilding()
        {
            var str = new string(charBuffer, 0, queryLen);
            PooledExponentialBufferAllocator<char>.ReturnToPool(charBuffer);
            charBuffer = null;
            return str;
        }
    }

    struct DebugCommandFactory : ICommandFactory
    {
        FastShortStringBuilder debugText;

        [NotNull]
        public string RegisterParameterAndGetName<T>([NotNull] T o) where T : IQueryParameter => SqlCommandDebugStringifier.InsecureSqlDebugString(o.EquatableValue, true);

        public void AppendSql([NotNull] string sql, int startIndex, int length) => debugText.AppendText(sql, startIndex, length);

        [NotNull]
        public static string DebugTextFor([CanBeNull] ISqlComponent impl)
        {
            var factory = new DebugCommandFactory { debugText = FastShortStringBuilder.Create() };
            impl?.AppendTo(ref factory);
            return factory.debugText.FinishBuilding();
        }
    }

    struct EqualityKeyCommandFactory : ICommandFactory
    {
        FastShortStringBuilder debugText;
        int argOffset;
        ArrayBuilder<object> paramValues;

        [NotNull]
        public string RegisterParameterAndGetName<T>([NotNull] T o) where T : IQueryParameter
        {
            paramValues.Add(o.EquatableValue);
            return CommandFactory.IndexToParameterName(argOffset++);
        }

        public void AppendSql([NotNull] string sql, int startIndex, int length) => debugText.AppendText(sql, startIndex, length);

        public static ParameterizedSqlEquatableKey EqualityKey([CanBeNull] ISqlComponent impl)
        {
            var factory = new EqualityKeyCommandFactory {
                debugText = FastShortStringBuilder.Create(),
                argOffset = 0,
                paramValues = default,
            };
            impl?.AppendTo(ref factory);
            var key = new ParameterizedSqlEquatableKey {
                SqlTextKey = factory.debugText.FinishBuilding(),
                Params = factory.paramValues.ToArray(),
            };
            return key;
        }
    }

    struct ParameterizedSqlEquatableKey : IEquatable<ParameterizedSqlEquatableKey>
    {
        public string SqlTextKey { get; set; }
        public object[] Params { get; set; }

        public bool Equals(ParameterizedSqlEquatableKey other)
            => SqlTextKey == other.SqlTextKey && StructuralComparisons.StructuralEqualityComparer.Equals(Params, other.Params);

        public override bool Equals(object obj) => obj is ParameterizedSqlEquatableKey && Equals((ParameterizedSqlEquatableKey)obj);
        public override int GetHashCode() => (SqlTextKey?.GetHashCode() ?? 0) + 237 * StructuralComparisons.StructuralEqualityComparer.GetHashCode(Params);
    }
}
