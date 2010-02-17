using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

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
	[Serializable]
	public class GenericMetaDataException : ProgressNetException
	{ //TODO: this exception should provide for naming the table and the type of metadata where the error occured.
		public GenericMetaDataException() : base() { }
		public GenericMetaDataException(string debugMessage) : base(debugMessage) { }
		public GenericMetaDataException(string debugMessage, Exception innerException) : base(debugMessage, innerException) { }
		protected GenericMetaDataException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

}
