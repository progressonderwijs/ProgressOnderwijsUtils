using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ProgressOnderwijsUtils
{
	public static class KoppelTabelHelpers
	{
		public static DataTable ToDataTable(this IEnumerable<KoppelTabelEntry> entries)
		{
			return MetaObject.ToDataTable(entries, null);
		}

	}
	public struct KoppelTabelEntry : IMetaObject
	{
		public int Id { get; set; }
		public string Tekst { get; set; }
	}

}
