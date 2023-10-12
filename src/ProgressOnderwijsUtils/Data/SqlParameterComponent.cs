using System.ComponentModel.DataAnnotations;
using ProgressOnderwijsUtils.Internal;

namespace ProgressOnderwijsUtils
{
    static class SqlParameterComponent
    {
        static readonly Dictionary<long, string> empty = new();
        static readonly ConcurrentDictionary<Type, Dictionary<long, string>> enumStringRepresentations = new();

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
                foreach (var v in enumValues) {
                    var valueAsLong = ((IConvertible)v!).ToInt64(null);
                    literalSqlByEnumValue.Add(valueAsLong, $"{valueAsLong.ToStringInvariant()}/*{ObjectToCode.PlainObjectToCode(v)}*/");
                }
                return literalSqlByEnumValue;
            };

        static string? GetEnumStringRepresentationOrNull(Enum? val)
            => val == null
                ? null
                : enumStringRepresentations
                    .GetOrAdd(val.GetType(), enumStringRepresentationValueFactory)
                    .GetValueOrDefault(((IConvertible)val).ToInt64(null));

        static string? GetBooleanStringRepresentationOrNull(bool? val)
            => val == null
                ? null
                : val.Value
                    ? "cast(1 as bit)"
                    : "cast(0 as bit)";

        public static void AppendParamTo<TCommandFactory>(ref TCommandFactory factory, object? o)
            where TCommandFactory : struct, ICommandFactory
        {
            if (o is IEnumerable enumerable and not string and not byte[]) {
                ToTableValuedParameterFromPlainValues(enumerable).AppendTo(ref factory);
            } else {
                var literalSqlRepresentation =
                    GetBooleanStringRepresentationOrNull(o as bool?)
                    ?? GetEnumStringRepresentationOrNull(o as Enum);

                if (literalSqlRepresentation != null) {
                    factory.AppendSql(literalSqlRepresentation);
                } else {
                    QueryScalarParameterComponent.AppendScalarParameter(ref factory, o);
                }
            }
        }

        public static ISqlComponent ToTableValuedParameterFromPlainValues(IEnumerable set)
        {
            var enumerableType = set.GetType();
            if (!tableValuedParameterFactoryCache.TryGetValue(enumerableType, out var factory)) {
                factory = CreateTableValuedParameterFactory(enumerableType);
                _ = tableValuedParameterFactoryCache.TryAdd(enumerableType, factory);
            }
            if (factory == null) {
                throw new ArgumentException($"Cannot interpret {enumerableType.ToCSharpFriendlyTypeName()} as a table valued parameter", nameof(set));
            }
            return factory.CreateFromPlainValues(set);
        }

        public static ISqlComponent ToTableValuedParameter<TIn, TOut>(string tableTypeName, IEnumerable<TIn> set, Func<IEnumerable<TIn>, TOut[]> projection)
            where TOut : IReadImplicitly, new()
            => set is IReadOnlyList<TIn> { Count: 1, }
                ? new SingletonQueryTableValuedParameterComponent<TOut>(projection(set)[0])
                : new QueryTableValuedParameterComponent<TIn, TOut>(tableTypeName, set, projection);

        static readonly ConcurrentDictionary<Type, ITableValuedParameterFactory?> tableValuedParameterFactoryCache = new();

        static ITableValuedParameterFactory? CreateTableValuedParameterFactory(Type enumerableType)
        {
            var elementType = TryGetNonAmbiguousEnumerableElementType(enumerableType);
            if (elementType == null) {
                return null;
            }
            var converter = AutomaticValueConverters.GetOrNull(elementType);
            var underlyingType = converter?.ProviderClrType ?? elementType.GetUnderlyingType();
            var sqlTableTypeName = CustomTableType.SqlTableTypeNameByDotnetType.GetValueOrDefault(underlyingType);
            if (sqlTableTypeName == null) {
                return null;
            }
            var factoryType = typeof(TableValuedParameterFactory<>).MakeGenericType(elementType);
            return (ITableValuedParameterFactory?)Activator.CreateInstance(factoryType, sqlTableTypeName);
        }

        internal readonly struct CustomTableType
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

            public static readonly CustomTableType[] All = {
                new(typeof(long), "TVar_Bigint", "querytablevalue bigint not null"),
                new(typeof(bool), "TVar_Bit", "querytablevalue bit not null"),
                new(typeof(DateTime), "TVar_DateTime2", "querytablevalue datetime2(7) not null"),
                new(typeof(decimal), "TVar_Decimal", "querytablevalue decimal(18, 0) not null"),
                new(typeof(double), "TVar_Float", "querytablevalue float not null"),
                new(typeof(int), "TVar_Int", "querytablevalue int not null, primary key clustered (querytablevalue asc) with (ignore_dup_key = off)"),
                new(typeof(char), "TVar_NChar1", "querytablevalue nchar(1) not null"),
                new(typeof(string), "TVar_NVarcharMax", "querytablevalue nvarchar(max) not null"),
                new(typeof(short), "TVar_Smallint", "querytablevalue smallint not null"),
                new(typeof(TimeSpan), "TVar_Time", "querytablevalue time(7) not null"),
                new(typeof(byte), "TVar_Tinyint", "querytablevalue tinyint not null"),
                new(typeof(byte[]), "TVar_VarBinaryMax", "querytablevalue varbinary(max) not null"),
                new(typeof(Guid), "TVar_Uniqueidentifier", "querytablevalue uniqueidentifier not null"),
            };

            public static readonly Dictionary<Type, string> SqlTableTypeNameByDotnetType = All.ToDictionary(o => o.Type, o => o.SqlTypeName);

            public static ParameterizedSql DefinitionScripts
                => SQL(
                    $@"
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
                "
                );
        }

        interface ITableValuedParameterFactory
        {
            ISqlComponent CreateFromPlainValues(IEnumerable enumerable);
        }

        sealed class TableValuedParameterFactory<T> : ITableValuedParameterFactory
        {
            readonly string sqlTableTypeName;

            //cache delegate to save some allocs and avoid risking slow paths like COMDelegate::DelegateConstruct
            readonly Func<IEnumerable<T>, TableValuedParameterWrapper<T>[]> WrapPlainValueInSinglePropertyPoco = TableValuedParameterWrapperHelper.WrapPlainValueInSinglePropertyPoco;

            public TableValuedParameterFactory(string sqlTableTypeName)
                => this.sqlTableTypeName = sqlTableTypeName;

            public ISqlComponent CreateFromPlainValues(IEnumerable enumerable)
                => ToTableValuedParameter(sqlTableTypeName, (IEnumerable<T>)enumerable, WrapPlainValueInSinglePropertyPoco);
        }

        static Type? TryGetNonAmbiguousEnumerableElementType(Type enumerableType)
        {
            var elementType = default(Type);
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

        public static void AppendParamOrFragment<TCommandFactory>(ref TCommandFactory factory, object? argument)
            where TCommandFactory : struct, ICommandFactory
        {
            if (argument is ParameterizedSql sql) {
                sql.AppendTo(ref factory);
            } else if (argument is INestableSql nestableSql) {
                nestableSql.Sql.AppendTo(ref factory);
            } else if (argument is { } and not Enum && AutomaticValueConverters.GetOrNull(argument.GetType()) is { } converter) {
                AppendParamTo(ref factory, converter.ConvertToProvider(argument));
            } else {
                AppendParamTo(ref factory, argument);
            }
        }
    }

    namespace Internal
    {
        //public needed for auto-mapping
        public struct TableValuedParameterWrapper<T> : IWrittenImplicitly, IOptionalObjectProjectionForDebugging, IReadImplicitly
        {
            [Key]
            public T QueryTableValue { get; init; }

            public override string? ToString()
                => QueryTableValue is null ? "NULL" : QueryTableValue.ToString();

            public object? ProjectionForDebuggingOrNull()
                => QueryTableValue;
        }

        public static class TableValuedParameterWrapperHelper
        {
            /// <summary>
            /// Efficiently wraps an enumerable of objects in DbTableValuedParameterWrapper and materialized the sequence as array.
            /// Effectively it's like .Select(x => new DbTableValuedParameterWrapper { querytablevalue = x }).ToArray() but faster.
            /// </summary>
            public static TableValuedParameterWrapper<T>[] WrapPlainValueInSinglePropertyPoco<T>(IEnumerable<T> typedEnumerable)
            {
                if (typedEnumerable is T[] typedArray) {
                    var projectedArray = new TableValuedParameterWrapper<T>[typedArray.Length];
                    for (var i = 0; i < projectedArray.Length; i++) {
                        projectedArray[i] = new() { QueryTableValue = typedArray[i], };
                    }
                    return projectedArray;
                }

                if (typedEnumerable is IReadOnlyList<T> typedList) {
                    var projectedArray = new TableValuedParameterWrapper<T>[typedList.Count];
                    for (var i = 0; i < projectedArray.Length; i++) {
                        projectedArray[i] = new() { QueryTableValue = typedList[i], };
                    }
                    return projectedArray;
                }

                var arrayBuilder = new ArrayBuilder<TableValuedParameterWrapper<T>>();
                foreach (var item in typedEnumerable) {
                    arrayBuilder.Add(new() { QueryTableValue = item, });
                }
                return arrayBuilder.ToArray();
            }
        }
    }
}
