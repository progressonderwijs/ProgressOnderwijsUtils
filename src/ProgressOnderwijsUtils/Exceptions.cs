using System;
using System.Collections.Generic;
using System.Text;

namespace ProgressOnderwijsUtils
{
	public class NoRowsFoundException : ApplicationException { }
	public class GeenRechtException : ApplicationException 
	{
		public GeenRechtException(string msg) : base(msg) { }
	}
	public class QueryException : ApplicationException
	{
		public QueryException(string msg) : base(msg) { }
		public QueryException() { }
		public QueryException(string msg, Exception inner) : base(msg, inner) { }
	}
}
