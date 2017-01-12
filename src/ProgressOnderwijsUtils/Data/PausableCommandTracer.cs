using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ProgressOnderwijsUtils
{
    public sealed class PausableCommandTracer : ISqlCommandTracer
    {
        readonly ISqlCommandTracer underlyingCommandTracer;
        readonly NestableToggle Toggle;
        public PausableCommandTracer(ISqlCommandTracer underlyingCommandTracer, NestableToggle toggle)
        {
            this.underlyingCommandTracer = underlyingCommandTracer;
            Toggle = toggle;
        }
        public IEnumerable<Tuple<string, TimeSpan>> AllCommands => underlyingCommandTracer.AllCommands;
        public TimeSpan TotalDuration => underlyingCommandTracer.TotalDuration;
        public int CommandCount => underlyingCommandTracer.CommandCount;
        public TimeSpan SlowestCommandDuration => underlyingCommandTracer.SlowestCommandDuration;

        public void FinishDisposableTimer(Func<string> commandText, TimeSpan duration)
            => underlyingCommandTracer.FinishDisposableTimer(commandText, duration);

        public IDisposable StartCommandTimer(string commandText)
            => Toggle.IsCurrentlyEnabled ? underlyingCommandTracer.StartCommandTimer(commandText) : null;

        public IDisposable StartCommandTimer(SqlCommand sqlCommand)
            => Toggle.IsCurrentlyEnabled ? underlyingCommandTracer.StartCommandTimer(sqlCommand) : null;
    }
}