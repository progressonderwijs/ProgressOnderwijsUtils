using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils
{
	public class EnumLabellingException : ProgressNetException
	{
		public EnumLabellingException() { }
		public EnumLabellingException(string msg) : base(msg) { }
		public EnumLabellingException(string msg, Exception inner) : base(msg, inner) { }
	}
}
