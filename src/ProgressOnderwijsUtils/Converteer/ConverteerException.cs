﻿using System;
using System.Runtime.Serialization;

namespace ProgressOnderwijsUtils
{
	public class ConverteerException : ProgressNetException
	{
		public ConverteerException() 
			: base()
		{
		}

		public ConverteerException(string debugMessage) 
			: base(debugMessage)
		{
		}
			
		public ConverteerException(string debugMessage, Exception innerException) 
			: base(debugMessage, innerException)
		{
		}
			
		protected ConverteerException(SerializationInfo info, StreamingContext context) 
			: base(info, context)
		{
		}
	}
}
