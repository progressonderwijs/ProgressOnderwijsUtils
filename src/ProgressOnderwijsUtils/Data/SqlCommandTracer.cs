using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils
{
    public interface ISqlCommandTracer
    {
        IEnumerable<Tuple<string, TimeSpan>> AllQueries { get; }
        TimeSpan AllQueryDurations { get; }
        int QueryCount { get; }
        TimeSpan SlowestQueryDuration { get; }
        void FinishDisposableTimer(Func<string> commandText, TimeSpan duration);
        IDisposable StartQueryTimer(string commandText);
        IDisposable StartQueryTimer(SqlCommand sqlCommand);
    }

    public enum SqlCommandTracerOptions
    {
        ExcludeArgumentValuesFromLog,
        IncludeArgumentValuesInLog
    }

    public static class SqlCommandTracer
    {
        public static ISqlCommandTracer CreateTracer(SqlCommandTracerOptions includeSensitiveInfo)
        {
            return new SqlCommandTracerImpl(includeSensitiveInfo);
        }

        public static ISqlCommandTracer CreateNullTracer()
        {
            return new NullTracer();
        }

        class NullTracer : ISqlCommandTracer
        {
            public IEnumerable<Tuple<string, TimeSpan>> AllQueries => ArrayExtensions.Empty<Tuple<string, TimeSpan>>();
            public TimeSpan AllQueryDurations => TimeSpan.Zero;
            public int QueryCount => 0;
            public TimeSpan SlowestQueryDuration => TimeSpan.Zero;
            public void FinishDisposableTimer(Func<string> commandText, TimeSpan duration) { }
            public IDisposable StartQueryTimer(string commandText) => null;
            public IDisposable StartQueryTimer(SqlCommand sqlCommand) => null;
        }

        public static string DebugFriendlyCommandText(SqlCommand sqlCommand, SqlCommandTracerOptions includeSensitiveInfo)
        {
            return CommandParamStringOrEmpty(sqlCommand, includeSensitiveInfo) + sqlCommand.CommandText;
        }

        static string CommandParamStringOrEmpty(SqlCommand sqlCommand, SqlCommandTracerOptions includeSensitiveInfo)
        {
            if (includeSensitiveInfo == SqlCommandTracerOptions.IncludeArgumentValuesInLog) {
                return CommandParamString(sqlCommand);
            } else {
                return "";
            }
        }

        static string CommandParamString(SqlCommand sqlCommand)
        {
            return
                sqlCommand.Parameters.Cast<SqlParameter>()
                    .Select(
                        par =>
                            "DECLARE " + par.ParameterName + " AS " + SqlParamTypeString(par) + ";\nSET " + par.ParameterName + " = " + InsecureSqlDebugString(par.Value)
                                + ";\n")
                    .JoinStrings();
        }

        static string SqlParamTypeString(SqlParameter par) => par.SqlDbType + (par.SqlDbType == SqlDbType.NVarChar ? "(max)" : "");

        public static string InsecureSqlDebugString(object p)
        {
            if (p is DBNull || p == null) {
                return "NULL";
            } else if (p is DateTime) {
                return "'" + ((DateTime)p).ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'", CultureInfo.InvariantCulture) + "'";
            } else if (p is string) {
                return "'" + ((string)p).Replace("'", "''") + "'";
            } else if (p is long) {
                return ((long)p).ToStringInvariant();
            } else if (p is int) {
                return ((int)p).ToStringInvariant();
            } else if (p is bool) {
                return (bool)p ? "1" : "0";
            } else if (p is Enum) {
                return ((IConvertible)p).ToInt64(null).ToStringInvariant() + "/*" + ObjectToCode.PlainObjectToCode(p) + "*/";
            } else if (p is IFormattable) {
                return ((IFormattable)p).ToString(null, CultureInfo.InvariantCulture);
            } else {
                try {
                    return "{!" + p + "!}";
                } catch (Exception e) {
                    return "[[Exception in QueryTracer.SqlValueString: " + e.Message + "]]";
                }
            }
        }

        public sealed class SqlCommandTracerImpl : ISqlCommandTracer
        {
            public int QueryCount => queryCount;
            int queryCount;
            int queriesCompleted;
            readonly SqlCommandTracerOptions IncludeSensitiveInfo;

            public SqlCommandTracerImpl(SqlCommandTracerOptions inlcudeSensiveInfo)
            {
                IncludeSensitiveInfo = inlcudeSensiveInfo;
            }

            readonly object Sync = new object();
            readonly List<Tuple<TimeSpan, Func<string>>> allqueries = new List<Tuple<TimeSpan, Func<string>>>();
            Tuple<TimeSpan, Func<string>> slowest = Tuple.Create(default(TimeSpan), (Func<string>)(() => "(none)"));

            [UsefulToKeep("library method")]
            public Func<string> SlowestQuery => slowest.Item2;

            public TimeSpan SlowestQueryDuration => slowest.Item1;
            public IEnumerable<Tuple<string, TimeSpan>> AllQueries => allqueries.Select(tup => Tuple.Create(tup.Item2(), tup.Item1));
            public TimeSpan AllQueryDurations { get; private set; }

            IDisposable StartQueryTimer(Func<string> commandText)
            {
                if (commandText == null) {
                    throw new ArgumentNullException(nameof(commandText));
                }

                return new QueryTimer(this, commandText);
            }

            public IDisposable StartQueryTimer(string commandText)
            {
                if (commandText == null) {
                    throw new ArgumentNullException(nameof(commandText));
                }

                return StartQueryTimer(() => commandText);
            }

            public IDisposable StartQueryTimer(SqlCommand sqlCommand) => StartQueryTimer(DebugFriendlyCommandText(sqlCommand, IncludeSensitiveInfo));

            public void FinishDisposableTimer(Func<string> commandText, TimeSpan duration)
            {
                lock (Sync) {
                    var entry = Tuple.Create(duration, commandText);

                    if (SlowestQueryDuration < duration) {
                        slowest = Tuple.Create(duration, commandText);
                    }
                    allqueries.Add(entry);
                    AllQueryDurations += duration;
                }
            }

            sealed class QueryTimer : IDisposable
            {
                readonly SqlCommandTracerImpl tracer;
                readonly Func<string> query;
                readonly Stopwatch queryTimer;

                internal QueryTimer(SqlCommandTracerImpl tracer, Func<string> query)
                {
                    this.tracer = tracer;
                    this.query = query;
                    Interlocked.Increment(ref tracer.queryCount);
                    queryTimer = Stopwatch.StartNew();
                }

                public void Dispose()
                {
                    queryTimer.Stop();
                    Interlocked.Increment(ref tracer.queriesCompleted);
                    tracer.FinishDisposableTimer(query, queryTimer.Elapsed);
                }
            }
        }
    }
}
