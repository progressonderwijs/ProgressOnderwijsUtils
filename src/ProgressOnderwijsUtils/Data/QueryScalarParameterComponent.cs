using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using System;
using ProgressOnderwijsUtils;
using MoreLinq;

namespace ProgressOnderwijsUtils.Data
{
	sealed class QueryScalarParameterComponent : IQueryParameter
	{
		readonly object paramval;
		internal QueryScalarParameterComponent(object o) { paramval = o ?? DBNull.Value; }

		public string ToSqlString(CommandFactory qnum) { return "@par" + qnum.GetNumberForParam(this); }

		public SqlParameter ToSqlParameter(int paramNum)
		{
			return new SqlParameter {
			                        	IsNullable = paramval == DBNull.Value,
			                        	ParameterName = "@par" + paramNum,
			                        	Value = paramval,
			                        };
		}

		public string ToDebugText()
		{
			if (paramval == null || paramval == DBNull.Value)
				return "null";
			else if (paramval is string)
				return "'" + (paramval as string).Replace("'", "''") + "'";
			else if (paramval is int)
				return ((int)paramval).ToStringInvariant();
			else if (paramval is decimal)
				return ((decimal)paramval).ToStringInvariant();
			else if (paramval is DateTime)
				return ((DateTime)paramval).ToString(@"\'yyyy-MM-dd HH:mm:ss.fffffff\'");
			else
				return "{!" + paramval + "!}";
		}


		public bool CanShareParamNumberWith(IQueryParameter other) {
			return Equals(other);
			//return ReferenceEquals(this, other) ||
			//(other is QueryScalarParameterComponent) 
			//&& paramval != DBNull.Value 
			//&& !(paramval is int) 
			//&& Equals(paramval, ((QueryScalarParameterComponent)other).paramval);
		}
		public int ParamNumberSharingHashCode() {
			return GetHashCode();
			//return paramval.GetHashCode() + (
			//paramval == DBNull.Value
			//|| paramval is int 
			//? base.GetHashCode() : 37); //paramval never null!
		}

		public bool Equals(IQueryComponent other) { return (other is QueryScalarParameterComponent) && Equals(paramval, ((QueryScalarParameterComponent)other).paramval); }
		public override bool Equals(object obj) { return (obj is QueryScalarParameterComponent) && Equals((QueryScalarParameterComponent)obj); }
		public override int GetHashCode() { return paramval.GetHashCode() + 37; }//paramval never null!
	}
}