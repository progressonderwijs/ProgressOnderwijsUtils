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
		/// Returns content of an attribute replacing DBNull.Value with null.  Throws an exception for value types
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="row">DataRow</param>
		/// <param name="fieldname">Name of attribute</param>
		/// <returns>Content of field or default type initialization</returns>
		public static T Get<T>(this DataRow row, string fieldname)
		{
			return row.Field<T>(fieldname);
		}

		/// <summary>
		/// Returns content of an attribute, replacing DBNull.Value with the specified default value.
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="row">DataRow</param>
		/// <param name="fieldname">Name of attribute</param>
		/// <param name="defaultvalue">Value to return in case attribute is DBNull or null</param>
		/// <returns>Content of field or default value</returns>
		public static T FieldDefault<T>(this DataRow row, string fieldname, T defaultvalue)
		{
			object val = row[fieldname];
			return val == DBNull.Value ? defaultvalue : (T)val;
		}
		public static T Get<T>(this DataRow row, string fieldname, T defaultvalue)
		{
			return row.FieldDefault(fieldname, defaultvalue);
		}


		public static T Field<T>(this DataRowView row, string fieldname)
		{
			return row.Row.Field<T>(fieldname);
		}

		public static T Get<T>(this DataRowView row, string fieldname)
		{
			return row.Field<T>(fieldname);
		}


		public static T FieldDefault<T>(this DataRowView row, string fieldname, T defaultvalue)
		{
			return row.Row.FieldDefault<T>(fieldname, defaultvalue);
		}
		public static T Get<T>(this DataRowView row, string fieldname, T defaultvalue)
		{
			return row.FieldDefault<T>(fieldname, defaultvalue);
		}
	}
}
