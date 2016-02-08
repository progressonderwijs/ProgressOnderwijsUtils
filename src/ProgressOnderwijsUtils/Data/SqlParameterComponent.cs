using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.Collections;
using ProgressOnderwijsUtils.Internal;

namespace ProgressOnderwijsUtils
{
    static class SqlParameterComponent
    {
        public static void AppendParamTo<TCommandFactory>(ref TCommandFactory factory, object o)
            where TCommandFactory : struct, ICommandFactory
        {
            if (o is IEnumerable && !(o is string) && !(o is byte[])) {
                ToTableValuedParameterFromPlainValues((IEnumerable)o).AppendTo(ref factory);
            } else {
                QueryScalarParameterComponent.AppendScalarParameter(ref factory, o);
            }
        }

        public static ISqlComponent ToTableValuedParameterFromPlainValues(IEnumerable set)
        {
            var enumerableType = set.GetType();
            ITableValuedParameterFactory factory;
            if (!tableValuedParameterFactoryCache.TryGetValue(enumerableType, out factory)) {
                factory = CreateTableValuedParameterFactory(enumerableType);
                tableValuedParameterFactoryCache.TryAdd(enumerableType, factory);
            }
            if (factory == null) {
                throw new ArgumentException("Cannot interpret " + ObjectToCode.GetCSharpFriendlyTypeName(enumerableType) + " as a table valued parameter", nameof(set));
            }
            return factory.CreateFromPlainValues(set);
        }

        public static ISqlComponent ToTableValuedParameter<TIn, TOut>(string tableTypeName, IEnumerable<TIn> set, Func<IEnumerable<TIn>, TOut[]> projection) 
            where TOut : IMetaObject, new()
            => new QueryTableValuedParameterComponent<TIn,TOut>(tableTypeName, set, projection);

        static readonly ConcurrentDictionary<Type, ITableValuedParameterFactory> tableValuedParameterFactoryCache = new ConcurrentDictionary<Type, ITableValuedParameterFactory>();

        static ITableValuedParameterFactory CreateTableValuedParameterFactory(Type enumerableType)
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
            var factoryType = typeof(TableValuedParameterFactory<>).MakeGenericType(elementType);
            return (ITableValuedParameterFactory)Activator.CreateInstance(factoryType, sqlTableTypeName);
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

        interface ITableValuedParameterFactory
        {
            ISqlComponent CreateFromPlainValues(IEnumerable enumerable);
        }

        class TableValuedParameterFactory<T> : ITableValuedParameterFactory
        {
            readonly string sqlTableTypeName;

            public TableValuedParameterFactory(string sqlTableTypeName)
            {
                this.sqlTableTypeName = sqlTableTypeName;
            }

            public ISqlComponent CreateFromPlainValues(IEnumerable enumerable)
            {
                return ToTableValuedParameter(sqlTableTypeName, (IEnumerable<T>)enumerable, TableValuedParameterWrapperHelper.WrapPlainValueInMetaObject);
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
    }

    namespace Internal
    {
        //public needed for auto-mapping
        public struct TableValuedParameterWrapper<T> : IMetaObject
        {
            [Key]
            public T QueryTableValue { get; set; }

            public override string ToString() => QueryTableValue == null ? "NULL" : QueryTableValue.ToString();
        }

        public static class TableValuedParameterWrapperHelper
        {
            /// <summary>
            /// Efficiently wraps an enumerable of objects in DbTableValuedParameterWrapper and materialized the sequence as array.
            /// Effectively it's like .Select(x => new DbTableValuedParameterWrapper { querytablevalue = x }).ToArray() but faster.
            /// </summary>
            public static TableValuedParameterWrapper<T>[] WrapPlainValueInMetaObject<T>(IEnumerable<T> typedEnumerable)
            {
                var typedArray = typedEnumerable as T[];
                if (typedArray != null) {
                    var projectedArray = new TableValuedParameterWrapper<T>[typedArray.Length];
                    for (var i = 0; i < projectedArray.Length; i++) {
                        projectedArray[i].QueryTableValue = typedArray[i];
                    }
                    return projectedArray;
                }

                var typedList = typedEnumerable as IReadOnlyList<T>;
                if (typedList != null) {
                    var projectedArray = new TableValuedParameterWrapper<T>[typedList.Count];
                    for (var i = 0; i < projectedArray.Length; i++) {
                        projectedArray[i].QueryTableValue = typedList[i];
                    }
                    return projectedArray;
                }

                var arrayBuilder = FastArrayBuilder<TableValuedParameterWrapper<T>>.Create();
                foreach (var item in typedEnumerable) {
                    arrayBuilder.Add(new TableValuedParameterWrapper<T> { QueryTableValue = item });
                }
                return arrayBuilder.ToArray();
            }
        }
    }
}
