﻿using System.Data.SqlClient;
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
				Value = paramval is Filter.CurrentTimeToken ? DateTime.Now : paramval,
			};
		}

		public string ToDebugText(Taal? taalOrNull)
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
			else if (paramval is Enum)
				return Converteer.ToString(paramval, taalOrNull ?? Taal.NL);
			else
				return "{!" + (taalOrNull.HasValue ? Converteer.ToString(paramval, taalOrNull.Value) : paramval.ToString()) + "!}";
		}

		public bool Equals(IQueryComponent other) { return (other is QueryScalarParameterComponent) && Equals(paramval, ((QueryScalarParameterComponent)other).paramval); }
		public override bool Equals(object obj) { return (obj is QueryScalarParameterComponent) && Equals((QueryScalarParameterComponent)obj); }
		public override int GetHashCode() { return paramval.GetHashCode() + 37; }//paramval never null!
	}
}