using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using System;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils;
using MoreLinq;

namespace ProgressOnderwijsUtils.Data
{
	sealed class QueryTableValuedParameterComponent : IQueryParameter
	{
		readonly object paramval;
		readonly string DbTypeName;
		internal QueryTableValuedParameterComponent(string dbTypeName, object o) { paramval = o; DbTypeName = dbTypeName; }

		public string ToSqlString(QueryFactory qnum) { return "@par" + qnum.GetNumberForParam(this); }

		public SqlParameter ToSqlParameter(int paramnum)
		{
			return new SqlParameter {
			                        	IsNullable = false,
			                        	ParameterName = "@par" + paramnum,
			                        	Value = paramval,
			                        	SqlDbType = SqlDbType.Structured,
			                        	TypeName = DbTypeName
			                        };
		}

		public string ToDebugText()
		{
			return "{!" + ObjectToCode.ComplexObjectToPseudoCode(paramval) + "!}";
		}

		public bool CanShareParamNumberWith(IQueryParameter other) { return Equals((object)other); }
		public int ParamNumberSharingHashCode() { return paramval.GetHashCode() + 37 * DbTypeName.GetHashCode() + 200; }//paramval never null!

		public bool Equals(IQueryComponent other) { return Equals((object)other); }
		public override bool Equals(object other) { return ReferenceEquals(this, other) || (other is QueryTableValuedParameterComponent) && Equals(DbTypeName, ((QueryTableValuedParameterComponent)other).DbTypeName) && Equals(paramval, ((QueryTableValuedParameterComponent)other).paramval); }
		public override int GetHashCode() { return ParamNumberSharingHashCode(); }//paramval never null!
	}
}