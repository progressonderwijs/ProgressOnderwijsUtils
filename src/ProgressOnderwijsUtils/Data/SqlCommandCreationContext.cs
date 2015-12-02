using System;
using System.Data.SqlClient;

namespace ProgressOnderwijsUtils
{
    public class SqlCommandCreationContext : IDisposable
    {
        public SqlConnection Connection { get; }
        public IQueryTracer Tracer { get; }
        public int CommandTimeoutInS { get; }
        // ReSharper disable UnusedMember.Global
        // Handige generieke functionaliteit, maar niet altijd gebruikt
        public SqlCommandCreationContext OverrideTimeout(int timeoutSeconds) => new SqlCommandCreationContext(Connection, timeoutSeconds, Tracer);
        // ReSharper restore UnusedMember.Global
        public SqlCommandCreationContext(SqlConnection conn, int defaultTimeoutInS, IQueryTracer tracer)
        {
            Connection = conn;
            CommandTimeoutInS = defaultTimeoutInS;
            Tracer = tracer;
        }

        public void Dispose()
        {
            Connection.Dispose();
        }

        public static implicit operator SqlCommandCreationContext(SqlConnection conn) => new SqlCommandCreationContext(conn, 0, null);
    }
}
