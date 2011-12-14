using System;
using System.Linq;
using System.Collections.Generic;

namespace ProgressOnderwijsUtils
{
	[Serializable]
	public sealed class CombinedFilter : FilterBase, IEquatable<CombinedFilter>
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

		protected internal override IEnumerable<string> ColumnsReferenced { get { return FilterLijst.SelectMany(f => f.ColumnsReferenced); } }

		protected internal override QueryBuilder ToQueryBuilderImpl()
		{
			QueryBuilder andorQ = QueryBuilder.Create(" " + andor + " ");
			return "(" + filterLijst.Aggregate(default(QueryBuilder), (q, f) => null == q ? f.ToQueryBuilder() : q + andorQ + f.ToQueryBuilder()) + ")";
		}

		protected internal override FilterBase ReplaceImpl(FilterBase toReplace, FilterBase replaceWith) { return this == toReplace ? replaceWith : Filter.CreateCombined(AndOr, filterLijst.Select(child => child.ReplaceImpl(toReplace, replaceWith))); }

		protected internal override FilterBase AddToImpl(FilterBase filterInEditMode, BooleanOperator booleanOperator, FilterBase c)
		{
			return filterInEditMode == this
					? Filter.CreateCombined(booleanOperator, this, c)
					: Filter.CreateCombined(AndOr, FilterLijst.Select(f => f.AddToImpl(filterInEditMode, booleanOperator, c)));
		}

		public override int GetHashCode()
		{
			long val = andor.GetHashCode() + filterLijst.Select((f, i) => f.GetHashCode() * ((long)2 * i + 1)).Sum();
			return (int)((uint)val ^ ((uint)val >> 32));
		}

		public override bool Equals(FilterBase other) { return Equals(other as CombinedFilter); }
		public override bool Equals(object obj) { return Equals(obj as CombinedFilter); }
		public bool Equals(CombinedFilter obj) { return obj != null && andor == obj.andor && FilterLijst.SequenceEqual(obj.FilterLijst); }
	}
}