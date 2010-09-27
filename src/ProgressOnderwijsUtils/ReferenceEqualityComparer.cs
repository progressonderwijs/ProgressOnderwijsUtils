using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace ProgressOnderwijsUtils
{
	/// <summary>
	/// Equality comparer that will compare on reference equality.
	/// </summary>
	/// <remarks>This might be handy to have collections on reference equality while the elements are value comparable.</remarks>
	public class ReferenceEqualityComparer<T> : IEqualityComparer<T>
	{
		public bool Equals(T one, T other) { return object.ReferenceEquals(one, other); }
		public int GetHashCode(T obj) { return RuntimeHelpers.GetHashCode(obj); }
	}
}
