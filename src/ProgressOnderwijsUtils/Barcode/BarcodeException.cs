using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils.Barcode
{
	[Serializable]
	public class BarcodeException : NietZoErgeException
	{
		public BarcodeException(string message)
			: base(message)
		{
		}
	}
}
