using System.Data.Common;
using System.Threading.Tasks;

namespace ProgressOnderwijsUtils;

static class BulkInsertImplementation
{
    public static void Execute(SqlConnection sqlConn, DbDataReader source, BulkInsertTarget target, string sourceNameForTracing, CommandTimeout timeout)
    {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }
        if (sqlConn == null) {
            throw new ArgumentNullException(nameof(sqlConn));
        }
        if (sqlConn.State != ConnectionState.Open) {
            throw new InvalidOperationException($"Cannot bulk copy into {target.TableName}: connection isn't open but {sqlConn.State}.");
        }

        using var sqlBulkCopy = new SqlBulkCopy(sqlConn, target.Options, null);
        sqlBulkCopy.BulkCopyTimeout = timeout.ComputeAbsoluteTimeout(sqlConn);
        sqlBulkCopy.DestinationTableName = target.TableName;
        var mapping = CreateMapping(source, target, sourceNameForTracing);

        BulkInsertFieldMapping.ApplyFieldMappingsToBulkCopy(mapping, sqlBulkCopy);
        var sw = Stopwatch.StartNew();
        if (!connectionsInBulkCopy.TryAdd(sqlConn, 0)) {
            throw new InvalidOperationException($"Cannot bulk copy into {target.TableName}: another bulk copy is already in progress on this SqlConnection.");
        }
        try {
            sqlBulkCopy.WriteToServer(source);
        } catch (SqlException ex) when (ParseDestinationColumnIndexFromMessage(ex.Message) is { } destinationColumnIndex) {
            throw HelpfulException(sqlBulkCopy, destinationColumnIndex, ex) ?? GenericBcpColumnLengthErrorWithFieldNames(mapping, destinationColumnIndex, ex, sourceNameForTracing);
        } finally {
            _ = connectionsInBulkCopy.TryRemove(sqlConn, out _);
            TraceBulkInsertDuration(sqlConn.Tracer(), target.TableName, sw, sqlBulkCopy, sourceNameForTracing);
        }
    }

    /// <summary>
    /// WriteToServerAsync "supports" cancellation, but causes deadlocks when buggy code uses the connection while enumerating pocos,
    /// and that's hard to detect and very nasty on production servers. The sync version throws exceptions instead.
    /// </summary>
    public static async Task ExecuteAsync(SqlConnection sqlConn, DbDataReader source, BulkInsertTarget target, string sourceNameForTracing, CommandTimeout timeout, CancellationToken cancel)
    {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }
        if (sqlConn == null) {
            throw new ArgumentNullException(nameof(sqlConn));
        }
        if (sqlConn.State != ConnectionState.Open) {
            throw new InvalidOperationException($"Cannot bulk copy into {target.TableName}: connection isn't open but {sqlConn.State}.");
        }
        if (TryGetReaderConnection(source) is { } readerConn && ReferenceEquals(readerConn, sqlConn)) {
            throw new InvalidOperationException(
                $"Cannot bulk copy into {target.TableName}: the source DbDataReader is reading from the same SqlConnection. "
                + "This causes corrupt state and deadlocks with async bulk copy. Use a separate connection for the source reader."
            );
        }

        using var sqlBulkCopy = new SqlBulkCopy(sqlConn, target.Options, null);
        sqlBulkCopy.BulkCopyTimeout = timeout.ComputeAbsoluteTimeout(sqlConn);
        sqlBulkCopy.DestinationTableName = target.TableName;
        var mapping = CreateMapping(source, target, sourceNameForTracing);

        BulkInsertFieldMapping.ApplyFieldMappingsToBulkCopy(mapping, sqlBulkCopy);
        var sw = Stopwatch.StartNew();
        if (!connectionsInBulkCopy.TryAdd(sqlConn, 0)) {
            throw new InvalidOperationException($"Cannot bulk copy into {target.TableName}: another bulk copy is already in progress on this SqlConnection.");
        }
        try {
            await sqlBulkCopy.WriteToServerAsync(source, cancel).ConfigureAwait(false);
        } catch (Exception ex) when (ex.IsCancellationExceptionOfToken(cancel)) {
            throw;
        } catch (Exception ex) when (SqlCancellationBoundary.ShouldConvertToOperationCancelled(ex, cancel)) {
            throw SqlCancellationBoundary.ToOperationCancelled(ex, cancel);
        } catch (SqlException ex) when (ParseDestinationColumnIndexFromMessage(ex.Message) is { } destinationColumnIndex) {
            throw HelpfulException(sqlBulkCopy, destinationColumnIndex, ex) ?? GenericBcpColumnLengthErrorWithFieldNames(mapping, destinationColumnIndex, ex, sourceNameForTracing);
        } finally {
            _ = connectionsInBulkCopy.TryRemove(sqlConn, out _);
            TraceBulkInsertDuration(sqlConn.Tracer(), target.TableName, sw, sqlBulkCopy, sourceNameForTracing);
        }
    }

    /// <summary>
    /// Set of <see cref="SqlConnection"/> instances that currently have a bulk-copy operation in flight.
    /// Checked by SQL execution entry points (<see cref="ParameterizedSql.CreateSqlCommand"/>) to fail fast
    /// when user code accidentally tries to run a query on the destination connection while it is being
    /// written to — this would otherwise deadlock async bulk copy on the connection's internal semaphore.
    /// Uses reference equality (the default for reference types).
    /// </summary>
    static readonly ConcurrentDictionary<SqlConnection, byte> connectionsInBulkCopy = new();

    internal static void ThrowIfConnectionInBulkCopy(SqlConnection conn)
    {
        if (connectionsInBulkCopy.ContainsKey(conn)) {
            throw new InvalidOperationException(
                "Cannot execute a query on this SqlConnection: a bulk copy is currently in progress on it. "
                + "Running a query on the destination connection during bulk copy causes corrupt state and deadlocks with async bulk copy. "
                + "Use a separate SqlConnection for any queries that run while enumerating the bulk-copy source."
            );
        }
    }

    static Exception GenericBcpColumnLengthErrorWithFieldNames(BulkInsertFieldMapping[] mapping, int destinationColumnIndex, SqlException ex, string sourceName)
    {
        var sourceColumnName = "??unknown??";
        foreach (var m in mapping) {
            if (m.Dst.Index == destinationColumnIndex) {
                sourceColumnName = m.Src.Name;
            }
        }

        return new($"Received an invalid column length from the bcp client for source field {sourceColumnName} of source {sourceName}.", ex);
    }

    static Exception? HelpfulException(SqlBulkCopy bulkCopy, int destinationColumnIndex, SqlException ex)
    {
        var fi = typeof(SqlBulkCopy).GetField("_sortedColumnMappings", BindingFlags.NonPublic | BindingFlags.Instance).AssertNotNull();
        var sortedColumns = fi.GetValue(bulkCopy).AssertNotNull();
        var items = (object[]?)sortedColumns.GetType().GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(sortedColumns);

        var itemdata = items?[destinationColumnIndex].GetType().GetField("_metadata", BindingFlags.NonPublic | BindingFlags.Instance);
        var metadata = itemdata?.GetValue(items?[destinationColumnIndex]);

        var column = metadata?.GetType().GetField("column", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(metadata);
        var length = metadata?.GetType().GetField("length", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(metadata);
        return column == null || length == null ? null : new Exception($"Column: {column} contains data with a length greater than: {length}", ex);
    }

    static void TraceBulkInsertDuration(ISqlCommandTracer? tracerOrNull, string destinationTableName, Stopwatch sw, SqlBulkCopy sqlBulkCopy, string sourceNameForTracing)
    {
        if (tracerOrNull is { IsTracing: true }) {
            tracerOrNull.RegisterEvent($"Bulk inserted {sqlBulkCopy.RowsCopied64} rows from {sourceNameForTracing} into table {destinationTableName}.", sw.Elapsed);
        }
    }

    static readonly Regex colidMessageRegex = new(@"Received an invalid column length from the bcp client for colid ([0-9]+).", RegexOptions.Compiled);

    static int? ParseDestinationColumnIndexFromMessage(string message)
    {
        var match = colidMessageRegex.Match(message);
        return !match.Success ? default(int?) : int.Parse(match.Groups[1].Value) - 1;
    }

    /// <summary>
    /// Best-effort discovery of the <see cref="SqlConnection"/> a <see cref="DbDataReader"/> is reading from.
    /// Duck-types on a public/non-public instance property named <c>Connection</c> whose type is (assignable to) <see cref="SqlConnection"/>.
    /// Catches <see cref="SqlDataReader"/> and common wrapper readers that faithfully expose their underlying connection.
    /// Returns null when the reader type has no such property (e.g. hostile custom wrappers).
    /// </summary>
    static readonly ConcurrentDictionary<Type, Func<DbDataReader, SqlConnection?>?> readerConnectionAccessorByType = new();

    static SqlConnection? TryGetReaderConnection(DbDataReader source)
    {
        var accessor = readerConnectionAccessorByType.GetOrAdd(
            source.GetType(),
            static type => {
                var prop = type.GetProperty("Connection", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop?.GetMethod == null || !typeof(DbConnection).IsAssignableFrom(prop.PropertyType)) {
                    return null;
                }
                return reader => prop.GetValue(reader) as SqlConnection;
            }
        );
        return accessor?.Invoke(source);
    }

    static BulkInsertFieldMapping[] CreateMapping(DbDataReader source, BulkInsertTarget target, string sourceName)
        => target.CreateValidatedMapping(ColumnDefinition.GetFromReader(source))
            .AssertOk(error => new InvalidOperationException($"Failed to map source {sourceName} to the table {target.TableName}. Errors:\n{error}"));
}
