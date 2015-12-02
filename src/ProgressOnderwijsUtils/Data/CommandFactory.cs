using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
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

        public void Dispose()
        {
            if (Command != null) {
                PooledSqlCommandAllocator.ReturnToPool(Command);
                Command = null;
            }
        }
    }

    /// <summary>
    /// Mutable value type - do not make copies!
    /// </summary>
    struct CommandFactory : ICommandFactory
    {
        static readonly ConcurrentQueue<Dictionary<object, string>> nameLookupBag = new ConcurrentQueue<Dictionary<object, string>>();
        FastShortStringBuilder queryText;
        SqlParamArgs[] paramObjs;
        int paramCount;
        Dictionary<object, string> lookup;

        public static CommandFactory Create()
        {
            var retval = new CommandFactory();
            retval.queryText = FastShortStringBuilder.Create();
            retval.paramObjs = PooledExponentialBufferAllocator<SqlParamArgs>.GetByLength(16);
            retval.paramCount = 0;
            if (!nameLookupBag.TryDequeue(out retval.lookup)) {
                retval.lookup = new Dictionary<object, string>(8);
            }
            return retval;
        }

        public ReusableCommand CreateCommand(SqlConnection conn, int commandTimeout)
        {
            var command = PooledSqlCommandAllocator.GetByLength(paramCount);
            command.Connection = conn;
            command.CommandTimeout = commandTimeout; //60 by default
            command.CommandText = queryText.Finish();
            var cmdParams = command.Parameters;
            for (int i = 0; i < paramCount; i++) {
                cmdParams[i].Value = paramObjs[i].Value;
                cmdParams[i].IsNullable = paramObjs[i].Value == DBNull.Value;
                if (paramObjs[i].TypeName != null) {
                    cmdParams[i].SqlDbType = SqlDbType.Structured;
                    cmdParams[i].TypeName = paramObjs[i].TypeName;
                }
            }

            PooledExponentialBufferAllocator<SqlParamArgs>.ReturnToPool(paramObjs);
            paramObjs = null;

            lookup.Clear();
            nameLookupBag.Enqueue(lookup);
            lookup = null;

            return new ReusableCommand { Command = command };
        }

        const int ParameterNameCacheSize = 20;

        static readonly string[] CachedParameterNames =
            Enumerable.Range(0, ParameterNameCacheSize).Select(IndexToParameterName).ToArray();

        public static string IndexToParameterName(int parameterIndex) => "@par" + parameterIndex.ToStringInvariant();

        public string RegisterParameterAndGetName<T>(T o)
            where T : IQueryParameter
        {
            string paramName;
            if (!lookup.TryGetValue(o.EquatableValue, out paramName)) {
                var parameterIndex = lookup.Count;
                paramName = parameterIndex < CachedParameterNames.Length
                    ? CachedParameterNames[parameterIndex]
                    : IndexToParameterName(parameterIndex);
                if (paramObjs.Length == paramCount) {
                    var newArray = PooledExponentialBufferAllocator<SqlParamArgs>.GetByLength((uint)paramCount * 2);
                    Array.Copy(paramObjs, newArray, paramCount);
                    Array.Clear(paramObjs, 0, paramCount);
                    PooledExponentialBufferAllocator<SqlParamArgs>.ReturnToPool(paramObjs);
                    paramObjs = newArray;
                }
                o.ToSqlParameter(ref paramObjs[paramCount]);
                paramCount++;
                lookup.Add(o.EquatableValue, paramName);
            }
            return paramName;
        }

        public void AppendSql(string sql, int startIndex, int length) => queryText.AppendText(sql, startIndex, length);
    }

    /// <summary>
    /// faster than StringBuilder since we don't need insert-in-the-middle capability and can reuse this memory
    /// </summary>
    struct FastShortStringBuilder
    {
        char[] charBuffer;
        int queryLen;
        public static FastShortStringBuilder Create() => new FastShortStringBuilder { charBuffer = PooledExponentialBufferAllocator<char>.GetByLength(2048) };

        public void AppendText(string text, int startIndex, int length)
        {
            if (charBuffer.Length < queryLen + length) {
                var newLen = (uint)Math.Max(charBuffer.Length * 3 / 2, queryLen + length + 5);
                var newArray = PooledExponentialBufferAllocator<char>.GetByLength(newLen);
                Array.Copy(charBuffer, newArray, queryLen);
                PooledExponentialBufferAllocator<char>.ReturnToPool(charBuffer);
                charBuffer = newArray;
            }
            text.CopyTo(startIndex, charBuffer, queryLen, length);
            queryLen += length;
        }

        public string Finish()
        {
            var retval = new string(charBuffer, 0, queryLen);
            PooledExponentialBufferAllocator<char>.ReturnToPool(charBuffer);
            charBuffer = null;
            return retval;
        }
    }

    struct DebugCommandFactory : ICommandFactory
    {
        FastShortStringBuilder debugText;
        public string RegisterParameterAndGetName<T>(T o) where T : IQueryParameter => QueryTracer.InsecureSqlDebugString(o.EquatableValue);
        public void AppendSql(string sql, int startIndex, int length) => debugText.AppendText(sql, startIndex, length);

        public static string DebugTextFor(IQueryComponent impl)
        {
            var factory = new DebugCommandFactory { debugText = FastShortStringBuilder.Create() };
            impl?.AppendTo(ref factory);
            return factory.debugText.Finish();
        }
    }

    struct EqualityKeyCommandFactory : ICommandFactory
    {
        FastShortStringBuilder debugText;
        int argOffset;
        FastArrayBuilder<object> paramValues;

        public string RegisterParameterAndGetName<T>(T o) where T : IQueryParameter
        {
            paramValues.Add(o.EquatableValue);
            return CommandFactory.IndexToParameterName(argOffset++);
        }

        public void AppendSql(string sql, int startIndex, int length) => debugText.AppendText(sql, startIndex, length);

        public static QueryKey EqualityKey(IQueryComponent impl)
        {
            var factory = new EqualityKeyCommandFactory {
                debugText = FastShortStringBuilder.Create(),
                argOffset = 0,
                paramValues = FastArrayBuilder<object>.Create(),
            };
            impl?.AppendTo(ref factory);
            return new QueryKey {
                SqlTextKey = factory.debugText.Finish(),
                Params = new ComparableArray<object>(factory.paramValues.ToArray()),
            };
        }
    }

    sealed class QueryKey : ValueBase<QueryKey>
    {
        public string SqlTextKey { get; set; }
        public ComparableArray<object> Params { get; set; }
    }

    static class PooledSqlCommandAllocator
    {
        static readonly int IndexCount = 129;
        static readonly int MaxArrayLength = IndexCount - 1;

        static readonly ConcurrentQueue<SqlCommand>[] bagsByIndex = new ConcurrentQueue<SqlCommand>[IndexCount];

        static ConcurrentQueue<SqlCommand> GetBag(int index)
        {
            var bag = bagsByIndex[index];
            if (bag != null)
                return bag;
            var otherBag = Interlocked.CompareExchange(ref bagsByIndex[index], bag = new ConcurrentQueue<SqlCommand>(), null);
            //CompareExchange returns previous value
            //if that wasn't null, this was a no-op and that value shold be returned
            //if that *was* null, we set that location, and return our bag
            return otherBag ?? bag;
        }

        public static SqlCommand GetByLength(int length)
        {
            if (length <= MaxArrayLength) {
                var bag = GetBag(length);
                SqlCommand result;
                if (bag.TryDequeue(out result))
                    return result;
            }
            var cmd = new SqlCommand();
            var parameters = new SqlParameter[length];
            for (int i = 0; i < length; i++)
                parameters[i] = new SqlParameter { ParameterName = CommandFactory.IndexToParameterName(i) };
            cmd.Parameters.AddRange(parameters);
            return cmd;
        }

        public static void ReturnToPool(SqlCommand cmd)
        {
            var parameters = cmd.Parameters;
            var parameterCount = parameters.Count;
            if (parameterCount > MaxArrayLength)
                return;
            var bag = GetBag(parameterCount);
            for (int i = 0; i < parameterCount; i++) {
                parameters[i].Value = DBNull.Value;
                parameters[i].TypeName = null;
            }
            cmd.CommandText = null;
            cmd.Connection = null;
            bag.Enqueue(cmd);
        }
    }
}
