using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
	/// <summary>
	/// Helper base for metaobjects; uses ValueBase to provide by-field equality.
	/// Also provides handy clone methods.
	/// </summary>
	/// <typeparam name="T">The sealed derived type.</typeparam>
	public abstract class MetaObjectBase<T> : ValueBase<T>, IMetaObject where T : MetaObjectBase<T>, new()
	{
#if DEBUG
		protected MetaObjectBase() { if (!(this is T)) throw new InvalidOperationException("Only T can subclass ValueClass<T>."); }
#endif
	}
}
