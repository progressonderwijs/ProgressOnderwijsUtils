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
    public static class MetaObjectBulkCopy
    {
        /// <summary>
        /// Performs a bulk insert.  Maps columns based on name, not order (unlike SqlBulkCopy by default); uses a 1 hour timeout, and options CheckConstraints | UseInternalTransaction.
        /// For more fine-grained control, create an SqlBulkCopy instance manually, and call bulkCopy.WriteMetaObjectsToServer(objs, sqlConnection, tableName)
        /// </summary>
        /// <typeparam name="T">The type of metaobject to be inserted</typeparam>
        /// <param name="metaObjects">The list of entities to insert</param>
        /// <param name="context">The Sql connection to write to</param>
        /// <param name="tableName">The name of the table to import into; must be a valid sql identifier (i.e. you must escape special characters if any).</param>
        public static void BulkCopyToSqlServer<T>([NotNull] this IEnumerable<T> metaObjects, [NotNull] SqlCommandCreationContext context, [NotNull] string tableName)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
            => BulkCopyToSqlServerWithSpecificColumnMapping(metaObjects, context, tableName, FieldMappingMode.IgnoreExtraDestinationFields);

        public static void BulkCopyToSqlServerWithSpecificColumnMapping<T>([NotNull] this IEnumerable<T> metaObjects, [NotNull] SqlCommandCreationContext context, [NotNull] string tableName, FieldMappingMode mode)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
        {
            using (var bulkCopy = new SqlBulkCopy(context.Connection, SqlBulkCopyOptions.CheckConstraints, null)) {
                bulkCopy.BulkCopyTimeout = context.CommandTimeoutInS;
                var token = context.CommandTimeoutInS == 0
                    ? CancellationToken.None
                    : new CancellationTokenSource(TimeSpan.FromSeconds(context.CommandTimeoutInS)).Token;
                bulkCopy.WriteMetaObjectsToServerWithSpecificColumnMapping(metaObjects, context, tableName, token, mode); //.Wait(token);
            }
        }

        /// <summary>
        /// Writes meta-objects to the server.  If you use this method, it must be the only "WriteToServer" method you call on this bulk-copy instance because it sets the column mapping.
        /// </summary>
        public static void WriteMetaObjectsToServer<T>([NotNull] this SqlBulkCopy bulkCopy, [NotNull] IEnumerable<T> metaObjects, [NotNull] SqlCommandCreationContext context, [NotNull] string tableName, CancellationToken cancellationToken)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
            => WriteMetaObjectsToServerWithSpecificColumnMapping(bulkCopy, metaObjects, context, tableName, cancellationToken, FieldMappingMode.IgnoreExtraDestinationFields);

        public static void WriteMetaObjectsToServerWithSpecificColumnMapping<T>([NotNull] this SqlBulkCopy bulkCopy, [NotNull] IEnumerable<T> metaObjects, [NotNull] SqlCommandCreationContext context, [NotNull] string tableName, CancellationToken cancellationToken, FieldMappingMode mode)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
        {
            var sqlconn = context.Connection;
            if (metaObjects == null) {
                throw new ArgumentNullException(nameof(metaObjects));
            }
            if (sqlconn == null) {
                throw new ArgumentNullException(nameof(sqlconn));
            }
            if (sqlconn.State != ConnectionState.Open) {
                throw new InvalidOperationException("Cannot bulk copy into " + tableName + ": connection isn't open but " + sqlconn.State);
            }
            bulkCopy.DestinationTableName = tableName;

            using (var objectReader = new MetaObjectDataReader<T>(metaObjects, cancellationToken)) {
                var mapping = ApplyMetaObjectColumnMapping(bulkCopy, objectReader, sqlconn, tableName, mode);
                var sw = Stopwatch.StartNew();
                try {
                    bulkCopy.WriteToServer(objectReader);
                    //so why no async?
                    //WriteToServerAsync "supports" cancellation, but causes deadlocks when buggy code uses the connection while enumerating metaObjects, and that's hard to detect and very nasty on production servers, so we stick to sync instead - that throws exceptions instead, and hey, it's slightly faster too.
                } catch (SqlException ex) when (ParseDestinationColumnIndexFromMessage(ex.Message).HasValue) {
                    var destinationColumnIndex = ParseDestinationColumnIndexFromMessage(ex.Message).Value;
                    throw HelpfulException(bulkCopy, destinationColumnIndex, ex) ?? MetaObjectBasedException<T>(mapping, destinationColumnIndex, ex);
                } finally {
                    TraceBulkInsertDuration(context.Tracer, tableName, sw, objectReader.RowsProcessed);
                }
            }
        }

        [NotNull]
        static Exception MetaObjectBasedException<T>([NotNull] FieldMapping[] mapping, int destinationColumnIndex, SqlException ex) where T : IMetaObject, IPropertiesAreUsedImplicitly
        {
            var sourceColumnName = mapping.Where(m => m.DstIndex == destinationColumnIndex).Select(m => m.SourceColumnDefinition.Name).FirstOrDefault();
            var metaPropName = typeof(T).ToCSharpFriendlyTypeName() + "." + (sourceColumnName ?? "??unknown??");
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
                tracerOrNull.RegisterEvent("Bulk inserted " + rowsInserted + " rows into " + tableName, sw.Elapsed);
            }
        }

        static readonly Regex colidMessageRegex = new Regex(@"Received an invalid column length from the bcp client for colid ([0-9]+).", RegexOptions.Compiled);

        static int? ParseDestinationColumnIndexFromMessage([NotNull] string message)
        {
            //note: sql colid is 1-based!
            var match = colidMessageRegex.Match(message);
            return !match.Success ? default(int?) : int.Parse(match.Groups[1].Value) - 1;
        }

        [NotNull]
        static FieldMapping[] ApplyMetaObjectColumnMapping<T>([NotNull] SqlBulkCopy bulkCopy, [NotNull] MetaObjectDataReader<T> objectReader, [NotNull] SqlConnection sqlconn, string tableName, FieldMappingMode mode)
            where T : IMetaObject
        {
            var dataColumns = ColumnDefinition.GetFromTable(sqlconn, tableName);
            var clrColumns = ColumnDefinition.GetFromReader(objectReader);
            var mapping = FieldMapping.VerifyAndCreate(
                clrColumns,
                typeof(T).ToCSharpFriendlyTypeName(),
                dataColumns,
                "table " + tableName,
                mode);

            FieldMapping.ApplyFieldMappingsToBulkCopy(mapping, bulkCopy);
            return mapping;
        }
    }
}
