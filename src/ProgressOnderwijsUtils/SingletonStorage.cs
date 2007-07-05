using System;
using System.Collections.Generic;
using System.Text;

namespace ProgressOnderwijsUtils
{
	/// <summary>
	/// This class provides storage for no more than one object of any given type.  Forces those objects to be subclasses of S.
	/// </summary>
	public class SingletonStorage<S>
	{
		Dictionary<Type, object> backingstore=new Dictionary<Type,object>(1);//this will store at most one object of each type.
		public T Get<T>() where T : S,new()
		{
			lock (this)
			{
				if (backingstore.ContainsKey(typeof(T)))
				{
					return (T)backingstore[typeof(T)];
				}
				else
				{
					T retval = new T();
					backingstore[typeof(T)] = retval;
					return retval;
				}
			}
		}
		public bool Remove<T>() where T : S, new()
		{
			lock(this) return backingstore.Remove(typeof(T));
		}
	}
}
