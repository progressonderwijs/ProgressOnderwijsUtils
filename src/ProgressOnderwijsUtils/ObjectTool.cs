using System;

namespace ProgressOnderwijsUtils
{
	/// <summary>
	/// Summary description for formatstring.
	/// </summary>
	public class objecttool
	{
		

		public static string toString(object o , string defaultreturnwaarde)
		{
			string returnwaarde = defaultreturnwaarde;
			if (o != DBNull.Value) 
			{
				if (o is int) returnwaarde = ( (int) o).ToString();
				else		returnwaarde = (string) o;
			}
			return returnwaarde; 

		}
	}
}
