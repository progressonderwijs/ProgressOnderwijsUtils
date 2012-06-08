using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ProgressOnderwijsUtils
{
	[Serializable]
	public abstract class FilterBase : IEquatable<FilterBase>
	{
		protected internal abstract QueryBuilder ToQueryBuilderImpl();
		protected internal abstract FilterBase ReplaceImpl(FilterBase toReplace, FilterBase replaceWith);
		protected internal abstract FilterBase AddToImpl(FilterBase filterInEditMode, BooleanOperator booleanOperator, FilterBase c);
		protected internal abstract IEnumerable<string> ColumnsReferenced { get; }
		protected internal abstract Expression ToMetaObjectFilterExpr<T>(Expression objParamExpr);//where T : IMetaObject;
		public override string ToString() { return ToQueryBuilderImpl().DebugText(); }
		public abstract string SerializeToString();
		public abstract bool Equals(FilterBase other);
	}
}