using System;
using System.Runtime.Caching;

namespace ProgressOnderwijsUtils
{
	public static class ObjectCacheExtensions
	{
		public static T GetOrAdd<T>(this ObjectCache cache,
			string key, Func<T> factory, DateTimeOffset absoluteExpiration, string regionName = null)
		{
			return cache.GetOrAdd(key, factory, new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration }, regionName);
		}

		public static T GetOrAdd<T>(this ObjectCache cache,
			string key, Func<T> factory, CacheItemPolicy policy, string regionName = null)
		{
			var newValue = new Lazy<T>(factory);
			var value = cache.AddOrGetExisting(key, newValue, policy, regionName) as Lazy<T>;
			return (value ?? newValue).Value;
		}
	}
}
