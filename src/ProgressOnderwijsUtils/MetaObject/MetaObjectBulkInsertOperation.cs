using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using ExpressionToCodeLib;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    struct MetaObjectBulkInsertOperation<T>
        where T : IMetaObject, IPropertiesAreUsedImplicitly
    {
        public SqlBulkCopy bulkCopy;
        public SqlCommandCreationContext context;
        public IEnumerable<T> metaObjects;
        public string tableName;
        public ColumnDefinition[] tableColumnDefinitions; //var tableColumnDefinitions = ColumnDefinition.GetFromCompleteSetOfColumns(columns.Where(column => !column.Is_Computed && !column.Is_RowVersion).ToArray());
        public BulkCopyFieldMappingMode mode;
        public CancellationToken cancellationToken;

        public void Execute()
        {
            var sqlconn = context.Connection;
            if (metaObjects == null) {
                throw new ArgumentNullException(nameof(metaObjects));
            }
            if (sqlconn == null) {
                throw new ArgumentNullException(nameof(sqlconn));
            }
            if (sqlconn.State != ConnectionState.Open) {
                throw new InvalidOperationException($"Cannot bulk copy into {tableName}: connection isn't open but {sqlconn.State}.");
            }
            bulkCopy.DestinationTableName = tableName;

            using (var objectReader = new MetaObjectDataReader<T>(metaObjects, cancellationToken)) {
                var mapping = ApplyMetaObjectColumnMapping(bulkCopy, objectReader, tableName, mode, tableColumnDefinitions);
                var sw = Stopwatch.StartNew();
                try {
                    bulkCopy.WriteToServer(objectReader);
                    //so why no async?
                    //WriteToServerAsync "supports" cancellation, but causes deadlocks when buggy code uses the connection while enumerating metaObjects, and that's hard to detect and very nasty on production servers, so we stick to sync instead - that throws exceptions instead, and hey, it's slightly faster too.
                } catch (SqlException ex) when (ParseDestinationColumnIndexFromMessage(ex.Message).HasValue) {
                    var destinationColumnIndex = ParseDestinationColumnIndexFromMessage(ex.Message).Value;
                    throw HelpfulException(bulkCopy, destinationColumnIndex, ex) ?? MetaObjectBasedException(mapping, destinationColumnIndex, ex);
                } finally {
                    TraceBulkInsertDuration(context.Tracer, tableName, sw, objectReader.RowsProcessed);
                }
            }
        }

        [NotNull]
        static Exception MetaObjectBasedException([NotNull] FieldMapping[] mapping, int destinationColumnIndex, SqlException ex)
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
            //note: sql colid is 1-based!
            var match = colidMessageRegex.Match(message);
            return !match.Success ? default(int?) : int.Parse(match.Groups[1].Value) - 1;
        }

        [NotNull]
        static FieldMapping[] ApplyMetaObjectColumnMapping([NotNull] SqlBulkCopy bulkCopy, [NotNull] MetaObjectDataReader<T> objectReader, string tableName, BulkCopyFieldMappingMode mode, [NotNull] ColumnDefinition[] tableColumns)
        {
            var clrColumns = ColumnDefinition.GetFromReader(objectReader);
            var mapping = FieldMapping.VerifyAndCreate(
                clrColumns,
                typeof(T).ToCSharpFriendlyTypeName(),
                mode == BulkCopyFieldMappingMode.AllowExtraMetaObjectProperties,
                tableColumns,
                "table " + tableName,
                mode == BulkCopyFieldMappingMode.AllowExtraDatabaseColumns);

            FieldMapping.ApplyFieldMappingsToBulkCopy(mapping, bulkCopy);
            return mapping;
        }
    }
}
