using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
    interface ICommandFactory
    {
        string RegisterParameterAndGetName<T>(T o) where T : IQueryParameter;
        void AppendSql(string sql, int startIndex, int length);
    }

    struct CommandFactory : ICommandFactory
    {
        char[] queryText; //faster than StringBuilder since we're append only and know the size quite reliably;
        int queryLen;
        readonly SqlCommand command;
        readonly SqlParameterCollection commandParameters;
        readonly Dictionary<object, string> lookup;

        public CommandFactory(int estimatedLength)
        {
            queryText = new char[estimatedLength];
            queryLen = 0;
            command = new SqlCommand();
            commandParameters = command.Parameters;
            lookup = new Dictionary<object, string>();
        }

        public SqlCommand CreateCommand(SqlConnection conn, int commandTimeout)
        {
            command.Connection = conn;
            command.CommandTimeout = commandTimeout; //60 by default
            command.CommandText = new string(queryText, 0, queryLen);
            return command;
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
                commandParameters.Add(o.ToSqlParameter(paramName));
                lookup.Add(o.EquatableValue, paramName);
            }
            return paramName;
        }

        public void AppendSql(string sql, int startIndex, int length)
        {
            if (queryText.Length < queryLen + length) {
                Array.Resize(ref queryText, Math.Max(queryText.Length * 3 / 2, queryLen + length + 5));
            }
            sql.CopyTo(startIndex, queryText, queryLen, length);
            queryLen += length;
        }
    }

    struct DebugCommandFactory : ICommandFactory
    {
        readonly StringBuilder debugText;

        DebugCommandFactory(int estimatedLength)
        {
            debugText = new StringBuilder(estimatedLength + 30); //extra length for argument values.
        }

        public static DebugCommandFactory Create(int estimatedLength) => new DebugCommandFactory(estimatedLength);
        public string RegisterParameterAndGetName<T>(T o) where T : IQueryParameter => QueryTracer.InsecureSqlDebugString(o.EquatableValue);
        public void AppendSql(string sql, int startIndex, int length) => debugText.Append(sql, startIndex, length);

        public string DebugTextFor(IQueryComponent impl)
        {
            impl?.AppendTo(ref this);
            return debugText.ToString();
        }
    }

    struct LengthEstimationCommandFactory : ICommandFactory
    {
        public int QueryLength;
        static readonly string ExampleParameterName = CommandFactory.IndexToParameterName(9);
        public string RegisterParameterAndGetName<T>(T o) where T : IQueryParameter => ExampleParameterName;
        public void AppendSql(string sql, int startIndex, int length) => QueryLength += length;
    }

    struct EqualityKeyCommandFactory : ICommandFactory
    {
        readonly StringBuilder debugText;
        int argOffset;
        FastArrayBuilder<object> paramValues;

        EqualityKeyCommandFactory(int estimatedLength)
        {
            debugText = new StringBuilder(estimatedLength + 30); //extra length for argument values.
            argOffset = 0;
            paramValues = FastArrayBuilder<object>.Create();
        }

        public string RegisterParameterAndGetName<T>(T o) where T : IQueryParameter
        {
            paramValues.Add(o.EquatableValue);
            return CommandFactory.IndexToParameterName(argOffset++);
        }

        public void AppendSql(string sql, int startIndex, int length) => debugText.Append(sql, startIndex, length);

        public static QueryKey EqualityKey(IQueryComponent impl)
        {
            var factory = new EqualityKeyCommandFactory(impl?.EstimateLength() ?? 0);
            impl?.AppendTo(ref factory);
            return new QueryKey {
                SqlTextKey = factory.debugText.ToString(),
                Params = new ComparableArray<object>(factory.paramValues.ToArray()),
            };
        }
    }

    sealed class QueryKey : ValueBase<QueryKey>
    {
        public string SqlTextKey { get; set; }
        public ComparableArray<object> Params { get; set; }
    }

    static class PooledSmallBufferAllocator<T>
    {
        static readonly int IndexCount = 129;
        static readonly int MaxArrayLength = IndexCount-1;

        static readonly ConcurrentQueue<T[]>[] bagsByIndex = new ConcurrentQueue<T[]>[IndexCount];

        static ConcurrentQueue<T[]> GetBag(int index)
        {
            var bag = bagsByIndex[index];
            if (bag != null)
                return bag;
            var otherBag = Interlocked.CompareExchange(ref bagsByIndex[index], bag = new ConcurrentQueue<T[]>(), null);
            //CompareExchange returns previous value
            //if that wasn't null, this was a no-op and that value shold be returned
            //if that *was* null, we set that location, and return our bag
            return otherBag ?? bag;
        }

        public static T[] GetByLength(int length)
        {
            if (length > MaxArrayLength)
                return new T[length];
            var bag = GetBag(length);
            T[] result;
            if (bag.TryDequeue(out result))
                return result;
            return new T[length];
        }

        public static void ReturnToPool(T[] arr)
        {
            if (arr.Length > MaxArrayLength)
                return;
            var bag = GetBag(arr.Length);
            Array.Clear(arr, 0, arr.Length);
            bag.Enqueue(arr);
        }
    }
}
