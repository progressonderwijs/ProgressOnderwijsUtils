using System;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    public class TransactedLocalConnection : IDisposable
    {
        public readonly SqlCommandCreationContext Connection;
        public readonly CommittableTransaction Transaction;

        public TransactedLocalConnection()
        {
            Transaction = new CommittableTransaction();
            Connection = new SqlCommandCreationContext(new SqlConnection(@"Server = (localdb)\MSSQLLocalDB; Integrated Security = true"), 60, SqlCommandTracer.CreateAlwaysOffTracer());
            try {
                Connection.Connection.Open();
                ParameterizedSql.TableValuedTypeDefinitionScripts.ExecuteNonQuery(Connection);
                Connection.Connection.EnlistTransaction(Transaction);
            } catch {
                Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            Transaction.Dispose();
            Connection.Dispose();
        }
    }
}
