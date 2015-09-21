using System;
using System.Linq;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class SortedListExtension
    {
        [Pure]
        public static bool EqualsKeyValue(this RowKey a, RowKey b)
        {
            return a == b ||
                (a != null && b != null
                    && a.Values.SequenceEqual(b.Values)
                    && a.Keys.SequenceEqual(b.Keys, StringComparer.OrdinalIgnoreCase)
                    );
        }
    }
}
