using System.Threading.Tasks;

namespace ProgressOnderwijsUtils;

public static class SmallBatchInsertImplementation
{
    internal const int ThresholdForUsingSqlBulkCopy = 6;

    public static IEnumerable<T>? TrySmallBatchInsertOptimization<T>(SqlConnection sqlConn, BulkInsertTarget bulkInsertTarget, IEnumerable<T> pocos, CommandTimeout timeout)
        where T : IReadImplicitly
    {
        if (!IsCompatibleWithSmallBatchInsert(bulkInsertTarget)) {
            return pocos;
        }
        var (head, all) = PeekAtPrefix(pocos, ThresholdForUsingSqlBulkCopy);
        if (head.Length >= ThresholdForUsingSqlBulkCopy) {
            return all;
        }
        SmallBatchInsert(sqlConn, bulkInsertTarget, head, timeout);
        return null;
    }

    public static async Task<IEnumerable<T>?> TrySmallBatchInsertOptimizationAsync<T>(SqlConnection sqlConn, BulkInsertTarget bulkInsertTarget, IEnumerable<T> pocos, CommandTimeout timeout, CancellationToken cancel)
        where T : IReadImplicitly
    {
        if (!IsCompatibleWithSmallBatchInsert(bulkInsertTarget)) {
            return pocos;
        }
        var (head, all) = PeekAtPrefix(pocos, ThresholdForUsingSqlBulkCopy);
        if (head.Length >= ThresholdForUsingSqlBulkCopy) {
            return all;
        }
        await SmallBatchInsertAsync(sqlConn, bulkInsertTarget, head, timeout, cancel).ConfigureAwait(false);
        return null;
    }

    static bool IsCompatibleWithSmallBatchInsert(BulkInsertTarget bulkInsertTarget)
        // alleen heel specifieke options gedragen zich hetzelfde als "insert into", dus als iemand afwijkende options heeft gekozen: abort.
        => (bulkInsertTarget.Options | SqlBulkCopyOptions.KeepNulls) == (BulkInsertTarget.DefaultOptionsCorrespondingToInsertIntoBehavior | SqlBulkCopyOptions.KeepNulls);

    static void SmallBatchInsert<T>(SqlConnection sqlConn, BulkInsertTarget target, T[] rows, CommandTimeout timeout)
        where T : IReadImplicitly
    {
        if (rows.None()) {
            return;
        }
        var mapping = BuildValidatedMapping<T>(target);
        foreach (var row in rows) {
            BuildInsertRowSql(target, mapping, row).OfNonQuery().WithTimeout(timeout).Execute(sqlConn);
        }
    }

    static async Task SmallBatchInsertAsync<T>(SqlConnection sqlConn, BulkInsertTarget target, T[] rows, CommandTimeout timeout, CancellationToken cancel)
        where T : IReadImplicitly
    {
        if (rows.None()) {
            return;
        }
        var mapping = BuildValidatedMapping<T>(target);
        foreach (var row in rows) {
            await BuildInsertRowSql(target, mapping, row).OfNonQuery().WithTimeout(timeout).ExecuteAsync(sqlConn, cancel).ConfigureAwait(false);
        }
    }

    static BulkInsertFieldMapping[] BuildValidatedMapping<T>(BulkInsertTarget target)
        where T : IReadImplicitly
    {
        var srcFields = PocoProperties<T>.Instance.Where(o => o.CanRead)
            .Select(o => new ColumnDefinition(TypeThatWillBeActuallyInserted(o.DataType), o.Name, o.Index, ColumnAccessibility.Readonly))
            .ToArray();
        var maybeMapping = target.CreateValidatedMapping(srcFields);
        if (!maybeMapping.TryGet(out var mapping, out var error)) {
            throw new InvalidOperationException($"Failed to map source {typeof(T).ToCSharpFriendlyTypeName()} to the table {target.TableName}. Errors:\n{error}");
        }
        return mapping;
    }

    static ParameterizedSql BuildInsertRowSql<T>(BulkInsertTarget target, BulkInsertFieldMapping[] mapping, T row)
        where T : IReadImplicitly
    {
        var destinationColumns = mapping.Select(o => ParameterizedSql.RawSql_PotentialForSqlInjection(o.Dst.Name));
        var sourceValues = mapping.Select(
            o => {
                var fieldVal = PocoProperties<T>.Instance[o.Src.Index].Getter.AssertNotNull()(row);
                return fieldVal == null && !target.Options.HasFlag(SqlBulkCopyOptions.KeepNulls) ? SQL($"default") : ParameterizedSql.Param(fieldVal);
            }
        );
        var tableSql = ParameterizedSql.RawSql_PotentialForSqlInjection(target.TableName);
        var columnSpec = destinationColumns.ConcatenateSql(SQL($","));
        var valuesSpec = sourceValues.ConcatenateSql(SQL($", "));
        return SQL($"insert into {tableSql} ({columnSpec}) values ({valuesSpec});");
    }

    static Type TypeThatWillBeActuallyInserted(Type srcType)
        => AutomaticValueConverters.GetOrNull(srcType.GetNonNullableType())?.ProviderClrType
            switch {
                null => srcType,
                var forNullable when srcType.IsNullableValueType() => forNullable.MakeNullableType() ?? forNullable,
                var forRefOrNonNullable => forRefOrNonNullable,
            };

    public static (T[] head, IEnumerable<T> fullSequence) PeekAtPrefix<T>(IEnumerable<T> enumerable, int firstN)
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
                // ReSharper disable once AssignNullToNotNullAttribute //https://youtrack.jetbrains.com/issue/RSRP-483519
                yield return enumerator.Current;
            }
            enumerator.Dispose();
        }

        return (completeHead, FullSequence());
    }
}
