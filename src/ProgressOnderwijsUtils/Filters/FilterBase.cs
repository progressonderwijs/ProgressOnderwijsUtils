using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
	[Serializable] 
	public abstract class FilterBase : IEquatable<FilterBase>
	{
		protected internal abstract QueryBuilder ToQueryBuilderImpl();
		protected internal abstract FilterBase ReplaceImpl(FilterBase toReplace, FilterBase replaceWith);
		protected internal abstract FilterBase AddToImpl(FilterBase filterInEditMode, BooleanOperator booleanOperator, FilterBase c);
		protected internal abstract IEnumerable<string> ColumnsReferenced { get; }
		public override string ToString() { return ToQueryBuilderImpl().DebugText(); }
		public abstract bool Equals(FilterBase other);
	}
}