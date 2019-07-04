using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;
using ProgressOnderwijsUtils.Internal;
using static ProgressOnderwijsUtils.SafeSql;

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

        [CanBeNull]
        static string GetEnumStringRepresentationOrNull([CanBeNull] Enum val)
            => val == null
                ? null
                : enumStringRepresentations
                    .GetOrAdd(val.GetType(), enumStringRepresentationValueFactory)
                    .GetOrDefault(((IConvertible)val).ToInt64(null));

        [CanBeNull]
        static string GetBooleanStringRepresentationOrNull(bool? val)
            => val == null ? null : val.Value ? "cast(1 as bit)" : "cast(0 as bit)";

        public static void AppendParamTo<TCommandFactory>(ref TCommandFactory factory, object o)
            where TCommandFactory : struct, ICommandFactory
        {
            if (o is IEnumerable enumerable && !(enumerable is string) && !(enumerable is byte[])) {
                ToTableValuedParameterFromPlainValues(enumerable).AppendTo(ref factory);
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

        public static ISqlComponent ToTableValuedParameterFromPlainValues([NotNull] IEnumerable set)
        {
            var enumerableType = set.GetType();
            if (!tableValuedParameterFactoryCache.TryGetValue(enumerableType, out var factory)) {
                factory = CreateTableValuedParameterFactory(enumerableType);
                tableValuedParameterFactoryCache.TryAdd(enumerableType, factory);
            }
            if (factory == null) {
                throw new ArgumentException("Cannot interpret " + enumerableType.ToCSharpFriendlyTypeName() + " as a table valued parameter", nameof(set));
            }
            return factory.CreateFromPlainValues(set);
        }

        [NotNull]
        public static ISqlComponent ToTableValuedParameter<TIn, TOut>(string tableTypeName, IEnumerable<TIn> set, Func<IEnumerable<TIn>, TOut[]> projection)
            where TOut : IMetaObject, new()
            => set is IReadOnlyList<TIn> fixedSizeList && fixedSizeList.Count == 1
                ? (ISqlComponent)new SingletonQueryTableValuedParameterComponent<TOut>(projection(set)[0])
                : new QueryTableValuedParameterComponent<TIn, TOut>(tableTypeName, set, projection);

        static readonly ConcurrentDictionary<Type, ITableValuedParameterFactory> tableValuedParameterFactoryCache = new ConcurrentDictionary<Type, ITableValuedParameterFactory>();

        [CanBeNull]
        static ITableValuedParameterFactory CreateTableValuedParameterFactory([NotNull] Type enumerableType)
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

            public static ParameterizedSql DefinitionScripts
                => SQL($@"
                    begin tran;
                    {All
                        .Select(o => SQL($@"
                            drop type if exists {ParameterizedSql.CreateDynamic(o.SqlTypeName)};
                            create type {ParameterizedSql.CreateDynamic(o.SqlTypeName)}
                            as table ({ParameterizedSql.CreateDynamic(o.TableDeclaration)});
                        "))
                        .ConcatenateSql()
                    }
                    commit;
                ");
        }

        interface ITableValuedParameterFactory
        {
            ISqlComponent CreateFromPlainValues(IEnumerable enumerable);
        }

        sealed class TableValuedParameterFactory<T> : ITableValuedParameterFactory
        {
            readonly string sqlTableTypeName;

            //cache delegate to save some allocs and avoid risking slow paths like COMDelegate::DelegateConstruct
            readonly Func<IEnumerable<T>, TableValuedParameterWrapper<T>[]> WrapPlainValueInMetaObject = TableValuedParameterWrapperHelper.WrapPlainValueInMetaObject;

            public TableValuedParameterFactory(string sqlTableTypeName)
                => this.sqlTableTypeName = sqlTableTypeName;

            [NotNull]
            public ISqlComponent CreateFromPlainValues(IEnumerable enumerable)
                => ToTableValuedParameter(sqlTableTypeName, (IEnumerable<T>)enumerable, WrapPlainValueInMetaObject);
        }

        [CanBeNull]
        static Type TryGetNonAmbiguousEnumerableElementType([NotNull] Type enumerableType)
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

        public static void AppendParamOrFragment<TCommandFactory>(ref TCommandFactory factory, object argument)
            where TCommandFactory : struct, ICommandFactory
        {
            var converter = argument == null ? null : MetaObjectPropertyConverter.GetOrNull(argument.GetType());
            if (argument is ParameterizedSql sql) {
                sql.AppendTo(ref factory);
            } else if (argument is INestableSql nestableSql) {
                nestableSql.Sql.AppendTo(ref factory);
            } else if (converter != null) {
                AppendParamTo(ref factory, converter.ConvertToDb(argument));
            } else {
                AppendParamTo(ref factory, argument);
            }
        }
    }

    namespace Internal
    {
        //public needed for auto-mapping
        public struct TableValuedParameterWrapper<T> : IMetaObject, IOptionalObjectProjectionForDebugging, IReadByReflection
        {
            [Key]
            public T QueryTableValue { get; set; }

            public override string ToString()
                => QueryTableValue == null ? "NULL" : QueryTableValue.ToString();

            public object ProjectionForDebuggingOrNull()
                => QueryTableValue;
        }

        public static class TableValuedParameterWrapperHelper
        {
            /// <summary>
            /// Efficiently wraps an enumerable of objects in DbTableValuedParameterWrapper and materialized the sequence as array.
            /// Effectively it's like .Select(x => new DbTableValuedParameterWrapper { querytablevalue = x }).ToArray() but faster.
            /// </summary>
            [NotNull]
            public static TableValuedParameterWrapper<T>[] WrapPlainValueInMetaObject<T>([NotNull] IEnumerable<T> typedEnumerable)
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

                var arrayBuilder = new ArrayBuilder<TableValuedParameterWrapper<T>>();
                foreach (var item in typedEnumerable) {
                    arrayBuilder.Add(new TableValuedParameterWrapper<T> { QueryTableValue = item });
                }
                return arrayBuilder.ToArray();
            }
        }
    }
}
