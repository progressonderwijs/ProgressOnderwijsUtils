using System;
using System.Collections.Generic;
using System.Data;
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
		public sealed class StringComponent : IQueryComponent
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

		sealed class ParamComponent : IQueryComponent, IQueryParameter
		{
			readonly object paramval;
			internal ParamComponent(object o) { paramval = o ?? DBNull.Value; }

			public string ToSqlString(QueryParamNumberer qnum) { return "@par" + qnum.GetNumberForParam(this); }

			public SqlParameter ToParameter(QueryParamNumberer qnum)
			{
				return new SqlParameter {
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


			public bool QueryEquals(IQueryParameter other) { return ReferenceEquals(this, other) || (other is ParamComponent) && paramval != DBNull.Value && !(paramval is int) && Equals(paramval, ((ParamComponent)other).paramval); }
			public int QueryHashCode() { return paramval.GetHashCode() + (paramval == DBNull.Value || paramval is int ? base.GetHashCode() : 37); }//paramval never null!

			public bool Equals(IQueryComponent other) { return (other is ParamComponent) && Equals(paramval, ((ParamComponent)other).paramval); }
			public override bool Equals(object obj) { return (obj is ParamComponent) && Equals((ParamComponent)obj); }
			public override int GetHashCode() { return paramval.GetHashCode() + 37; }//paramval never null!
		}

		sealed class TableParamComponent : IQueryComponent, IQueryParameter
		{
			readonly object paramval;
			readonly string DbTypeName;
			internal TableParamComponent(string dbTypeName, object o) { paramval = o; DbTypeName = dbTypeName; }

			public string ToSqlString(QueryParamNumberer qnum) { return "@par" + qnum.GetNumberForParam(this); }

			public SqlParameter ToParameter(QueryParamNumberer qnum)
			{
				return new SqlParameter {
					IsNullable = false,
					ParameterName = "@par" + qnum.GetNumberForParam(this),
					Value = paramval,
					SqlDbType = SqlDbType.Structured,
					TypeName = DbTypeName
				};
			}

			public string ToDebugText()
			{
				return "{!" + ObjectToCode.ComplexObjectToPseudoCode(paramval) + "!}";
			}


			public bool QueryEquals(IQueryParameter other) { return Equals((object)other); }
			public int QueryHashCode() { return paramval.GetHashCode() + 37 * DbTypeName.GetHashCode() + 200; }//paramval never null!

			public bool Equals(IQueryComponent other) { return Equals((object)other); }
			public override bool Equals(object other) { return ReferenceEquals(this, other) || (other is TableParamComponent) && Equals(DbTypeName, ((TableParamComponent)other).DbTypeName) && Equals(paramval, ((TableParamComponent)other).paramval); }
			public override int GetHashCode() { return QueryHashCode(); }//paramval never null!
		}

		public sealed class QueryParamEquality : IEqualityComparer<IQueryParameter>
		{
			public bool Equals(IQueryParameter x, IQueryParameter y) { return x.QueryEquals(y); }
			public int GetHashCode(IQueryParameter obj) { return obj.QueryHashCode(); }
			public static readonly QueryParamEquality Instance = new QueryParamEquality();
			QueryParamEquality() { }
		}



		public static IQueryComponent CreateString(string val) { return new StringComponent(val); }
		public static IQueryComponent CreateParam(object o)
		{
			return 
				o is IQueryParameter && o is IQueryComponent ? (IQueryComponent)o
				: o is LiteralSqlInt ? new StringComponent(((LiteralSqlInt)o).Value.ToStringInvariant()) 
				: (IQueryComponent)new ParamComponent(o);
		}


		public static IQueryComponent ToTableParameter<T>(string tableTypeName, IEnumerable<T> set) where T : IMetaObject, new()
		{
			return new TableParamComponent(tableTypeName, MetaObjectDataReader.Create(set));
		}
		public static IQueryComponent ToTableParameter(IEnumerable<int> set) { return ToTableParameter("IntValues", set.Select(i => new IntValues_DbTableType { val = i })); }

		public static IQueryComponent ToTableParameterWithDeducedType(string tableTypeName, IEnumerable<IMetaObject> set) { return new TableParamComponent(tableTypeName, MetaObjectDataReader.CreateDynamically(set)); }

	}

	public struct IntValues_DbTableType : IMetaObject
	{
		public int val { get; set; }
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
