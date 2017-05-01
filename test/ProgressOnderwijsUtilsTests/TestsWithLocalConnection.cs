using System;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    public class TestsWithLocalConnection : IDisposable
    {
        protected readonly SqlCommandCreationContext conn;
        protected readonly CommittableTransaction transaction;

        public TestsWithLocalConnection()
        {
            transaction = new CommittableTransaction(new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
            conn = new SqlCommandCreationContext(new SqlConnection(@"Server = (localdb)\MSSQLLocalDB; Integrated Security = true"), 60, SqlCommandTracer.CreateAlwaysOffTracer());
            try {
                conn.Connection.Open();
                foreach(var script in ParameterizedSql.TableValuedTypeDefinitionScripts)
                    script.ExecuteNonQuery(conn);
                conn.Connection.EnlistTransaction(transaction);
            } catch {
                Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            transaction.Dispose();
            conn.Dispose();
        }
    }
}
