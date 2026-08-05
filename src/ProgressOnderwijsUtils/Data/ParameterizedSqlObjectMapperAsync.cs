using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProgressOnderwijsUtils;

public static partial class ParameterizedSqlObjectMapper
{
    [MustUseReturnValue]
    public static Task<T?> ReadScalarAsync<T>(this ParameterizedSql sql, SqlConnection sqlConn, CancellationToken cancel = default)
        => sql.OfScalar<T>().ExecuteAsync(sqlConn, cancel);

    public static Task ExecuteNonQueryAsync(this ParameterizedSql sql, SqlConnection sqlConn, CancellationToken cancel = default)
        => sql.OfNonQuery().ExecuteAsync(sqlConn, cancel);

    [MustUseReturnValue]
    public static Task<int> ExecuteNonQueryWithRowCountAsync(this ParameterizedSql sql, SqlConnection sqlConn, CancellationToken cancel = default)
        => sql.OfNonQuery().ExecuteWithRowCountAsync(sqlConn, cancel);

    [MustUseReturnValue]
    public static Task<T[]> ReadPocosAsync<[MeansImplicitUse(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)] T>(this ParameterizedSql q, SqlConnection sqlConn, CancellationToken cancel = default)
        where T : IWrittenImplicitly
        => q.OfPocos<T>().ExecuteAsync(sqlConn, cancel);

    [MustUseReturnValue]
    public static Task<T?[]> ReadPlainAsync<T>(this ParameterizedSql q, SqlConnection sqlConn, CancellationToken cancel = default)
        => q.OfBuiltins<T>().ExecuteAsync(sqlConn, cancel);

    [MustUseReturnValue]
    public static Task<T[]> ReadTuplesAsync<[MeansImplicitUse(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)] T>(this ParameterizedSql q, SqlConnection sqlConn, CancellationToken cancel = default)
        where T : struct, IStructuralEquatable, ITuple
        => q.OfTuples<T>().ExecuteAsync(sqlConn, cancel);

    public static Task ReadJsonAsync(
        this ParameterizedSql q,
        SqlConnection sqlConn,
        IBufferWriter<byte> buffer,
        JsonWriterOptions options,
        CancellationToken cancel = default,
        JsonIgnoreCondition defaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        bool rowVersionAsNumber = false)
        => q.OfJson().ExecuteAsync(sqlConn, buffer, options, cancel, defaultIgnoreCondition, rowVersionAsNumber);

    internal static async Task<T[]> ReaderToArrayAsync<TOriginCommand, T>(TOriginCommand command, SqlDataReader reader, TRowReader<SqlDataReader, T> unpacker, ReusableCommand cmd, CancellationToken cancel)
        where TOriginCommand : IWithTimeout<TOriginCommand>
    {
        var lastColumnRead = -1;
        try {
            var builder = new ArrayBuilder<T>();
            while (await reader.ReadAsync(cancel).ConfigureAwait(false)) {
                var nextRow = unpacker(reader, out lastColumnRead);
                builder.Add(nextRow);
            }
            return builder.ToArray();
        } catch (Exception ex) {
            throw cmd.CreateExceptionWithTextAndArguments(ex, command, UnpackingErrorMessage<T>(reader, lastColumnRead));
        }
    }
}
