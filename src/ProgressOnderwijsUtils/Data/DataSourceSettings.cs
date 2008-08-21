using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace ProgressOnderwijsUtils.Data
{
	/// <summary>
	/// Slaat de sorteer-modus en filters van een IDataSource op.
	/// </summary>
	[Serializable]
	public class DataSourceSettings
	{
		public SortOrder SortOrder = null;//meaning; use default.
	}
}
