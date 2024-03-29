using System.Data.Common;

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
        try {
            sqlBulkCopy.WriteToServer(source);
            //so why no async?
            //WriteToServerAsync "supports" cancellation, but causes deadlocks when buggy code uses the connection while enumerating pocos, and that's hard to detect and very nasty on production servers, so we stick to sync instead - that throws exceptions instead, and hey, it's slightly faster too.
        } catch (SqlException ex) when (ParseDestinationColumnIndexFromMessage(ex.Message) is { } destinationColumnIndex) {
            throw HelpfulException(sqlBulkCopy, destinationColumnIndex, ex) ?? GenericBcpColumnLengthErrorWithFieldNames(mapping, destinationColumnIndex, ex, sourceNameForTracing);
        } finally {
            TraceBulkInsertDuration(sqlConn.Tracer(), target.TableName, sw, sqlBulkCopy, sourceNameForTracing);
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

    static BulkInsertFieldMapping[] CreateMapping(DbDataReader source, BulkInsertTarget target, string sourceName)
        => target.CreateValidatedMapping(ColumnDefinition.GetFromReader(source))
            .AssertOk(error => new InvalidOperationException($"Failed to map source {sourceName} to the table {target.TableName}. Errors:\n{error}"));
}
