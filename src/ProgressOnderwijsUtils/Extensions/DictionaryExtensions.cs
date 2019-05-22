using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using MoreLinq;

namespace ProgressOnderwijsUtils
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Casts the boxed objects to a typed representation.  Supports directly unboxing int's into (nullable) enums.
        /// </summary>
        public static T Field<T>([NotNull] this IDictionary<string, object> dict, [NotNull] string key)
        {
            return FromDbValueConverter.Cast<T>(dict[key]);
        }

        /// <summary>
        /// Casts the boxed objects to a typed representation.  Supports directly unboxing int's into (nullable) enums.
        /// </summary>
        [UsefulToKeep("library method; interface is used, since method above is used")]
        public static T Field<T>([NotNull] this IReadOnlyDictionary<string, object> dict, string key)
        {
            return FromDbValueConverter.Cast<T>(dict[key]);
        }

        /// <summary>
        /// Utility method to retrieve a value with a default from a dictionary; you can use GetOrLazyDefault if finding the default is expensive.
        /// </summary>
        /// <param name="dict">The dictionary to extract  from</param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="defaultValue">The default value of the key.</param>
        /// <returns>The value of the key, or the default if the dictionary does not contain the key.</returns>
        public static TValue GetOrDefaultR<TKey, TValue>(
            [NotNull] this IReadOnlyDictionary<TKey, TValue> dict,
            [NotNull] TKey key,
            TValue defaultValue)
        {
            return dict.TryGetValue(key, out var result) ? result : defaultValue;
        }

        public static TValue GetOrDefault<TKey, TValue>(
            [NotNull] this IDictionary<TKey, TValue> dict,
            [NotNull] TKey key,
            TValue defaultValue)
        {
            return dict.TryGetValue(key, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// Utility method to retrieve a value with a default from a dictionary; you can use GetOrLazyDefault if finding the default is expensive.
        /// </summary>
        /// <param name="dict">The dictionary to extract  from</param>
        /// <param name="key">The key whose value to get.</param>
        /// <returns>The value of the key, or the default if the dictionary does not contain the key.</returns>
        public static TValue GetOrDefaultR<TKey, TValue>([NotNull] this IReadOnlyDictionary<TKey, TValue> dict, [NotNull] TKey key)
        {
            return GetOrDefaultR(dict, key, default(TValue));
        }

        public static TValue GetOrDefault<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> dict, [NotNull] TKey key)
        {
            return GetOrDefault(dict, key, default(TValue));
        }

        /// <summary>
        /// Utility method to retrieve a value with a default from a dictionary; you can use GetOrCreateDefault if finding the default is expensive.
        /// </summary>
        /// <param name="dict">The dictionary to extract  from</param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="defaultValue">The default value of the key.</param>
        /// <returns>The value of the key, or the default if the dictionary does not contain the key.</returns>
        public static TValue? GetOrDefaultR<TKey, TValue>(
            [NotNull] this IReadOnlyDictionary<TKey, TValue> dict,
            [NotNull] TKey key,
            TValue? defaultValue)
            where TValue : struct
        {
            return dict.TryGetValue(key, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// Utility method to retrieve a value with a default from a dictionary; you can use GetOrCreateDefault if finding the default is expensive.
        /// </summary>
        /// <param name="dict">The dictionary to extract  from</param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="defaultValue">The default value of the key.</param>
        /// <returns>The value of the key, or the default if the dictionary does not contain the key.</returns>
        [UsefulToKeep("library method; interface is used, since method above is used")]
        public static TValue? GetOrDefault<TKey, TValue>(
            [NotNull] this IDictionary<TKey, TValue> dict,
            [NotNull] TKey key,
            TValue? defaultValue)
            where TValue : struct
        {
            return dict.TryGetValue(key, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// Utility method to retrieve a value with a default from a dictionary.
        /// </summary>
        /// <param name="dict">The dictionary to extract from</param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="defaultFactory">The factory method to call to create a default value if not found.</param>
        /// <returns>The value of the key, or the default if the dictionary does not contain the key.</returns>
        public static TValue GetOrLazyDefault<TKey, TValue>(
            [NotNull] this IDictionary<TKey, TValue> dict,
            [NotNull] TKey key,
            Func<TValue> defaultFactory)
        {
            return dict.TryGetValue(key, out var result) ? result : defaultFactory();
        }

        /// <summary>
        /// Retrieves a value from a dictionary or adds a new value if the key does not yet exist.  An overload to lazily create the new value also exists.
        /// </summary>
        /// <param name="dict">The dictionary possibly containing the key</param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">The default value to set if the key does not yet exists.</param>
        /// <returns>The value corresponding to the key in the dictionary (which may have just been added).</returns>
        public static TValue GetOrAdd<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> dict, [NotNull] TKey key, TValue value)
        {
            if (!dict.ContainsKey(key)) {
                dict.Add(key, value);
            }
            return dict[key];
        }

        /// <summary>
        /// Retrieves a value from a dictionary or adds a new value if the key does not yet exist.
        /// </summary>
        /// <param name="dict">The dictionary possibly containing the key</param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="factory">The factory to create the value if the key does not yet exists.</param>
        /// <returns>The value corresponding to the key in the dictionary (which may have just been added).</returns>
        public static TValue GetOrAdd<TKey, TValue>(
            [NotNull] this IDictionary<TKey, TValue> dict,
            [NotNull] TKey key,
            Func<TKey, TValue> factory)
        {
            if (dict.TryGetValue(key, out var val)) {
                return val;
            }
            val = factory(key);
            dict.Add(key, val);
            return val;
        }

        [NotNull]
        public static Dictionary<TKey, TValue> Clone<TKey, TValue>([NotNull] this Dictionary<TKey, TValue> old)
            => new Dictionary<TKey, TValue>(old, old.Comparer);

        /// <summary>
        /// Merges two dictionaries. When both dictionaries contain the same key, the last value is used
        /// </summary>
        /// <param name="old">This dictionary</param>
        /// <param name="others">The dictionary which should be merged into this array</param>
        [CanBeNull]
        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
            [NotNull] this Dictionary<TKey, TValue> old,
            [NotNull] params Dictionary<TKey, TValue>[] others)
        {
            if (old == null) {
                throw new ArgumentNullException(nameof(old));
            }

            var merged = old.Clone();
            others.SelectMany(other => other)
                .ForEach(kv => merged[kv.Key] = kv.Value);

            return merged;
        }

        [NotNull]
        public static ReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        {
            return new ReadOnlyDictionary<TKey, TValue>(dict);
        }
    }
}
