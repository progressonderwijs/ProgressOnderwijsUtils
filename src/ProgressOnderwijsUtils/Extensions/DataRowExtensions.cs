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
		/// <summary>
		/// Returns content of an attribute, replacing DBNull.Value with the specified default value.
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="row">DataRow</param>
		/// <param name="fieldname">Name of attribute</param>
		/// <param name="defaultvalue">Value to return in case attribute is DBNull or null</param>
		/// <returns>Content of field or default value</returns>
		[Obsolete("Ipv DataRow.Get kun je tegenwoordig DataRow.Field gebruiken e.g.: dr.Field<int?>(\"kolomnaam\") ?? defaultwaarde")]
		public static T Get<T>(this DataRow row, string fieldname, T defaultvalue)
		{
			object val = row[fieldname];
			return val == DBNull.Value ? defaultvalue : (T)val;
		}

		public static T Field<T>(this DataRowView row, string fieldname)
		{
			return row.Row.Field<T>(fieldname);
		}
	}
}
