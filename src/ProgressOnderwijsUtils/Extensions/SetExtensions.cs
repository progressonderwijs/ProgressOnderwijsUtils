using System;
using System.Collections.Generic;
using System.Linq;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
	public static class SetExtensions
	{
		public static ReadOnlySet<T> AsReadOnly<T>(this ISet<T> set)
		{
			return new ReadOnlySet<T>(set);
		}
	}
}
