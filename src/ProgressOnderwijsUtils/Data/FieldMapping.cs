using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
    public readonly struct BulkInsertFieldMapping
    {
        public readonly ColumnDefinition Src;
        public readonly ColumnDefinition Dst;

        BulkInsertFieldMapping(ColumnDefinition src, ColumnDefinition dst)
        {
            Src = src;
            Dst = dst;
        }

        [NotNull]
        public static BulkInsertFieldMapping[] Create([NotNull] ColumnDefinition[] srcColumns, [NotNull] ColumnDefinition[] dstColumns)
        {
            var dstColumnsByName = dstColumns.ToDictionary(o => o.Name ?? "!!UNNAMED COLUMN!!", StringComparer.OrdinalIgnoreCase);

            var list = new List<BulkInsertFieldMapping>(srcColumns.Length + dstColumns.Length);
            foreach (var srcColumn in srcColumns) {
                if (dstColumnsByName.TryGetValue(srcColumn.Name, out var dstColumn)) {
                    dstColumnsByName.Remove(dstColumn.Name);
                    list.Add(new BulkInsertFieldMapping(srcColumn, dstColumn));
                } else {
                    list.Add(new BulkInsertFieldMapping(srcColumn, null));
                }
            }
            list.AddRange(dstColumnsByName.Values.Select(dstColumn => new BulkInsertFieldMapping(null, dstColumn)));

            return list.ToArray();
        }

        public static void ApplyFieldMappingsToBulkCopy([NotNull] BulkInsertFieldMapping[] mapping, [NotNull] SqlBulkCopy bulkCopy)
        {
            bulkCopy.ColumnMappings.Clear();
            foreach (var mapEntry in mapping) {
                bulkCopy.ColumnMappings.Add(mapEntry.Src.Index, mapEntry.Dst.Index);
            }
        }

        public static Maybe<BulkInsertFieldMapping[], string> FilterAndValidate([NotNull] BulkInsertFieldMapping[] mapping, bool allowExtraSrcColumns, bool allowExtraDstColumns)
        {
            var errors = new List<string>(mapping.Length);
            var mapped = new List<BulkInsertFieldMapping>(mapping.Length);

            foreach (var entry in mapping) {
                if (entry.Src != null && entry.Dst != null) {
                    if (entry.Src.DataType.GetNonNullableUnderlyingType() == entry.Dst.DataType.GetNonNullableUnderlyingType()) {
                        mapped.Add(entry);
                    } else {
                        errors.Add($"Source field {entry.Src.Name} of type {entry.Src.DataType.ToCSharpFriendlyTypeName()} has a type mismatch with target field {entry.Dst.Name} of type {entry.Dst.DataType.ToCSharpFriendlyTypeName()}.");
                    }
                } else if (entry.Src == null && entry.Dst == null) {
                    errors.Add($"Empty mapping entry is invalid.");
                } else if (entry.Src == null && !allowExtraDstColumns) {
                    errors.Add($"Target field {entry.Dst.Name} of type {entry.Src.DataType.ToCSharpFriendlyTypeName()} is not filled by any corresponding source field.");
                } else if (entry.Dst == null && !allowExtraSrcColumns) {
                    errors.Add($"Source field {entry.Src.Name} of type {entry.Src.DataType.ToCSharpFriendlyTypeName()} does not fill any corresponding target field.");
                }
            }

            if (errors.Any()) {
                return Maybe.Error(errors.JoinStrings("\n"));
            } else {
                return Maybe.Ok(mapped.ToArray());
            }
        }
    }
}
