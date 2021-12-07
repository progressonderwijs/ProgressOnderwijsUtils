namespace ProgressOnderwijsUtils.Tests.Data;

public class TransactedLocalConnection : IDisposable
{
    public readonly System.Transactions.CommittableTransaction Transaction = new();
    protected static readonly string ConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? @"Server = (localdb)\MSSQLLocalDB; Integrated Security = true";

    public TransactedLocalConnection()
    {
        var connectionString = ConnectionString;

        var sqlCommandTracer = SqlCommandTracer.CreateAlwaysOffTracer(SqlTracerAgumentInclusion.IncludingArgumentValues);
        Connection = new(connectionString) { Site = new SqlConnectionContext(sqlCommandTracer, new(60, 1.0)), };
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
