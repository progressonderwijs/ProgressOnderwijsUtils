﻿using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
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
                ToTableParameterFromPlainValues((IEnumerable)o).AppendTo(ref factory);
            } else {
                QueryScalarParameterComponent.AppendScalarParameter(ref factory, o);
            }
        }

        public static IQueryComponent ToTableParameterFromPlainValues(IEnumerable set)
        {
            var enumerableType = set.GetType();
            IWrappedTableParameterFactory factory;
            if (!tvpFactoryCache.TryGetValue(enumerableType, out factory)) {
                factory = TvpFactoryForEnumerableType(enumerableType);
                tvpFactoryCache.TryAdd(enumerableType, factory);
            }
            if (factory == null) {
                throw new ArgumentException("Cannot interpret " + ObjectToCode.GetCSharpFriendlyTypeName(enumerableType) + " as a table valued parameter", nameof(set));
            }
            return factory.ToWrappedTableParameter(set);
        }

        public static IQueryComponent ToTableParameter<T>(string tableTypeName, T[] set) where T : IMetaObject, new()
            => new QueryTableValuedParameterComponent<T>(tableTypeName, set);

        static readonly ConcurrentDictionary<Type, IWrappedTableParameterFactory> tvpFactoryCache = new ConcurrentDictionary<Type, IWrappedTableParameterFactory>();

        static IWrappedTableParameterFactory TvpFactoryForEnumerableType(Type enumerableType)
        {
            var elementType = TryGetNonAmbiguousEnumerableElementType(enumerableType);
            if (elementType == null) {
                return null;
            }
            var underlyingType = elementType.GetUnderlyingType();
            var sqlTableTypeName = SqlTableTypeNameByDotnetType.GetOrDefault(underlyingType);
            if (sqlTableTypeName == null) {
                return null;
            }
            var factoryType = typeof(WrappedTableParameterFactory<>).MakeGenericType(elementType);
            return (IWrappedTableParameterFactory)Activator.CreateInstance(factoryType, sqlTableTypeName);
        }

        static readonly Dictionary<Type, string> SqlTableTypeNameByDotnetType = new Dictionary<Type, string> {
            { typeof(int), "TVar_Int" },
            { typeof(string), "TVar_NVarcharMax" },
            { typeof(DateTime), "TVar_DateTime2" },
            { typeof(TimeSpan), "TVar_Time" },
            { typeof(decimal), "TVar_Decimal" },
            { typeof(char), "TVar_NChar1" },
            { typeof(bool), "TVar_Bit" },
            { typeof(byte), "TVar_Tinyint" },
            { typeof(short), "TVar_Smallint" },
            { typeof(long), "TVar_Bigint" },
            { typeof(double), "TVar_Float" },
            { typeof(byte[]), "TVar_VarBinaryMax" },
        };

        interface IWrappedTableParameterFactory
        {
            IQueryComponent ToWrappedTableParameter(IEnumerable enumerable);
        }

        class WrappedTableParameterFactory<T> : IWrappedTableParameterFactory
        {
            readonly string sqlTableTypeName;

            public WrappedTableParameterFactory(string sqlTableTypeName)
            {
                this.sqlTableTypeName = sqlTableTypeName;
            }

            public IQueryComponent ToWrappedTableParameter(IEnumerable enumerable)
            {
                var metaObjects = DbTableValuedParameterWrapperHelper.WrapPlainValueInMetaObject((IEnumerable<T>)enumerable);
                return ToTableParameter(sqlTableTypeName, metaObjects);
            }
        }

        static Type TryGetNonAmbiguousEnumerableElementType(Type enumerableType)
        {
            Type elementType = null;
            foreach (var interfaceType in enumerableType.GetInterfaces()) {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
                    if (elementType != null) {
                        return null; //non-unique, no best match
                    }
                    elementType = interfaceType.GetGenericArguments()[0];
                }
            }
            return elementType;
        }

        /*
        TODO: once we're using Sql2014, memory optimization appears considerably faster, ala:
         CREATE TYPE TVar_Int AS TABLE ( 
             val int NOT NULL, 
             PRIMARY KEY NONCLUSTERED (val ASC)
         ) WITH ( MEMORY_OPTIMIZED = ON )
         
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
