using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
    public sealed class DistinctArray<T> : IEnumerable<T>
    {
        readonly T[] items;

        public DistinctArray(IEnumerable<T> items)
        {
            this.items = items.Distinct().ToArray();
        }

        public int Length => items.Length;
        public T this[int index] => items[index];
        public IEnumerator<T> GetEnumerator() => items.Cast<T>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
