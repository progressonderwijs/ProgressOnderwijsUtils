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
        create type TVar_Bigint as table(querytablevalue bigint not null)
        create type TVar_Bit as table(querytablevalue bit not null)
        create type TVar_DateTime2 as table(querytablevalue datetime2(7) not null)
        create type TVar_Decimal as table(querytablevalue decimal(18, 0) not null)
        create type TVar_Float as table(querytablevalue float not null)
        create type TVar_Int as table(querytablevalue int not null, primary key clustered (querytablevalue asc) with (ignore_dup_key = off))
        create type TVar_NChar1 as table(querytablevalue nchar(1) not null)
        create type TVar_NVarcharMax as table(querytablevalue nvarchar(max) not null)
        create type TVar_Smallint as table(querytablevalue smallint not null)
        create type TVar_StudentStudielast as table(studentid int not null, studielast int not null)
        create type TVar_Time as table(querytablevalue time(7) not null)
        create type TVar_Tinyint as table(querytablevalue tinyint not null)

        grant exec on TYPE::TVar_Bigint to [webprogress-readonly]
        grant exec on TYPE::TVar_Bit to [webprogress-readonly]
        grant exec on TYPE::TVar_DateTime2 to [webprogress-readonly]
        grant exec on TYPE::TVar_Decimal to [webprogress-readonly]
        grant exec on TYPE::TVar_Float to [webprogress-readonly]
        grant exec on TYPE::TVar_Int to [webprogress-readonly]
        grant exec on TYPE::TVar_NChar1 to [webprogress-readonly]
        grant exec on TYPE::TVar_NVarcharMax to [webprogress-readonly]
        grant exec on TYPE::TVar_Smallint to [webprogress-readonly]
        grant exec on TYPE::TVar_StudentStudielast to [webprogress-readonly]
        grant exec on TYPE::TVar_Time to [webprogress-readonly]
        grant exec on TYPE::TVar_Tinyint to [webprogress-readonly]

        grant exec on TYPE::TVar_Bigint to [webprogress]
        grant exec on TYPE::TVar_Bit to [webprogress]
        grant exec on TYPE::TVar_DateTime2 to [webprogress]
        grant exec on TYPE::TVar_Decimal to [webprogress]
        grant exec on TYPE::TVar_Float to [webprogress]
        grant exec on TYPE::TVar_Int to [webprogress]
        grant exec on TYPE::TVar_NChar1 to [webprogress]
        grant exec on TYPE::TVar_NVarcharMax to [webprogress]
        grant exec on TYPE::TVar_Smallint to [webprogress]
        grant exec on TYPE::TVar_StudentStudielast to [webprogress]
        grant exec on TYPE::TVar_Time to [webprogress]
        grant exec on TYPE::TVar_Tinyint to [webprogress]
		 

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
