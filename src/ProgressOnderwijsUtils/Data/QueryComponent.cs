using System.Linq;
using System.Collections.Generic;
using System;
using ProgressOnderwijsUtils;
using MoreLinq;

namespace ProgressOnderwijsUtils.Data
{
	static class QueryComponent
	{

		public static IQueryComponent CreateString(string val) { return new QueryStringComponent(val); }
		public static IQueryComponent CreateParam(object o)
		{
			return
				o is IQueryParameter ? (IQueryComponent)o
					: o is LiteralSqlInt ? new QueryStringComponent(((LiteralSqlInt)o).Value.ToStringInvariant())
					: o is IEnumerable<int> ? ToTableParameter((IEnumerable<int>)o)
					: new QueryScalarParameterComponent(o);
		}


		public static IQueryComponent ToTableParameter<T>(string tableTypeName, IEnumerable<T> set) where T : IMetaObject, new() { return new QueryTableValuedParameterComponent(tableTypeName, MetaObjectDataReader.Create(set)); }
		public static IQueryComponent ToTableParameter(IEnumerable<int> set) { return ToTableParameter("IntValues", set.Select(i => new IntValues_DbTableType { val = i })); }
		public static IQueryComponent ToTableParameterWithDeducedType(string tableTypeName, IEnumerable<IMetaObject> set) { return new QueryTableValuedParameterComponent(tableTypeName, MetaObjectDataReader.CreateDynamically(set)); }
	}
}