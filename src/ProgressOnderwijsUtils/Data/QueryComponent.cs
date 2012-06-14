using System.Collections;
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
			if (o is QueryBuilder)
				throw new ArithmeticException("Cannot pass a querybuilder as a parameter");
			return
				o is IQueryParameter ? (IQueryComponent)o
					: o is LiteralSqlInt ? new QueryStringComponent(((LiteralSqlInt)o).Value.ToStringInvariant())
					: (o is IEnumerable && !(o is string) && !(o is byte[])) ? ToTableParameter((dynamic)o)
					: new QueryScalarParameterComponent(o);
		}


		public static IQueryComponent ToTableParameter<T>(string tableTypeName, IEnumerable<T> set) where T : IMetaObject, new() { return new QueryTableValuedParameterComponent<T>(tableTypeName, set); }

		static IQueryComponent BasicTypeTable<T>(string tableTypeName, IEnumerable<T> set)
			where T : IComparable, IComparable<T>, IEquatable<T> //add conditions to help detect accidentally using an invalid type at compile time.
		{
			return ToTableParameter(tableTypeName, set.Select(i => new Internal.DbTableValuedParameterWrapper<T> { val = i }));
		}

		public static IQueryComponent ToTableParameter(IEnumerable<int> set) { return BasicTypeTable("TVar_Int", set); }
		public static IQueryComponent ToTableParameter(IEnumerable<string> set) { return BasicTypeTable("TVar_NVarcharMax", set); }
		public static IQueryComponent ToTableParameter(IEnumerable<DateTime> set) { return BasicTypeTable("TVar_DateTime2", set); }
		public static IQueryComponent ToTableParameter(IEnumerable<TimeSpan> set) { return BasicTypeTable("TVar_Time", set); }
		public static IQueryComponent ToTableParameter(IEnumerable<decimal> set) { return BasicTypeTable("TVar_Decimal", set); }
		public static IQueryComponent ToTableParameter(IEnumerable<char> set) { return BasicTypeTable("TVar_NChar1", set); }
		public static IQueryComponent ToTableParameter(IEnumerable<bool> set) { return BasicTypeTable("TVar_Bit", set); }
		public static IQueryComponent ToTableParameter(IEnumerable<byte> set) { return BasicTypeTable("TVar_Tinyint", set); }
		public static IQueryComponent ToTableParameter(IEnumerable<short> set) { return BasicTypeTable("TVar_Smallint", set); }
		public static IQueryComponent ToTableParameter(IEnumerable<long> set) { return BasicTypeTable("TVar_Bigint", set); }
		public static IQueryComponent ToTableParameter(IEnumerable<double> set) { return BasicTypeTable("TVar_Float", set); }


		/*CREATE TYPE TVar_Int AS TABLE (val int NOT NULL)
		CREATE TYPE TVar_NVarcharMax AS TABLE (val nvarchar(max) NOT NULL)
		CREATE TYPE TVar_DateTime2 AS TABLE (val datetime2 NOT NULL)
		CREATE TYPE TVar_Time AS TABLE (val time NOT NULL)
		CREATE TYPE TVar_Decimal AS TABLE (val decimal NOT NULL)
		CREATE TYPE TVar_NChar1 AS TABLE (val nchar(1) NOT NULL)
		CREATE TYPE TVar_Bit AS TABLE (val bit NOT NULL)
		CREATE TYPE TVar_Tinyint AS TABLE (val tinyint NOT NULL)
		CREATE TYPE TVar_Smallint AS TABLE (val smallint NOT NULL)
		CREATE TYPE TVar_Bigint AS TABLE (val bigint NOT NULL)
		CREATE TYPE TVar_Float AS TABLE (val float NOT NULL)*/
	}

	namespace Internal
	{
		public struct DbTableValuedParameterWrapper<T> : IMetaObject
		{
			public T val { get; set; }
			public override string ToString() { return val == null ? "NULL" : val.ToString(); }
		}
	}

}