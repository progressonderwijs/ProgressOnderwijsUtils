using System.Linq;
using System.Collections.Generic;
using System;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils.Data
{
	public struct FieldMapping
	{
		public readonly int SrcIndex, DstIndex;
		FieldMapping(int srcIndex, int dstIndex) { SrcIndex = srcIndex; DstIndex = dstIndex; }

		public static FieldMapping[] VerifyAndCreate(DbColumnDefinition[] srcFields, string srcSetDebugName, DbColumnDefinition[] dstFields, string dstSetDebugName)
		{
			var mkFieldDict = Utils.F(
				(DbColumnDefinition[] fields) => fields
					.Select((colDef, i) => new { Index = i, Def = colDef, UnderlyingType = colDef.Type.GetNonNullableUnderlyingType() })
					.ToDictionary(col => col.Def.ColumnName ?? "!!UNNAMED COLUMN!!", StringComparer.OrdinalIgnoreCase)
				);

			var srcFieldsByName = mkFieldDict(srcFields);
			var dstFieldsByName = mkFieldDict(dstFields);

			if (srcFieldsByName.Count != dstFieldsByName.Count || srcFieldsByName.Any(dbCol => !dstFieldsByName.ContainsKey(dbCol.Key) || dstFieldsByName[dbCol.Key].UnderlyingType != dbCol.Value.UnderlyingType))
			{

				var extraSrcCols = srcFieldsByName.Keys.Where(dbcol => !dstFieldsByName.ContainsKey(dbcol)).ToArray();
				var extraDstCols = dstFieldsByName.Keys.Where(prop => !srcFieldsByName.ContainsKey(prop)).ToArray();
				var nameMatchedCols = srcFieldsByName.Keys.Where(dstFieldsByName.ContainsKey);
				var typeMismatchCols = nameMatchedCols.Where(name => srcFieldsByName[name].UnderlyingType != dstFieldsByName[name].UnderlyingType);

				string typeMismatchMessage = (!typeMismatchCols.Any() ? "" :
																			   "\n\nType mismatches (src <> dst):\n"
																				   + typeMismatchCols.Select(name => "  " +
																						srcFieldsByName[name].Def.ToString() + "  <>  " + dstFieldsByName[name].Def.ToString() + "\n"
																					   ).JoinStrings()
					);

				throw new InvalidOperationException("Source " + srcSetDebugName + " has different shape than destination " + dstSetDebugName + ":\n"
					+ (!extraSrcCols.Any() ? "" : "\n" + "Fields only on source " + srcSetDebugName + ": " + String.Join(", ", extraSrcCols))
					+ (!extraDstCols.Any() ? "" : "\n" + "Fields only on destination " + dstSetDebugName + ": " + String.Join(", ", extraDstCols))
					+ typeMismatchMessage
					+ "\n" + "All source " + srcSetDebugName + " fields: " + srcFieldsByName.Select(col => col.Value.Def.ToString()).JoinStrings(", ")
					+ "\n" + "All destination " + dstSetDebugName + " fields: " + dstFieldsByName.Select(col => col.Value.Def.ToString()).JoinStrings(", ")
					);
			}

			return srcFieldsByName.Values.Select(srcCol => new FieldMapping(srcCol.Index, dstFieldsByName[srcCol.Def.ColumnName].Index)).ToArray();
		}
	}
}