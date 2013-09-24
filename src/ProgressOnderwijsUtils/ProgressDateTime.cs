using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
	/// <summary>
	/// Deze class maakt het mogelijk om de tijd terug of vooruit te zetten.
	/// </summary>
	public class ProgressDateTime
	{
		/// <summary>
		/// Het aantal dagen dat de tijd terug of vooruit moet worden gezet
		/// </summary>
		public static int DaysToAdd { get; set; }

		/// <summary>
		/// De datum die vanaf nu als huidige datum moet gelden 
		/// (de Datetime wordt afgerond naar hele dagen)
		/// </summary>
		/// <param name="value"></param>
		public static void SetDate(DateTime value)
		{
			// Als het nu 21-9 is en value is 21-8 dan is DaysToAdd -31
			DaysToAdd = value.Date.Subtract(DateTime.Now.Date).Days;
		}

		/// <summary>
		/// Zet de datum weer terug naar vandaag
		/// </summary>
		public static void Reset()
		{
			DaysToAdd = 0;
		}

		/// <summary>
		/// De (voor de software) huidige datumtijd
		/// </summary>
		public static DateTime Now
		{
			get
			{
				return DateTime.Now.AddDays(DaysToAdd);
			}
		}

		/// <summary>
		/// De (voor de software) huidige datum
		/// </summary>
		public static DateTime Today
		{
			get
			{
				return DateTime.Today.AddDays(DaysToAdd);
			}
		}
	}
}
