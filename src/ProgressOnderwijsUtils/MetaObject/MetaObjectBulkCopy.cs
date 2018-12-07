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
using ProgressOnderwijsUtils.SchemaReflection;

namespace ProgressOnderwijsUtils
{
    public enum BulkCopyFieldMappingMode
    {
        ExactMatch,
        AllowExtraMetaObjectProperties,
        AllowExtraDatabaseColumns,
    }

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

    /// <summary>
    /// Contains extension methods to insert metaobjects (POCOs) into database tables using SqlBulkCopy.
    /// </summary>
    public static class MetaObjectBulkCopy
    {
        /// <summary>
        /// Performs a bulk insert.  Maps columns based on name, not order (unlike SqlBulkCopy by default); uses a 1 hour timeout, and options CheckConstraints | UseInternalTransaction.
        /// For more fine-grained control, create an SqlBulkCopy instance manually, and call bulkCopy.WriteMetaObjectsToServer(objs, sqlConnection, tableName)
        /// </summary>
        /// <typeparam name="T">The type of metaobject to be inserted</typeparam>
        /// <param name="metaObjects">The list of entities to insert</param>
        /// <param name="sqlconn">The Sql connection to write to</param>
        /// <param name="table">The table, including schema information, to write the entities into.</param>
        public static void BulkCopyToSqlServer<T>([NotNull] this IEnumerable<T> metaObjects, [NotNull] SqlCommandCreationContext sqlconn, [NotNull] DatabaseDescription.Table table)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
        {
            metaObjects.BulkCopyToSqlServer(sqlconn, table, BulkCopyFieldMappingMode.ExactMatch);
        }

        public static void BulkCopyToSqlServer<T>([NotNull] this IEnumerable<T> metaObjects, [NotNull] SqlCommandCreationContext sqlconn, [NotNull] DatabaseDescription.Table table, BulkCopyFieldMappingMode mode)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
        {
            metaObjects.BulkCopyToSqlServer(sqlconn, table.QualifiedName, table.Columns.ArraySelect(column => column.ColumnMetaData), mode);
        }

        /// <summary>
        /// Performs a bulk insert.  Maps columns based on name, not order (unlike SqlBulkCopy by default); uses a 1 hour timeout, and options CheckConstraints | UseInternalTransaction.
        /// For more fine-grained control, create an SqlBulkCopy instance manually, and call bulkCopy.WriteMetaObjectsToServer(objs, sqlConnection, tableName)
        /// </summary>
        /// <typeparam name="T">The type of metaobject to be inserted</typeparam>
        /// <param name="metaObjects">The list of entities to insert</param>
        /// <param name="sqlconn">The Sql connection to write to</param>
        /// <param name="tableName">The name of the table to insert into.</param>
        /// <param name="columns">The schema of the table as it currently exists in the database.</param>
        public static void BulkCopyToSqlServer<T>([NotNull] this IEnumerable<T> metaObjects, [NotNull] SqlCommandCreationContext sqlconn, [NotNull] string tableName, [NotNull] DbColumnMetaData[] columns)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
        {
            metaObjects.BulkCopyToSqlServer(sqlconn, tableName, columns, BulkCopyFieldMappingMode.ExactMatch);
        }

        public static void BulkCopyToSqlServer<T>([NotNull] this IEnumerable<T> metaObjects, [NotNull] SqlCommandCreationContext sqlconn, [NotNull] string tableName, [NotNull] DbColumnMetaData[] columns, BulkCopyFieldMappingMode mode)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
        {
            using (var bulkCopy = new SqlBulkCopy(sqlconn.Connection, SqlBulkCopyOptions.CheckConstraints, null)) {
                bulkCopy.BulkCopyTimeout = sqlconn.CommandTimeoutInS;
                var token = sqlconn.CommandTimeoutInS == 0
                    ? CancellationToken.None
                    : new CancellationTokenSource(TimeSpan.FromSeconds(sqlconn.CommandTimeoutInS)).Token;
                bulkCopy.WriteMetaObjectsToServer(metaObjects, sqlconn, tableName, columns, mode, token); //.Wait(token);
            }
        }

        /// <summary>
        /// Writes meta-objects to the server.  If you use this method, it must be the only "WriteToServer" method you call on this bulk-copy instance because it sets the column mapping.
        /// </summary>
        public static void WriteMetaObjectsToServer<T>(
            [NotNull] this SqlBulkCopy bulkCopy,
            [NotNull] IEnumerable<T> metaObjects,
            [NotNull] SqlCommandCreationContext context,
            [NotNull] DatabaseDescription.Table table,
            CancellationToken cancellationToken)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
        {
            bulkCopy.WriteMetaObjectsToServer(metaObjects, context, table, BulkCopyFieldMappingMode.ExactMatch, cancellationToken);
        }

        public static void WriteMetaObjectsToServer<T>(
            [NotNull] this SqlBulkCopy bulkCopy,
            [NotNull] IEnumerable<T> metaObjects,
            [NotNull] SqlCommandCreationContext context,
            [NotNull] DatabaseDescription.Table table,
            BulkCopyFieldMappingMode mode,
            CancellationToken cancellationToken)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
        {
            bulkCopy.WriteMetaObjectsToServer(metaObjects, context, table.QualifiedName, table.Columns.ArraySelect(column => column.ColumnMetaData), mode, cancellationToken);
        }

        /// <summary>
        /// Writes meta-objects to the server.  If you use this method, it must be the only "WriteToServer" method you call on this bulk-copy instance because it sets the column mapping.
        /// </summary>
        public static void WriteMetaObjectsToServer<T>(
            [NotNull] this SqlBulkCopy bulkCopy,
            [NotNull] IEnumerable<T> metaObjects,
            [NotNull] SqlCommandCreationContext context,
            [NotNull] string tableName,
            [NotNull] DbColumnMetaData[] columns,
            CancellationToken cancellationToken)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
        {
            bulkCopy.WriteMetaObjectsToServer(metaObjects, context, tableName, columns, BulkCopyFieldMappingMode.ExactMatch, cancellationToken);
        }

        public static void WriteMetaObjectsToServer<T>(
            [NotNull] this SqlBulkCopy bulkCopy,
            [NotNull] IEnumerable<T> metaObjects,
            [NotNull] SqlCommandCreationContext context,
            [NotNull] string tableName,
            [NotNull] DbColumnMetaData[] columns,
            BulkCopyFieldMappingMode mode,
            CancellationToken cancellationToken)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
        {
            new MetaObjectBulkInsertOperation<T> {
                bulkCopy = bulkCopy,
                cancellationToken = cancellationToken,
                context = context,
                metaObjects = metaObjects,
                mode = mode,
                tableColumnDefinitions = ColumnDefinition.GetFromCompleteSetOfColumns(columns.Where(column => !column.Is_Computed && !column.Is_RowVersion).ToArray()),
                tableName = tableName,
            }.Execute();
        }
    }
}
