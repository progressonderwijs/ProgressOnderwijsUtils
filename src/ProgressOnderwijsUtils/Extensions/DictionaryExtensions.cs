using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProgressOnderwijsUtils.Converteer;

namespace ProgressOnderwijsUtils
{
	public static class DictionaryExtensions
	{
		/// <summary>
		/// Casts the boxed objects to a typed representation.  Supports directly unboxing int's into (nullable) enums.
		/// </summary>
		public static T Field<T>(this IDictionary<string, object> dict, string key) { return DBNullRemover.Cast<T>(dict[key]); }

		/// <summary>
		/// Utility method to retrieve a value with a default from a dictionary; you can use GetOrCreateDefault if finding the default is expensive.
		/// </summary>
		/// <param name="dict">The dictionary to extract  from</param>
		/// <param name="key">The key whose value to get.</param>
		/// <param name="defaultValue">The default value of the key.</param>
		/// <returns>The value of the key, or the default if the dictionary does not contain the key.</returns>
		public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
		{
			TValue result;
			return dict.TryGetValue(key, out result) ? result : defaultValue;
		}

		/// <summary>
		/// Utility method to retrieve a value with a default from a dictionary.
		/// </summary>
		/// <param name="dict">The dictionary to extract from</param>
		/// <param name="key">The key whose value to get.</param>
		/// <param name="defaultFactory">The factory method to call to create a default value if not found.</param>
		/// <returns>The value of the key, or the default if the dictionary does not contain the key.</returns>
		public static TValue GetOrCreateDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> defaultFactory)
		{
			TValue result;
			return dict.TryGetValue(key, out result) ? result : defaultFactory();
		}

		/// <summary>
		/// Retrieves the value of a dictionary with setting it to a default if the key does not yet exist.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key">The key whose value to get.</param>
		/// <param name="value">The default value to set if the key does not yet exists.</param>
		/// <returns>The the key existed in the dictionary, its associated value. If not, insert key with th edefault value and return this default.</returns>
		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
		{
			if (!dict.ContainsKey(key))
				dict.Add(key, value);
			return dict[key];
		}

		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> factory)
		{
			if (!dict.ContainsKey(key))
				dict.Add(key, factory());
			return dict[key];
		}
	}

	[TestFixture]
	public class DictionaryExtensionsTests
	{
		[Test]
		public void GetDefault()
		{
			IDictionary<int, int> sut = new Dictionary<int, int> { { 0, 0 } };
			Assert.That(sut.GetOrDefault(0, 1), Is.EqualTo(0));
			Assert.That(sut.GetOrDefault(1, 2), Is.EqualTo(2));
			Assert.That(!sut.ContainsKey(1));
		}

		[Test]
		public void SetDefault()
		{
			IDictionary<int, int> sut = new Dictionary<int, int> { { 0, 0 } };
			Assert.That(sut.GetOrAdd(0, 1), Is.EqualTo(0));
			Assert.That(sut.GetOrAdd(1, 2), Is.EqualTo(2));
			Assert.That(sut.ContainsKey(1));
		}
	}
}
