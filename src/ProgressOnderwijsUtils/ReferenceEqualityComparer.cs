using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ProgressOnderwijsUtils
{
    /// <summary>
    /// Equality comparer that will compare on reference equality.
    /// </summary>
    /// <remarks>This might be handy to have collections on reference equality while the elements are value comparable.</remarks>
    public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>, IEqualityComparer
    {
        public static readonly ReferenceEqualityComparer<T> Default = new ReferenceEqualityComparer<T>();
        public bool Equals(T one, T other) { return object.ReferenceEquals(one, other); }
        public int GetHashCode(T obj) { return RuntimeHelpers.GetHashCode(obj); }
        bool IEqualityComparer.Equals(object x, object y) { return object.ReferenceEquals(x, y); }
        int IEqualityComparer.GetHashCode(object obj) { return RuntimeHelpers.GetHashCode(obj); }
    }
}
