using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ProgressOnderwijsUtils.Extensions
{
	/// <summary>
	/// Extension methods for generic collections.
	/// </summary>
	public static class CollectionExtensions
	{
		/// <summary>
		/// Derived query to test whether a collection is empty or not.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection"></param>
		/// <returns>true when Count == 0, false otherwise</returns>
		public static bool Empty<T>(this ICollection<T> collection)
		{
			return collection.Count == 0;
		}

		/// <summary>
		/// Utility method to retrieve a value with a default from a dictionary.
		/// </summary>
		/// <param name="dict">The dictionary to extract  from</param>
		/// <param name="key">The key whose value to get.</param>
		/// <param name="value">The default value of the key.</param>
		/// <returns>The value of the key, or the default if the dictionary does not contain the key.</returns>
		public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
		{
			TValue result;
			return dict.TryGetValue(key, out result) ? result : value;
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
	}

	[TestFixture]
	public class CollectionExtensionsTests
	{
		[Test]
		public void Empty()
		{
			Assert.That(new List<int>().Empty());
			Assert.That(!new Dictionary<int, int>() { { 0, 0 } }.Empty());
		}

		[Test]
		public void GetDefault()
		{
			IDictionary<int, int> sut = new Dictionary<int, int>() { { 0, 0 } };
			Assert.That(sut.GetOrDefault(0, 1), Is.EqualTo(0));
			Assert.That(sut.GetOrDefault(1, 2), Is.EqualTo(2));
			Assert.That(!sut.ContainsKey(1));
		}

		[Test]
		public void SetDefault()
		{
			IDictionary<int, int> sut = new Dictionary<int, int>() { { 0, 0 } };
			Assert.That(sut.GetOrAdd(0, 1), Is.EqualTo(0));
			Assert.That(sut.GetOrAdd(1, 2), Is.EqualTo(2));
			Assert.That(sut.ContainsKey(1));
		}
	}
}
