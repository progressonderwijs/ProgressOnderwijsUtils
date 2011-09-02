using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils.Data
{
	interface IQueryComponent : IEquatable<IQueryComponent>
	{
		string ToSqlString(QueryParamNumberer qnum);

		string ToDebugText();
	}

	static class QueryComponent
	{
		public class StringComponent : IQueryComponent
		{
			public readonly string val;
			public string ToSqlString(QueryParamNumberer qnum) { return val; }
			internal StringComponent(string val)
			{
				if (val == null) throw new ArgumentNullException("val");
				this.val = val;
			}

			public string ToDebugText() { return val; }

			public bool Equals(IQueryComponent other) { return (other is StringComponent) && val == ((StringComponent)other).val; }
			public override bool Equals(object obj) { return (obj is StringComponent) && Equals((StringComponent)obj); }
			public override int GetHashCode() { return val.GetHashCode() + 31; }
		}

		public class ParamComponent : IQueryComponent, IQueryParameter
		{
			readonly object paramval;
			internal ParamComponent(object o) { paramval = o ?? DBNull.Value; }

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
				else if (paramval is int)
					return ((int)paramval).ToStringInvariant();
				else if (paramval is decimal)
					return ((decimal)paramval).ToStringInvariant();
				else if (paramval is DateTime)
					return ((DateTime)paramval).ToString(@"\'yyyy-MM-dd HH:mm:ss.fffffff\'");
				else
					return "{!" + paramval + "!}";
			}


			public bool Equals(IQueryComponent other) { return ReferenceEquals(this, other) || (other is ParamComponent) && paramval != DBNull.Value && !(paramval is int) && Equals(paramval, ((ParamComponent)other).paramval); }
			public override bool Equals(object obj) { return (obj is ParamComponent) && Equals((ParamComponent)obj); }
			public override int GetHashCode() { return paramval.GetHashCode() +  (paramval == DBNull.Value || paramval is int ? base.GetHashCode() : 37); }//paramval never null!
			//public bool Equals(IQueryComponent other) { return (other is ParamComponent) && ReferenceEquals(this, other); }

		}

		public static IQueryComponent CreateString(string val) { return new StringComponent(val); }
		public static IQueryComponent CreateParam(object o) { return new ParamComponent(o); }
	}

	[TestFixture]
	public class TestQueryComponent
	{
		[Test]
		public void ValidatesArgumentsOK()
		{
			Assert.Throws<ArgumentNullException>(() => QueryComponent.CreateString(null));
			Assert.DoesNotThrow(() => QueryComponent.CreateString("bla"));

			PAssert.That(() => QueryComponent.CreateString("bla" + 0).GetHashCode() == QueryComponent.CreateString("bla0").GetHashCode());
			PAssert.That(() => QueryComponent.CreateString("bla" + 0).GetHashCode() != QueryComponent.CreateString("bla").GetHashCode());
			PAssert.That(() => QueryComponent.CreateString("bla" + 0).Equals(QueryComponent.CreateString("bla0")));

			PAssert.That(() => QueryComponent.CreateParam("bla" + 0).GetHashCode() == QueryComponent.CreateParam("bla0").GetHashCode());
			PAssert.That(() => QueryComponent.CreateParam("bla" + 0).GetHashCode() != QueryComponent.CreateParam("bla").GetHashCode());
			PAssert.That(() => QueryComponent.CreateParam("bla" + 0).Equals(QueryComponent.CreateParam("bla0")));

			var someday = new DateTime(2012, 3, 4);
			PAssert.That(() => QueryComponent.CreateParam(someday).ToDebugText() == "'2012-03-04 00:00:00.0000000'");
			PAssert.That(() => QueryComponent.CreateParam(null).ToDebugText() == "null");
			PAssert.That(() => QueryComponent.CreateParam("abc").ToDebugText() == "'abc'");
			PAssert.That(() => QueryComponent.CreateParam("ab'c").ToDebugText() == "'ab''c'");
			PAssert.That(() => QueryComponent.CreateParam(12345).ToDebugText() == "12345");
			PAssert.That(() => QueryComponent.CreateParam(12345.6m).ToDebugText() == "12345.6");
			PAssert.That(() => QueryComponent.CreateParam(new object()).ToDebugText() == "{!System.Object!}");
		}
	}
}
