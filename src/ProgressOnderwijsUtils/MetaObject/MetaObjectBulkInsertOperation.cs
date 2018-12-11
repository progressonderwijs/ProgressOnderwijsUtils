using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using ExpressionToCodeLib;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    static class MetaObjectBulkInsertOperation
    {
        public static void Execute<[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
            T>([NotNull] SqlCommandCreationContext sqlContext, string tableName, [NotNull] ColumnDefinition[] columnDefinitions, BulkCopyFieldMappingMode bulkCopyFieldMappingMode, SqlBulkCopyOptions options, [NotNull] IEnumerable<T> enumerable, CancellationToken cancellationToken)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
        {
            if (enumerable == null) {
                throw new ArgumentNullException(nameof(enumerable));
            }
            if (sqlContext == null) {
                throw new ArgumentNullException(nameof(sqlContext));
            }
            if (sqlContext.Connection.State != ConnectionState.Open) {
                throw new InvalidOperationException($"Cannot bulk copy into {tableName}: connection isn't open but {sqlContext.Connection.State}.");
            }

            var effectiveReaderToken =
                sqlContext.CommandTimeoutInS == 0 ? cancellationToken
                : cancellationToken == default ? new CancellationTokenSource(TimeSpan.FromSeconds(sqlContext.CommandTimeoutInS)).Token
                : CancellationTokenSource.CreateLinkedTokenSource(new CancellationTokenSource(TimeSpan.FromSeconds(sqlContext.CommandTimeoutInS)).Token, cancellationToken).Token;
            using (var sqlBulkCopy = new SqlBulkCopy(sqlContext.Connection, options, null))
            using (var objectReader = new MetaObjectDataReader<T>(enumerable, effectiveReaderToken)) {
                sqlBulkCopy.BulkCopyTimeout = sqlContext.CommandTimeoutInS;
                sqlBulkCopy.DestinationTableName = tableName;
                var mapping = CreateMapping(objectReader, tableName, columnDefinitions, bulkCopyFieldMappingMode, options);

                BulkInsertFieldMapping.ApplyFieldMappingsToBulkCopy(mapping, sqlBulkCopy);
                var sw = Stopwatch.StartNew();
                try {
                    sqlBulkCopy.WriteToServer(objectReader);
                    //so why no async?
                    //WriteToServerAsync "supports" cancellation, but causes deadlocks when buggy code uses the connection while enumerating metaObjects, and that's hard to detect and very nasty on production servers, so we stick to sync instead - that throws exceptions instead, and hey, it's slightly faster too.
                } catch (SqlException ex) when (ParseDestinationColumnIndexFromMessage(ex.Message) is int destinationColumnIndex) {
                    throw HelpfulException(sqlBulkCopy, destinationColumnIndex, ex) ?? MetaObjectBasedException<T>(mapping, destinationColumnIndex, ex);
                } finally {
                    TraceBulkInsertDuration(sqlContext.Tracer, tableName, sw, objectReader.RowsProcessed);
                }
            }
        }

        [NotNull]
        static Exception MetaObjectBasedException<T>([NotNull] BulkInsertFieldMapping[] mapping, int destinationColumnIndex, SqlException ex)
        {
            var sourceColumnName = "??unknown??";
            foreach (var m in mapping) {
                if (m.Dst.Index == destinationColumnIndex) {
                    sourceColumnName = m.Src.Name;
                }
            }

            var metaPropName = typeof(T).ToCSharpFriendlyTypeName() + "." + sourceColumnName;
            return new Exception($"Received an invalid column length from the bcp client for metaobject property ${metaPropName}.", ex);
        }

        [CanBeNull]
        static Exception HelpfulException(SqlBulkCopy bulkCopy, int destinationColumnIndex, SqlException ex)
        {
            var fi = typeof(SqlBulkCopy).GetField("_sortedColumnMappings", BindingFlags.NonPublic | BindingFlags.Instance);
            var sortedColumns = fi.GetValue(bulkCopy);
            var items = (object[])sortedColumns.GetType().GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(sortedColumns);

            var itemdata = items?[destinationColumnIndex].GetType().GetField("_metadata", BindingFlags.NonPublic | BindingFlags.Instance);
            var metadata = itemdata?.GetValue(items[destinationColumnIndex]);

            var column = metadata?.GetType().GetField("column", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(metadata);
            var length = metadata?.GetType().GetField("length", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(metadata);
            return column == null || length == null ? null : new Exception($"Column: {column} contains data with a length greater than: {length}", ex);
        }

        static void TraceBulkInsertDuration([CanBeNull] ISqlCommandTracer tracerOrNull, string tableName, Stopwatch sw, int rowsInserted)
        {
            if (tracerOrNull?.IsTracing ?? false) {
                tracerOrNull.RegisterEvent($"Bulk inserted {rowsInserted} rows into {tableName}.", sw.Elapsed);
            }
        }

        static readonly Regex colidMessageRegex = new Regex(@"Received an invalid column length from the bcp client for colid ([0-9]+).", RegexOptions.Compiled);

        static int? ParseDestinationColumnIndexFromMessage([NotNull] string message)
        {
            var match = colidMessageRegex.Match(message);
            return !match.Success ? default(int?) : int.Parse(match.Groups[1].Value) - 1;
        }

        [NotNull]
        static BulkInsertFieldMapping[] CreateMapping<T>([NotNull] MetaObjectDataReader<T> objectReader, string tableName, [NotNull] ColumnDefinition[] tableColumns, BulkCopyFieldMappingMode mode, SqlBulkCopyOptions options)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
        {
            var effectiveTableColumns = tableColumns.Where(c => c.ColumnAccessibility != ColumnAccessibility.Readonly && (c.ColumnAccessibility != ColumnAccessibility.AutoIncrement || options.HasFlag(SqlBulkCopyOptions.KeepIdentity)));

            var unfilteredMapping = BulkInsertFieldMapping.Create(ColumnDefinition.GetFromReader(objectReader), effectiveTableColumns.ToArray());

            return BulkInsertFieldMapping.FilterAndValidate(unfilteredMapping, typeof(T).ToCSharpFriendlyTypeName(), mode == BulkCopyFieldMappingMode.AllowExtraMetaObjectProperties, "table " + tableName, mode == BulkCopyFieldMappingMode.AllowExtraDatabaseColumns);
        }
    }
}
