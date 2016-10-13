using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
    public struct DistinctArray<T> : IReadOnlyList<T>
    {
        readonly T[] items;

        public DistinctArray(IEnumerable<T> items)
        {
            this.items = items.Distinct().ToArray();
        }

        public int Count => items.Length;
        public T this[int index] => items[index];
        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)items).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
