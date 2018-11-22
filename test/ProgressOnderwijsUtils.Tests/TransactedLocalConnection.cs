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
            Context = new SqlCommandCreationContext(new SqlConnection(Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? @"Server = (localdb)\ProjectsV13; Integrated Security = true"), 60, SqlCommandTracer.CreateAlwaysOffTracer(SqlTracerAgumentInclusion.IncludingArgumentValues));
            try {
                Context.Connection.Open();
                ParameterizedSql.TableValuedTypeDefinitionScripts.ExecuteNonQuery(Context);
                Context.Connection.EnlistTransaction(Transaction);
            } catch {
                Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            Transaction.Dispose();
            Context.Dispose();
        }
    }
}
