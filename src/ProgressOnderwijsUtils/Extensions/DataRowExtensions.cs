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
		public static T FieldOrDefault<T>(this DataRow row, string fieldname, T defaultvalue)
		{
			object val = row[fieldname];
			return val == DBNull.Value ? defaultvalue : (T)val;
		}

		public static T Field<T>(this DataRowView row, string fieldname)
		{
			return row.Row.Field<T>(fieldname);
		}

		public static T FieldOrDefault<T>(this DataRowView row, string fieldname, T defaultvalue)
		{
			return row.Row.FieldOrDefault<T>(fieldname, defaultvalue);
		}
	}
}
