using System.Linq;
using System.Collections.Generic;
using System;
using MoreLinq;

namespace ProgressOnderwijsUtils
{
	sealed class QueryStringComponent : IQueryComponent
	{
		public readonly string val;
		public string ToSqlString(CommandFactory qnum) { return val; }
		internal QueryStringComponent(string val)
		{
			if (val == null) throw new ArgumentNullException("val");
			this.val = val;
		}

		public string ToDebugText(Taal? taalOrNull) { return val; }

		public bool Equals(IQueryComponent other) { return (other is QueryStringComponent) && val == ((QueryStringComponent)other).val; }
		public override bool Equals(object obj) { return (obj is QueryStringComponent) && Equals((QueryStringComponent)obj); }
		public override int GetHashCode() { return val.GetHashCode() + 31; }
	}
}