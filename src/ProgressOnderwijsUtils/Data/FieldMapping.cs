using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using ExpressionToCodeLib;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public readonly struct FieldMapping
    {
        public readonly ColumnDefinition Src;
        public readonly ColumnDefinition Dst;

        FieldMapping(ColumnDefinition src, ColumnDefinition dst)
        {
            Src = src;
            Dst = dst;
        }

        [NotNull]
        public static FieldMapping[] Create([NotNull] ColumnDefinition[] srcColumns, [NotNull] ColumnDefinition[] dstColumns)
        {
            var dstColumnsByName = dstColumns.ToDictionary(o => o.Name ?? "!!UNNAMED COLUMN!!", StringComparer.OrdinalIgnoreCase);

            var list = new List<FieldMapping>(srcColumns.Length + dstColumns.Length);
            foreach (var srcColumn in srcColumns) {
                if (dstColumnsByName.TryGetValue(srcColumn.Name, out var dstColumn)) {
                    dstColumnsByName.Remove(dstColumn.Name);
                    list.Add(new FieldMapping(srcColumn, dstColumn));
                } else {
                    list.Add(new FieldMapping(srcColumn, null));
                }
            }
            list.AddRange(dstColumnsByName.Values.Select(dstColumn => new FieldMapping(null, dstColumn)));

            return list.ToArray();
        }

        public static void ApplyFieldMappingsToBulkCopy([NotNull] FieldMapping[] mapping, [NotNull] SqlBulkCopy bulkCopy)
        {
            bulkCopy.ColumnMappings.Clear();
            foreach (var mapEntry in mapping) {
                bulkCopy.ColumnMappings.Add(mapEntry.Src.Index, mapEntry.Dst.Index);
            }
        }

        [NotNull]
        public static FieldMapping[] VerifyAndCreate(
            [NotNull] ColumnDefinition[] srcColumns,
            string srcName,
            bool allowExtraSrcColumns,
            [NotNull] ColumnDefinition[] dstColumns,
            string dstName,
            bool allowExtraDstColumns)
        {
            var mapping = Create(srcColumns, dstColumns);
            var errors = new List<string>(mapping.Length);
            var mapped = new List<FieldMapping>(mapping.Length);

            foreach (var entry in mapping) {
                if (entry.Src != null && entry.Dst != null) {
                    if (entry.Src.DataType.GetNonNullableUnderlyingType() == entry.Dst.DataType.GetNonNullableUnderlyingType()) {
                        mapped.Add(entry);
                    } else {
                        errors.Add($"Type '{entry.Src.DataType.ToCSharpFriendlyTypeName()}' of property '{srcName}.{entry.Src.Name}' differs from type '{entry.Dst.DataType.ToCSharpFriendlyTypeName()}' of column '{dstName}.{entry.Dst.Name}'.");
                    }
                } else if (entry.Src == null && entry.Dst == null) {
                    errors.Add($"Empty mapping entry is invalid.");
                } else if (entry.Src == null && !allowExtraDstColumns) {
                    errors.Add($"Entity '{srcName}' is missing property '{entry.Dst.Name}' to insert into table '{dstName}'.");
                } else if (entry.Dst == null && !allowExtraSrcColumns) {
                    errors.Add($"Table '{dstName}' has no column '{entry.Src.Name}' to insert from entity '{srcName}'.");
                }
            }

            if (errors.Any()) {
                throw new InvalidOperationException(errors.JoinStrings("\n"));
            } else {
                return mapped.ToArray();
            }
        }
    }
}
