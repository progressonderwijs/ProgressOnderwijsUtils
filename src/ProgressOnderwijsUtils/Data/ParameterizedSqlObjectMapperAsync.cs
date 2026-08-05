using System.Threading.Tasks;

namespace ProgressOnderwijsUtils;

public static partial class ParameterizedSqlObjectMapper
{
    [MustUseReturnValue]
    public static Task<T?> ReadScalarAsync<T>(this ParameterizedSql sql, SqlConnection sqlConn, CancellationToken cancellationToken = default)
        => sql.OfScalar<T>().ExecuteAsync(sqlConn, cancellationToken);
}
