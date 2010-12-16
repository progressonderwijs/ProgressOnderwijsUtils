using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using ProgressOnderwijsUtils.Data;

namespace ProgressOnderwijsUtils
{
	[Serializable]
	public sealed class CombinedFilter : FilterBase
	{
		readonly BooleanOperator andor;
		readonly FilterBase[] filterLijst;
		public IEnumerable<FilterBase> FilterLijst { get { return filterLijst; } }
		public BooleanOperator AndOr { get { return andor; } }

		internal CombinedFilter(BooleanOperator andor, FilterBase[] condities)
		{
			this.andor = andor;
			filterLijst = condities;
		}

		protected internal override QueryBuilder ToSqlStringImpl(Func<string, string> colRename)
		{
			QueryBuilder andorQ = QueryBuilder.Create(" " + andor + " ");
			return "(" + filterLijst.Aggregate(default(QueryBuilder), (q, f) => q == null ? f.ToSqlString(colRename) : q + andorQ + f.ToSqlString(colRename)) + ")";
		}

		protected internal override FilterBase ReplaceImpl(FilterBase toReplace, CriteriumFilter replaceWith)
		{
			return this == toReplace
					? (FilterBase)replaceWith
					: Filter.CreateCombined(AndOr, filterLijst.Select(child => child.ReplaceImpl(toReplace, replaceWith)));
		}

		protected internal override FilterBase AddToImpl(FilterBase filterInEditMode, BooleanOperator booleanOperator, CriteriumFilter c)
		{
			return filterInEditMode == this
				? Filter.CreateCombined(booleanOperator, this, c)
				: Filter.CreateCombined(AndOr, FilterLijst.Select(f => f.AddToImpl(filterInEditMode, booleanOperator, c)));
		}
	}
}
