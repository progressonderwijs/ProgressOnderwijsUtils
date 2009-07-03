using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace ProgressOnderwijsUtils
{
	public static class SortedListExtension
	{
		public static bool EqualsKeyValue(this SortedList a, SortedList b)
		{
			return a == b ||
				(a != null && b != null
					&& a.Values.Cast<object>().SequenceEqual(b.Values.Cast<object>())
					&& a.Keys.Cast<object>().SequenceEqual(b.Keys.Cast<object>())
				);
		}
	}
}
