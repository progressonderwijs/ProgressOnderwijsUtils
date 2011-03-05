using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils
{
	/// <summary>
	/// Extensions for DataRow
	/// </summary>
	public static class DataRowExtensions
	{

		public static T Field<T>(this DataRowView row, string fieldname)
		{
			return row.Row.Field<T>(fieldname);
		}
	}
}
