using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils.Extensions
{
	/// <summary>
	/// Extension methods for generic collections.
	/// </summary>
	public static class CollectionExtensions
	{
		/// <summary>
		/// Utility method to retrieve a value with a default from a dictionary.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key">The key whose value to get.</param>
		/// <param name="defaultValue">The default value of the key.</param>
		/// <returns>The value of the key, or the default if the dictionary does not contain the key.</returns>
		public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
		{
			TValue result;
			if (!dict.TryGetValue(key, out result))
			{
				result = defaultValue;
			}
			return result;
		}
	}
}
