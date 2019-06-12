using System;
using System.Data.SqlClient;

namespace ProgressOnderwijsUtils.Tests
{
    public class TransactedLocalConnection : IDisposable
    {
        public readonly System.Transactions.CommittableTransaction Transaction = new System.Transactions.CommittableTransaction();

        public TransactedLocalConnection()
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? @"Server = (localdb)\MSSQLLocalDB; Integrated Security = true";

            var sqlCommandTracer = SqlCommandTracer.CreateAlwaysOffTracer(SqlTracerAgumentInclusion.IncludingArgumentValues);
            Connection = new SqlConnection(connectionString) { Site = new SqlConnectionContext(60, sqlCommandTracer, 1.0) };
            try {
                Connection.Open();
                ParameterizedSql.TableValuedTypeDefinitionScripts.ExecuteNonQuery(Connection);
                Connection.EnlistTransaction(Transaction);
            } catch {
                Dispose();
                throw;
            }
        }

        public SqlConnection Connection { get; }

        public void Dispose()
        {
            Transaction.Dispose();
            Connection.Dispose();
        }
    }
}
