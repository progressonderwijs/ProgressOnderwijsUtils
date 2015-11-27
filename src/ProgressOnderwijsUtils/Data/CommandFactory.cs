﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
    interface ICommandFactory
    {
        string RegisterParameterAndGetName<T>(T o)
            where T : IQueryParameter;

        void AppendSql(string sql, int startIndex, int length);
        void AppendSql(string sql);
    }

    struct CommandFactory : ICommandFactory
    {
        char[] queryText; //readonly StringBuilder;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public SqlCommand CreateCommand(SqlConnection conn, int commandTimeout)
        {
            command.Connection = conn;
#if USE_RAW_TRANSACTIONS
			command.Transaction = conn.SqlTransaction;
#endif
            command.CommandTimeout = commandTimeout; //60 by default
            command.CommandText = new string(queryText, 0, queryLen);
            return command;
        }

        const int ParameterNameCacheSize = 20;

        static readonly string[] CachedParameterNames =
            Enumerable.Range(0, ParameterNameCacheSize)
                .Select(IndexToParameterName)
                .ToArray();

        public static string IndexToParameterName(int parameterIndex) => "@par" + parameterIndex.ToStringInvariant();
        public static readonly int EstimatedParameterLength = "@par0".Length;

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

        public void AppendSql(string sql) => AppendSql(sql, 0, sql.Length);
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
        public void AppendSql(string sql) => debugText.Append(sql);

        public string DebugTextFor(IBuildableQuery impl)
        {
            impl?.AppendTo(ref this);
            return debugText.ToString();
        }
    }
}
