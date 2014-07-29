using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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

		public static T GetId<T>(this DataRow row) where T : IIdentifier, new()
		{
			var id = new T();
			string columnName;
			if (row.Table.Columns.Contains(id.DbPrimaryKeyName))
				columnName = id.DbPrimaryKeyName;
			else if (row.Table.Columns.Contains(id.DbForeignKeyName))
				columnName = id.DbForeignKeyName;
			else
				throw new NietZoErgeException(string.Format("Geen waarde voor {0} of {1}", id.DbPrimaryKeyName, id.DbForeignKeyName));

			if (!row.IsNull(columnName))
				id.Value = (int)row.Field<int>(columnName);

			return id;
		}

	}
}
