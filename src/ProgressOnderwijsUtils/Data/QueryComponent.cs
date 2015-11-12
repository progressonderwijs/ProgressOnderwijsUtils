using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections.Generic;
using System;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils
{
    static class QueryComponent
    {
        public static IQueryComponent CreateString(string val)
        {
            if (val == "") {
                return null;
            } else {
                return new QueryStringComponent(val);
            }
        }

        public static IQueryComponent CreateParam(object o)
        {
            if (o is QueryBuilder) {
                throw new ArgumentException("Cannot pass a querybuilder as a parameter");
            } else if (o is IQueryParameter) {
                return (IQueryComponent)o;
            } else if (o is LiteralSqlInt) {
                return new QueryStringComponent(((LiteralSqlInt)o).Value.ToStringInvariant());
            } else if (o is IEnumerable && !(o is string) && !(o is byte[])) {
                return ToTableParameter((IEnumerable)o);
            } else {
                return new QueryScalarParameterComponent(o);
            }
        }

        public static IQueryComponent ToTableParameter<T>(string tableTypeName, IEnumerable<T> set) where T : IMetaObject, new()
        {
            return new QueryTableValuedParameterComponent<T>(tableTypeName, set);
        }

        static IQueryComponent TryToTableParameter<T>(string tableTypeName, IEnumerable set)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if (set is IEnumerable<T>) {
                var typedSet = ((IEnumerable<T>)set);
                var projectedSet = typedSet.Select(i => new Internal.DbTableValuedParameterWrapper<T> { querytablevalue = i });
                return ToTableParameter(tableTypeName, projectedSet);
            } else {
                return null;
            }
        }

        public static IQueryComponent ToTableParameter(IEnumerable set)
        {
            var retval = null
                ?? TryToTableParameter<int>("TVar_Int", set)
                    ?? TryToTableParameter<string>("TVar_NVarcharMax", set)
                        ?? TryToTableParameter<DateTime>("TVar_DateTime2", set)
                            ?? TryToTableParameter<TimeSpan>("TVar_Time", set)
                                ?? TryToTableParameter<decimal>("TVar_Decimal", set)
                                    ?? TryToTableParameter<char>("TVar_NChar1", set)
                                        ?? TryToTableParameter<bool>("TVar_Bit", set)
                                            ?? TryToTableParameter<byte>("TVar_Tinyint", set)
                                                ?? TryToTableParameter<short>("TVar_Smallint", set)
                                                    ?? TryToTableParameter<long>("TVar_Bigint", set)
                                                        ?? TryToTableParameter<double>("TVar_Float", set)
                ;
            if (retval == null) {
                throw new ArgumentException("Cannot interpret " + ObjectToCode.GetCSharpFriendlyTypeName(set.GetType()) + " as a table valued parameter", nameof(set));
            }
            return retval;
        }

        /*
		CREATE TYPE TVar_Int AS TABLE (val int NOT NULL)
		CREATE TYPE TVar_NVarcharMax AS TABLE (val nvarchar(max) NOT NULL)
		CREATE TYPE TVar_DateTime2 AS TABLE (val datetime2 NOT NULL)
		CREATE TYPE TVar_Time AS TABLE (val time NOT NULL)
		CREATE TYPE TVar_Decimal AS TABLE (val decimal NOT NULL)
		CREATE TYPE TVar_NChar1 AS TABLE (val nchar(1) NOT NULL)
		CREATE TYPE TVar_Bit AS TABLE (val bit NOT NULL)
		CREATE TYPE TVar_Tinyint AS TABLE (val tinyint NOT NULL)
		CREATE TYPE TVar_Smallint AS TABLE (val smallint NOT NULL)
		CREATE TYPE TVar_Bigint AS TABLE (val bigint NOT NULL)
		CREATE TYPE TVar_Float AS TABLE (val float NOT NULL)

		GRANT EXECUTE ON TYPE::dbo.TVar_Int TO public;
		GRANT EXECUTE ON TYPE::dbo.TVar_NVarcharMax TO public;
		GRANT EXECUTE ON TYPE::dbo.TVar_DateTime2 TO public;
		GRANT EXECUTE ON TYPE::dbo.TVar_Time TO public;
		GRANT EXECUTE ON TYPE::dbo.TVar_Decimal TO public;
		GRANT EXECUTE ON TYPE::dbo.TVar_NChar1 TO public;
		GRANT EXECUTE ON TYPE::dbo.TVar_Bit TO public;
		GRANT EXECUTE ON TYPE::dbo.TVar_Tinyint TO public;
		GRANT EXECUTE ON TYPE::dbo.TVar_Smallint TO public;
		GRANT EXECUTE ON TYPE::dbo.TVar_Bigint TO public;
		GRANT EXECUTE ON TYPE::dbo.TVar_Float TO public;
		 

        TODO: once we're using Sql2014, the following types appear to be considerably faster:
         CREATE TYPE TVar_Int AS TABLE ( 
             val int NOT NULL, 
             PRIMARY KEY NONCLUSTERED (val ASC)
         )
         WITH ( MEMORY_OPTIMIZED = ON )
         
         Memory optimized types also need a new MEMORY_OPTIMIZED_DATA filegroup, even if we're not going to store anything in that filegroup.
         
		 */
    }

    namespace Internal
    {
        //public needed for auto-mapping
        public struct DbTableValuedParameterWrapper<T> : IMetaObject
        {
            [Key]
            public T querytablevalue { get; set; }

            public override string ToString() => querytablevalue == null ? "NULL" : querytablevalue.ToString();
        }
    }
}
