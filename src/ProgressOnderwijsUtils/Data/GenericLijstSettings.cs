using System;
using System.Collections;
using System.Collections.Generic;

namespace ProgressOnderwijsUtils
{
	[Serializable]
	public class RowKey : IEnumerable<KeyValuePair<string, object>>
	{
		readonly SortedList<string, object> underlying;
		public RowKey(Dictionary<string, object> dict)
		{
			underlying = dict.ToSortedList(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
		}

		public RowKey() { underlying = new SortedList<string, object>(StringComparer.OrdinalIgnoreCase); }
		public object this[string column] { get { return underlying.GetOrDefault(column); } }
		public void Add(string column, object value)
		{
			underlying.Add(column, value);
		}

		IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() { return underlying.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return underlying.GetEnumerator(); }
		public int Count { get { return underlying.Count; } }
		public IList<string> Keys { get { return underlying.Keys; } }
		public IList<object> Values { get { return underlying.Values; } }
		public bool ContainsKey(string studentid) { return underlying.ContainsKey(studentid); }
	}

	/// <summary>
	/// Slaat de selectedkey, de currentpage en de datasource settings van een genericlijst op.
	/// </summary>
	[Serializable]
	public sealed class GenericLijstSettings
	{
		public RowKey SelectedKey;
		public int CurrentPage;
	}
}
