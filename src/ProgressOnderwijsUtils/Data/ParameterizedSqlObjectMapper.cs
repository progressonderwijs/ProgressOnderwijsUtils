using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable ConvertToUsingDeclaration
namespace ProgressOnderwijsUtils
{
    public enum FieldMappingMode
    {
        RequireExactColumnMatches,
        IgnoreExtraPocoProperties,
    }

    public static class ParameterizedSqlObjectMapper
    {
        public static NonQuerySqlCommand OfNonQuery(this ParameterizedSql sql)
            => new NonQuerySqlCommand(sql, CommandTimeout.DeferToConnectionDefault);

        public static DataTableSqlCommand OfDataTable(this ParameterizedSql sql)
            => new DataTableSqlCommand(sql, CommandTimeout.DeferToConnectionDefault, MissingSchemaAction.Add);

        public static ScalarSqlCommand<T> OfScalar<T>(this ParameterizedSql sql)
            => new ScalarSqlCommand<T>(sql, CommandTimeout.DeferToConnectionDefault);

        public static BuiltinsSqlCommand<T> OfBuiltins<T>(this ParameterizedSql sql)
            => new BuiltinsSqlCommand<T>(sql, CommandTimeout.DeferToConnectionDefault);

        public static PocosSqlCommand<T> OfPocos<T>(this ParameterizedSql sql)
            where T : IWrittenImplicitly, new()
            => new PocosSqlCommand<T>(sql, CommandTimeout.DeferToConnectionDefault, FieldMappingMode.RequireExactColumnMatches);

        [return: MaybeNull]
        [MustUseReturnValue]
        public static T ReadScalar<T>(this ParameterizedSql sql, SqlConnection sqlConn)
            => sql.OfScalar<T>().Execute(sqlConn);

        /// <summary>Executes an sql statement and returns the number of rows affected.  Returns 0 without server interaction for whitespace-only commands.</summary>
        public static int ExecuteNonQuery(this ParameterizedSql sql, SqlConnection sqlConn)
            => sql.OfNonQuery().Execute(sqlConn);

        /// <summary>
        /// Reads all records of the given query from the database, unpacking into a C# array using each item's publicly writable fields and properties.
        /// Type T must have a public parameterless constructor; both structs and classes are supported
        /// The type T must match the queries columns by name (the order is not relevant).  Matching columns to properties/fields is case insensitive.
        /// The number of fields+properties must be the same as the number of columns
        /// </summary>
        /// <typeparam name="T">The type to unpack each record into</typeparam>
        /// <param name="q">The query to execute</param>
        /// <param name="sqlConn">The database connection</param>
        /// <returns>An array of strongly-typed objects; never null</returns>
        [MustUseReturnValue]
        public static T[] ReadPocos<[MeansImplicitUse(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
            T>(this ParameterizedSql q, SqlConnection sqlConn)
            where T : IWrittenImplicitly, new()
            => q.OfPocos<T>().Execute(sqlConn);

        internal static string UnpackingErrorMessage<T>(SqlDataReader? reader, int lastColumnRead)
            where T : IWrittenImplicitly, new()
        {
            if (reader?.IsClosed != false || lastColumnRead < 0) {
                return "";
            }
            var mps = PocoUtils.GetProperties<T>();
            var pocoTypeName = typeof(T).ToCSharpFriendlyTypeName();

            var sqlColName = reader.GetName(lastColumnRead);
            var pocoProperty = mps.GetByName(sqlColName);

            var sqlTypeName = reader.GetDataTypeName(lastColumnRead);
            var nonNullableFieldType = reader.GetFieldType(lastColumnRead) ?? throw new Exception("Missing field type for field " + lastColumnRead + " named " + sqlColName);

            bool? isValueNull = null;
            try {
                isValueNull = reader.IsDBNull(lastColumnRead);
            } catch {
                // ignore crash in error handling.
            }
            var fieldType = isValueNull ?? true ? nonNullableFieldType.MakeNullableType() ?? nonNullableFieldType : nonNullableFieldType;

            var expectedCsTypeName = fieldType.ToCSharpFriendlyTypeName();
            var actualCsTypeName = pocoProperty.DataType.ToCSharpFriendlyTypeName();
            var nullValueWarning = isValueNull ?? false ? "NULL value from " : "";

            return $"Cannot unpack {nullValueWarning}column {sqlColName} of type {sqlTypeName} (C#:{expectedCsTypeName}) into {pocoTypeName}.{pocoProperty.Name} of type {actualCsTypeName}";
        }

        /// <summary>
        /// Reads all records of the given query from the database, unpacking into a C# array using basic types (i.e. scalars)
        /// Type T must be int, long, string, decimal, double, bool, DateTime or byte[].
        /// </summary>
        /// <typeparam name="T">The type to unpack each record into</typeparam>
        /// <param name="q">The query to execute</param>
        /// <param name="sqlConn">The command creation context</param>
        /// <returns>An array of strongly-typed objects; never null</returns>
        [MustUseReturnValue]
        public static T[] ReadPlain<T>(this ParameterizedSql q, SqlConnection sqlConn)
            => q.OfBuiltins<T>().Execute(sqlConn);

        [MustUseReturnValue]
        internal static T[] ReadPlainUnpacker<T>(SqlCommand cmd)
        {
            using var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
            DataReaderSpecialization<SqlDataReader>.PlainImpl<T>.VerifyDataReaderShape(reader);
            var unpacker = DataReaderSpecialization<SqlDataReader>.PlainImpl<T>.ReadValue;
            var builder = new ArrayBuilder<T>();
            while (reader.Read()) {
                var nextRow = unpacker(reader);
                builder.Add(nextRow);
            }
            return builder.ToArray();
        }

        const BindingFlags binding = BindingFlags.Public | BindingFlags.Instance;

        static readonly Dictionary<Type, MethodInfo> getterMethodsByType =
            new Dictionary<Type, MethodInfo> {
                { typeof(byte[]), typeof(DbLoadingHelperImpl).GetMethod(nameof(DbLoadingHelperImpl.GetBytes), BindingFlags.Public | BindingFlags.Static)! },
                { typeof(char[]), typeof(DbLoadingHelperImpl).GetMethod(nameof(DbLoadingHelperImpl.GetChars), BindingFlags.Public | BindingFlags.Static)! },
                { typeof(bool), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetBoolean), binding)! },
                { typeof(byte), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetByte), binding)! },
                { typeof(char), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetChar), binding)! },
                { typeof(DateTime), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetDateTime), binding)! },
                { typeof(decimal), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetDecimal), binding)! },
                { typeof(double), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetDouble), binding)! },
                { typeof(float), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetFloat), binding)! },
                { typeof(Guid), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetGuid), binding)! },
                { typeof(short), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetInt16), binding)! },
                { typeof(int), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetInt32), binding)! },
                { typeof(long), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetInt64), binding)! },
                { typeof(string), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetString), binding)! },
            };

        static Dictionary<MethodInfo, MethodInfo> MakeMap(params InterfaceMapping[] mappings)
            => mappings
                .SelectMany(map => map.InterfaceMethods.Zip(map.TargetMethods, (interfaceMethod, targetMethod) => (interfaceMethod, targetMethod)))
                .ToDictionary(methodPair => methodPair.interfaceMethod, methodPair => methodPair.targetMethod);

        static readonly MethodInfo getTimeSpan_SqlDataReader = typeof(SqlDataReader).GetMethod(nameof(SqlDataReader.GetTimeSpan), binding)!;
        static readonly MethodInfo getDateTimeOffset_SqlDataReader = typeof(SqlDataReader).GetMethod(nameof(SqlDataReader.GetDateTimeOffset), binding)!;

        internal static class DataReaderSpecialization<TReader>
            where TReader : IDataReader
        {
            public delegate T TRowReader<out T>(TReader reader, out int lastColumnRead);

            static readonly Dictionary<MethodInfo, MethodInfo> InterfaceMap = MakeMap(
                typeof(TReader).GetInterfaceMap(typeof(IDataRecord)),
                typeof(TReader).GetInterfaceMap(typeof(IDataReader))
            );

            // ReSharper disable AssignNullToNotNullAttribute
            static readonly MethodInfo IsDBNullMethod = InterfaceMap[typeof(IDataRecord).GetMethod(nameof(IDataRecord.IsDBNull), binding)!];
            // ReSharper restore AssignNullToNotNullAttribute

            static readonly bool isSqlDataReader = typeof(TReader) == typeof(SqlDataReader);

            static bool IsSupportedBasicType(Type type)
            {
                var underlyingType = type.GetNonNullableUnderlyingType();

                return getterMethodsByType.ContainsKey(underlyingType)
                    || isSqlDataReader && (underlyingType == typeof(TimeSpan) || underlyingType == typeof(DateTimeOffset))
                    ;
            }

            static bool IsSupportedType(Type type)
                => IsSupportedBasicType(type) || PocoPropertyConverter.GetOrNull(type) is PocoPropertyConverter converter && IsSupportedBasicType(converter.DbType);

            static MethodInfo GetterForType(Type underlyingType)
            {
                if (isSqlDataReader && underlyingType == typeof(TimeSpan)) {
                    return getTimeSpan_SqlDataReader;
                } else if (isSqlDataReader && underlyingType == typeof(DateTimeOffset)) {
                    return getDateTimeOffset_SqlDataReader;
                } else if (PocoPropertyConverter.GetOrNull(underlyingType) is PocoPropertyConverter converter) {
                    return InterfaceMap[getterMethodsByType[converter.DbType]];
                } else {
                    return InterfaceMap[getterMethodsByType[underlyingType]];
                }
            }

            static Expression GetCastExpression(Expression callExpression, Type type)
            {
                var underlyingType = type.GetNonNullableUnderlyingType();
                var converter = PocoPropertyConverter.GetOrNull(underlyingType);

                var needsCast = underlyingType != type.GetNonNullableType();

                if (converter != null) {
                    return Expression.Invoke(Expression.Constant(converter.CompiledConverterFromDb), callExpression);
                } else if (needsCast) {
                    return Expression.Convert(callExpression, type.GetNonNullableType());
                } else {
                    return callExpression;
                }
            }

            public static Expression GetColValueExpr(ParameterExpression readerParamExpr, int i, Type type)
            {
                var canBeNull = type.CanBeNull();
                var underlyingType = type.GetNonNullableUnderlyingType();
                var iConstant = Expression.Constant(i);
                MethodCallExpression callExpr;
                if (underlyingType == typeof(byte[])) {
                    callExpr = Expression.Call(getterMethodsByType[underlyingType], readerParamExpr, iConstant);
                } else {
                    callExpr = Expression.Call(readerParamExpr, GetterForType(underlyingType), iConstant);
                }
                Expression colValueExpr;
                if (canBeNull) {
                    var test = Expression.Call(readerParamExpr, IsDBNullMethod, iConstant);
                    var ifDbNull = Expression.Default(type);
                    var ifNotDbNull = Expression.Convert(GetCastExpression(callExpr, type), type);
                    colValueExpr = Expression.Condition(test, ifDbNull, ifNotDbNull);
                } else {
                    colValueExpr = GetCastExpression(callExpr, type);
                }

                return colValueExpr;
            }

            public static class ByPocoImpl<T>
                where T : IWrittenImplicitly, new()
            {
                static readonly object constructionSync = new object();
                static readonly ConcurrentDictionary<ColumnOrdering, TRowReader<T>> loadRow_by_ordering;

                static Type type
                    => typeof(T);

                static string FriendlyName
                    => type.ToCSharpFriendlyTypeName();

                static readonly PocoProperties<T> pocoProperties = PocoProperties<T>.Instance;
                static readonly bool hasUnsupportedColumns;
                static readonly int WritablePropCount;

                static ByPocoImpl()
                {
                    WritablePropCount = 0;
                    foreach (var pocoProperty in pocoProperties) { //perf:no LINQ
                        if (pocoProperty.CanWrite && IsSupportedType(pocoProperty.DataType)) {
                            WritablePropCount++;
                        }
                    }
                    hasUnsupportedColumns = false;
                    foreach (var pocoProperty in pocoProperties) { //perf:no LINQ
                        if (pocoProperty.CanWrite && !IsSupportedType(pocoProperty.DataType)) {
                            hasUnsupportedColumns = true;
                            break;
                        }
                    }

                    loadRow_by_ordering = new ConcurrentDictionary<ColumnOrdering, TRowReader<T>>();
                }

                public static TRowReader<T> DataReaderToSingleRowUnpacker(TReader reader, FieldMappingMode fieldMappingMode)
                {
                    AssertColumnsCanBeMappedToObject(reader, fieldMappingMode);
                    var ordering = ColumnOrdering.FromReader(reader);
                    if (loadRow_by_ordering.TryGetValue(ordering, out var cachedRowReaderWithCols)) {
                        PooledSmallBufferAllocator<string>.ReturnToPool(ordering.Cols);
                        return cachedRowReaderWithCols;
                    } else {
                        lock (constructionSync) {
                            return loadRow_by_ordering.GetOrAdd(ordering, constructTRowReaderWithCols);
                        }
                    }
                }

                static void AssertColumnsCanBeMappedToObject(TReader reader, FieldMappingMode fieldMappingMode)
                {
                    if (reader.FieldCount > WritablePropCount
                        || (reader.FieldCount < WritablePropCount || hasUnsupportedColumns) && fieldMappingMode == FieldMappingMode.RequireExactColumnMatches) {
                        var columnNames = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToArray();
                        var publicWritableProperties = pocoProperties.Where(pocoProperty => IsSupportedType(pocoProperty.DataType)).Select(mp => mp.Name).ToArray();
                        var columnsThatCannotBeMapped = columnNames.Except(publicWritableProperties, StringComparer.OrdinalIgnoreCase);
                        var propertiesWithoutColumns = publicWritableProperties.Except(columnNames, StringComparer.OrdinalIgnoreCase);
                        throw new InvalidOperationException(
                            $"Cannot unpack DbDataReader ({reader.FieldCount} columns) into type {FriendlyName} ({WritablePropCount} properties with public setters)\n" +
                            $"Query columns without corresponding property: {columnsThatCannotBeMapped.JoinStrings(", ")}\n" +
                            $".net properties without corresponding query column: {propertiesWithoutColumns.JoinStrings(", ")}\n" +
                            $"All columns: {columnNames.Select(n => "\n\t- " + n).JoinStrings()}\n" +
                            $"{FriendlyName} properties: {publicWritableProperties.Select(n => "\n\t- " + n).JoinStrings()}\n"
                        );
                    }
                    if (WritablePropCount == 0) {
                        throw new InvalidOperationException("Poco " + FriendlyName + " has no writable columns with a supported type!");
                    }
                }

                static readonly Func<ColumnOrdering, TRowReader<T>> constructTRowReaderWithCols = columnOrdering => {
                    var cols = columnOrdering.Cols;
                    var dataReaderParamExpr = Expression.Parameter(typeof(TReader), "dataReader");
                    var lastColumnReadParamExpr = Expression.Parameter(typeof(int).MakeByRefType(), "lastColumnRead");
                    var statements = new List<Expression>(2 + cols.Length * 2);
                    var rowVar = Expression.Variable(typeof(T));
                    statements.Add(Expression.Assign(rowVar, Expression.New(typeof(T))));
                    ReadAllFields(dataReaderParamExpr, rowVar, cols, lastColumnReadParamExpr, statements);
                    statements.Add(rowVar);
                    var constructRowExpr = Expression.Block(typeof(T), new[] { rowVar }, statements);
                    var loadRowsParamExprs = new[] { dataReaderParamExpr, lastColumnReadParamExpr };
                    var loadRowsLambda = Expression.Lambda<TRowReader<T>>(constructRowExpr, "LoadRows", loadRowsParamExprs);
                    return loadRowsLambda.Compile();
                };

                static void ReadAllFields(ParameterExpression dataReaderParamExpr, ParameterExpression rowVar, string[] cols, ParameterExpression lastColumnReadParamExpr, List<Expression> statements)
                {
                    var isPocoPropertyIndexAlreadyUsed = new bool[pocoProperties.Count];
                    for (var i = 0; i < cols.Length; i++) {
                        var colName = cols[i];
                        if (!pocoProperties.IndexByName.TryGetValue(colName, out var propertyIndex)) {
                            throw new ArgumentOutOfRangeException("Cannot resolve IDataReader column " + colName + " in type " + FriendlyName);
                        }
                        if (isPocoPropertyIndexAlreadyUsed[propertyIndex]) {
                            throw new InvalidOperationException("IDataReader has two identically named columns " + colName + "!");
                        }
                        isPocoPropertyIndexAlreadyUsed[propertyIndex] = true;
                        var member = pocoProperties[propertyIndex];
                        var memberInfo = BackingFieldDetector.BackingFieldOfPropertyOrNull(member.PropertyInfo) ?? (MemberInfo)member.PropertyInfo;
                        statements.Add(Expression.Assign(lastColumnReadParamExpr, Expression.Constant(i)));
                        statements.Add(Expression.Assign(Expression.MakeMemberAccess(rowVar, memberInfo), GetColValueExpr(dataReaderParamExpr, i, member.DataType)));
                    }
                }
            }

            public static class PlainImpl<T>
            {
                static string FriendlyName
                    => type.ToCSharpFriendlyTypeName();

                public static readonly Func<TReader, T> ReadValue;

                static Type type
                    => typeof(T);

                static Type UnderlyingType
                    => (PocoPropertyConverter.GetOrNull(type) is PocoPropertyConverter converter ? converter.DbType : type).GetNonNullableUnderlyingType();

                static PlainImpl()
                {
                    VerifyTypeValidity();
                    var dataReaderParamExpr = Expression.Parameter(typeof(TReader), "dataReader");
                    var loadRowsLambda = Expression.Lambda<Func<TReader, T>>(GetColValueExpr(dataReaderParamExpr, 0, type), dataReaderParamExpr);
                    ReadValue = loadRowsLambda.Compile();
                }

                static void VerifyTypeValidity()
                {
                    if (!IsSupportedType(type)) {
                        throw new ArgumentException(
                            FriendlyName + " cannot be auto loaded as plain data since it isn't a basic type ("
                            + getterMethodsByType.Keys.Select(ObjectToCode.ToCSharpFriendlyTypeName).JoinStrings(", ") + ")!"
                        );
                    }
                }

                public static void VerifyDataReaderShape(TReader reader)
                {
                    if (reader.FieldCount != 1) {
                        throw new InvalidOperationException("Cannot unpack DbDataReader into type " + FriendlyName + "; column count = " + reader.FieldCount + " != 1");
                    }
                    if (!Enumerable.Range(0, reader.FieldCount)
                        .Select(reader.GetFieldType)
                        .SequenceEqual(new[] { UnderlyingType })) {
                        throw new InvalidOperationException(
                            "Cannot unpack DbDataReader into type " + FriendlyName + ":\n"
                            + Enumerable.Range(0, reader.FieldCount)
                                .Select(i => reader.GetName(i) + " : " + reader.GetFieldType(i).ToCSharpFriendlyTypeName())
                                .JoinStrings(", ")
                        );
                    }
                }
            }
        }
    }
}
