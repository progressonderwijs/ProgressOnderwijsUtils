using System;
using System.Linq;
using System.Collections.Generic;

namespace ProgressOnderwijsUtils
{
	public static class GenericExtensions
	{
		/// <summary>
		/// Pseudo 'in' operation (someObj.In([values])
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="values"></param>
		/// <returns>true/false</returns>
		/// <remarks>
		/// **voorbeelden
		/// 1.In(1,2,3,4); // true
		/// "cando".In("nocando","cando") //true
		/// Enum Vandaag = weekdays.monday;
		/// Vandaag.In(weekdays.thursday,weekdays.friday); //false
		/// </remarks>
		public static bool In<T>(this T obj, params T[] values)
			where T : struct, IConvertible, IComparable
		{ return values.Contains(obj); }

		public static bool In(this string obj, params string[] values)
		{ return values.Contains(obj); }

		public static bool In<T>(this T? obj, params T?[] values)
			where T : struct, IConvertible, IComparable
		{ return values.Contains(obj); }
	}
}
