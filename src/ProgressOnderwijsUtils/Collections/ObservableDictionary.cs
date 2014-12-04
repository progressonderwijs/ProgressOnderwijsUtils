using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils.Collections
{
    public enum DictionaryChange
    {
        Add,
        Set,
        Remove,
        Clear
    }

    public static class ObservableDictionary
    {
        public static IDictionary<TKey, TValue> ObserveChanges<TKey, TValue>(this IDictionary<TKey, TValue> dict, Action<DictionaryChange, TKey, TValue, TValue> onchange)
        {
            if (dict == null) {
                throw new ArgumentNullException("dict");
            }
            if (onchange == null) {
                throw new ArgumentNullException("onchange");
            }
            return new ObservableDictionaryImpl<TKey, TValue>(dict, onchange);
        }

        public static IDictionary<TKey, TValue> ObserveRealChanges<TKey, TValue>(this IDictionary<TKey, TValue> dict, Action onchange) where TValue : IEquatable<TValue>
        {
            if (dict == null) {
                throw new ArgumentNullException("dict");
            }
            if (onchange == null) {
                throw new ArgumentNullException("onchange");
            }
            return new ObservableDictionaryImpl<TKey, TValue>(
                dict,
                (change, key, newval, oldval) => {
                    if (change != DictionaryChange.Set || !ReferenceEquals(newval, oldval) && (ReferenceEquals(newval, null) || !newval.Equals(oldval))) {
                        onchange();
                    }
                });
        }

        sealed class ObservableDictionaryImpl<TKey, TValue> : IDictionary<TKey, TValue>
        {
            readonly IDictionary<TKey, TValue> impl;
            readonly Action<DictionaryChange, TKey, TValue, TValue> changed;

            internal ObservableDictionaryImpl(IDictionary<TKey, TValue> implementationToWrap, Action<DictionaryChange, TKey, TValue, TValue> onchange)
            {
                impl = implementationToWrap;
                changed = onchange;
            }

            public void Add(TKey key, TValue value)
            {
                impl.Add(key, value);
                changed(DictionaryChange.Add, key, value, default(TValue));
            }

            public bool ContainsKey(TKey key) { return impl.ContainsKey(key); }
            public ICollection<TKey> Keys { get { return impl.Keys; } }

            public bool Remove(TKey key)
            {
                TValue oldVal;
                if (impl.TryGetValue(key, out oldVal)) {
                    impl.Remove(key);
                    changed(DictionaryChange.Remove, key, default(TValue), oldVal);
                    return true;
                }
                return false;
            }

            public bool TryGetValue(TKey key, out TValue value) { return impl.TryGetValue(key, out value); }
            public ICollection<TValue> Values { get { return impl.Values; } }

            public TValue this[TKey key]
            {
                get { return impl[key]; }
                set
                {
                    TValue oldVal;
                    if (impl.TryGetValue(key, out oldVal)) {
                        impl[key] = value;
                        changed(DictionaryChange.Set, key, value, oldVal);
                    } else {
                        impl[key] = value;
                        changed(DictionaryChange.Add, key, value, default(TValue));
                    }
                }
            }

            public void Add(KeyValuePair<TKey, TValue> item)
            {
                impl.Add(item);
                changed(DictionaryChange.Add, item.Key, item.Value, default(TValue));
            }

            public void Clear()
            {
                impl.Clear();
                changed(DictionaryChange.Clear, default(TKey), default(TValue), default(TValue));
            }

            public bool Contains(KeyValuePair<TKey, TValue> item) { return impl.Contains(item); }
            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) { impl.CopyTo(array, arrayIndex); }
            public int Count { get { return impl.Count; } }
            public bool IsReadOnly { get { return impl.IsReadOnly; } }

            public bool Remove(KeyValuePair<TKey, TValue> item)
            {
                var retval = impl.Remove(item);
                if (retval) {
                    changed(DictionaryChange.Remove, item.Key, default(TValue), item.Value);
                }
                return retval;
            }

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { return impl.GetEnumerator(); }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return ((System.Collections.IEnumerable)impl).GetEnumerator(); }
        }
    }
}
