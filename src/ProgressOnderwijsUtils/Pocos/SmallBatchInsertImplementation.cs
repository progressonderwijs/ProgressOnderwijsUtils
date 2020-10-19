using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ExpressionToCodeLib;
using Microsoft.Data.SqlClient;
using ProgressOnderwijsUtils.Collections;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils
{
    static class SmallBatchInsertImplementation<T>
        where T : IReadImplicitly
    {
        internal const int ThresholdForUsingSqlBulkCopy = 6;

        public static IEnumerable<T>? TrySmallBatchInsertOptimization(SqlConnection sqlConn, BulkInsertTarget bulkInsertTarget, IEnumerable<T> pocos, CommandTimeout timeout)
        {
            if (bulkInsertTarget.Options != BulkInsertTarget.defaultBulkCopyOptions) {
                return pocos;
            }
            var (head, all) = PeekAtPrefix(pocos, ThresholdForUsingSqlBulkCopy);
            if (head.Length >= ThresholdForUsingSqlBulkCopy) {
                return all;
            }
            SmallBatchInsert(sqlConn, bulkInsertTarget, head, timeout);
            return null;
        }

        static void SmallBatchInsert(SqlConnection sqlConn, BulkInsertTarget target, T[] rows, CommandTimeout timeout)
        {
            if (rows.None()) {
                return;
            }

            var srcFields = PocoProperties<T>.Instance.Where(o => o.CanRead).Select(o => new ColumnDefinition(PocoPropertyConverter.GetOrNull(o.DataType)?.DbType ?? o.DataType, o.Name, o.Index, ColumnAccessibility.Readonly)).ToArray();
            var maybeMapping = BulkInsertImplementation.CreateValidatedMapping(srcFields, target);
            if (maybeMapping.IsError) {
                throw new InvalidOperationException($"Failed to map source {typeof(T).ToCSharpFriendlyTypeName()} to the table {target.TableName}. Errors:\r\n{maybeMapping.AssertError()}");
            }
            var mapping = maybeMapping.AssertOk();
            foreach (var row in rows) {
                SQL(
                    $@"
                    insert into {ParameterizedSql.CreateDynamic(target.TableName)} ({mapping.Select(o => ParameterizedSql.CreateDynamic(o.Dst.Name)).ConcatenateSql(SQL($","))})
                    values ({mapping.Select(o => ParameterizedSql.Param(PocoProperties<T>.Instance[o.Src.Index].Getter.AssertNotNull()(row))).ConcatenateSql(SQL($", "))});
                "
                ).OfNonQuery().WithTimeout(timeout).Execute(sqlConn);
            }
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        static (T[] head, IEnumerable<T> fullSequence) PeekAtPrefix(IEnumerable<T> enumerable, int firstN)
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
    }
}
