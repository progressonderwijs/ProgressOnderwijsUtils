﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
	public static class Filter
	{
		public static QueryBuilder ToSqlString(this FilterBase filter, Func<string, string> colRename) { return filter == null ? QueryBuilder.Create("1=1") : filter.ToSqlStringImpl(colRename); }

		public static FilterBase Replace(this FilterBase filter, FilterBase toReplace, FilterBase replaceWith)
		{
			return filter == null ? (toReplace == null ? replaceWith : null)
			       	: filter.ReplaceImpl(toReplace, replaceWith);
		}

		public static FilterBase AddTo(this FilterBase filter, FilterBase filterInEditMode, BooleanOperator booleanOperator, FilterBase c)
		{
			return filter == null
			       	? (filterInEditMode == null ? c : null)
			       	: filter.AddToImpl(filterInEditMode, booleanOperator, c);
		}

		public static QueryBuilder ToSqlString(this FilterBase filter, Dictionary<string, string> computedcolumns)
		{
			if (computedcolumns == null) return filter.ToSqlString(x => x);
			return filter.ToSqlString(col =>
			{
				string outcol;
				if (computedcolumns.TryGetValue(col, out outcol)) return outcol;
				else return col;
			});
		}

		public static FilterBase Remove(this FilterBase filter, FilterBase filterToRemove) { return filter.ReplaceImpl(filterToRemove, null); }

		/// <summary>
		/// Maakt een filter definitie aan.  Om twee kolommen onderling te vergelijken, moet de waarde van type ColumnReference zijn.
		/// </summary>
		public static FilterBase CreateCriterium(string kolomnaam, BooleanComparer comparer, object waarde) { return new CriteriumFilter(kolomnaam, comparer, waarde); }
		public static FilterBase CreateCombined(BooleanOperator andor, FilterBase a, FilterBase b) { return CreateCombined(andor, new[] { a, b }); }
		public static FilterBase CreateCombined(BooleanOperator andor, FilterBase a, FilterBase b, FilterBase c) { return CreateCombined(andor, new[] { a, b, c }); }
		public static FilterBase CreateCombined(BooleanOperator andor, FilterBase a, FilterBase b, FilterBase c, FilterBase d) { return CreateCombined(andor, new[] { a, b, c, d }); }

		public static FilterBase CreateCombined(BooleanOperator andor, IEnumerable<FilterBase> condities)
		{
			var conditiesArr = condities.Where(f => f != null).ToArray();
			if (conditiesArr.Length == 0)
				return null;
			else if (conditiesArr.Length == 1)
				return conditiesArr[0];
			else
			{
				return new CombinedFilter(andor,
				                          conditiesArr.SelectMany(conditie =>
				                                                  conditie is CombinedFilter && ((CombinedFilter)conditie).AndOr == andor
				                                                  	? ((CombinedFilter)conditie).FilterLijst
				                                                  	: new[] { conditie }
				                          	).ToArray()
					);
			}
		}

		public static IEnumerable<Tuple<string, object>> ExtractInsertWaarden(this FilterBase filter)
		{
			var crit = filter as CriteriumFilter;
			var combined = filter as CombinedFilter;
			if (crit != null && crit.Comparer == BooleanComparer.Equal)
				yield return Tuple.Create(crit.KolomNaam, crit.Waarde);
			else if (combined != null && combined.AndOr == BooleanOperator.And)
			{
				bool alleenGeldigeCriteriums =
					combined.FilterLijst.All(f1 => f1 is CriteriumFilter && ((CriteriumFilter)f1).Comparer == BooleanComparer.Equal);

				if (alleenGeldigeCriteriums)
				{
					foreach (CriteriumFilter f1 in combined.FilterLijst)
						yield return Tuple.Create(f1.KolomNaam, f1.Waarde);
				}
			}
		}

		public static bool CanReferenceColumn(this BooleanComparer comparer) { return comparer.In(BooleanComparer.Equal, BooleanComparer.GreaterThan, BooleanComparer.GreaterThanOrEqual, BooleanComparer.LessThan, BooleanComparer.LessThanOrEqual, BooleanComparer.NotEqual); }

		public static string NiceString(this BooleanComparer comparer)
		{
			switch (comparer)
			{
			case BooleanComparer.LessThan:
				return "<";
			case BooleanComparer.LessThanOrEqual:
				return "<=";
			case BooleanComparer.Equal:
				return "=";
			case BooleanComparer.GreaterThanOrEqual:
				return ">=";
			case BooleanComparer.GreaterThan:
				return ">";
			case BooleanComparer.NotEqual:
				return "!=";
			case BooleanComparer.In:
				return "in";
			case BooleanComparer.StartsWith:
				return "starts with";
			case BooleanComparer.Contains:
				return "contains";
			case BooleanComparer.IsNull:
				return "is null";
			case BooleanComparer.IsNotNull:
				return "is not null";
			default:
				throw new InvalidOperationException("Geen geldige operator");
			}
		}

		public static FilterBase ClearFilterWhenItContainsInvalidColumns(this FilterBase filter, Func<string, bool> isColValid) { return filter == null || !filter.ColumnsReferenced.All(isColValid) ? null : filter; }
	}
}