using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

// ReSharper disable ConvertToUsingDeclaration
namespace ProgressOnderwijsUtils
{
    public enum FieldMappingMode
    {
        RequireExactColumnMatches,
        IgnoreExtraMetaProperties,
    }

    public static class ParameterizedSqlObjectMapper
    {
        [MustUseReturnValue]
        public static T ReadScalar<T>(this ParameterizedSql sql, [NotNull] SqlCommandCreationContext commandCreationContext)
        {
            using (var cmd = sql.CreateSqlCommand(commandCreationContext)) {
                try {
                    var value = cmd.Command.ExecuteScalar();
                    var converter = MetaObjectPropertyConverter.GetOrNull(typeof(T));
                    if (converter == null) {
                        return DBNullRemover.Cast<T>(value);
                    }
                    if (value is DBNull && typeof(T).IsNullableValueType()) {
                        return default;
                    }
                    return (T)converter.CompiledConverterFromDb.DynamicInvoke(value);
                } catch (Exception e) {
                    throw cmd.CreateExceptionWithTextAndArguments(CurrentMethodName<T>() + " failed.", e);
                }
            }
        }

        /// <summary>Executes an sql statement and returns the number of rows affected.  Returns 0 without server interaction for whitespace-only commands.</summary>
        public static int ExecuteNonQuery(this ParameterizedSql sql, [NotNull] SqlCommandCreationContext commandCreationContext)
        {
            using (var cmd = sql.CreateSqlCommand(commandCreationContext)) {
                try {
                    if (string.IsNullOrWhiteSpace(cmd.Command.CommandText)) {
                        return 0;
                    }
                    return cmd.Command.ExecuteNonQuery();
                } catch (Exception e) {
                    throw cmd.CreateExceptionWithTextAndArguments(nameof(ExecuteNonQuery) + " failed", e);
                }
            }
        }

        /// <summary>
        /// Reads all records of the given query from the database, unpacking into a C# array using each item's publicly writable fields and properties.
        /// Type T must have a public parameterless constructor; both structs and classes are supported
        /// The type T must match the queries columns by name (the order is not relevant).  Matching columns to properties/fields is case insensitive.
        /// The number of fields+properties must be the same as the number of columns
        /// </summary>
        /// <typeparam name="T">The type to unpack each record into</typeparam>
        /// <param name="q">The query to execute</param>
        /// <param name="qCommandCreationContext">The database connection</param>
        /// <param name="fieldMapping"> </param>
        /// <returns>An array of strongly-typed objects; never null</returns>
        [MustUseReturnValue]
        [NotNull]
        public static T[] ReadMetaObjects<[MeansImplicitUse(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
            T>(this ParameterizedSql q, [NotNull] SqlCommandCreationContext qCommandCreationContext, FieldMappingMode fieldMapping = FieldMappingMode.RequireExactColumnMatches)
            where T : IMetaObject, new()
        {
            using (var cmd = q.CreateSqlCommand(qCommandCreationContext)) {
                var lastColumnRead = -1;
                SqlDataReader reader = null;
                try {
                    reader = cmd.Command.ExecuteReader(CommandBehavior.SequentialAccess);
                    var unpacker = DataReaderSpecialization<SqlDataReader>.ByMetaObjectImpl<T>.DataReaderToSingleRowUnpacker(reader, fieldMapping);
                    var builder = new ArrayBuilder<T>();
                    while (reader.Read()) {
                        var nextRow = unpacker(reader, out lastColumnRead);
                        builder.Add(nextRow);
                    }
                    return builder.ToArray();
                } catch (Exception ex) {
                    throw cmd.CreateExceptionWithTextAndArguments(CurrentMethodName<T>() + " failed. " + UnpackingErrorMessage<T>(reader, lastColumnRead), ex);
                } finally {
                    reader?.Dispose();
                }
            }
        }

        [NotNull]
        static string CurrentMethodName<T>([CallerMemberName] string callingMethod = null)
            => callingMethod + "<" + typeof(T).ToCSharpFriendlyTypeName() + ">()";

        /// <summary>
        /// Executes a  DataTable op basis van het huidige commando met de huidige parameters
        /// </summary>
        [MustUseReturnValue]
        [NotNull]
        public static DataTable ReadDataTable(this ParameterizedSql sql, [NotNull] SqlCommandCreationContext conn, MissingSchemaAction missingSchemaAction)
        {
            using (var cmd = sql.CreateSqlCommand(conn))
            using (var adapter = new SqlDataAdapter(cmd.Command)) {
                try {
                    adapter.MissingSchemaAction = missingSchemaAction;
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                } catch (Exception e) {
                    throw cmd.CreateExceptionWithTextAndArguments(nameof(ReadDataTable) + "() failed", e);
                }
            }
        }

        /// <summary>
        /// Reads all records of the given query from the database, lazily unpacking them into the yielded rows using each item's publicly writable fields and properties.
        /// Type T must have a public parameterless constructor; both structs and classes are supported
        /// The type T must match the queries columns by name (the order is not relevant).  Matching columns to properties/fields is case insensitive.
        /// The number of fields+properties must be the same as the number of columns
        /// Watch out: while this enumerator is open, the underlying connection remains in use.
        /// </summary>
        /// <typeparam name="T">The type to unpack each record into</typeparam>
        [MustUseReturnValue]
        [NotNull]
        public static IEnumerable<T> EnumerateMetaObjects<[MeansImplicitUse(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
            T>(this ParameterizedSql q, [NotNull] SqlCommandCreationContext qCommandCreationContext, FieldMappingMode fieldMappingMode = FieldMappingMode.RequireExactColumnMatches)
            where T : IMetaObject, new()
        {
            var cmd = q.CreateSqlCommand(qCommandCreationContext);
            SqlDataReader reader = null;
            var lastColumnRead = -1;
            ParameterizedSqlExecutionException CreateHelpfulException(Exception ex)
                => cmd.CreateExceptionWithTextAndArguments(CurrentMethodName<T>() + " failed. " + UnpackingErrorMessage<T>(reader, lastColumnRead), ex);

            try {
                DataReaderSpecialization<SqlDataReader>.TRowReader<T> unpacker;
                try {
                    reader = cmd.Command.ExecuteReader(CommandBehavior.SequentialAccess);
                    unpacker = DataReaderSpecialization<SqlDataReader>.ByMetaObjectImpl<T>.DataReaderToSingleRowUnpacker(reader, fieldMappingMode);
                } catch (Exception e) {
                    throw CreateHelpfulException(e);
                }

                while (true) {
                    bool isDone;
                    try {
                        isDone = !reader.Read();
                    } catch (Exception e) {
                        throw CreateHelpfulException(e);
                    }

                    if (isDone) {
                        break;
                    }
                    T nextRow;
                    try {
                        nextRow = unpacker(reader, out lastColumnRead);
                    } catch (Exception e) {
                        throw CreateHelpfulException(e);
                    }

                    yield return nextRow; //cannot yield in try-catch block
                }
            } finally {
                reader?.Dispose();
                cmd.Dispose();
            }
        }

        [NotNull]
        static string UnpackingErrorMessage<T>([CanBeNull] SqlDataReader reader, int lastColumnRead)
            where T : IMetaObject, new()
        {
            if (reader?.IsClosed != false || lastColumnRead < 0) {
                return "";
            }
            var mps = MetaObject.GetMetaProperties<T>();
            var metaObjectTypeName = typeof(T).ToCSharpFriendlyTypeName();

            var sqlColName = reader.GetName(lastColumnRead);
            var mp = mps.GetByName(sqlColName);

            var sqlTypeName = reader.GetDataTypeName(lastColumnRead);
            var nonNullableFieldType = reader.GetFieldType(lastColumnRead);

            bool? isValueNull = null;
            try {
                isValueNull = reader.IsDBNull(lastColumnRead);
            } catch {
                // ignore crash in error handling.
            }
            var fieldType = isValueNull ?? true ? nonNullableFieldType?.MakeNullableType() ?? nonNullableFieldType : nonNullableFieldType;

            var expectedCsTypeName = fieldType.ToCSharpFriendlyTypeName();
            var actualCsTypeName = mp.DataType.ToCSharpFriendlyTypeName();
            var nullValueWarning = isValueNull ?? false ? "NULL value from " : "";

            return $"Cannot unpack {nullValueWarning}column {sqlColName} of type {sqlTypeName} (C#:{expectedCsTypeName}) into {metaObjectTypeName}.{mp.Name} of type {actualCsTypeName}";
        }

        /// <summary>
        /// Reads all records of the given query from the database, unpacking into a C# array using basic types (i.e. scalars)
        /// Type T must be int, long, string, decimal, double, bool, DateTime or byte[].
        /// </summary>
        /// <typeparam name="T">The type to unpack each record into</typeparam>
        /// <param name="q">The query to execute</param>
        /// <param name="qCommandCreationContext">The command creation context</param>
        /// <returns>An array of strongly-typed objects; never null</returns>
        [MustUseReturnValue]
        [NotNull]
        public static T[] ReadPlain<T>(this ParameterizedSql q, [NotNull] SqlCommandCreationContext qCommandCreationContext)
        {
            using (var cmd = q.CreateSqlCommand(qCommandCreationContext)) {
                try {
                    return ReadPlainUnpacker<T>(cmd.Command);
                } catch (Exception e) {
                    throw cmd.CreateExceptionWithTextAndArguments(CurrentMethodName<T>() + " failed.", e);
                }
            }
        }

        [MustUseReturnValue]
        [NotNull]
        public static T[] ReadPlainUnpacker<T>([NotNull] SqlCommand cmd)
        {
            using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess)) {
                DataReaderSpecialization<SqlDataReader>.PlainImpl<T>.VerifyDataReaderShape(reader);
                var unpacker = DataReaderSpecialization<SqlDataReader>.PlainImpl<T>.ReadValue;
                var builder = new ArrayBuilder<T>();
                while (reader.Read()) {
                    var nextRow = unpacker(reader);
                    builder.Add(nextRow);
                }
                return builder.ToArray();
            }
        }

        const BindingFlags binding = BindingFlags.Public | BindingFlags.Instance;

        static readonly Dictionary<Type, MethodInfo> getterMethodsByType =
            new Dictionary<Type, MethodInfo> {
                { typeof(bool), typeof(IDataRecord).GetMethod("GetBoolean", binding) },
                { typeof(byte), typeof(IDataRecord).GetMethod("GetByte", binding) },
                { typeof(byte[]), typeof(DbLoadingHelperImpl).GetMethod("GetBytes", BindingFlags.Public | BindingFlags.Static) },
                { typeof(char), typeof(IDataRecord).GetMethod("GetChar", binding) },
                { typeof(char[]), typeof(DbLoadingHelperImpl).GetMethod("GetChars", BindingFlags.Public | BindingFlags.Static) },
                { typeof(DateTime), typeof(IDataRecord).GetMethod("GetDateTime", binding) },
                { typeof(decimal), typeof(IDataRecord).GetMethod("GetDecimal", binding) },
                { typeof(double), typeof(IDataRecord).GetMethod("GetDouble", binding) },
                { typeof(float), typeof(IDataRecord).GetMethod("GetFloat", binding) },
                { typeof(Guid), typeof(IDataRecord).GetMethod("GetGuid", binding) },
                { typeof(short), typeof(IDataRecord).GetMethod("GetInt16", binding) },
                { typeof(int), typeof(IDataRecord).GetMethod("GetInt32", binding) },
                { typeof(long), typeof(IDataRecord).GetMethod("GetInt64", binding) },
                { typeof(string), typeof(IDataRecord).GetMethod("GetString", binding) },
            };

        [NotNull]
        static Dictionary<MethodInfo, MethodInfo> MakeMap([NotNull] params InterfaceMapping[] mappings)
            => mappings
                .SelectMany(map => map.InterfaceMethods.Zip(map.TargetMethods, (interfaceMethod, targetMethod) => (interfaceMethod, targetMethod)))
                .ToDictionary(methodPair => methodPair.interfaceMethod, methodPair => methodPair.targetMethod);

        static readonly MethodInfo getTimeSpan_SqlDataReader = typeof(SqlDataReader).GetMethod("GetTimeSpan", binding);
        static readonly MethodInfo getDateTimeOffset_SqlDataReader = typeof(SqlDataReader).GetMethod("GetDateTimeOffset", binding);
        const int AsciiUpperToLowerDiff = 'a' - 'A';

        static ulong CaseInsensitiveHash([NotNull] string s)
        {
            //Much faster than StringComparer.OrdinalIgnoreCase.GetHashCode(...)
            //Based on java's String.hashCode(): http://docs.oracle.com/javase/6/docs/api/java/lang/String.html#hashCode%28%29
            //In particularly, need not produce great hash quality since column names are non-hostile (and it's good enough for the largest language on the planet...)

            var hash = 0ul;
            foreach (var c in s) {
                var code = c > 'Z' || c < 'A' ? c : c + (uint)AsciiUpperToLowerDiff;
                hash = (hash << 5) - hash + code;
            }
            return hash;
        }

        static bool CaseInsensitiveEquality([NotNull] string a, [NotNull] string b)
        {
            //Much faster than StringComparer.OrdinalIgnoreCase.Equals(a,b)
            //optimized for strings that are equal, because that's the expected use case.
            if (a.Length != b.Length) {
                return false;
            }
            for (var i = 0; i < a.Length; i++) {
                int aChar = a[i];
                int bChar = b[i];
                if (aChar != bChar) {
                    //although comparison is case insensitve, exact equality implies case insensitive equality
                    //exact equality is commonly true and faster, so we test that first.
                    var aCode = aChar > 'Z' || aChar < 'A' ? aChar : aChar + AsciiUpperToLowerDiff;
                    var bCode = bChar > 'Z' || bChar < 'A' ? bChar : bChar + AsciiUpperToLowerDiff;

                    if (aCode != bCode) {
                        return false;
                    }
                }
            }
            return true;
        }

        static class DataReaderSpecialization<TReader>
            where TReader : IDataReader
        {
            public delegate T TRowReader<out T>(TReader reader, out int lastColumnRead);

            static readonly Dictionary<MethodInfo, MethodInfo> InterfaceMap = MakeMap(
                typeof(TReader).GetInterfaceMap(typeof(IDataRecord)),
                typeof(TReader).GetInterfaceMap(typeof(IDataReader)));

            // ReSharper disable AssignNullToNotNullAttribute
            static readonly MethodInfo IsDBNullMethod = InterfaceMap[typeof(IDataRecord).GetMethod("IsDBNull", binding)];
            // ReSharper restore AssignNullToNotNullAttribute

            static readonly bool isSqlDataReader = typeof(TReader) == typeof(SqlDataReader);

            static bool IsSupportedBasicType([NotNull] Type type)
            {
                var underlyingType = type.GetNonNullableUnderlyingType();

                return getterMethodsByType.ContainsKey(underlyingType)
                    || isSqlDataReader && (underlyingType == typeof(TimeSpan) || underlyingType == typeof(DateTimeOffset))
                    ;
            }

            static bool IsSupportedType([NotNull] Type type)
                => IsSupportedBasicType(type) || MetaObjectPropertyConverter.GetOrNull(type) is MetaObjectPropertyConverter converter && IsSupportedBasicType(converter.DbType);

            static MethodInfo GetterForType([NotNull] Type underlyingType)
            {
                if (isSqlDataReader && underlyingType == typeof(TimeSpan)) {
                    return getTimeSpan_SqlDataReader;
                } else if (isSqlDataReader && underlyingType == typeof(DateTimeOffset)) {
                    return getDateTimeOffset_SqlDataReader;
                } else if (MetaObjectPropertyConverter.GetOrNull(underlyingType) is MetaObjectPropertyConverter converter) {
                    return InterfaceMap[getterMethodsByType[converter.DbType]];
                } else {
                    return InterfaceMap[getterMethodsByType[underlyingType]];
                }
            }

            static Expression GetCastExpression(Expression callExpression, [NotNull] Type type)
            {
                var underlyingType = type.GetNonNullableUnderlyingType();
                var converter = MetaObjectPropertyConverter.GetOrNull(underlyingType);

                var needsCast = underlyingType != type.GetNonNullableType();

                if (converter != null) {
                    return Expression.Invoke(Expression.Constant(converter.CompiledConverterFromDb), callExpression);
                } else if (needsCast) {
                    return Expression.Convert(callExpression, type.GetNonNullableType());
                } else {
                    return callExpression;
                }
            }

            public static Expression GetColValueExpr(ParameterExpression readerParamExpr, int i, [NotNull] Type type)
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

            public static class ByMetaObjectImpl<T>
                where T : IMetaObject, new()
            {
                readonly struct ColumnOrdering : IEquatable<ColumnOrdering>
                {
                    readonly ulong cachedHash;
                    public readonly string[] Cols;

                    ColumnOrdering(ulong _cachedHash, string[] _cols)
                        => (cachedHash, Cols) = (_cachedHash, _cols);

                    public static ColumnOrdering FromReader([NotNull] TReader reader)
                    {
                        var primeArr = ColHashPrimes;
                        var cols = PooledSmallBufferAllocator<string>.GetByLength(reader.FieldCount);
                        var cachedHash = 0uL;
                        for (var i = 0; i < cols.Length; i++) {
                            var name = reader.GetName(i);
                            cols[i] = name;
                            cachedHash += primeArr[i] * CaseInsensitiveHash(name);
                        }
                        return new ColumnOrdering(cachedHash, cols);
                    }

                    public bool Equals(ColumnOrdering other)
                    {
                        var oCols = other.Cols;
                        if (cachedHash != other.cachedHash || Cols.Length != oCols.Length) {
                            return false;
                        }
                        for (var i = 0; i < Cols.Length; i++) {
                            if (!CaseInsensitiveEquality(Cols[i], oCols[i])) {
                                return false;
                            }
                        }
                        return true;
                    }

                    public override int GetHashCode()
                        => (int)(uint)((cachedHash >> 32) + cachedHash);

                    public override bool Equals(object obj)
                        => obj is ColumnOrdering columnOrdering && Equals(columnOrdering);
                }

                static readonly object constructionSync = new object();
                static readonly ConcurrentDictionary<ColumnOrdering, TRowReader<T>> loadRow_by_ordering;

                [NotNull]
                static Type type
                    => typeof(T);

                static string FriendlyName
                    => type.ToCSharpFriendlyTypeName();

                static readonly uint[] ColHashPrimes;
                static readonly MetaInfo<T> metadata = MetaInfo<T>.Instance;
                static readonly bool hasUnsupportedColumns;

                static ByMetaObjectImpl()
                {
                    var writablePropCount = 0;
                    foreach (var mp in metadata) { //perf:no LINQ
                        if (mp.CanWrite && IsSupportedType(mp.DataType)) {
                            writablePropCount++;
                        }
                    }
                    ColHashPrimes = new uint[writablePropCount];

                    using (var pGen = Utils.Primes().GetEnumerator()) {
                        for (var i = 0; i < ColHashPrimes.Length && pGen.MoveNext(); i++) {
                            ColHashPrimes[i] = (uint)pGen.Current;
                        }
                    }
                    hasUnsupportedColumns = false;
                    foreach (var mp in metadata) { //perf:no LINQ
                        if (mp.CanWrite && !IsSupportedType(mp.DataType)) {
                            hasUnsupportedColumns = true;
                            break;
                        }
                    }

                    loadRow_by_ordering = new ConcurrentDictionary<ColumnOrdering, TRowReader<T>>();
                }

                public static TRowReader<T> DataReaderToSingleRowUnpacker([NotNull] TReader reader, FieldMappingMode fieldMappingMode)
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

                static void AssertColumnsCanBeMappedToObject([NotNull] TReader reader, FieldMappingMode fieldMappingMode)
                {
                    if (reader.FieldCount > ColHashPrimes.Length
                        || (reader.FieldCount < ColHashPrimes.Length || hasUnsupportedColumns) && fieldMappingMode == FieldMappingMode.RequireExactColumnMatches) {
                        var columnNames = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToArray();
                        var publicWritableProperties = metadata.Where(mp => IsSupportedType(mp.DataType)).Select(mp => mp.Name).ToArray();
                        var columnsThatCannotBeMapped = columnNames.Except(publicWritableProperties, StringComparer.OrdinalIgnoreCase);
                        var propertiesWithoutColumns = publicWritableProperties.Except(columnNames, StringComparer.OrdinalIgnoreCase);
                        throw new InvalidOperationException(
                            $"Cannot unpack DbDataReader ({reader.FieldCount} columns) into type {FriendlyName} ({ColHashPrimes.Length} properties with public setters)\n" +
                            $"Query columns without corresponding property: {columnsThatCannotBeMapped.JoinStrings(", ")}\n" +
                            $".net properties without corresponding query column: {propertiesWithoutColumns.JoinStrings(", ")}\n" +
                            $"All columns: {columnNames.Select(n => "\n\t- " + n).JoinStrings()}\n" +
                            $"{FriendlyName} properties: {publicWritableProperties.Select(n => "\n\t- " + n).JoinStrings()}\n");
                    }
                    if (ColHashPrimes.Length == 0) {
                        throw new InvalidOperationException("MetaObject " + FriendlyName + " has no writable columns with a supported type!");
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
                    var isMetaPropertyIndexAlreadyUsed = new bool[metadata.Count];
                    for (var i = 0; i < cols.Length; i++) {
                        var colName = cols[i];
                        if (!metadata.IndexByName.TryGetValue(colName, out var metaPropertyIndex)) {
                            throw new ArgumentOutOfRangeException("Cannot resolve IDataReader column " + colName + " in type " + FriendlyName);
                        }
                        if (isMetaPropertyIndexAlreadyUsed[metaPropertyIndex]) {
                            throw new InvalidOperationException("IDataReader has two identically named columns " + colName + "!");
                        }
                        isMetaPropertyIndexAlreadyUsed[metaPropertyIndex] = true;
                        var member = metadata[metaPropertyIndex];
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

                [NotNull]
                static Type type
                    => typeof(T);

                static PlainImpl()
                {
                    VerifyTypeValidity();
                    var dataReaderParamExpr = Expression.Parameter(typeof(TReader), "dataReader");
                    var loadRowsLambda = Expression.Lambda<Func<TReader, T>>(GetColValueExpr(dataReaderParamExpr, 0, type), dataReaderParamExpr);
                    ReadValue = loadRowsLambda.Compile();
                }

                static void VerifyTypeValidity()
                {
                    if (!IsSupportedBasicType(type)) {
                        throw new ArgumentException(
                            FriendlyName + " cannot be auto loaded as plain data since it isn't a basic type ("
                            + getterMethodsByType.Keys.Select(ObjectToCode.ToCSharpFriendlyTypeName).JoinStrings(", ") + ")!");
                    }
                }

                public static void VerifyDataReaderShape([NotNull] TReader reader)
                {
                    if (reader.FieldCount != 1) {
                        throw new InvalidOperationException("Cannot unpack DbDataReader into type " + FriendlyName + "; column count = " + reader.FieldCount + " != 1");
                    }
                    if (!Enumerable.Range(0, reader.FieldCount)
                        .Select(reader.GetFieldType)
                        .SequenceEqual(new[] { typeof(T).GetNonNullableUnderlyingType() })) {
                        throw new InvalidOperationException(
                            "Cannot unpack DbDataReader into type " + FriendlyName + ":\n"
                            + Enumerable.Range(0, reader.FieldCount)
                                .Select(i => reader.GetName(i) + " : " + reader.GetFieldType(i).ToCSharpFriendlyTypeName())
                                .JoinStrings(", "));
                    }
                }
            }
        }

        static class BackingFieldDetector
        {
            const string BackingFieldPrefix = "<";
            const string BackingFieldSuffix = ">k__BackingField";
            const BindingFlags privateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
            const BindingFlags anyInstance = privateInstance | BindingFlags.Public;

            [NotNull]
            static string BackingFieldFromAutoPropName([NotNull] string propertyName)
                => BackingFieldPrefix + propertyName + BackingFieldSuffix;

            [CanBeNull]
            static string AutoPropNameFromBackingField([NotNull] string fieldName)
                => fieldName.StartsWith(BackingFieldPrefix, StringComparison.Ordinal) && fieldName.EndsWith(BackingFieldSuffix, StringComparison.Ordinal)
                    ? fieldName.Substring(BackingFieldPrefix.Length, fieldName.Length - BackingFieldPrefix.Length - BackingFieldSuffix.Length)
                    : null;

            static bool IsCompilerGenerated([CanBeNull] MemberInfo member)
                => member != null && member.IsDefined(typeof(CompilerGeneratedAttribute), true);

            static bool IsAutoProp(PropertyInfo autoProperty)
                => IsCompilerGenerated(autoProperty.GetGetMethod(true));

            [CanBeNull]
            public static FieldInfo BackingFieldOfPropertyOrNull([NotNull] PropertyInfo propertyInfo)
                => IsAutoProp(propertyInfo)
                    && propertyInfo.DeclaringType.GetField(BackingFieldFromAutoPropName(propertyInfo.Name), privateInstance) is FieldInfo backingField
                    && IsCompilerGenerated(backingField)
                        ? backingField
                        : null;

            [UsefulToKeep("for symmetry with BackingFieldOfPropertyOrNull")]
            public static PropertyInfo AutoPropertyOfFieldOrNull(FieldInfo fieldInfo)
                => IsCompilerGenerated(fieldInfo)
                    && AutoPropNameFromBackingField(fieldInfo.Name) is string autoPropertyName
                    && fieldInfo.DeclaringType.GetProperty(autoPropertyName, anyInstance) is PropertyInfo autoProperty
                    && IsAutoProp(autoProperty)
                        ? autoProperty
                        : null;
        }
    }

    public static class DbLoadingHelperImpl
    {
        [NotNull]
        [UsedImplicitly]
        public static byte[] GetBytes([NotNull] this IDataRecord row, int colIndex)
        {
            var byteCount = row.GetBytes(colIndex, 0L, null, 0, 0);
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

        [NotNull]
        [UsedImplicitly]
        public static char[] GetChars([NotNull] this IDataRecord row, int colIndex)
        {
            var charCount = row.GetChars(colIndex, 0L, null, 0, 0);
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
    }
}
