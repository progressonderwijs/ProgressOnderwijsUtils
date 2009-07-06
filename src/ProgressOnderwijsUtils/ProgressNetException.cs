using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ProgressOnderwijsUtils
{
	public class ProgressNetException : Exception
	{ //TODO: this exception type might provide for TextDef integration such that throwing code can indicate the text-based error to show users.
		public ProgressNetException() : base() { }
		public ProgressNetException(string debugMessage) : base(debugMessage) { }
		public ProgressNetException(string debugMessage, Exception innerException) : base(debugMessage,innerException) { }
		protected ProgressNetException(SerializationInfo info,StreamingContext context) : base(info,context) { }
	}
}
