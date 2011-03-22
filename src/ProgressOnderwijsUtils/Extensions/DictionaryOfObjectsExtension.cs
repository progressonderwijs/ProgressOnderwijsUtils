using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProgressOnderwijsUtils.Converteer;

namespace ProgressOnderwijsUtils
{
	public static class DictionaryOfObjectsExtension
	{
		/// <summary>
		/// Casts the boxed objects to a typed representation.  Supports directly unboxing int's into (nullable) enums.
		/// </summary>
		public static T Field<T>(this Dictionary<string, object> dict, string key) { return DBNullRemover.Cast<T>(dict[key]); }
	}
}
