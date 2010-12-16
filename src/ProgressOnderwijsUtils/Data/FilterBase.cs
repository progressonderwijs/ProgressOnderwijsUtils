using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils
{
	[Serializable]
	public abstract class FilterBase
	{
		protected internal abstract QueryBuilder ToSqlStringImpl(Func<string, string> colRename);
		protected internal abstract FilterBase ReplaceImpl(FilterBase toReplace, CriteriumFilter replaceWith);
		protected internal abstract FilterBase AddToImpl(FilterBase filterInEditMode, BooleanOperator booleanOperator, CriteriumFilter c);
		public override string ToString() { return ToSqlStringImpl(x => x).DebugText(); }
	}
}
