using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;
using System.Threading;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.SchemaReflection;
using System.Linq;
using ProgressOnderwijsUtils.Collections;
using static ProgressOnderwijsUtils.SafeSql;
using System;

namespace ProgressOnderwijsUtils
{
    public sealed class BulkInsertTarget
    {
        const SqlBulkCopyOptions defaultBulkCopyOptions = SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.KeepNulls;
        readonly string TableName;
        readonly ColumnDefinition[] Columns;
        readonly BulkCopyFieldMappingMode Mode;
        readonly SqlBulkCopyOptions Options;

        public BulkInsertTarget(string tableName, ColumnDefinition[] columnDefinition)
            : this(tableName, columnDefinition, BulkCopyFieldMappingMode.ExactMatch, defaultBulkCopyOptions) { }

        BulkInsertTarget(string tableName, ColumnDefinition[] columnDefinition, BulkCopyFieldMappingMode mode, SqlBulkCopyOptions options)
            => (TableName, Columns, Mode, Options) = (tableName, columnDefinition, mode, options);

        public static BulkInsertTarget FromDatabaseDescription(DatabaseDescription.Table table)
            => new BulkInsertTarget(table.QualifiedName, table.Columns.ArraySelect((col, colIdx) => ColumnDefinition.FromDbColumnMetaData(col.ColumnMetaData, colIdx)));

        public static BulkInsertTarget LoadFromTable(SqlConnection conn, ParameterizedSql tableName)
            => LoadFromTable(conn, tableName.CommandText());

        public static BulkInsertTarget LoadFromTable(SqlConnection conn, string tableName)
            => FromCompleteSetOfColumns(tableName, DbColumnMetaData.ColumnMetaDatas(conn, tableName));

        public static BulkInsertTarget FromCompleteSetOfColumns(string tableName, DbColumnMetaData[] columns)
            => new BulkInsertTarget(tableName, columns.ArraySelect(ColumnDefinition.FromDbColumnMetaData));

        public BulkInsertTarget With(BulkCopyFieldMappingMode mode)
            => new BulkInsertTarget(TableName, Columns, mode, Options);

        public BulkInsertTarget With(SqlBulkCopyOptions options)
            => new BulkInsertTarget(TableName, Columns, Mode, options);

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        static (T[] head, IEnumerable<T> fullSequence) PeekAtPrefix<T>(IEnumerable<T> enumerable, int firstN)
        {
            //Waarom bestaat deze method?  Zodat wij een mogelijke lazy enumerable gedeeltelijks kunnen evalueren, zonder *meermaals* te hoeven evalueren.

            var enumerator = enumerable.GetEnumerator();
            var buffer = new List<T>();
            for (var i = 0; i < firstN; i++) {
                if (enumerator.MoveNext()) {
                    buffer.Add(enumerator.Current);
                } else {
                    var partialHead = buffer.ToArray();
                    return (partialHead, partialHead);
                }
            }
            var completeHead = buffer.ToArray();
            if (enumerable is IReadOnlyList<T> || enumerable is ICollection<T>) {
                return (completeHead, enumerable);
            }

            IEnumerable<T> FullSequence()
            {
                if (completeHead.PretendNullable() != null) {
                    foreach (var item in completeHead) {
                        yield return item;
                    }
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                    completeHead = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                } else {
                    enumerator = enumerable.GetEnumerator();
                }

                while (enumerator.MoveNext()) {
                    yield return enumerator.Current;
                }
                enumerator.Dispose();
            }

            return (completeHead, FullSequence());
        }

        public void BulkInsert<[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
            T>(SqlConnection sqlConn, IEnumerable<T> pocos, CommandTimeout timeout = default, CancellationToken cancellationToken = default)
            where T : IReadImplicitly
        {
            if (Options == defaultBulkCopyOptions) {
                var (head, all) = PeekAtPrefix(pocos, 10);
                pocos = all;
                if (head.Length < 10) {
                    SmallBatchInsert(sqlConn, head, timeout);
                    return;
                }
            }

            using var dbDataReader = new PocoDataReader<T>(pocos, cancellationToken.CreateLinkedTokenWith(timeout.ToCancellationToken(sqlConn)));
            BulkInsert(sqlConn, dbDataReader, pocos.GetType().ToCSharpFriendlyTypeName(), timeout);
        }

        public void BulkInsert(SqlConnection sqlConn, DataTable dataTable, CommandTimeout timeout = default)
        {
            using var dbDataReader = dataTable.CreateDataReader();
            BulkInsert(sqlConn, dbDataReader, $"DataTable({dataTable.TableName})", timeout);
        }

        public void BulkInsert(SqlConnection sqlConn, DbDataReader dbDataReader, string sourceNameForTracing, CommandTimeout timeout = default)
            => BulkInsertImplementation.Execute(sqlConn, TableName, Columns, Mode, Options, timeout, dbDataReader, sourceNameForTracing);

        void SmallBatchInsert<T>(SqlConnection sqlConn, T[] rows, CommandTimeout timeout)
            where T : IReadImplicitly
        {
            if (rows.None()) {
                return;
            }

            var srcFields = PocoProperties<T>.Instance.Where(o => o.CanRead).Select(o => new ColumnDefinition(PocoPropertyConverter.GetOrNull(o.DataType)?.DbType ?? o.DataType, o.Name, o.Index, ColumnAccessibility.Readonly)).ToArray();
            var maybeMapping = BulkInsertImplementation.CreateValidatedMapping(srcFields, Columns, Mode, Options);
            if (maybeMapping.IsError) {
                throw new InvalidOperationException($"Failed to map source {typeof(T).ToCSharpFriendlyTypeName()} to the table {TableName}. Errors:\r\n{maybeMapping.AssertError()}");
            }
            var mapping = maybeMapping.AssertOk();
            foreach (var row in rows) {
                SQL(
                    $@"
                    insert into {ParameterizedSql.CreateDynamic(TableName)} ({mapping.Select(o => ParameterizedSql.CreateDynamic(o.Dst.Name)).ConcatenateSql(SQL($","))})
                    values ({mapping.Select(o => ParameterizedSql.Param(PocoProperties<T>.Instance[o.Src.Index].Getter.AssertNotNull()(row))).ConcatenateSql(SQL($", "))});
                "
                ).OfNonQuery().WithTimeout(timeout).Execute(sqlConn);
            }
        }
    }
}
