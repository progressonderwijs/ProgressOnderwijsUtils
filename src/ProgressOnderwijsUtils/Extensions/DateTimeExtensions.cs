using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils
{
	public static class DateTimeExtensions
	{

		public static int CollegeJaar(this DateTime datetime)
		{
			int collegejaar = datetime.Year;
			//Als startdatum januari t/m augustus dan is het collegejaar een jaar eerder
			if (datetime.Month < 9)
				collegejaar--;
			return collegejaar;
		}
	}
}
