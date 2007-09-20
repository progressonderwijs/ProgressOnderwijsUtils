using System;
using System.Collections.Generic;
using System.Text;

namespace ProgressOnderwijsUtils
{
	public class NietZoErgeException : ApplicationException
	{
		public NietZoErgeException(string message) : base(message) { }
		public NietZoErgeException(string message, Exception inner) : base(message + " ("+(inner != null ? inner.Message : "InnerException:<null>")+")",inner) { }
	}
}
