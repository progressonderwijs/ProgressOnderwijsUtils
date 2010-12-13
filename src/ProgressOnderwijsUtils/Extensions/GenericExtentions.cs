using System.Linq;
using System.Collections.Generic;

namespace ProgressOnderwijsUtils.Extensions
{
	public static class GenericExtentions
	{
		/// <summary>
		/// Pseudo 'in' operation (someObj.In([values])
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="values"></param>
		/// <returns>true/false</returns>
		/// <remarks>
		/// **voorbeelden
		/// 1.In(1,2,3,4); // false
		/// "cando".In("nocando","cando") //true
		/// Enum Vandaag = weekdays.monday;
		/// Vandaag.In(weekdays.thursday,weekdays.friday); //false
		/// </remarks>
		public static bool In<T>(this T obj, params T[] values)
		{
			return values.Contains(obj);
		}

		/// <summary>
		/// Pseudo 'in' operation (someObj.In(List of [values])
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="valueList"></param>
		/// <returns>true/false</returns>
		public static bool In<T>(this T obj, List<T> valueList)
		{
			return valueList.Contains(obj);
		}

	}
}
