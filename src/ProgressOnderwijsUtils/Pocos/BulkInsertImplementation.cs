using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
    static class BulkInsertImplementation
    {
        public static void Execute(SqlConnection sqlConn, string tableName, ColumnDefinition[] columnDefinitions, BulkCopyFieldMappingMode bulkCopyFieldMappingMode, SqlBulkCopyOptions options, CommandTimeout timeout, DbDataReader dbDataReader, string sourceNameForTracing)
        {
            if (dbDataReader == null) {
                throw new ArgumentNullException(nameof(dbDataReader));
            }
            if (sqlConn == null) {
                throw new ArgumentNullException(nameof(sqlConn));
            }
            if (sqlConn.State != ConnectionState.Open) {
                throw new InvalidOperationException($"Cannot bulk copy into {tableName}: connection isn't open but {sqlConn.State}.");
            }

            using (var sqlBulkCopy = new SqlBulkCopy(sqlConn, options, null)) {
                sqlBulkCopy.BulkCopyTimeout = timeout.ComputeAbsoluteTimeout(sqlConn);
                sqlBulkCopy.DestinationTableName = tableName;
                var mapping = CreateMapping(dbDataReader, tableName, columnDefinitions, bulkCopyFieldMappingMode, options, sourceNameForTracing);

                BulkInsertFieldMapping.ApplyFieldMappingsToBulkCopy(mapping, sqlBulkCopy);
                var sw = Stopwatch.StartNew();
                try {
                    sqlBulkCopy.WriteToServer(dbDataReader);
                    //so why no async?
                    //WriteToServerAsync "supports" cancellation, but causes deadlocks when buggy code uses the connection while enumerating pocos, and that's hard to detect and very nasty on production servers, so we stick to sync instead - that throws exceptions instead, and hey, it's slightly faster too.
                } catch (SqlException ex) when (ParseDestinationColumnIndexFromMessage(ex.Message) is int destinationColumnIndex) {
                    throw HelpfulException(sqlBulkCopy, destinationColumnIndex, ex) ?? GenericBcpColumnLengthErrorWithFieldNames(mapping, destinationColumnIndex, ex, sourceNameForTracing);
                } finally {
                    TraceBulkInsertDuration(sqlConn.Tracer(), tableName, sw, sqlBulkCopy, sourceNameForTracing);
                }
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

            return new Exception($"Received an invalid column length from the bcp client for source field {sourceColumnName} of source {sourceName}.", ex);
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

        static FieldInfo? rowsCopiedField;

        public static int GetRowsCopied(SqlBulkCopy bulkCopy)
        {
            //Why oh why isn't this public... https://stackoverflow.com/a/4474394/42921
            if (rowsCopiedField == null) {
                rowsCopiedField = typeof(SqlBulkCopy).GetField("_rowsCopied", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance).AssertNotNull();
            }

            return (int)rowsCopiedField.GetValue(bulkCopy).AssertNotNull();
        }

        static void TraceBulkInsertDuration(ISqlCommandTracer? tracerOrNull, string destinationTableName, Stopwatch sw, SqlBulkCopy sqlBulkCopy, string sourceNameForTracing)
        {
            if (tracerOrNull != null && tracerOrNull.IsTracing) {
                var rowsInserted = GetRowsCopied(sqlBulkCopy);
                tracerOrNull.RegisterEvent($"Bulk inserted {rowsInserted} rows from {sourceNameForTracing} into table {destinationTableName}.", sw.Elapsed);
            }
        }

        static readonly Regex colidMessageRegex = new Regex(@"Received an invalid column length from the bcp client for colid ([0-9]+).", RegexOptions.Compiled);

        static int? ParseDestinationColumnIndexFromMessage(string message)
        {
            var match = colidMessageRegex.Match(message);
            return !match.Success ? default(int?) : int.Parse(match.Groups[1].Value) - 1;
        }

        static BulkInsertFieldMapping[] CreateMapping(DbDataReader objectReader, string tableName, ColumnDefinition[] tableColumns, BulkCopyFieldMappingMode mode, SqlBulkCopyOptions options, string sourceName)
        {
            var unfilteredMapping = BulkInsertFieldMapping.Create(ColumnDefinition.GetFromReader(objectReader), tableColumns);

            var validatedMapping = new FieldMappingValidation {
                AllowExtraSourceColumns = mode == BulkCopyFieldMappingMode.AllowExtraPocoProperties,
                AllowExtraTargetColumns = mode == BulkCopyFieldMappingMode.AllowExtraDatabaseColumns,
                OverwriteAutoIncrement = options.HasFlag(SqlBulkCopyOptions.KeepIdentity),
            }.ValidateAndFilter(unfilteredMapping);

            if (validatedMapping.IsOk) {
                return validatedMapping.AssertOk();
            } else {
                throw new InvalidOperationException($"Failed to map source {sourceName} to the table {tableName}. Errors:\r\n{validatedMapping.ErrorOrNull()}");
            }
        }
    }
}
