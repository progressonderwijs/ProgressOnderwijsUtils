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
        IEnumerable<Tuple<string, TimeSpan>> AllCommands { get; }
        TimeSpan TotalDuration { get; }
        int CommandCount { get; }
        TimeSpan SlowestCommandDuration { get; }
        void FinishDisposableTimer(Func<string> commandText, TimeSpan duration);
        IDisposable StartCommandTimer(string commandText);
        IDisposable StartCommandTimer(SqlCommand sqlCommand);
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
            public IEnumerable<Tuple<string, TimeSpan>> AllCommands => ArrayExtensions.Empty<Tuple<string, TimeSpan>>();
            public TimeSpan TotalDuration => TimeSpan.Zero;
            public int CommandCount => 0;
            public TimeSpan SlowestCommandDuration => TimeSpan.Zero;
            public void FinishDisposableTimer(Func<string> commandText, TimeSpan duration) { }
            public IDisposable StartCommandTimer(string commandText) => null;
            public IDisposable StartCommandTimer(SqlCommand sqlCommand) => null;
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
                            "DECLARE " + par.ParameterName + " AS " + SqlParamTypeString(par) + DeclareTail(par)
                                + ";\n")
                    .JoinStrings();
        }

        static string DeclareTail(SqlParameter par)
        {
            if (par.SqlDbType != SqlDbType.Structured) {
                return " = " + InsecureSqlDebugString(par.Value, true);
            } else {
                var tableValuesDeclarationStart = "/*" + ObjectToCode.GetCSharpFriendlyTypeName(par.Value.GetType()) + "*/;\n" + "insert into " + par.ParameterName + " values ";
                var tableValue = (par.Value as IOptionalObjectListForDebugging)?.ContentsForDebuggingOrNull();
                if (tableValue == null) {
                    return tableValuesDeclarationStart + "(/* UNKNOWN? */)";
                } else {
                    const int maxValuesToInsert = 20;
                    return tableValuesDeclarationStart
                        + tableValue.Take(maxValuesToInsert).Select(v => $"({InsecureSqlDebugString(v, false)})").JoinStrings(", ")
                        + (tableValue.Count <= maxValuesToInsert ? "" : "\n    /* ... and more; " + tableValue.Count + " total */")
                        ;
                }
            }
        }

        static string SqlParamTypeString(SqlParameter par)
            => par.SqlDbType == SqlDbType.Structured
                ? par.TypeName
                : par.SqlDbType + (par.SqlDbType == SqlDbType.NVarChar ? "(max)" : "");

        public static string InsecureSqlDebugString(object p, bool includeEnumType)
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
                return ((IConvertible)p).ToInt64(null).ToStringInvariant() + (includeEnumType ? "/*" + ObjectToCode.PlainObjectToCode(p) + "*/" : "");
            } else if (p is IFormattable) {
                return ((IFormattable)p).ToString(null, CultureInfo.InvariantCulture);
            } else {
                try {
                    return "{!" + p + "!}";
                } catch (Exception e) {
                    return $"[[Exception in {nameof(SqlCommandTracer)}.{nameof(InsecureSqlDebugString)}: {e.Message}]]";
                }
            }
        }

        sealed class SqlCommandTracerImpl : ISqlCommandTracer
        {
            public int CommandCount => commandCount;
            int commandCount;
            int commandsCompleted;
            readonly SqlCommandTracerOptions IncludeSensitiveInfo;

            public SqlCommandTracerImpl(SqlCommandTracerOptions inlcudeSensiveInfo)
            {
                IncludeSensitiveInfo = inlcudeSensiveInfo;
            }

            readonly object Sync = new object();
            readonly List<Tuple<TimeSpan, Func<string>>> allqueries = new List<Tuple<TimeSpan, Func<string>>>();
            Tuple<TimeSpan, Func<string>> slowest = Tuple.Create(default(TimeSpan), (Func<string>)(() => "(none)"));
            public TimeSpan SlowestCommandDuration => slowest.Item1;
            public IEnumerable<Tuple<string, TimeSpan>> AllCommands => allqueries.Select(tup => Tuple.Create(tup.Item2(), tup.Item1));
            public TimeSpan TotalDuration { get; private set; }

            IDisposable StartCommandTimer(Func<string> commandText)
            {
                if (commandText == null) {
                    throw new ArgumentNullException(nameof(commandText));
                }

                return new SqlCommandTimer(this, commandText);
            }

            public IDisposable StartCommandTimer(string commandText)
            {
                if (commandText == null) {
                    throw new ArgumentNullException(nameof(commandText));
                }

                return StartCommandTimer(() => commandText);
            }

            public IDisposable StartCommandTimer(SqlCommand sqlCommand) => StartCommandTimer(DebugFriendlyCommandText(sqlCommand, IncludeSensitiveInfo));

            public void FinishDisposableTimer(Func<string> commandText, TimeSpan duration)
            {
                lock (Sync) {
                    var entry = Tuple.Create(duration, commandText);

                    if (SlowestCommandDuration < duration) {
                        slowest = Tuple.Create(duration, commandText);
                    }
                    allqueries.Add(entry);
                    TotalDuration += duration;
                }
            }

            sealed class SqlCommandTimer : IDisposable
            {
                readonly SqlCommandTracerImpl tracer;
                readonly Func<string> lazySqlCommandText;
                readonly Stopwatch commandStopwatch;

                internal SqlCommandTimer(SqlCommandTracerImpl tracer, Func<string> lazySqlCommandText)
                {
                    this.tracer = tracer;
                    this.lazySqlCommandText = lazySqlCommandText;
                    Interlocked.Increment(ref tracer.commandCount);
                    commandStopwatch = Stopwatch.StartNew();
                }

                public void Dispose()
                {
                    commandStopwatch.Stop();
                    Interlocked.Increment(ref tracer.commandsCompleted);
                    tracer.FinishDisposableTimer(lazySqlCommandText, commandStopwatch.Elapsed);
                }
            }
        }
    }
}
