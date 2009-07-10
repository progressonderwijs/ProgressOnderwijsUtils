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
		/// Returns content of an attribute or default type initialization (no need to check for DBNull or null)
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="row">DataRow</param>
		/// <param name="fieldname">Name of attribute</param>
		/// <returns>Content of field or default type initialization</returns>
		public static T Get<T>(this DataRow row, string fieldname)
		{
			object val = row[fieldname];
//			return (T)(val == DBNull.Value ? null : val); //throws exception if (T) is a value type and the value is DBNull.Value
			return val == DBNull.Value ? default(T) : (T)val;
		}

		/// <summary>
		/// Returns content of an attribute or default value (no need to check for DBNull or null)
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="row">DataRow</param>
		/// <param name="fieldname">Name of attribute</param>
		/// <param name="defaultvalue">Value to return in case attribute is DBNull or null</param>
		/// <returns>Content of field or default value</returns>
		public static T Get<T>(this DataRow row, string fieldname, T defaultvalue)
		{
			object val = row[fieldname];
			return val == DBNull.Value ? defaultvalue : (T)val;
		}

		public static T Get<T>(this DataRowView row, string fieldname)
		{
			return row.Row.Get<T>(fieldname);
		}

		public static T Get<T>(this DataRowView row, string fieldname, T defaultvalue)
		{
			return row.Row.Get<T>(fieldname, defaultvalue);
		}
	}
}
