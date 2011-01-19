using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ProgressOnderwijsUtils
{
	[Serializable]
	public class ProgressNetException : Exception
	{ //TODO: this exception type might provide for TextDef integration such that throwing code can indicate the text-based error to show users.
		public ProgressNetException() : base() { }
		public ProgressNetException(string debugMessage) : base(debugMessage) { }
		public ProgressNetException(string debugMessage, Exception innerException) : base(debugMessage,innerException) { }
		protected ProgressNetException(SerializationInfo info,StreamingContext context) : base(info,context) { }
	}

	/// <summary>
	/// This exception is thrown when an impossible program state is encountered - i.e. when a programmer made a mistake and violated a assumption.
	/// </summary>
	[Serializable]
	public class PNAssertException : ProgressNetException
	{ 
		public PNAssertException() : base() { }
		public PNAssertException(string debugMessage) : base(debugMessage) { }
		public PNAssertException(string debugMessage, Exception innerException) : base(debugMessage, innerException) { }
		protected PNAssertException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
