using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils
{
	public static class DateTimeExtensions
	{
		/// <summary>
		/// Geeft het collegejaar van een bepaalde datum
		/// voorbeelden :
		/// 04-05-2009 -> 2008
		/// 01-09-2008 -> 2008
		/// </summary>
		/// <param name="datetime"></param>
		/// <returns></returns>
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
