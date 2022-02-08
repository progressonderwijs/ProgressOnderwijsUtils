using System.Buffers;
using Microsoft.EntityFrameworkCore.Query;

// ReSharper disable ConvertToUsingDeclaration
namespace ProgressOnderwijsUtils;

public enum FieldMappingMode { RequireExactColumnMatches, IgnoreExtraPocoProperties, }

public static class ParameterizedSqlObjectMapper
{
    public static NonQuerySqlCommand OfNonQuery(this ParameterizedSql sql)
        => new(sql, CommandTimeout.DeferToConnectionDefault);

    public static DataTableSqlCommand OfDataTable(this ParameterizedSql sql)
        => new(sql, CommandTimeout.DeferToConnectionDefault, MissingSchemaAction.Add);

    public static ScalarSqlCommand<T> OfScalar<T>(this ParameterizedSql sql)
        => new(sql, CommandTimeout.DeferToConnectionDefault);

    public static BuiltinsSqlCommand<T> OfBuiltins<T>(this ParameterizedSql sql)
        => new(sql, CommandTimeout.DeferToConnectionDefault);

    public static PocosSqlCommand<T> OfPocos<T>(this ParameterizedSql sql)
        where T : IWrittenImplicitly
        => new(sql, CommandTimeout.DeferToConnectionDefault, FieldMappingMode.RequireExactColumnMatches);

    [return: MaybeNull]
    [MustUseReturnValue]
    public static T ReadScalar<T>(this ParameterizedSql sql, SqlConnection sqlConn)
        => sql.OfScalar<T>().Execute(sqlConn);

    /// <summary>Executes an sql statement</summary>
    public static void ExecuteNonQuery(this ParameterizedSql sql, SqlConnection sqlConn)
        => sql.OfNonQuery().Execute(sqlConn);

    /// <summary>Executes an sql statement and returns the number of rows affected.  Returns 0 without server voideraction for whitespace-only commands.</summary>
    public static void ExecuteNonQuery(this ParameterizedSql sql, SqlConnection sqlConn, out int nrOfRowsAffected)
        => sql.OfNonQuery().Execute(sqlConn, out nrOfRowsAffected);

    /// <summary>
    /// Reads all records of the given query from the database, unpacking into a C# array using each item's publicly writable fields and properties.
    /// Type T can be of type record, struct class
    /// The type T must match the queries columns by name (the order is not relevant).  Matching columns to properties/fields is case insensitive.
    /// The number of fields+properties must be the same as the number of columns
    /// </summary>
    /// <typeparam name="T">The type to unpack each record into</typeparam>
    /// <param name="q">The query to execute</param>
    /// <param name="sqlConn">The database connection</param>
    /// <returns>An array of strongly-typed objects; never null</returns>
    [MustUseReturnValue]
    public static T[] ReadPocos<[MeansImplicitUse(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)] T>(this ParameterizedSql q, SqlConnection sqlConn)
        where T : IWrittenImplicitly
        => q.OfPocos<T>().Execute(sqlConn);

    internal static string UnpackingErrorMessage<T>(SqlDataReader? reader, int lastColumnRead)
        where T : IWrittenImplicitly
    {
        if (reader?.IsClosed != false || lastColumnRead < 0) {
            return "";
        }
        var mps = PocoUtils.GetProperties<T>();
        var pocoTypeName = typeof(T).ToCSharpFriendlyTypeName();

        var sqlColName = reader.GetName(lastColumnRead);
        var pocoProperty = mps.GetByName(sqlColName);

        var sqlTypeName = reader.GetDataTypeName(lastColumnRead);
        var nonNullableFieldType = reader.GetFieldType(lastColumnRead) ?? throw new($"Missing field type for field {lastColumnRead} named {sqlColName}");

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
    public static T?[] ReadPlain<T>(this ParameterizedSql q, SqlConnection sqlConn)
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
        new() {
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

    static readonly ArrayPool<byte> pool = ArrayPool<byte>.Create(16, Environment.ProcessorCount * 2);

    static byte[] GetBytes(this IDataRecord row, int colIndex)
    {
        var byteCount = row.GetBytes(colIndex, 0L, null!, 0, 0);
        if (byteCount > int.MaxValue) {
            throw new NotSupportedException("Array too large!");
        }
        var arr = new byte[byteCount];
        long offset = 0;
        while (offset < byteCount) {
            offset += row.GetBytes(colIndex, offset, arr, (int)offset, (int)byteCount);
        }
        return arr;
    }

    static char[] GetChars(this IDataRecord row, int colIndex)
    {
        var charCount = row.GetChars(colIndex, 0L, null!, 0, 0);
        if (charCount > int.MaxValue) {
            throw new NotSupportedException("Array too large!");
        }
        var arr = new char[charCount];
        long offset = 0;
        while (offset < charCount) {
            offset += row.GetChars(colIndex, offset, arr, (int)offset, (int)charCount);
        }
        return arr;
    }

    static ulong ReadUInt64(IDataRecord reader, int i)
    {
        var arr = pool.Rent(12);
        var bytesRead = reader.GetBytes(i, 0, arr, 0, 12);
        if (bytesRead > 8) {
            pool.Return(arr);
            throw new("Tried to read a ulong, but result too much data");
        }
        var uint64val = SqlBinaryToUInt64(arr);
        arr.AsSpan(0, 8).Clear();
        pool.Return(arr);
        return uint64val;
    }

    internal static ulong SqlBinaryToUInt64(byte[] arr)
    {
        var uint64val = BitConverter.ToUInt64(arr, 0); //or this: Unsafe.ReadUnaligned<ulong>(ref arr[0]);
        //https://stackoverflow.com/questions/19560436/bitwise-endian-swap-for-various-types
        uint64val = uint64val >> 32 | uint64val << 32;
        uint64val = (uint64val & 0xFFFF0000FFFF0000U) >> 16 | (uint64val & 0x0000FFFF0000FFFFU) << 16;
        uint64val = (uint64val & 0xFF00FF00FF00FF00U) >> 8 | (uint64val & 0x00FF00FF00FF00FFU) << 8;
        return uint64val;
    }

    static uint ReadUInt32(IDataRecord reader, int i)
    {
        var arr = pool.Rent(12);
        var bytesRead = reader.GetBytes(i, 0, arr, 0, 12);
        if (bytesRead > 4) {
            pool.Return(arr);
            throw new("Tried to read a ulong, but result had too much data");
        }
        var uint32val = SqlBinaryToUInt32(arr);
        arr.AsSpan(0, 8).Clear();
        pool.Return(arr);
        return uint32val;
    }

    internal static uint SqlBinaryToUInt32(byte[] arr)
    {
        var uint32val = BitConverter.ToUInt32(arr, 0); //or this: Unsafe.ReadUnaligned<ulong>(ref arr[0]);
        uint32val = (uint32val & 0xFFFF0000U) >> 16 | (uint32val & 0x0000FFFFU) << 16;
        uint32val = (uint32val & 0xFF00FF00U) >> 8 | (uint32val & 0x00FF00FFU) << 8;
        return uint32val;
    }

    static readonly MethodInfo getTimeSpan_SqlDataReader = typeof(SqlDataReader).GetMethod(nameof(SqlDataReader.GetTimeSpan), binding)!;
    static readonly MethodInfo getDateTimeOffset_SqlDataReader = typeof(SqlDataReader).GetMethod(nameof(SqlDataReader.GetDateTimeOffset), binding)!;
    static readonly MethodInfo getUInt64 = ((Func<IDataRecord, int, ulong>)ReadUInt64).Method;
    static readonly MethodInfo getUInt32 = ((Func<IDataRecord, int, uint>)ReadUInt32).Method;
    static readonly MethodInfo getBytes = ((Func<IDataRecord, int, byte[]>)GetBytes).Method;
    static readonly MethodInfo getChars = ((Func<IDataRecord, int, char[]>)GetChars).Method;

    internal static class DataReaderSpecialization<TReader>
        where TReader : IDataReader
    {
        public delegate T TRowReader<out T>(TReader reader, out int lastColumnRead);

        static readonly Dictionary<MethodInfo, MethodInfo> InterfaceMap = MakeMap(typeof(TReader).GetInterfaceMap(typeof(IDataRecord)));
        static readonly MethodInfo IsDBNullMethod = InterfaceMap[typeof(IDataRecord).GetMethod(nameof(IDataRecord.IsDBNull), binding)!];
        static readonly bool isSqlDataReader = typeof(TReader) == typeof(SqlDataReader);

        static bool IsSupportedType(Type type)
            => GetBuiltInMethod(type.GetNonNullableUnderlyingType()) is not null
                || AutomaticValueConverters.GetOrNull(type.GetNonNullableType()) is { } converter && GetBuiltInMethod(converter.ProviderClrType) is not null;

        static MethodInfo? GetBuiltInMethod(Type underlyingType)
            => underlyingType switch {
                _ when underlyingType == typeof(ulong) => getUInt64,
                _ when underlyingType == typeof(uint) => getUInt32,
                _ when underlyingType == typeof(byte[]) => getBytes,
                _ when underlyingType == typeof(char[]) => getChars,
                _ when underlyingType == typeof(TimeSpan) && isSqlDataReader => getTimeSpan_SqlDataReader,
                _ when underlyingType == typeof(DateTimeOffset) && isSqlDataReader => getDateTimeOffset_SqlDataReader,
                _ when getterMethodsByType.TryGetValue(underlyingType, out var interfaceGetter) => InterfaceMap[interfaceGetter],
                _ => null,
            };

        static Expression GetColValueExpr(ParameterExpression readerParamExpr, ConstantExpression fieldIdxExpr, Type type)
        {
            var colValueExpr = GetColValueExpr_AssumeNonnull(readerParamExpr, type, fieldIdxExpr);
            if (type.CanBeNull()) {
                var test = Expression.Call(readerParamExpr, IsDBNullMethod, fieldIdxExpr);
                var ifDbNull = Expression.Default(type);
                var ifNotDbNull = Expression.Convert(colValueExpr, type);
                return Expression.Condition(test, ifDbNull, ifNotDbNull);
            } else {
                return colValueExpr;
            }
        }

        static Expression GetColValueExpr_AssumeNonnull(ParameterExpression readerParamExpr, Type type, ConstantExpression fieldIdxExpr)
        {
            var nonNullableType = type.GetNonNullableType();
            var nonNullableUnderlyingType = nonNullableType.GetUnderlyingType();
            if (GetBuiltInExprOrNull(readerParamExpr, fieldIdxExpr, nonNullableUnderlyingType) is { } builtin) {
                return nonNullableUnderlyingType != nonNullableType ? Expression.Convert(builtin, nonNullableType) : builtin;
            } else {
                var converter = AutomaticValueConverters.GetOrNull(nonNullableUnderlyingType) ?? throw new($"Type {type.ToCSharpFriendlyTypeName()} is not  built-in and has no PocoPropertyConverter");
                var callExpr = GetBuiltInExprOrNull(readerParamExpr, fieldIdxExpr, converter.ProviderClrType) ?? throw new($"The converter for {type.ToCSharpFriendlyTypeName()} produces {converter.ProviderClrType.ToCSharpFriendlyTypeName()} which is not db-mappable");
                return ReplacingExpressionVisitor.Replace(converter.ConvertFromProviderExpression.Parameters.Single(), callExpr, converter.ConvertFromProviderExpression.Body);
            }
        }

        static Expression? GetBuiltInExprOrNull(ParameterExpression readerParamExpr, ConstantExpression fieldIdxExpr, Type nonNullableUnderlyingType)
            => GetBuiltInMethod(nonNullableUnderlyingType) is not { } builtin
                ? null
                : builtin.IsStatic
                    ? Expression.Call(builtin, readerParamExpr, fieldIdxExpr)
                    : Expression.Call(readerParamExpr, builtin, fieldIdxExpr);

        public static class ByPocoImpl<T>
            where T : IWrittenImplicitly
        {
            static readonly object constructionSync = new();
            static readonly ConcurrentDictionary<ColumnOrdering, (TRowReader<T> rowToPoco, IPocoProperty<T>[] unmappedProperties)> rowToPocoByColumnOrdering = new();

            public static TRowReader<T> DataReaderToSingleRowUnpacker(TReader reader, FieldMappingMode fieldMappingMode)
            {
                var ordering = ColumnOrdering.FromReader(reader);
                if (rowToPocoByColumnOrdering.TryGetValue(ordering, out var match)) {
                    PooledSmallBufferAllocator<string>.ReturnToPool(ordering.Cols);
                } else {
                    lock (constructionSync) {
                        match = rowToPocoByColumnOrdering.GetOrAdd(ordering, constructTRowReaderWithCols);
                    }
                }
                if (fieldMappingMode == FieldMappingMode.RequireExactColumnMatches && match.unmappedProperties.Length > 0) {
                    throw new(
                        "Some properties were unmapped: "
                        + match.unmappedProperties
                            .Select(prop => $"{prop.DataType.ToCSharpFriendlyTypeName()} {prop.Name}")
                            .JoinStrings("; ")
                    );
                }
                return match.rowToPoco;
            }

            static readonly Func<ColumnOrdering, (TRowReader<T> rowToPoco, IPocoProperty<T>[] unmappedProperties)> constructTRowReaderWithCols = columnOrdering => {
                var (rowToPoco, unmappedProperties) = ConstructPocoTRowReader(columnOrdering, typeof(TRowReader<T>), PocoProperties<T>.Instance);
                return (rowToPoco: (TRowReader<T>)rowToPoco, unmappedProperties.ArraySelect(o => (IPocoProperty<T>)o));
            };
        }

        public static class PlainImpl<T>
        {
            public static readonly Func<TReader, T> ReadValue;

            static Type type
                => typeof(T);

            static PlainImpl()
            {
                VerifyTypeValidity();
                var dataReaderParamExpr = Expression.Parameter(typeof(TReader), "dataReader");
                var loadRowsLambda = Expression.Lambda<Func<TReader, T>>(GetColValueExpr(dataReaderParamExpr, Expression.Constant(0), type), dataReaderParamExpr);
                ReadValue = loadRowsLambda.Compile();
            }

            static void VerifyTypeValidity()
            {
                if (!IsSupportedType(type)) {
                    throw new ArgumentException(
                        type.ToCSharpFriendlyTypeName() + " cannot be auto loaded as plain data since it isn't a basic type ("
                        + getterMethodsByType.Keys.Select(ObjectToCode.ToCSharpFriendlyTypeName).JoinStrings(", ") + ")!"
                    );
                }
            }

            public static void VerifyDataReaderShape(TReader reader)
            {
                if (reader.FieldCount != 1) {
                    throw new InvalidOperationException($"Cannot unpack DbDataReader into type {type.ToCSharpFriendlyTypeName()}; column count = {reader.FieldCount} != 1");
                }
            }
        }

        public static (Delegate rowToPoco, IPocoProperty[] unmappedProperties) ConstructPocoTRowReader(ColumnOrdering columnOrdering, Type constructedTRowReaderType, IPocoProperties<IPocoProperty> pocoProperties)
        {
            var cols = columnOrdering.Cols;
            var dataReaderParamExpr = Expression.Parameter(typeof(TReader), "dataReader");
            var lastColumnReadParamExpr = Expression.Parameter(typeof(int).MakeByRefType(), "lastColumnRead");
            var (constructRowExpr, unmappedProperties) = ReadAllFieldsExpression(dataReaderParamExpr, cols, lastColumnReadParamExpr, pocoProperties);
            var rowToPocoParamExprs = new[] { dataReaderParamExpr, lastColumnReadParamExpr, };
            var rowToPocoLambda = Expression.Lambda(constructedTRowReaderType, constructRowExpr, "RowToPoco", rowToPocoParamExprs);
            return (rowToPoco: rowToPocoLambda.Compile(), unmappedProperties);
        }

        public static (BlockExpression constructRowExpr, IPocoProperty[] unmappedProperties) ReadAllFieldsExpression(ParameterExpression dataReaderParamExpr, string[] cols, ParameterExpression lastColumnReadParamExpr, IPocoProperties<IPocoProperty> pocoProperties)
        {
            string FriendlyName()
                => pocoProperties.PocoType.ToCSharpFriendlyTypeName();
            var statements = new List<Expression>(2 + cols.Length * 2);

            var propertyFlags = new (bool coveredByReaderColumn, bool viaConstructor)[pocoProperties.Count];
            var variablesByPropIdx = new ParameterExpression[pocoProperties.Count];
            var errors = new List<string>();
            var minimalViaConstructorCount = 0;
            for (var columnIndex = 0; columnIndex < cols.Length; columnIndex++) {
                if (!(
                        pocoProperties.IndexByName.TryGetValue(cols[columnIndex], out var propertyIndex)
                        && pocoProperties[propertyIndex] is { } member
                    )) {
                    errors.Add($"Cannot resolve IDataReader column {cols[columnIndex]} in type {FriendlyName()}");
                } else if (propertyFlags[propertyIndex].coveredByReaderColumn) {
                    errors.Add($"The C# property {pocoProperties.PocoType.ToCSharpFriendlyTypeName()}.{member.Name} has already been mapped; are there two identically names columns?");
                } else if (!IsSupportedType(member.DataType)) {
                    errors.Add(
                        "The C# property " + pocoProperties.PocoType.ToCSharpFriendlyTypeName() + "." + member.Name + " if of type " + member.DataType.ToCSharpFriendlyTypeName()
                        + " which has no supported conversion from a DbDataReaderColumn."
                    );
                } else {
                    propertyFlags[propertyIndex].coveredByReaderColumn = true;
                    var variable = Expression.Variable(member.DataType, member.Name);
                    variablesByPropIdx[propertyIndex] = variable;
                    var fieldIdxExpr = Expression.Constant(columnIndex);
                    statements.Add(Expression.Assign(lastColumnReadParamExpr, fieldIdxExpr));
                    statements.Add(Expression.Assign(variablesByPropIdx[propertyIndex], GetColValueExpr(dataReaderParamExpr, fieldIdxExpr, member.DataType)));
                    if (!member.CanWrite) {
                        minimalViaConstructorCount++;
                        propertyFlags[propertyIndex].viaConstructor = true;
                    }
                }
            }

            if (errors.Any()) {
                throw new InvalidOperationException($"Cannot unpack DbDataReader ({cols.Length} columns) into type {FriendlyName()} ({pocoProperties.Count} properties)\n{errors.JoinStrings("\n")}\n");
            }

            ConstructorInfo? bestCtor = null;
            ParameterInfo[]? bestCtorParameters = null;

            foreach (var ctorInfo in pocoProperties.PocoType.GetConstructors()) {
                var ctorParameters = ctorInfo.GetParameters();
                if (bestCtorParameters != null && ctorParameters.Length <= bestCtorParameters.Length) {
                    continue;
                }

                var possibleMatch = true;
                var propsWithoutSetterWithoutConstructorArg = minimalViaConstructorCount;
                foreach (var parameter in ctorParameters) {
                    if (parameter.Name is { } paramName
                        && pocoProperties.IndexByName.TryGetValue(paramName, out var propIdx)
                        && pocoProperties[propIdx] is { } property
                        && property.DataType == parameter.ParameterType
                        && IsSupportedType(parameter.ParameterType)
                    ) {
                        if (propertyFlags[propIdx].viaConstructor) {
                            propsWithoutSetterWithoutConstructorArg--;
                        }
                    } else {
                        possibleMatch = false;
                        break;
                    }
                }
                if (!possibleMatch || propsWithoutSetterWithoutConstructorArg > 0) {
                    continue;
                }

                bestCtor = ctorInfo;
                bestCtorParameters = ctorParameters;
            }

            if (bestCtor == null && minimalViaConstructorCount == 0 && pocoProperties.PocoType.IsValueType) {
                bestCtorParameters = Array.Empty<ParameterInfo>(); // use implied parameterless pseudo-constructor for value types
            }

            if (bestCtorParameters == null) {
                throw new(
                    $"Cannot unpack DbDataReader ({cols.Length} columns) into type {FriendlyName()} ({pocoProperties.Count} properties)\n"
                    + "No applicable constructor found. The type must have at least one public contructor for which all parameters have an identically named and typed public property.\n"
                    + "Since they have no setter, the constructor must at least have parameters covering " + pocoProperties.Select((prop, idx) => (prop, propertyFlags[idx].viaConstructor)).Where(o => o.viaConstructor).Select(o => o.prop.Name).JoinStrings(", ")
                );
            }
            var ctorArguments = new ParameterExpression[bestCtorParameters.Length];
            for (var ctorIdx = 0; ctorIdx < ctorArguments.Length; ctorIdx++) {
                var ctorArg = bestCtorParameters[ctorIdx];
                var propIdx = pocoProperties.IndexByName[ctorArg.Name.AssertNotNull()];
                propertyFlags[propIdx].viaConstructor = true;
                ctorArguments[ctorIdx] = variablesByPropIdx[propIdx];
            }

            var newExpression = bestCtor != null ? Expression.New(bestCtor, ctorArguments) : Expression.New(pocoProperties.PocoType);
            var memberInits = new List<MemberAssignment>();

            var unmappedProperties = new List<IPocoProperty>();
            foreach (var prop in pocoProperties) {
                if (!propertyFlags[prop.Index].coveredByReaderColumn && prop.CanWrite) {
                    unmappedProperties.Add(prop);
                }
                if (propertyFlags[prop.Index].coveredByReaderColumn && !propertyFlags[prop.Index].viaConstructor) {
                    memberInits.Add(Expression.Bind(prop.PropertyInfo, variablesByPropIdx[prop.Index]));
                }
            }
            statements.Add(Expression.MemberInit(newExpression, memberInits));
            var constructRowExpr = Expression.Block(pocoProperties.PocoType, variablesByPropIdx.WhereNotNull(), statements);

            return (constructRowExpr, unmappedProperties.ToArray());
        }
    }
}
