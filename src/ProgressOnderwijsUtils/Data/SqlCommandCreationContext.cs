using System;
using System.Data.SqlClient;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public interface IAttachedToTracer
    {
        ISqlCommandTracer Tracer { get; }
    }

    public interface IHasDefaultCommandTimeout
    {
        BatchTimeoutDefaults TimeoutDefaults { get; }
    }

    public sealed class SqlConnectionContext : SiteBase, IAttachedToTracer, IHasDefaultCommandTimeout
    {
        public SqlConnectionContext(ISqlCommandTracer tracer,  BatchTimeoutDefaults timeoutDefaults)
        {
            Tracer = tracer;
            TimeoutDefaults = timeoutDefaults;
        }

        public ISqlCommandTracer Tracer { get; }

        public BatchTimeoutDefaults TimeoutDefaults { get; }
    }
}
