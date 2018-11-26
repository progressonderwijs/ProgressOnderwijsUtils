using JetBrains.Annotations;
using System;
using System.Data.SqlClient;
using System.Linq;

namespace ProgressOnderwijsUtils
{
    public enum FieldMappingMode
    {
        RequireExactColumnMatches,
        IgnoreExtraDestinationFields,
    }

    public struct FieldMapping
    {
        public readonly IColumnDefinition SourceColumnDefinition;
        public readonly int SrcIndex, DstIndex;

        FieldMapping(IColumnDefinition sourceColumnDefinition, int srcIndex, int dstIndex)
        {
            SourceColumnDefinition = sourceColumnDefinition;
            SrcIndex = srcIndex;
            DstIndex = dstIndex;
        }

        [NotNull]
        public static FieldMapping[] VerifyAndCreate(
            IColumnDefinition[] srcFields,
            string srcSetDebugName,
            IColumnDefinition[] dstFields,
            string dstSetDebugName,
            FieldMappingMode mode)
        {
            var mkFieldDict = Utils.F(
                (IColumnDefinition[] fields) => fields
                    .Select((colDef, i) => new { Index = i, Def = colDef, UnderlyingType = colDef.DataType.GetNonNullableUnderlyingType() })
                    .ToDictionary(col => col.Def.Name ?? "!!UNNAMED COLUMN!!", StringComparer.OrdinalIgnoreCase)
                );

            var srcFieldsByName = mkFieldDict(srcFields);
            var dstFieldsByName = mkFieldDict(dstFields);
            var colCountMismatch = srcFieldsByName.Count > dstFieldsByName.Count
                || mode == FieldMappingMode.RequireExactColumnMatches && srcFieldsByName.Count < dstFieldsByName.Count;

            if (colCountMismatch || srcFieldsByName.Any(
                srcField =>
                    !dstFieldsByName.ContainsKey(srcField.Key) || dstFieldsByName[srcField.Key].UnderlyingType != srcField.Value.UnderlyingType
                )) {
                var extraSrcCols = srcFieldsByName.Keys.Where(dbcol => !dstFieldsByName.ContainsKey(dbcol)).ToArray();
                var extraDstCols = dstFieldsByName.Keys.Where(prop => !srcFieldsByName.ContainsKey(prop)).ToArray();
                var nameMatchedCols = srcFieldsByName.Keys.Where(dstFieldsByName.ContainsKey);
                var typeMismatchCols = nameMatchedCols.Where(name => srcFieldsByName[name].UnderlyingType != dstFieldsByName[name].UnderlyingType);

                var typeMismatchMessage = !typeMismatchCols.Any()
                    ? ""
                    : "\n\nType mismatches (src <> dst):\n"
                        + typeMismatchCols.Select(
                            name => "  " +
                                srcFieldsByName[name].Def.ToString() + "  <>  " + dstFieldsByName[name].Def.ToString() + "\n"
                            ).JoinStrings();

                throw new InvalidOperationException(
                    "Source " + srcSetDebugName + " has different shape than destination " + dstSetDebugName + " with mode " + mode + ":\n"
                        + (!extraSrcCols.Any() ? "" : "\n" + "Fields only on source " + srcSetDebugName + ": " + string.Join(", ", extraSrcCols))
                        + (!extraDstCols.Any() ? "" : "\n" + "Fields only on destination " + dstSetDebugName + ": " + string.Join(", ", extraDstCols))
                        + typeMismatchMessage
                        + "\n" + "All source " + srcSetDebugName + " fields: " + srcFieldsByName.Select(col => col.Value.Def.ToString()).JoinStrings(", ")
                        + "\n" + "All destination " + dstSetDebugName + " fields: " + dstFieldsByName.Select(col => col.Value.Def.ToString()).JoinStrings(", ")
                    );
            }

            return srcFieldsByName.Values.Select(srcCol => new FieldMapping(srcCol.Def, srcCol.Index, dstFieldsByName[srcCol.Def.Name].Index)).ToArray();
        }

        public static void ApplyFieldMappingsToBulkCopy([NotNull] FieldMapping[] mapping, [NotNull] SqlBulkCopy bulkCopy)
        {
            bulkCopy.ColumnMappings.Clear();
            foreach (var mapEntry in mapping) {
                bulkCopy.ColumnMappings.Add(mapEntry.SrcIndex, mapEntry.DstIndex);
            }
        }
    }
}
