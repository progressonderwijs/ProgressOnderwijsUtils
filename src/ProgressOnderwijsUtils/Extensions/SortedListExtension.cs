using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
	public static class SortedListExtension
	{
		public static bool EqualsKeyValue(this RowKey a, RowKey b)
		{
			return a == b ||
				(a != null && b != null
					&& a.Values.SequenceEqual(b.Values)
					&& a.Keys.SequenceEqual(b.Keys, StringComparer.OrdinalIgnoreCase)
				);
		}
	}
}
