using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils
{
	public abstract class ResourceStore<T> where T : ResourceStore<T>
	{
		protected ResourceStore()
		{
			if (typeof(T) != GetType())
				throw new InvalidOperationException("Invalid inheritance:\n" +
					GetType().FriendlyName() + " inherits from " +
					GetType().BaseType.FriendlyName() + " but it was expected to inherit from " +
					typeof(ResourceStore<T>).FriendlyName());
		}

		public Stream GetResource(string filename)
		{
			return typeof(T).GetResource(filename);
		}
	}
}
