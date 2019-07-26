using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ProgressOnderwijsUtils
{
    /// <summary>
    /// Equality comparer that will compare on reference equality.
    /// </summary>
    /// <remarks>This might be handy to have collections on reference equality while the elements are value comparable.</remarks>
    public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>, IEqualityComparer
    {
        public bool Equals(T one, T other)
            => ReferenceEquals(one, other);

        public int GetHashCode(T obj)
            => RuntimeHelpers.GetHashCode(obj);

        bool IEqualityComparer.Equals(object x, object y)
            => ReferenceEquals(x, y);

        int IEqualityComparer.GetHashCode(object obj)
            => RuntimeHelpers.GetHashCode(obj);
    }
}
