using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
    public readonly struct BulkInsertFieldMapping
    {
        public readonly ColumnDefinition? Src;
        public readonly ColumnDefinition? Dst;

        BulkInsertFieldMapping(ColumnDefinition? src, ColumnDefinition? dst)
        {
            Src = src;
            Dst = dst;
        }

        public static BulkInsertFieldMapping[] Create(ColumnDefinition[] srcColumns, ColumnDefinition[] dstColumns)
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

        public static void ApplyFieldMappingsToBulkCopy(BulkInsertFieldMapping[] mapping, SqlBulkCopy bulkCopy)
        {
            bulkCopy.ColumnMappings.Clear();
            foreach (var mapEntry in mapping) {
                bulkCopy.ColumnMappings.Add(mapEntry.Src! /*dubious*/.Index, mapEntry.Dst! /*dubious*/.Index);
            }
        }
    }

    public struct FieldMappingValidation
    {
        public bool AllowExtraSourceColumns;
        public bool AllowExtraTargetColumns;
        public bool OverwriteAutoIncrement;

        public Maybe<BulkInsertFieldMapping[], string> ValidateAndFilter(BulkInsertFieldMapping[] mapping)
        {
            var errors = new List<string>(mapping.Length);
            var mapped = new List<BulkInsertFieldMapping>(mapping.Length);

            foreach (var entry in mapping) {
                var src = entry.Src;
                var dst = entry.Dst;
                if (dst == null) {
                    if (src == null) {
                        errors.Add($"Empty mapping entry is invalid.");
                    } else if (!AllowExtraSourceColumns) {
                        errors.Add($"Source field {src.Name} of type {src.DataType.ToCSharpFriendlyTypeName()} does not fill any corresponding target field.");
                    }
                } else if (src == null) {
                    if (dst.ColumnAccessibility == ColumnAccessibility.Normal || dst.ColumnAccessibility == ColumnAccessibility.NormalWithDefaultValue) {
                        if (!AllowExtraTargetColumns) {
                            errors.Add($"Target field {dst.Name} of type {dst.DataType.ToCSharpFriendlyTypeName()} is not filled by any corresponding source field.");
                        }
                    } else if (dst.ColumnAccessibility == ColumnAccessibility.AutoIncrement) {
                        if (OverwriteAutoIncrement) {
                            errors.Add($"Target auto-increment field {dst.Name} of type {dst.DataType.ToCSharpFriendlyTypeName()} is not filled by any corresponding source field.");
                        }
                    } else if (dst.ColumnAccessibility != ColumnAccessibility.Readonly) {
                        throw new Exception("impossible value " + dst.ColumnAccessibility);
                    }
                } else {
                    //src & dst not null

                    if (src.DataType.GetNonNullableUnderlyingType() != dst.DataType.GetNonNullableUnderlyingType()) {
                        errors.Add($"Source field {src.Name} of type {src.DataType.ToCSharpFriendlyTypeName()} has a type mismatch with target field {dst.Name} of type {dst.DataType.ToCSharpFriendlyTypeName()}.");
                    } else if (dst.ColumnAccessibility == ColumnAccessibility.Readonly) {
                        errors.Add($"Cannot fill readonly field {dst.Name}.");
                    } else if (dst.ColumnAccessibility != ColumnAccessibility.AutoIncrement || OverwriteAutoIncrement) {
                        mapped.Add(entry);
                    }
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
