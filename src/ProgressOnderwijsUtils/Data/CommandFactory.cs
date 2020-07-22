using System;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using ExpressionToCodeLib;
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
        string RegisterParameterAndGetName<T>(T o)
            where T : IQueryParameter;

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
        public IDisposable? QueryTimer;

        public void Dispose()
        {
            QueryTimer?.Dispose();
            if (Command != null) {
                PooledSqlCommandAllocator.ReturnToPool(Command);
                Command = null!;
            }
        }

        internal ParameterizedSqlExecutionException CreateExceptionWithTextAndArguments<TOriginCommand>(Exception innerException, TOriginCommand command, string? extraMessage = null)
            where TOriginCommand : IWithTimeout<TOriginCommand>
            => SqlCommandDebugStringifier.ExceptionWithTextAndArguments(command.GetType().ToCSharpFriendlyTypeName() + " failed" + (extraMessage == null ? "." : ": " + extraMessage), Command, innerException);
    }

    /// <summary>
    /// Mutable value type - do not make copies!
    /// </summary>
    struct CommandFactory : ICommandFactory
    {
        static readonly ConcurrentQueue<Dictionary<object, string>> nameLookupBag = new ConcurrentQueue<Dictionary<object, string>>();
        static readonly ArrayPool<SqlParamArgs> sqlParamsArgsPool = ArrayPool<SqlParamArgs>.Shared;

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
            => new CommandFactory {
                queryText = FastShortStringBuilder.Create(),
                paramObjs = sqlParamsArgsPool.Rent(16),
                paramCount = 0,
                lookup = GetLookup()
            };

        public ReusableCommand FinishBuilding(SqlConnection conn, CommandTimeout timeout)
        {
            var command = PooledSqlCommandAllocator.GetByLength(paramCount);
            command.Connection = conn;
            command.CommandTimeout = timeout.ComputeAbsoluteTimeout(conn);
            command.CommandText = queryText.FinishBuilding();
            var cmdParams = command.Parameters;
            for (var i = 0; i < paramCount; i++) {
                if (paramObjs[i].TypeName != null) {
                    cmdParams[i].SqlDbType = SqlDbType.Structured;
                    cmdParams[i].TypeName = paramObjs[i].TypeName;
                } else if (paramObjs[i].Value is DateTime) {
                    cmdParams[i].SqlDbType = SqlDbType.DateTime2;
                } else {
                    cmdParams[i].ResetSqlDbType();
                }
                cmdParams[i].Value = paramObjs[i].Value;
                cmdParams[i].IsNullable = paramObjs[i].Value == DBNull.Value;
            }

            FreeParamsAndLookup();

            var timer = conn.Tracer()?.StartCommandTimer(command);
            return new ReusableCommand { Command = command, QueryTimer = timer };
        }

        public string FinishBuilding_CommandTextOnly()
        {
            FreeParamsAndLookup();
            return queryText.FinishBuilding();
        }

        void FreeParamsAndLookup()
        {
            Array.Clear(paramObjs, 0, paramCount);
            sqlParamsArgsPool.Return(paramObjs);
            paramObjs = null!;
            lookup.Clear();
            nameLookupBag.Enqueue(lookup);
            lookup = null!;
        }

        const int ParameterNameCacheSize = 100;

        static readonly string[] CachedParameterNames =
            Enumerable.Range(0, ParameterNameCacheSize).Select(parameterIndex => "@par" + parameterIndex).ToArray();

        public static string IndexToParameterName(int parameterIndex)
            => parameterIndex < CachedParameterNames.Length
                ? CachedParameterNames[parameterIndex]
                : "@par" + parameterIndex;

        public string RegisterParameterAndGetName<T>(T o)
            where T : IQueryParameter
        {
            if (!lookup.TryGetValue(o.EquatableValue, out var paramName)) {
                var parameterIndex = lookup.Count;
                paramName = IndexToParameterName(parameterIndex);
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
                var newArray = sqlParamsArgsPool.Rent(paramCount * 2);
                Array.Copy(paramObjs, newArray, paramCount);
                Array.Clear(paramObjs, 0, paramCount);
                sqlParamsArgsPool.Return(paramObjs);
                paramObjs = newArray;
            }
        }

        public void AppendSql(string sql, int startIndex, int length)
            => queryText.AppendText(sql, startIndex, length);
    }

    /// <summary>
    /// faster than StringBuilder since we don't need insert-in-the-middle capability and can reuse this memory
    /// </summary>
    struct FastShortStringBuilder
    {
        public char[] CurrentCharacterBuffer;
        public int CurrentLength;

        public static FastShortStringBuilder Create()
            => new FastShortStringBuilder { CurrentCharacterBuffer = Allocate(4096) };

        static char[] Allocate(int length)
            => ArrayPool<char>.Shared.Rent(length);

        void Free()
            => ArrayPool<char>.Shared.Return(CurrentCharacterBuffer);

        public static FastShortStringBuilder Create(int length)
            => new FastShortStringBuilder { CurrentCharacterBuffer = Allocate(length) };

        public void AppendText(string text)
            => AppendText(text, 0, text.Length);

        public void AppendText(string text, int startIndex, int length)
        {
            if (CurrentCharacterBuffer.Length < CurrentLength + length) {
                var newLen = Math.Max(CurrentCharacterBuffer.Length * 2, CurrentLength + length);
                var newArray = Allocate(newLen);
                Array.Copy(CurrentCharacterBuffer, newArray, CurrentLength);
                Free();
                CurrentCharacterBuffer = newArray;
            }
            text.CopyTo(startIndex, CurrentCharacterBuffer, CurrentLength, length);
            CurrentLength += length;
        }

        public void DiscardBuilder()
        {
            Free();
            CurrentCharacterBuffer = null!;
        }

        public string FinishBuilding()
        {
            var str = new string(CurrentCharacterBuffer, 0, CurrentLength);
            DiscardBuilder();
            return str;
        }
    }

    struct DebugCommandFactory : ICommandFactory
    {
        FastShortStringBuilder debugText;

        public string RegisterParameterAndGetName<T>(T o)
            where T : IQueryParameter
            => SqlCommandDebugStringifier.InsecureSqlDebugString(o.EquatableValue, true);

        public void AppendSql(string sql, int startIndex, int length)
            => debugText.AppendText(sql, startIndex, length);

        public static string DebugTextFor(ISqlComponent? impl)
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

        public string RegisterParameterAndGetName<T>(T o)
            where T : IQueryParameter
        {
            paramValues.Add(o.EquatableValue);
            return CommandFactory.IndexToParameterName(argOffset++);
        }

        public void AppendSql(string sql, int startIndex, int length)
            => debugText.AppendText(sql, startIndex, length);

        public static ParameterizedSqlEquatableKey EqualityKey(ISqlComponent? impl)
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
        public string? SqlTextKey;
        public object[]? Params;

        public bool Equals(ParameterizedSqlEquatableKey other)
            => SqlTextKey == other.SqlTextKey && StructuralComparisons.StructuralEqualityComparer.Equals(Params, other.Params);

        public override bool Equals(object? obj)
            => obj is ParameterizedSqlEquatableKey parameterizedSqlEquatableKey && Equals(parameterizedSqlEquatableKey);

        public override int GetHashCode()
            => (SqlTextKey?.GetHashCode() ?? 0) + 237 * StructuralComparisons.StructuralEqualityComparer.GetHashCode(Params!);
    }
}
