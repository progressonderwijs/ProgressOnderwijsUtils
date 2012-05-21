using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ProgressOnderwijsUtils
{
	public abstract class ValueClass<T> : ByValueHelperBase<T>, IMetaObject where T : ValueClass<T>, new()
	{

#if DEBUG
		protected ValueClass() { if (!(this is T)) throw new InvalidOperationException("Only T can subclass ValueClass<T>."); }
#endif
		public T Copy() { return (T)MemberwiseClone(); }
		public T CopyWith(Action<T> action) { var copied = Copy(); action(copied); return copied; }
	}
}
