using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
	public static class NullableExtensions
	{
		public static TResult? Select<TSource, TResult>(this TSource? source, Func<TSource, TResult> selector)
			where TSource : struct
			where TResult : struct
		{
			return source.HasValue ? selector(source.Value) : default(TResult?);
		}

		public static object OrDbNull<TSource>(this TSource? source)
			where TSource : struct
		{
			return (object) source ?? DBNull.Value;
		}

		public static object OrDbNull(this string source)
		{
			return (object)source ?? DBNull.Value;
		}
	}
}
