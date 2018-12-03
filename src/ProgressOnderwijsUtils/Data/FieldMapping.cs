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
        public readonly IColumnDefinition Src;
        public readonly IColumnDefinition Dst;

        FieldMapping(IColumnDefinition src, IColumnDefinition dst)
        {
            Src = src;
            Dst = dst;
        }

        [NotNull]
        public static FieldMapping[] Create([NotNull] IColumnDefinition[] srcColumns, [NotNull] IColumnDefinition[] dstColumns)
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
            [NotNull] IColumnDefinition[] srcColumns,
            string srcName,
            bool allowExtraSrcColumns,
            [NotNull] IColumnDefinition[] dstColumns,
            string dstName,
            bool allowExtraDstColumns)
        {
            var mapping = Create(srcColumns, dstColumns);
            var errors = new List<string>(mapping.Length);
            var mapped = new List<FieldMapping>(mapping.Length);

            foreach (var entry in mapping) {
                if (entry.Src == null && entry.Dst == null) {
                    throw new ArgumentException();
                } else {
                    if (entry.Src == null && !allowExtraDstColumns) {
                        errors.Add($"Entity '{srcName}' is missing property '{entry.Dst.Name}' to insert into table '{dstName}'.");
                    }

                    if (entry.Dst == null && !allowExtraSrcColumns) {
                        errors.Add($"Table '{dstName}' has no column '{entry.Src.Name}' to insert from entity '{srcName}'.");
                    }

                    if (entry.Src != null && entry.Dst != null) {
                        if (entry.Src.DataType.GetNonNullableUnderlyingType() != entry.Dst.DataType.GetNonNullableUnderlyingType()) {
                            errors.Add($"Type '{entry.Src.DataType.ToCSharpFriendlyTypeName()}' of property '{srcName}.{entry.Src.Name}' differs from type '{entry.Dst.DataType.ToCSharpFriendlyTypeName()}' of column '{dstName}.{entry.Dst.Name}'.");
                        } else {
                            mapped.Add(entry);
                        }
                    }
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
