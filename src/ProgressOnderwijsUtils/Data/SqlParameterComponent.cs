﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.Collections;
using ProgressOnderwijsUtils.Internal;

namespace ProgressOnderwijsUtils
{
    static class SqlParameterComponent
    {
        static readonly Dictionary<long, string> empty = new Dictionary<long, string>();
        static readonly ConcurrentDictionary<Type, Dictionary<long, string>> enumStringRepresentations = new ConcurrentDictionary<Type, Dictionary<long, string>>();

        static readonly Func<Type, Dictionary<long, string>> enumStringRepresentationValueFactory =
            type => {
                var enumAttributes = type.GetCustomAttributes(true);
                foreach (var customAttribute in enumAttributes) {
                    if (customAttribute is IEnumShouldBeParameterizedInSqlAttribute) {
                        return empty;
                    }
                }
                var enumValues = Enum.GetValues(type);
                if (enumValues.Length == 0) {
                    return empty;
                }
                var literalSqlByEnumValue = new Dictionary<long, string>();
                foreach (Enum v in enumValues) {
                    var valueAsLong = ((IConvertible)v).ToInt64(null);
                    literalSqlByEnumValue.Add(valueAsLong, valueAsLong.ToStringInvariant() + "/*" + ObjectToCode.PlainObjectToCode(v) + "*/");
                }
                return literalSqlByEnumValue;
            };

        static string GetEnumStringRepresentationOrNull(Enum val)
            => val == null
                ? null
                : enumStringRepresentations
                    .GetOrAdd(val.GetType(), enumStringRepresentationValueFactory)
                    .GetOrDefault(((IConvertible)val).ToInt64(null));

        static string GetBooleanStringRepresentationOrNull(bool? val)
            => val == null ? null : val.Value ? "cast(1 as bit)" : "cast(0 as bit)";

        public static void AppendParamTo<TCommandFactory>(ref TCommandFactory factory, object o)
            where TCommandFactory : struct, ICommandFactory
        {
            if (o is IEnumerable && !(o is string) && !(o is byte[])) {
                ToTableValuedParameterFromPlainValues((IEnumerable)o).AppendTo(ref factory);
            } else {
                var literalSqlRepresentation =
                    GetBooleanStringRepresentationOrNull(o as bool?)
                        ?? GetEnumStringRepresentationOrNull(o as Enum);

                if (literalSqlRepresentation != null) {
                    factory.AppendSql(literalSqlRepresentation, 0, literalSqlRepresentation.Length);
                } else {
                    QueryScalarParameterComponent.AppendScalarParameter(ref factory, o);
                }
            }
        }

        public static ISqlComponent ToTableValuedParameterFromPlainValues(IEnumerable set)
        {
            var enumerableType = set.GetType();
            if (!tableValuedParameterFactoryCache.TryGetValue(enumerableType, out ITableValuedParameterFactory factory)) {
                factory = CreateTableValuedParameterFactory(enumerableType);
                tableValuedParameterFactoryCache.TryAdd(enumerableType, factory);
            }
            if (factory == null) {
                throw new ArgumentException("Cannot interpret " + enumerableType.ToCSharpFriendlyTypeName() + " as a table valued parameter", nameof(set));
            }
            return factory.CreateFromPlainValues(set);
        }

        public static ISqlComponent ToTableValuedParameter<TIn, TOut>(string tableTypeName, IEnumerable<TIn> set, Func<IEnumerable<TIn>, TOut[]> projection)
            where TOut : IMetaObject, new()
            => new QueryTableValuedParameterComponent<TIn, TOut>(tableTypeName, set, projection);

        static readonly ConcurrentDictionary<Type, ITableValuedParameterFactory> tableValuedParameterFactoryCache = new ConcurrentDictionary<Type, ITableValuedParameterFactory>();

        static ITableValuedParameterFactory CreateTableValuedParameterFactory(Type enumerableType)
        {
            var elementType = TryGetNonAmbiguousEnumerableElementType(enumerableType);
            if (elementType == null) {
                return null;
            }
            var underlyingType = elementType.GetUnderlyingType();
            var sqlTableTypeName = CustomTableType.SqlTableTypeNameByDotnetType.GetOrDefault(underlyingType);
            if (sqlTableTypeName == null) {
                return null;
            }
            var factoryType = typeof(TableValuedParameterFactory<>).MakeGenericType(elementType);
            return (ITableValuedParameterFactory)Activator.CreateInstance(factoryType, sqlTableTypeName);
        }

        internal struct CustomTableType
        {
            public readonly Type Type;
            public readonly string SqlTypeName;
            public readonly string TableDeclaration;

            CustomTableType(Type type, string sqlTypeName, string tableDeclaration)
            {
                Type = type;
                SqlTypeName = sqlTypeName;
                TableDeclaration = tableDeclaration;
            }

            public static readonly CustomTableType[] All = new[] {
                new CustomTableType(typeof(long), "TVar_Bigint", "querytablevalue bigint not null"),
                new CustomTableType(typeof(bool), "TVar_Bit", "querytablevalue bit not null"),
                new CustomTableType(typeof(DateTime), "TVar_DateTime2", "querytablevalue datetime2(7) not null"),
                new CustomTableType(typeof(decimal), "TVar_Decimal", "querytablevalue decimal(18, 0) not null"),
                new CustomTableType(typeof(double), "TVar_Float", "querytablevalue float not null"),
                new CustomTableType(typeof(int), "TVar_Int", "querytablevalue int not null, primary key clustered (querytablevalue asc) with (ignore_dup_key = off)"),
                new CustomTableType(typeof(char), "TVar_NChar1", "querytablevalue nchar(1) not null"),
                new CustomTableType(typeof(string), "TVar_NVarcharMax", "querytablevalue nvarchar(max) not null"),
                new CustomTableType(typeof(short), "TVar_Smallint", "querytablevalue smallint not null"),
                new CustomTableType(typeof(TimeSpan), "TVar_Time", "querytablevalue time(7) not null"),
                new CustomTableType(typeof(byte), "TVar_Tinyint", "querytablevalue tinyint not null"),
                new CustomTableType(typeof(byte[]), "TVar_VarBinaryMax", "querytablevalue varbinary(max) not null"),
            };

            public static readonly Dictionary<Type, string> SqlTableTypeNameByDotnetType = All.ToDictionary(o => o.Type, o => o.SqlTypeName);

            public static ParameterizedSql DefinitionScripts =>
                ParameterizedSql.CreateDynamic($@"
                    set transaction isolation level serializable;
                    begin tran
                    {All.Select(o => $@"
                        drop type if exists {o.SqlTypeName}
                        create type {o.SqlTypeName} as table ({o.TableDeclaration})
                    ").JoinStrings("\n")}
                    commit");
        }

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
        public struct TableValuedParameterWrapper<T> : IMetaObject, IOptionalObjectProjectionForDebugging
        {
            [Key]
            public T QueryTableValue { get; set; }

            public override string ToString() => QueryTableValue == null ? "NULL" : QueryTableValue.ToString();
            public object ProjectionForDebuggingOrNull() => QueryTableValue;
        }

        public static class TableValuedParameterWrapperHelper
        {
            /// <summary>
            /// Efficiently wraps an enumerable of objects in DbTableValuedParameterWrapper and materialized the sequence as array.
            /// Effectively it's like .Select(x => new DbTableValuedParameterWrapper { querytablevalue = x }).ToArray() but faster.
            /// </summary>
            public static TableValuedParameterWrapper<T>[] WrapPlainValueInMetaObject<T>(IEnumerable<T> typedEnumerable)
            {
                if (typedEnumerable is T[] typedArray) {
                    var projectedArray = new TableValuedParameterWrapper<T>[typedArray.Length];
                    for (var i = 0; i < projectedArray.Length; i++) {
                        projectedArray[i].QueryTableValue = typedArray[i];
                    }
                    return projectedArray;
                }

                if (typedEnumerable is IReadOnlyList<T> typedList) {
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
