using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
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
    }

    public struct FieldMappingValidation
    {
        public bool AllowExtraSourceColumns;
        public bool AllowExtraTargetColumns;
        public bool OverwriteAutoIncrement;

        public Maybe<BulkInsertFieldMapping[], string> ValidateAndFilter([NotNull] BulkInsertFieldMapping[] mapping)
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

                    var typesMatchError = TypesMatchError(src, dst);
                    if (typesMatchError != null) {
                        errors.Add(typesMatchError);
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

        static string TypesMatchError(ColumnDefinition src, ColumnDefinition dst)
        {
            if (src.DataType.Attr<MetaObjectPropertyConvertibleAttribute>() != null) {
                if (typeof(IConvertible).IsAssignableFrom(src.DataType)) {
                    var instance = Activator.CreateInstance(src.DataType);
                    var sourceTypeCode = ((IConvertible)instance).GetTypeCode();
                    var destinationTypeCode = Type.GetTypeCode(dst.DataType);
                    return sourceTypeCode == destinationTypeCode
                        ? null
                        : $"Source field {src.Name} of TypeCode {sourceTypeCode} is not convertible to target field {dst.Name} of TypeCode {destinationTypeCode}.";
                } else {
                    return $"Source field {src.Name} of type {src.DataType.ToCSharpFriendlyTypeName()} has the {nameof(MetaObjectPropertyConvertibleAttribute)} but does not implement {nameof(IConvertible)}.";
                }
            } else if (src.DataType.GetNonNullableUnderlyingType() == dst.DataType.GetNonNullableUnderlyingType()) {
                return null;
            } else {
                return $"Source field {src.Name} of type {src.DataType.ToCSharpFriendlyTypeName()} has a type mismatch with target field {dst.Name} of type {dst.DataType.ToCSharpFriendlyTypeName()}.";
            }
        }
    }
}
