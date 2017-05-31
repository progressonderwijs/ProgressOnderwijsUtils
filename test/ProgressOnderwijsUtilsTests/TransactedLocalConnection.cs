using System;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    public class TransactedLocalConnection : IDisposable
    {
        public readonly SqlCommandCreationContext Context;
        public readonly CommittableTransaction Transaction;

        public TransactedLocalConnection()
        {
            Transaction = new CommittableTransaction();
            Context = new SqlCommandCreationContext(new SqlConnection(@"Server = (localdb)\MSSQLLocalDB; Integrated Security = true"), 60, SqlCommandTracer.CreateAlwaysOffTracer());
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
