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
			try
			{
				object val = row[fieldname];
				return val == DBNull.Value ? default(T) : (T)val;
			}
			catch
			{
				return default(T);
			}
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
			try
			{
				object val = row[fieldname];
				return val == DBNull.Value ? defaultvalue : (T)val;
			}
			catch
			{
				return defaultvalue;
			}
		}
	}
}
