using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using ExpressionToCodeLib;
using JetBrains.Annotations;

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
        void StartTracing();
        void StopTracing();
    }

    public enum SqlCommandTracerOptions
    {
        ExcludeArgumentValuesFromLog,
        IncludeArgumentValuesInLog
    }

    public static class SqlCommandTracer
    {
        [NotNull]
        public static ISqlCommandTracer CreateAlwaysOnTracer(SqlCommandTracerOptions includeSensitiveInfo)
            => new SqlCommandTracerImpl(includeSensitiveInfo);

        public static ISqlCommandTracer CreateAlwaysOffTracer() => NoopTracer.Instance;

        [NotNull]
        public static ISqlCommandTracer CreateTogglableTracer(SqlCommandTracerOptions includeSensitiveInfo)
            => new ResettableCommandTracer(() => CreateAlwaysOnTracer(includeSensitiveInfo));

        sealed class NoopTracer : ISqlCommandTracer
        {
            public static readonly NoopTracer Instance = new NoopTracer();

            [NotNull]
            public IEnumerable<Tuple<string, TimeSpan>> AllCommands => Array.Empty<Tuple<string, TimeSpan>>();

            public TimeSpan TotalDuration => TimeSpan.Zero;
            public int CommandCount => 0;
            public TimeSpan SlowestCommandDuration => TimeSpan.Zero;
            public void FinishDisposableTimer(Func<string> commandText, TimeSpan duration) { }

            [CanBeNull]
            public IDisposable StartCommandTimer(string commandText) => null;

            [CanBeNull]
            public IDisposable StartCommandTimer(SqlCommand sqlCommand) => null;

            public void StartTracing() { }
            public void StopTracing() { }
        }

        [NotNull]
        public static string DebugFriendlyCommandText([NotNull] SqlCommand sqlCommand, SqlCommandTracerOptions includeSensitiveInfo)
            => CommandParamStringOrEmpty(sqlCommand, includeSensitiveInfo) + sqlCommand.CommandText;

        [NotNull]
        static string CommandParamStringOrEmpty(SqlCommand sqlCommand, SqlCommandTracerOptions includeSensitiveInfo)
        {
            if (includeSensitiveInfo == SqlCommandTracerOptions.IncludeArgumentValuesInLog) {
                return CommandParamString(sqlCommand);
            } else {
                return "";
            }
        }

        [NotNull]
        static string CommandParamString([NotNull] SqlCommand sqlCommand)
            => sqlCommand.Parameters.Cast<SqlParameter>().Select(DeclareParameter).JoinStrings();

        [NotNull]
        static string DeclareParameter([NotNull] SqlParameter par)
        {
            var declareVariable = "DECLARE " + par.ParameterName + " AS " + SqlParamTypeString(par);
            if (par.SqlDbType != SqlDbType.Structured) {
                return declareVariable
                    + " = " + InsecureSqlDebugString(par.Value, true) + ";\n";
            } else {
                return declareVariable
                    + "/*" + par.Value.GetType().ToCSharpFriendlyTypeName() + "*/;\n"
                    + "insert into " + par.ParameterName + " values "
                    + ValuesClauseForTableValuedParameter((par.Value as IOptionalObjectListForDebugging)?.ContentsForDebuggingOrNull());
            }
        }

        [NotNull]
        static string ValuesClauseForTableValuedParameter([CanBeNull] IReadOnlyList<object> tableValue)
        {
            if (tableValue == null) {
                return "(/* UNKNOWN? */);\n";
            }
            const int maxValuesToInsert = 20;
            var valuesString = tableValue.Take(maxValuesToInsert).Select(v => $"({InsecureSqlDebugString(v, false)})").JoinStrings(", ");
            var valueCountCommentIfNecessary = (tableValue.Count <= maxValuesToInsert ? "" : "\n    /* ... and more; " + tableValue.Count + " total */") + ";\n";
            return valuesString + valueCountCommentIfNecessary;
        }

        [NotNull]
        static string SqlParamTypeString([NotNull] SqlParameter par)
            => par.SqlDbType == SqlDbType.Structured
                ? par.TypeName
                : par.SqlDbType + (par.SqlDbType == SqlDbType.NVarChar ? "(max)" : "");

        [NotNull]
        public static string InsecureSqlDebugString([CanBeNull] object p, bool includeReadableEnumValue)
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
                return ((IConvertible)p).ToInt64(null).ToStringInvariant() + (includeReadableEnumValue ? "/*" + ObjectToCode.PlainObjectToCode(p) + "*/" : "");
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

            [NotNull]
            public IEnumerable<Tuple<string, TimeSpan>> AllCommands => allqueries.Select(tup => Tuple.Create(tup.Item2(), tup.Item1));

            public TimeSpan TotalDuration { get; private set; }

            [NotNull]
            IDisposable StartCommandTimer([NotNull] Func<string> commandText)
            {
                if (commandText == null) {
                    throw new ArgumentNullException(nameof(commandText));
                }

                return new SqlCommandTimer(this, commandText);
            }

            [NotNull]
            public IDisposable StartCommandTimer([NotNull] string commandText)
            {
                if (commandText == null) {
                    throw new ArgumentNullException(nameof(commandText));
                }

                return StartCommandTimer(() => commandText);
            }

            [NotNull]
            public IDisposable StartCommandTimer([NotNull] SqlCommand sqlCommand) => StartCommandTimer(DebugFriendlyCommandText(sqlCommand, IncludeSensitiveInfo));

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

            public void StartTracing() { }
            public void StopTracing() { }

            sealed class SqlCommandTimer : IDisposable
            {
                readonly SqlCommandTracerImpl tracer;
                readonly Func<string> lazySqlCommandText;
                readonly Stopwatch commandStopwatch;

                internal SqlCommandTimer([NotNull] SqlCommandTracerImpl tracer, Func<string> lazySqlCommandText)
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

        public sealed class ResettableCommandTracer : ISqlCommandTracer
        {
            ISqlCommandTracer sqlCommandTracer;
            readonly Func<ISqlCommandTracer> tracerFactory;

            public ResettableCommandTracer(Func<ISqlCommandTracer> tracerFactory)
            {
                this.tracerFactory = tracerFactory;
                StopTracing();
            }

            public IEnumerable<Tuple<string, TimeSpan>> AllCommands => sqlCommandTracer.AllCommands;
            public TimeSpan TotalDuration => sqlCommandTracer.TotalDuration;
            public int CommandCount => sqlCommandTracer.CommandCount;
            public TimeSpan SlowestCommandDuration => sqlCommandTracer.SlowestCommandDuration;

            public void FinishDisposableTimer(Func<string> commandText, TimeSpan duration)
                => sqlCommandTracer.FinishDisposableTimer(commandText, duration);

            public IDisposable StartCommandTimer(string commandText) => sqlCommandTracer.StartCommandTimer(commandText);
            public IDisposable StartCommandTimer(SqlCommand sqlCommand) => sqlCommandTracer.StartCommandTimer(sqlCommand);
            public void StartTracing() => sqlCommandTracer = tracerFactory();
            public void StopTracing() => sqlCommandTracer = CreateAlwaysOffTracer();
        }
    }
}
