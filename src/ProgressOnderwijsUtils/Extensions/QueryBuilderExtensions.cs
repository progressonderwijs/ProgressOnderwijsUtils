using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
	public static class QueryBuilderExtensions
	{
		public static QueryBuilder Append(this QueryBuilder source, string str, params object[] parms)
		{
			return source + QueryBuilder.Create(Environment.NewLine + str + " ", parms);
		}
	}
}
