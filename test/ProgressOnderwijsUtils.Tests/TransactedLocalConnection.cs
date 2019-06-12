using System;
using System.Data.SqlClient;

namespace ProgressOnderwijsUtils.Tests
{
    public class TransactedLocalConnection : IDisposable
    {
        public readonly SqlCommandCreationContext Context;
        public readonly System.Transactions.CommittableTransaction Transaction = new System.Transactions.CommittableTransaction();

        public TransactedLocalConnection()
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? @"Server = (localdb)\MSSQLLocalDB; Integrated Security = true";

            var sqlCommandTracer = SqlCommandTracer.CreateAlwaysOffTracer(SqlTracerAgumentInclusion.IncludingArgumentValues);
            var sqlConnection = new SqlConnection(connectionString) { Site = new SqlConnectionContext(60, sqlCommandTracer, 1.0) };
            Context = new SqlCommandCreationContext(sqlConnection, 60, sqlCommandTracer);
            try {
                Context.Connection.Open();
                ParameterizedSql.TableValuedTypeDefinitionScripts.ExecuteNonQuery(Context);
                Context.Connection.EnlistTransaction(Transaction);
            } catch {
                Dispose();
                throw;
            }
        }

        public SqlConnection Connection
            => Context.Connection;

        public void Dispose()
        {
            Transaction.Dispose();
            Context.Dispose();
        }
    }
}
