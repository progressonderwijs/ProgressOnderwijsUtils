using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace ProgressOnderwijsUtils
{
    public interface IQueryTracer
    {
        IEnumerable<Tuple<string, TimeSpan>> AllQueries { get; }
        TimeSpan AllQueryDurations { get; }
        int QueryCount { get; }
        TimeSpan SlowestQueryDuration { get; }
        void FinishDisposableTimer(Func<string> commandText, TimeSpan duration);
        IDisposable StartQueryTimer(string commandText);
        IDisposable StartQueryTimer(SqlCommand sqlCommand);
    }

    public static class QueryTracer
    {
        public static IQueryTracer CreateTracer(bool includeSensitiveInfo) { return new QueryTracerImpl(includeSensitiveInfo); }
        public static IQueryTracer CreateNullTracer() { return new NullTracer(); }

        class NullTracer : IQueryTracer
        {
            public IEnumerable<Tuple<string, TimeSpan>> AllQueries => ArrayExtensions.Empty<Tuple<string, TimeSpan>>();
            public TimeSpan AllQueryDurations => TimeSpan.Zero;
            public int QueryCount => 0;
            public TimeSpan SlowestQueryDuration => TimeSpan.Zero;
            public void FinishDisposableTimer(Func<string> commandText, TimeSpan duration) { }
            public IDisposable StartQueryTimer(string commandText) { return NullDisposable.Instance; }
            public IDisposable StartQueryTimer(SqlCommand sqlCommand) { return NullDisposable.Instance; }
        }

        class NullDisposable : IDisposable
        {
            public void Dispose() { }
            public static readonly NullDisposable Instance = new NullDisposable();
        }

        public static string DebugFriendlyCommandText(SqlCommand sqlCommand, bool includeSensitiveInfo)
        {
            string prefix = !includeSensitiveInfo
                ? ""
                : CommandParamString(sqlCommand);
            //when machine is in LAN, we're not running on the production server: assume it's OK to include potentially confidential info like passwords in debug output.
            var commandText = prefix + sqlCommand.CommandText;
            return commandText;
        }

        public static string CommandParamString(SqlCommand sqlCommand)
        {
            return
                sqlCommand.Parameters.Cast<SqlParameter>()
                    .Select(
                        par => "DECLARE " + par.ParameterName + " AS " + SqlParamTypeString(par) + ";\nSET " + par.ParameterName + " = " + SqlValueString(par.Value) + ";\n")
                    .JoinStrings();
        }

        static string SqlParamTypeString(SqlParameter par) => par.SqlDbType + (par.SqlDbType == SqlDbType.NVarChar ? "(max)" : "");

        public static string SqlValueString(object p) // Not Secure, just a debug tool!
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
                return Convert.ToInt64(p) + "/*" + p + "*/";
            } else {
                try {
                    return p.ToString();
                } catch (Exception e) {
                    return "[[Exception in QueryTracer.SqlValueString: " + e.Message + "]]";
                }
            }
        }

        public sealed class QueryTracerImpl : IQueryTracer
        {
            public int QueryCount => queryCount;
            int queryCount;
            int queriesCompleted;
            readonly bool IncludeSensitiveInfo;
            public QueryTracerImpl(bool inlcudeSensiveInfo) { IncludeSensitiveInfo = inlcudeSensiveInfo; }
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

            public IDisposable StartQueryTimer(SqlCommand sqlCommand) => StartQueryTimer(QueryTracer.DebugFriendlyCommandText(sqlCommand, IncludeSensitiveInfo));

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
                readonly QueryTracerImpl tracer;
                readonly Func<string> query;
                readonly Stopwatch queryTimer;

                internal QueryTimer(QueryTracerImpl tracer, Func<string> query)
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
