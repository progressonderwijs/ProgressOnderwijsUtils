using System;
using System.Data.SqlClient;

namespace ProgressOnderwijsUtils.Tests
{
    public class TransactedLocalConnection : IDisposable
    {
        public readonly SqlCommandCreationContext Context;
#if NET461
        public readonly System.Transactions.CommittableTransaction Transaction = new System.Transactions.CommittableTransaction();
#endif

        public TransactedLocalConnection()
        {
            Context = new SqlCommandCreationContext(new SqlConnection(Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? @"Server = (localdb)\MSSQLLocalDB; Integrated Security = true"), 60, SqlCommandTracer.CreateAlwaysOffTracer());
            try {
                Context.Connection.Open();
                ParameterizedSql.TableValuedTypeDefinitionScripts.ExecuteNonQuery(Context);
#if NET461
                Context.Connection.EnlistTransaction(Transaction);
#else
                // .NET Core does not support EnlistTransaction
                SafeSql.SQL($"begin transaction").ExecuteNonQuery(Context);
#endif
            } catch {
                Dispose();
                throw;
            }
        }

        public void Dispose()
        {
#if NET461
            Transaction.Dispose();
#endif
            Context.Dispose();
        }
    }
}
