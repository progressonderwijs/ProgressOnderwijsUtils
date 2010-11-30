using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils.Data
{
	interface IQueryComponent : IEquatable<IQueryComponent>
	{
		string ToSqlString(QueryParamNumberer qnum);

		string ToDebugText();
	}


	static class QueryComponent
	{
		class StringComponent : IQueryComponent
		{
			readonly string val;
			public string ToSqlString(QueryParamNumberer qnum) { return val; }
			public StringComponent(string val)
			{
				if (val == null) throw new ArgumentNullException("val");
				this.val = val;
			}

			public string ToDebugText() { return val; }

			public bool Equals(IQueryComponent other) { return (other is StringComponent) && val == ((StringComponent)other).val; }
			public override bool Equals(object obj) { return (obj is StringComponent) && Equals((StringComponent)obj); }
			public override int GetHashCode() { return val.GetHashCode() + 31; }
		}

		class ParamComponent : IQueryComponent, IQueryParameter
		{
			private readonly object paramval;
			public ParamComponent(object o) { paramval = o ?? DBNull.Value; }

			public string ToSqlString(QueryParamNumberer qnum) { return "@par" + qnum.GetNumberForParam(this); }

			public SqlParameter ToParameter(QueryParamNumberer qnum)
			{
				return new SqlParameter
				{
					IsNullable = paramval == DBNull.Value,
					ParameterName = "@par" + qnum.GetNumberForParam(this),
					Value = paramval,
				};
			}

			public string ToDebugText()
			{
				if (paramval == null || paramval == DBNull.Value)
					return "null";
				else if (paramval is string)
					return "'" + (paramval as string).Replace("'", "''") + "'";
				else if (paramval is int || paramval is decimal)
					return paramval.ToString();
				else if (paramval is DateTime)
					return ((DateTime)paramval).ToString("'yyyy-MM-dd HH:mm:ss.fffffff'");
				else
					return "{!" + paramval + "!}";
			}


			public bool Equals(IQueryComponent other) { return (other is ParamComponent) && Equals(paramval, ((ParamComponent)other).paramval); }
			public override bool Equals(object obj) { return (obj is ParamComponent) && Equals((ParamComponent)obj); }
			public override int GetHashCode() { return paramval == null ? -1 : paramval.GetHashCode() + 37; }
		}

		public static IQueryComponent CreateString(string val) { return new StringComponent(val); }
		public static IQueryComponent CreateParam(object o) { return new ParamComponent(o); }
	}
}
