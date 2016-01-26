using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.Collections;
using ProgressOnderwijsUtils.Internal;

namespace ProgressOnderwijsUtils
{
    static class QueryComponent
    {
        public static void AppendParamTo<TCommandFactory>(ref TCommandFactory factory, object o)
            where TCommandFactory : struct, ICommandFactory
        {
            if (o is IEnumerable && !(o is string) && !(o is byte[])) {
                ToTableParameter((IEnumerable)o).AppendTo(ref factory);
            } else {
                QueryScalarParameterComponent.AppendScalarParameter(ref factory, o);
            }
        }

        public static IQueryComponent ToTableParameter<T>(string tableTypeName, T[] set) where T : IMetaObject, new()
            => new QueryTableValuedParameterComponent<T>(tableTypeName, set);

        static IQueryComponent TryToTableParameter(IEnumerable enumerable)
        {
            var enumerableType = enumerable.GetType();
            var elementType = GetEnumerableElementType(enumerableType);
            if (elementType == null) {
                return null;
            }
            var underlyingType = elementType.GetUnderlyingType();

            var sqlTableTypeName = TryGetSqlTableTypeName(underlyingType);

            if (sqlTableTypeName == null) {
                return null;
            }
            var specializedMethod = ToWrappedTableParameter_Method.MakeGenericMethod(elementType);
            var func = (Func<string, IEnumerable, IQueryComponent>)Delegate.CreateDelegate(typeof(Func<string, IEnumerable, IQueryComponent>), specializedMethod);

            return func(sqlTableTypeName, enumerable);
        }

        static readonly MethodInfo ToWrappedTableParameter_Method =
            ((Func<string, IEnumerable, IQueryComponent>)ToWrappedTableParameter<int>)
                .Method
                .GetGenericMethodDefinition();

        static IQueryComponent ToWrappedTableParameter<T>(string sqlTableTypeName, IEnumerable enumerable)
        {
            var metaObjects = DbTableValuedParameterWrapperHelper.WrapPlainValueInMetaObject((IEnumerable<T>)enumerable);
            return ToTableParameter(sqlTableTypeName, metaObjects);
        }

        static readonly ConcurrentDictionary<Type, Type> itemTypeByEnumerableType = new ConcurrentDictionary<Type, Type>();

        static Type GetEnumerableElementType(Type enumerableType)
        {
            Type elementType;
            if (itemTypeByEnumerableType.TryGetValue(enumerableType, out elementType)) {
                return elementType;
            }

            foreach (var interfaceType in enumerableType.GetInterfaces()) {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
                    if (elementType != null) {
                        itemTypeByEnumerableType.TryAdd(enumerableType, null);
                        return null; //non-unique, no best match
                    }

                    elementType = interfaceType.GetGenericArguments()[0];
                }
            }
            itemTypeByEnumerableType.TryAdd(enumerableType, elementType);
            return elementType;
        }

        static string TryGetSqlTableTypeName(Type dotnetElementType)
        {
            if (typeof(int) == dotnetElementType) {
                return "TVar_Int";
            } else if (typeof(string) == dotnetElementType) {
                return "TVar_NVarcharMax";
            } else if (typeof(DateTime) == dotnetElementType) {
                return "TVar_DateTime2";
            } else if (typeof(TimeSpan) == dotnetElementType) {
                return "TVar_Time";
            } else if (typeof(decimal) == dotnetElementType) {
                return "TVar_Decimal";
            } else if (typeof(char) == dotnetElementType) {
                return "TVar_NChar1";
            } else if (typeof(bool) == dotnetElementType) {
                return "TVar_Bit";
            } else if (typeof(byte) == dotnetElementType) {
                return "TVar_Tinyint";
            } else if (typeof(short) == dotnetElementType) {
                return "TVar_Smallint";
            } else if (typeof(long) == dotnetElementType) {
                return "TVar_Bigint";
            } else if (typeof(double) == dotnetElementType) {
                return "TVar_Float";
            } else if (typeof(byte[]) == dotnetElementType) {
                return "TVar_VarBinaryMax";
            } else {
                return null;
            }
        }

        public static IQueryComponent ToTableParameter(IEnumerable set)
        {
            var retval = TryToTableParameter(set);
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

        public static class DbTableValuedParameterWrapperHelper
        {
            /// <summary>
            /// Efficiently wraps an enumerable of objects in DbTableValuedParameterWrapper and materialized the sequence as array.
            /// Effectively it's like .Select(x => new DbTableValuedParameterWrapper { querytablevalue = x }).ToArray() but faster.
            /// </summary>
            public static DbTableValuedParameterWrapper<T>[] WrapPlainValueInMetaObject<T>(IEnumerable<T> typedEnumerable)
            {
                var typedArray = typedEnumerable as T[];
                if (typedArray != null) {
                    var projectedArray = new DbTableValuedParameterWrapper<T>[typedArray.Length];
                    for (int i = 0; i < projectedArray.Length; i++) {
                        projectedArray[i].querytablevalue = typedArray[i];
                    }
                    return projectedArray;
                }

                var typedList = typedEnumerable as IReadOnlyList<T>;
                if (typedList != null) {
                    var projectedArray = new DbTableValuedParameterWrapper<T>[typedList.Count];
                    for (int i = 0; i < projectedArray.Length; i++) {
                        projectedArray[i].querytablevalue = typedList[i];
                    }
                    return projectedArray;
                }

                var arrayBuilder = FastArrayBuilder<DbTableValuedParameterWrapper<T>>.Create();
                foreach (var item in typedEnumerable) {
                    arrayBuilder.Add(new DbTableValuedParameterWrapper<T> { querytablevalue = item });
                }
                return arrayBuilder.ToArray();
            }
        }
    }
}
