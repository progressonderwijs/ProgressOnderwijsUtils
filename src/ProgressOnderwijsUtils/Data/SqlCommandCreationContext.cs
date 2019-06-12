using System;
using System.Data.SqlClient;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class SqlConnectionExtensions
    {
        public static int DefaultCommandTimeout(this SqlConnection conn)
            => conn.Site is IHasDefaultCommandTimeout defaultTimeout ? defaultTimeout.DefaultCommandTimeoutInS : 0;
    }

    public interface IAttachedToTracer
    {
        ISqlCommandTracer Tracer { get; }
    }

    public interface IHasDefaultCommandTimeout
    {
        int DefaultCommandTimeoutInS { get; }
        double TimeoutScale { get; }
    }

    public sealed class SqlConnectionContext : SiteBase, IAttachedToTracer, IHasDefaultCommandTimeout
    {
        public SqlConnectionContext(int defaultCommandTimeoutInS, ISqlCommandTracer tracer, double timeoutScale)
        {
            DefaultCommandTimeoutInS = defaultCommandTimeoutInS;
            Tracer = tracer;
            TimeoutScale = timeoutScale;
        }

        public ISqlCommandTracer Tracer { get; }
        public int DefaultCommandTimeoutInS { get; }
        public double TimeoutScale { get; }
    }
}
