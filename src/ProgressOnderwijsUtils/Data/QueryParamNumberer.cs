using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils.Data
{
	public interface IQueryParameter
	{
		SqlParameter ToParameter(QueryParamNumberer qnum);
	}
	public class QueryParamNumberer
	{
		readonly List<IQueryParameter> parms = new List<IQueryParameter>();

		readonly Dictionary<IQueryParameter, int> lookup = new Dictionary<IQueryParameter, int>();
		public int GetNumberForParam(IQueryParameter o)
		{
			int retval;
			if (!lookup.TryGetValue(o, out retval))
			{
				parms.Add(o);
				lookup.Add(o, retval = lookup.Count);
			}
			return retval;
		}
		public IEnumerable<IQueryParameter> ParametersInOrder { get { return parms; } }
		public SqlParameter[] SqlParamters { get { return parms.Select(par => par.ToParameter(this)).ToArray(); } }
	}
}
