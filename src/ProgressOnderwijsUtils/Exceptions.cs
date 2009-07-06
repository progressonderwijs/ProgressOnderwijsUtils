using System;
using System.Collections.Generic;
using System.Text;

namespace ProgressOnderwijsUtils
{
	public class NoRowsFoundException : ProgressNetException
	{
		public NoRowsFoundException() { }
	}
	public class GeenRechtException : ProgressNetException
	{
		public GeenRechtException(string msg) : base(msg) { }
	}
	public class QueryException : ProgressNetException
	{
		public QueryException(string msg) : base(msg) { }
		public QueryException() { }
		public QueryException(string msg, Exception inner) : base(msg, inner) { }
	}
}
