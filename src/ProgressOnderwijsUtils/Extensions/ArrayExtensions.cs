using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils
{
	public static class ArrayExtensions
	{
		/// <summary>
		/// Utility method to retrieve a value with a default from an array
		/// </summary>
		/// <param name="array">The array to extract  from</param>
		/// <param name="index">The index whose to get the array element of.</param>
		/// <param name="defaultValue">The value to get if the index is out of range.</param>
		/// <returns>The value at the index, or the default if the array does encompass that index.</returns>
		public static TValue GetOrDefault<TValue>(this TValue[] array, int index, TValue defaultValue)
		{
			return array!=null && index < array.Length && index >= 0 ? array[index] : defaultValue;
		}

		static class Helper<T>
		{
			public static readonly T[] EmptyArray = new T[0];
		}

		/// <summary>
		/// Return an empty array if it's null
		/// </summary>
		public static T[] EmptyIfNull<T>(this T[] array) { return array ?? Helper<T>.EmptyArray; }

	}
}
