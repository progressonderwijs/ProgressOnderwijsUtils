using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils
{
	public static class IEnumerableExtensions
	{
		public static int IndexOf<T>(this IEnumerable<T> list, T elem)
		{
			if (list == null) throw new ArgumentNullException("list");
			if (elem == null) throw new ArgumentNullException("elem");
			int retval=0;
			foreach (T item in list)
			{
				if (elem.Equals(item))
					return retval;
				retval++;
			}
			return -1;
		}
	}
}
