using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils.Barcode
{
	public class BarcodeException : NietZoErgeException
	{
		public BarcodeException(string message)
			: base(message)
		{
		}
	}
}
