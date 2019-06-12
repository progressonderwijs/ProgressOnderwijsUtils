using System;
using System.Data.SqlClient;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public sealed class SqlCommandCreationContext : IDisposable
    {
        readonly SqlConnection connection;

        public SqlConnection GetConnection()
            => connection;

        readonly ISqlCommandTracer tracer;

        public ISqlCommandTracer Tracer()
            => tracer;

        readonly int commandTimeoutInS;

        public int DefaultCommandTimeout()
            => commandTimeoutInS;

        // ReSharper disable UnusedMember.Global
        // Handige generieke functionaliteit, maar niet altijd gebruikt
        [NotNull]
        public SqlCommandCreationContext OverrideTimeout(int timeoutSeconds)
            => new SqlCommandCreationContext(GetConnection(), timeoutSeconds, Tracer());

        // ReSharper restore UnusedMember.Global

        public SqlCommandCreationContext(SqlConnection conn, int defaultTimeoutInS, ISqlCommandTracer tracer)
        {
            connection = conn;
            commandTimeoutInS = defaultTimeoutInS;
            this.tracer = tracer;
        }

        public void Dispose()
            => GetConnection().Dispose();
    }

    public static class SqlConnectionExtensions
    {
        public static int DefaultCommandTimeout(this SqlConnection conn)
            => conn.Site is IHasDefaultCommandTimeout defaultTimeout ? defaultTimeout.DefaultCommandTimeoutInS : 0;

        public static SqlConnection GetConnection(this SqlConnection conn)
            => conn;
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
