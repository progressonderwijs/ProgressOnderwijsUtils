using System;
using System.Collections.Generic;
using System.Text;

namespace ProgressOnderwijsUtils
{
	public class NietZoErgeException : ProgressNetException
	{
		public NietZoErgeException(string message) : base(message) { }
		public NietZoErgeException(string message, Exception inner) : base(message + (inner != null ? " ("+inner.Message +")" : "") ,inner) { }
	}
}
