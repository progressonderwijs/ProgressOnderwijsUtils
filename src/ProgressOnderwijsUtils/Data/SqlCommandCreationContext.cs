using System;
using System.Data.SqlClient;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public sealed class SqlCommandCreationContext : IDisposable
    {
        public SqlConnection Connection { get; }
        public ISqlCommandTracer Tracer { get; }
        public int CommandTimeoutInS { get; }

        // ReSharper disable UnusedMember.Global
        // Handige generieke functionaliteit, maar niet altijd gebruikt
        [NotNull]
        public SqlCommandCreationContext OverrideTimeout(int timeoutSeconds)
            => new SqlCommandCreationContext(Connection, timeoutSeconds, Tracer);

        // ReSharper restore UnusedMember.Global

        public SqlCommandCreationContext(SqlConnection conn, int defaultTimeoutInS, ISqlCommandTracer tracer)
        {
            Connection = conn;
            CommandTimeoutInS = defaultTimeoutInS;
            Tracer = tracer;
        }

        public void Dispose()
            => Connection.Dispose();

        [NotNull]
        public static implicit operator SqlCommandCreationContext(SqlConnection conn)
            => new SqlCommandCreationContext(conn, 0, null);
    }
}
