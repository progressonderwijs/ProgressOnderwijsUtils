namespace ProgressOnderwijsUtils;

public readonly record struct SqlTraceEvent
{
    public string EventContent { get; init; }
    public TimeSpan Duration { get; init; }
    public TimeSpan CumulativeElapsedTime { get; init; }
}

public interface ISqlCommandTracer
{
    SqlTraceEvent[] ListAllCommands();
    void RegisterEvent(string commandText, TimeSpan duration);
    SqlTracerAgumentInclusion ArgumentInclusion { get; }
    bool IsTracing { get; }
}

public enum SqlTracerAgumentInclusion { ExcludingArgumentValues, IncludingArgumentValues, }

public static class SqlCommandTracer
{
    public static ISqlCommandTracer? Tracer(this SqlConnection conn)
        => (conn.Site as IAttachedToTracer)?.Tracer;

    public static ISqlCommandTracer CreateAlwaysOnTracer(SqlTracerAgumentInclusion agumentInclusion)
        => new AlwaysOnTracer(agumentInclusion);

    public static ISqlCommandTracer CreateAlwaysOffTracer(SqlTracerAgumentInclusion agumentInclusion)
        => new AlwaysOffTracer(agumentInclusion);

    public static ISqlCommandTracer WrapTracer(ISqlCommandTracer originalTracer)
        => new WrappingTracer(originalTracer);

    sealed class AlwaysOffTracer : ISqlCommandTracer
    {
        public AlwaysOffTracer(SqlTracerAgumentInclusion agumentInclusion)
            => ArgumentInclusion = agumentInclusion;

        public SqlTraceEvent[] ListAllCommands()
            => Array.Empty<SqlTraceEvent>();

        public void RegisterEvent(string commandText, TimeSpan duration) { }
        public SqlTracerAgumentInclusion ArgumentInclusion { get; }

        public bool IsTracing
            => false;
    }

    sealed class AlwaysOnTracer : ISqlCommandTracer
    {
        readonly Stopwatch ElapsedTime = Stopwatch.StartNew();
        readonly List<SqlTraceEvent> allqueries = new();
        const int maxTotalEventContentSize = 100_000_000;
        int totalEventContentSize;

        public AlwaysOnTracer(SqlTracerAgumentInclusion agumentInclusion)
            => ArgumentInclusion = agumentInclusion;

        public SqlTraceEvent[] ListAllCommands()
        {
            lock (allqueries) {
                return allqueries.ToArray();
            }
        }

        public void RegisterEvent(string commandText, TimeSpan duration)
        {
            lock (allqueries) {
                if (totalEventContentSize >= maxTotalEventContentSize) {
                    return;
                }
                allqueries.Add(new() { EventContent = commandText, Duration = duration, CumulativeElapsedTime = ElapsedTime.Elapsed, });
                totalEventContentSize += commandText.Length;
                if (totalEventContentSize >= maxTotalEventContentSize) {
                    allqueries.Add(new() { EventContent = $"Max event data size ({totalEventContentSize}) reached", CumulativeElapsedTime = ElapsedTime.Elapsed, });
                }
            }
        }

        public SqlTracerAgumentInclusion ArgumentInclusion { get; }

        public bool IsTracing
            => true;
    }

    sealed class WrappingTracer : ISqlCommandTracer
    {
        readonly ISqlCommandTracer Original;
        readonly ISqlCommandTracer Inner;

        public WrappingTracer(ISqlCommandTracer original)
        {
            Original = original;
            Inner = CreateAlwaysOnTracer(original.ArgumentInclusion);
        }

        public SqlTraceEvent[] ListAllCommands()
            => Inner.ListAllCommands();

        public void RegisterEvent(string commandText, TimeSpan duration)
        {
            Inner.RegisterEvent(commandText, duration);
            Original.RegisterEvent(commandText, duration);
        }

        public SqlTracerAgumentInclusion ArgumentInclusion
            => Original.ArgumentInclusion;

        public bool IsTracing
            => true;
    }

    public static IDisposable? StartCommandTimer(this ISqlCommandTracer tracer, string commandText)
        => !tracer.IsTracing ? null : new SqlCommandTimer(tracer, commandText);

    public static IDisposable? StartCommandTimer(this ISqlCommandTracer tracer, SqlCommand sqlCommand)
        => !tracer.IsTracing ? null : new SqlCommandTimer(tracer, SqlCommandDebugStringifier.DebugFriendlyCommandText(sqlCommand, tracer.ArgumentInclusion));

    sealed class SqlCommandTimer : IDisposable
    {
        readonly ISqlCommandTracer tracer;
        readonly string sqlCommandText;
        readonly Stopwatch commandStopwatch;

        internal SqlCommandTimer(ISqlCommandTracer tracer, string sqlCommandText)
        {
            this.tracer = tracer;
            this.sqlCommandText = sqlCommandText;
            commandStopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            commandStopwatch.Stop();
            tracer.RegisterEvent(sqlCommandText, commandStopwatch.Elapsed);
        }
    }
}
