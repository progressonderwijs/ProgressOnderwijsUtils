using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using System;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils;
using MoreLinq;

namespace ProgressOnderwijsUtils.Data
{
	sealed class QueryTableValuedParameterComponent<T> : IQueryParameter where T:IMetaObject 
	{
		readonly IEnumerable<T> objs;
		readonly string DbTypeName;
		internal QueryTableValuedParameterComponent(string dbTypeName, IEnumerable<T> list) { objs = list; DbTypeName = dbTypeName; }

		public string ToSqlString(CommandFactory qnum) { return "@par" + qnum.GetNumberForParam(this); }

		public SqlParameter ToSqlParameter(int paramnum)
		{
			return new SqlParameter {
			                        	IsNullable = false,
			                        	ParameterName = "@par" + paramnum,
										Value = MetaObject.CreateDataReader(objs),
			                        	SqlDbType = SqlDbType.Structured,
			                        	TypeName = DbTypeName
			                        };
		}

		public string ToDebugText()
		{
			return "(" + ObjectToCode.ComplexObjectToPseudoCode(objs) + ")";
		}

		public bool CanShareParamNumberWith(IQueryParameter other) { return Equals((object)other); }
		public int ParamNumberSharingHashCode() { return objs.GetHashCode() + 37 * DbTypeName.GetHashCode() + 200; }//paramval never null!

		public bool Equals(IQueryComponent other) { return Equals((object)other); }
		public override bool Equals(object other) { return ReferenceEquals(this, other) || (other is QueryTableValuedParameterComponent<T>) && Equals(DbTypeName, ((QueryTableValuedParameterComponent<T>)other).DbTypeName) && Equals(objs, ((QueryTableValuedParameterComponent<T>)other).objs); }
		public override int GetHashCode() { return ParamNumberSharingHashCode(); }//paramval never null!
	}
}