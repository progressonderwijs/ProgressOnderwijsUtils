// ReSharper disable PossiblyMistakenUseOfParamsMethod

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
// ReSharper disable RedundantUsingDirective
using System.Reflection.Emit;
using System.Threading;
// ReSharper restore RedundantUsingDirective
using ExpressionToCodeLib;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
    public static class ParameterizedSqlObjectMapper
    {
        [MustUseReturnValue]
        public static T ExecuteQuery<T>(ParameterizedSql sql, [NotNull] SqlCommandCreationContext commandCreationContext, Func<string> exceptionMessage, [NotNull] Func<SqlCommand, T> action)
        {
            using (var cmd = sql.CreateSqlCommand(commandCreationContext))
                try {
                    return action(cmd.Command);
                } catch (Exception e) {
                    throw new ParameterizedSqlExecutionException(exceptionMessage() + "\n\nQUERY:\n\n" + SqlCommandTracer.DebugFriendlyCommandText(cmd.Command, SqlCommandTracerOptions.IncludeArgumentValuesInLog), e);
                }
        }

        [MustUseReturnValue]
        public static T ReadScalar<T>(this ParameterizedSql sql, [NotNull] SqlCommandCreationContext commandCreationContext)
        {
            return ExecuteQuery(
                sql,
                commandCreationContext,
                () => "ReadScalar<" + typeof(T).ToCSharpFriendlyTypeName() + ">() failed.",
                command => DBNullRemover.Cast<T>(command.ExecuteScalar()));
        }

        public static int ExecuteNonQuery(this ParameterizedSql sql, [NotNull] SqlCommandCreationContext commandCreationContext)
        {
            return ExecuteQuery(
                sql,
                commandCreationContext,
                () => "Non-query failed",
                command => command.ExecuteNonQuery());
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
        /// <returns>An array of strongly-typed objects; never null</returns>
        [MustUseReturnValue]
        [NotNull]
        public static T[] ReadMetaObjects<T>(this ParameterizedSql q, [NotNull] SqlCommandCreationContext qCommandCreationContext) where T : IMetaObject, new()
        {
            return ExecuteQuery(
                q,
                qCommandCreationContext,
                () => "ReadMetaObjects<" + typeof(T).ToCSharpFriendlyTypeName() + ">() failed.",
                cmd => ReadMetaObjectsUnpacker<T>(cmd)
                );
        }

        /// <summary>
        /// Reads all records of the given query from the database, lazily unpacking them into the yielded rows using each item's publicly writable fields and properties.
        /// Type T must have a public parameterless constructor; both structs and classes are supported
        /// The type T must match the queries columns by name (the order is not relevant).  Matching columns to properties/fields is case insensitive.
        /// The number of fields+properties must be the same as the number of columns
        /// Watch out: while this enumerator is open, the underlying connection remains in use.
        /// </summary>
        /// <typeparam name="T">The type to unpack each record into</typeparam>
        /// <param name="q">The query to execute</param>
        /// <param name="qCommandCreationContext">The database connection</param>
        /// <param name="fieldMappingMode"></param>
        [MustUseReturnValue]
        [NotNull]
        public static IEnumerable<T> EnumerateMetaObjects<T>(this ParameterizedSql q, [NotNull] SqlCommandCreationContext qCommandCreationContext, FieldMappingMode fieldMappingMode = FieldMappingMode.RequireExactColumnMatches) where T : IMetaObject, new()
        {
            using (var reusableCmd = q.CreateSqlCommand(qCommandCreationContext)) {
                var cmd = reusableCmd.Command;
                SqlDataReader reader = null;
                DataReaderSpecialization<SqlDataReader>.TRowReader<T> unpacker;
                try {
                    reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
                    unpacker = DataReaderSpecialization<SqlDataReader>.ByMetaObjectImpl<T>.DataReaderToSingleRowUnpacker(reader, fieldMappingMode);
                } catch (Exception ex) {
                    reader?.Dispose();
                    throw new InvalidOperationException(QueryExecutionErrorMessage<T>(cmd), ex);
                }
                using (reader)
                    while (true) {
                        bool hasNext;
                        try {
                            hasNext = reader.Read();
                        } catch (Exception ex) {
                            throw new InvalidOperationException(QueryExecutionErrorMessage<T>(cmd), ex);
                        }
                        if (!hasNext) {
                            break;
                        }
                        T nextRow;
                        var lastColumnRead = 0;
                        try {
                            nextRow = unpacker(reader, out lastColumnRead);
                        } catch (Exception ex) {
                            var queryErr = QueryExecutionErrorMessage<T>(cmd);
                            var columnErr = ColumnUnpackingErrorMessage<T>(reader, lastColumnRead);
                            throw new InvalidOperationException(queryErr + "\n\n" + columnErr, ex);
                        }
                        yield return nextRow; //cannot yield in try-catch block
                    }
            }
        }

        [MustUseReturnValue]
        [NotNull]
        public static T[] ReadMetaObjectsUnpacker<T>([NotNull] SqlCommand cmd, FieldMappingMode fieldMappingMode = FieldMappingMode.RequireExactColumnMatches)
            where T : IMetaObject, new()
        {
            using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess)) {
                var unpacker = DataReaderSpecialization<SqlDataReader>.ByMetaObjectImpl<T>.DataReaderToRowArrayUnpacker(reader, fieldMappingMode);
                var lastColumnRead = 0;
                try {
                    return unpacker(reader, out lastColumnRead);
                } catch (Exception ex) when (!reader.IsClosed) {
                    throw new InvalidOperationException(ColumnUnpackingErrorMessage<T>(reader, lastColumnRead), ex);
                }
            }
        }

        [NotNull]
        static string ColumnUnpackingErrorMessage<T>([NotNull] SqlDataReader reader, int lastColumnRead) where T : IMetaObject, new()
        {
            var mps = MetaObject.GetMetaProperties<T>();
            var metaObjectTypeName = typeof(T).ToCSharpFriendlyTypeName();

            var sqlColName = reader.GetName(lastColumnRead);
            var mp = mps.GetByName(sqlColName);

            var sqlTypeName = reader.GetDataTypeName(lastColumnRead);
            var expectedCsTypeName = reader.GetFieldType(lastColumnRead).ToCSharpFriendlyTypeName();
            var actualCsTypeName = mp.DataType.ToCSharpFriendlyTypeName();

            return "Cannot unpack column " + sqlColName + " of type " + sqlTypeName + " (C#:" + expectedCsTypeName + ") into " + metaObjectTypeName + "." + mp.Name
                + " of type " + actualCsTypeName;
        }

        [NotNull]
        static string QueryExecutionErrorMessage<T>([NotNull] SqlCommand cmd, [CanBeNull] [CallerMemberName] string caller = null) where T : IMetaObject, new()
        {
            return caller + "<" + typeof(T).ToCSharpFriendlyTypeName() + ">() failed. \n\nQUERY:\n\n"
                + SqlCommandTracer.DebugFriendlyCommandText(cmd, SqlCommandTracerOptions.IncludeArgumentValuesInLog);
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
            return ExecuteQuery(
                q,
                qCommandCreationContext,
                () => "ReadPlain<" + typeof(T).ToCSharpFriendlyTypeName() + ">() failed.",
                ReadPlainUnpacker<T>);
        }

        [MustUseReturnValue]
        [NotNull]
        public static T[] ReadPlainUnpacker<T>([NotNull] SqlCommand cmd)
        {
            using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess)) {
                DataReaderSpecialization<SqlDataReader>.PlainImpl<T>.VerifyDataReaderShape(reader);
                return DataReaderSpecialization<SqlDataReader>.PlainImpl<T>.LoadRows(reader, out _);
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

        //static bool SupportsType(Type type) => GetterMethodsByType.ContainsKey(type);
        //static MethodInfo GetterForType(Type type) => GetterMethodsByType[type];

        //static readonly MethodInfo IsDBNullMethod = typeof(IDataRecord).GetMethod("IsDBNull", binding);
        //static readonly MethodInfo ReadMethod = typeof(IDataReader).GetMethod("Read", binding);
        [NotNull]
        static Dictionary<MethodInfo, MethodInfo> MakeMap([NotNull] params InterfaceMapping[] mappings)
        {
            return mappings.SelectMany(
                map => map.InterfaceMethods.Zip(map.TargetMethods, Tuple.Create))
                .ToDictionary(methodPair => methodPair.Item1, methodPair => methodPair.Item2);
        }

#if NET461
        static readonly ModuleBuilder moduleBuilder;
        static int counter;

        static ParameterizedSqlObjectMapper()
        {
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("AutoLoadFromDb_Helper"), AssemblyBuilderAccess.Run);
            moduleBuilder = assemblyBuilder.DefineDynamicModule("AutoLoadFromDb_HelperModule");
        }
#endif
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
            public delegate T[] TRowArrayReader<out T>(TReader reader, out int lastColumnRead);

            public delegate T TRowReader<out T>(TReader reader, out int lastColumnRead);

            struct TRowArrayReaderWithCols<T>
            {
                public string[] Cols;
                public TRowArrayReader<T> RowArrayReader;
            }

            struct TRowReaderWithCols<T>
            {
                public string[] Cols;
                public TRowReader<T> RowReader;
            }

            static readonly Dictionary<MethodInfo, MethodInfo> InterfaceMap = MakeMap(
                typeof(TReader).GetInterfaceMap(typeof(IDataRecord)),
                typeof(TReader).GetInterfaceMap(typeof(IDataReader)));

            static readonly MethodInfo IsDBNullMethod = InterfaceMap[typeof(IDataRecord).GetMethod("IsDBNull", binding)];
            static readonly MethodInfo ReadMethod = InterfaceMap[typeof(IDataReader).GetMethod("Read", binding)];
            static readonly bool isSqlDataReader = typeof(TReader) == typeof(SqlDataReader);

            static bool SupportsType([NotNull] Type type)
            {
                var underlyingType = type.GetNonNullableUnderlyingType();

                return getterMethodsByType.ContainsKey(underlyingType)
                    || isSqlDataReader && (underlyingType == typeof(TimeSpan) || underlyingType == typeof(DateTimeOffset))
                    ;
            }

            static bool SupportsMetaProperty([NotNull] IMetaProperty mp)
                => IsSimpletype(mp) || IsCreatableType(mp);

            static bool IsSimpletype([NotNull] IMetaProperty mp)
            {
                return SupportsType(mp.DataType);
            }

            static bool IsCreatableType([NotNull] IMetaProperty mp)
            {
                return CreateMethodOfTypeWithCreateMethod(mp.DataType, out var _);
            }

            static bool CreateMethodOfTypeWithCreateMethod([NotNull] Type type, [CanBeNull] out MethodInfo methodInfo)
            {
                methodInfo = null;
                var underlyingType = type.GetNonNullableUnderlyingType();
                var method = underlyingType.GetMethods(BindingFlags.Static | BindingFlags.Public).SingleOrDefault(m => m.GetCustomAttributes<MetaObjectPropertyLoaderAttribute>().Any());
                if (method == null) {
                    return false;
                }
                if (method.ReturnType != underlyingType) {
                    return false;
                }
                var parameters = method.GetParameters();
                if (parameters.Length != 1) {
                    return false;
                }
                if (SupportsType(parameters[0].ParameterType)) {
                    methodInfo = method;
                    return true;
                }
                return false;
            }

            static MethodInfo GetterForType([NotNull] Type underlyingType)
            {
                if (isSqlDataReader && underlyingType == typeof(TimeSpan)) {
                    return getTimeSpan_SqlDataReader;
                } else if (isSqlDataReader && underlyingType == typeof(DateTimeOffset)) {
                    return getDateTimeOffset_SqlDataReader;
                } else if (CreateMethodOfTypeWithCreateMethod(underlyingType, out var methodInfo)) {
                    return InterfaceMap[getterMethodsByType[methodInfo.GetParameters()[0].ParameterType]];
                } else {
                    return InterfaceMap[getterMethodsByType[underlyingType]];
                }
            }

            static Expression GetCastExpression(Expression callExpression, [NotNull] Type type)
            {
                var underlyingType = type.GetNonNullableUnderlyingType();
                var isTypeWithCreateMethod = CreateMethodOfTypeWithCreateMethod(underlyingType, out var methodInfo);
                var needsCast = underlyingType != type.GetNonNullableType();

                if (isTypeWithCreateMethod) {
                    return Expression.Call(methodInfo, callExpression);
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

            [NotNull]
            static TRowArrayReader<T> CreateLoadRowsMethod<T>([NotNull] Func<ParameterExpression, ParameterExpression, Expression> createRowObjectExpression)
            {
                //read this method bottom-to-top, because expression trees need to be constructed inside-out.
                var dataReaderParamExpr = Expression.Parameter(typeof(TReader), "dataReader");
                var lastColumnReadParamExpr = Expression.Parameter(typeof(int).MakeByRefType(), "lastColumnRead");
                var arrayBuilderOfRowsType = typeof(FastArrayBuilder<T>);
                var arrayBuilderVar = Expression.Variable(arrayBuilderOfRowsType, "rowList");
                var constructRowExpr = createRowObjectExpression(dataReaderParamExpr, lastColumnReadParamExpr);
                var addRowToBuilderExpr = Expression.Call(arrayBuilderVar, arrayBuilderOfRowsType.GetMethod("Add", new[] { typeof(T) }), constructRowExpr);
                var callReader_Read = Expression.Call(dataReaderParamExpr, ReadMethod);
                var loopExitLabel = Expression.Label("loopExit");
                var breakIfAtEndOfReaderExpr = Expression.IfThen(Expression.Not(callReader_Read), Expression.Break(loopExitLabel));
                var loopAddRowThenReadExpr = Expression.Loop(Expression.Block(addRowToBuilderExpr, breakIfAtEndOfReaderExpr), loopExitLabel);
                var finishArrayExpr = Expression.Call(arrayBuilderVar, arrayBuilderOfRowsType.GetMethod("ToArray"));
                var arrayBuilderCreateMethod = arrayBuilderOfRowsType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
                var initializeArrayBuilderExpr = Expression.Assign(arrayBuilderVar, Expression.Call(arrayBuilderCreateMethod));
                var rowVar = Expression.Variable(typeof(T), "row");
                var addRowVarToArrayBuilder = Expression.Call(arrayBuilderVar, arrayBuilderOfRowsType.GetMethod("Add", new[] { typeof(T) }), rowVar);
                var createArrayGivenRowInVarAndReaderAtValidRow =
                    Expression.Block(initializeArrayBuilderExpr, addRowVarToArrayBuilder, loopAddRowThenReadExpr, finishArrayExpr);
                var singleRowArrayExpr = Expression.NewArrayInit(typeof(T), rowVar);
                var createArrayGivenFirstRowInVar = Expression.Condition(callReader_Read, createArrayGivenRowInVarAndReaderAtValidRow, singleRowArrayExpr);
                var createArrayGivenReaderAtValidFirstRow = Expression.Block(Expression.Assign(rowVar, constructRowExpr), createArrayGivenFirstRowInVar);
                var callEmptyArrayMethod = Expression.Call(((Func<T[]>)Array.Empty<T>).Method);
                var returnEmptyArrayOrRunRestOfCode = Expression.Condition(callReader_Read, createArrayGivenReaderAtValidFirstRow, callEmptyArrayMethod);
                var loadRowsMethodBody = Expression.Block(typeof(T[]), new[] { rowVar, arrayBuilderVar, }, returnEmptyArrayOrRunRestOfCode);
                var loadRowsParamExprs = new[] { dataReaderParamExpr, lastColumnReadParamExpr };
                var loadRowsLambda = Expression.Lambda<TRowArrayReader<T>>(loadRowsMethodBody, "LoadRows", loadRowsParamExprs);

                return ConvertLambdaExpressionIntoDelegate<T, TRowArrayReader<T>>(loadRowsLambda);
            }

            [NotNull]
            static TRowReader<T> CreateLoadRowMethod<T>([NotNull] Func<ParameterExpression, ParameterExpression, Expression> createRowObjectExpression)
            {
                var dataReaderParamExpr = Expression.Parameter(typeof(TReader), "dataReader");
                var lastColumnReadParamExpr = Expression.Parameter(typeof(int).MakeByRefType(), "lastColumnRead");
                var constructRowExpr = createRowObjectExpression(dataReaderParamExpr, lastColumnReadParamExpr);
                var loadRowsParamExprs = new[] { dataReaderParamExpr, lastColumnReadParamExpr };
                var loadRowsLambda = Expression.Lambda<TRowReader<T>>(constructRowExpr, "LoadRows", loadRowsParamExprs);

                return ConvertLambdaExpressionIntoDelegate<T, TRowReader<T>>(loadRowsLambda);
            }

            [NotNull]
            static TDelegate ConvertLambdaExpressionIntoDelegate<T, TDelegate>([NotNull] Expression<TDelegate> loadRowsLambda)
            {
                try {
#if NET461
                    if (typeof(T).IsPublic) {
                        var typeBuilder = moduleBuilder.DefineType(
                            "AutoLoadFromDb_For_" + typeof(T).Name + "_" + typeof(TReader).Name + Interlocked.Increment(ref counter),
                            TypeAttributes.Public);
                        var methodBuilder = typeBuilder.DefineMethod("LoadRows", MethodAttributes.Public | MethodAttributes.Static);
                        loadRowsLambda.CompileToMethod(methodBuilder); //generates faster code
                        var newType = typeBuilder.CreateType();

                        return (TDelegate)(object)Delegate.CreateDelegate(typeof(TDelegate), newType.GetMethod("LoadRows"));
                    }
#endif

                    return loadRowsLambda.Compile(); //generates slower code but works on non-public types
                } catch (Exception e) {
                    throw new InvalidOperationException("Cannot dynamically compile unpacker method for type " + typeof(T) + ", where type.IsPublic: " + typeof(T).IsPublic, e);
                }
            }

            public static class ByMetaObjectImpl<T>
                where T : IMetaObject, new()
            {
                struct ColumnOrdering : IEquatable<ColumnOrdering>
                {
                    readonly ulong cachedHash;
                    public readonly string[] Cols;

                    public ColumnOrdering([NotNull] TReader reader)
                    {
                        var primeArr = ColHashPrimes;
                        Cols = PooledSmallBufferAllocator<string>.GetByLength(reader.FieldCount);
                        cachedHash = 0;
                        for (var i = 0; i < Cols.Length; i++) {
                            var name = reader.GetName(i);
                            Cols[i] = name;
                            cachedHash += primeArr[i] * CaseInsensitiveHash(name);
                        }
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

                    public override int GetHashCode() => (int)(uint)((cachedHash >> 32) + cachedHash);
                    public override bool Equals(object obj) => obj is ColumnOrdering && Equals((ColumnOrdering)obj);
                }

                static readonly ConcurrentDictionary<ColumnOrdering, TRowArrayReaderWithCols<T>> LoadRows;
                static readonly ConcurrentDictionary<ColumnOrdering, TRowReaderWithCols<T>> LoadRow;

                [NotNull]
                static Type type => typeof(T);

                static string FriendlyName => type.ToCSharpFriendlyTypeName();
                static readonly uint[] ColHashPrimes;
                static readonly MetaInfo<T> metadata = MetaInfo<T>.Instance;
                static readonly bool hasUnsupportedColumns;

                static ByMetaObjectImpl()
                {
                    var writablePropCount = 0;
                    foreach (var mp in metadata) { //perf:no LINQ
                        if (mp.CanWrite && SupportsMetaProperty(mp)) {
                            writablePropCount++;
                        }
                    }
                    ColHashPrimes = new uint[writablePropCount];

                    using (var pGen = Utils.Primes().GetEnumerator())
                        for (var i = 0; i < ColHashPrimes.Length && pGen.MoveNext(); i++) {
                            ColHashPrimes[i] = (uint)pGen.Current;
                        }
                    hasUnsupportedColumns = false;
                    foreach (var mp in metadata) { //perf:no LINQ
                        if (mp.CanWrite && !SupportsMetaProperty(mp)) {
                            hasUnsupportedColumns = true;
                            break;
                        }
                    }

                    LoadRows = new ConcurrentDictionary<ColumnOrdering, TRowArrayReaderWithCols<T>>();
                    LoadRow = new ConcurrentDictionary<ColumnOrdering, TRowReaderWithCols<T>>();
                }

                public static TRowArrayReader<T> DataReaderToRowArrayUnpacker([NotNull] TReader reader, FieldMappingMode fieldMappingMode)
                {
                    AssertColumnsCanBeMappedToObject(reader, fieldMappingMode);
                    var ordering = new ColumnOrdering(reader);

                    var cachedRowReaderWithCols = LoadRows.GetOrAdd(ordering, Delegate_ConstructTRowArrayReaderWithCols);
                    if (ordering.Cols != cachedRowReaderWithCols.Cols) {
                        //our ordering isn't in the cache, so it's string array can be returned to the pool
                        PooledSmallBufferAllocator<string>.ReturnToPool(ordering.Cols);
                    }
                    return cachedRowReaderWithCols.RowArrayReader;
                }

                public static TRowReader<T> DataReaderToSingleRowUnpacker([NotNull] TReader reader, FieldMappingMode fieldMappingMode)
                {
                    AssertColumnsCanBeMappedToObject(reader, fieldMappingMode);
                    var ordering = new ColumnOrdering(reader);

                    var cachedRowReaderWithCols = LoadRow.GetOrAdd(ordering, Delegate_ConstructTRowReaderWithCols);
                    if (ordering.Cols != cachedRowReaderWithCols.Cols) {
                        //our ordering isn't in the cache, so it's string array can be returned to the pool
                        PooledSmallBufferAllocator<string>.ReturnToPool(ordering.Cols);
                    }
                    return cachedRowReaderWithCols.RowReader;
                }

                static void AssertColumnsCanBeMappedToObject([NotNull] TReader reader, FieldMappingMode fieldMappingMode)
                {
                    if (reader.FieldCount > ColHashPrimes.Length
                        || (reader.FieldCount < ColHashPrimes.Length || hasUnsupportedColumns) && fieldMappingMode == FieldMappingMode.RequireExactColumnMatches) {
                        var columnNames = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToArray();
                        var publicWritableProperties = metadata.Where(SupportsMetaProperty).Select(mp => mp.Name).ToArray();
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

                /// <summary>
                /// Methodgroup-to-delegate conversion shows up on the profiles as COMDelegate::DelegateConstruct as around 4% of query exection.
                /// See also: http://blogs.msmvps.com/jonskeet/2011/08/22/optimization-and-generics-part-2-lambda-expressions-and-reference-types/
                /// </summary>
                static readonly Func<ColumnOrdering, TRowArrayReaderWithCols<T>> Delegate_ConstructTRowArrayReaderWithCols = ConstructTRowArrayReaderWithCols;

                static TRowArrayReaderWithCols<T> ConstructTRowArrayReaderWithCols(ColumnOrdering ordering)
                {
                    return new TRowArrayReaderWithCols<T> {
                        Cols = ordering.Cols,
                        RowArrayReader = CreateLoadRowsMethod<T>(
                            (readerParamExpr, lastColumnReadParameter) =>
                                Expression.MemberInit(
                                    Expression.New(type),
                                    createColumnBindings(ordering, readerParamExpr, lastColumnReadParameter))),
                    };
                }

                static readonly Func<ColumnOrdering, TRowReaderWithCols<T>> Delegate_ConstructTRowReaderWithCols = ConstructTRowReaderWithCols;

                static TRowReaderWithCols<T> ConstructTRowReaderWithCols(ColumnOrdering ordering)
                {
                    return new TRowReaderWithCols<T> {
                        Cols = ordering.Cols,
                        RowReader = CreateLoadRowMethod<T>(
                            (readerParamExpr, lastColumnReadParameter) =>
                                Expression.MemberInit(
                                    Expression.New(type),
                                    createColumnBindings(ordering, readerParamExpr, lastColumnReadParameter))),
                    };
                }

                static IEnumerable<MemberAssignment> createColumnBindings(
                    ColumnOrdering orderingP,
                    ParameterExpression readerParamExpr,
                    ParameterExpression lastColumnReadParameter)
                {
                    var isMetaPropertyIndexAlreadyUsed = new bool[metadata.Count];
                    var cols = orderingP.Cols;
                    for (var i = 0; i < cols.Length; i++) {
                        var colName = cols[i];
                        var metaPropertyIndexOrNull = metadata.IndexByName.GetOrDefaultR(colName, default(int?));
                        if (metaPropertyIndexOrNull == null) {
                            throw new ArgumentOutOfRangeException("Cannot resolve IDataReader column " + colName + " in type " + FriendlyName);
                        }
                        var metaPropertyIndex = metaPropertyIndexOrNull.Value;
                        if (isMetaPropertyIndexAlreadyUsed[metaPropertyIndex]) {
                            throw new InvalidOperationException("IDataReader has two identically named columns " + colName + "!");
                        }
                        isMetaPropertyIndexAlreadyUsed[metaPropertyIndex] = true;
                        var member = metadata[metaPropertyIndex];
                        yield return Expression.Bind(
                            member.PropertyInfo,
                            Expression.Block(
                                Expression.Assign(lastColumnReadParameter, Expression.Constant(i)),
                                GetColValueExpr(readerParamExpr, i, member.DataType)
                                )
                            );
                    }
                }
            }

            [UsefulToKeep("This might be a nice thing to stick in an OSS library")]
            public static class ReadByConstructorImpl<T>
            {
                // ReSharper disable UnusedMember.Local
                public static T[] VerifyShapeAndLoadRows([NotNull] SqlDataReader reader)
                    // ReSharper restore UnusedMember.Local
                {
                    DataReaderSpecialization<SqlDataReader>.ReadByConstructorImpl<T>.VerifyDataReaderShape(reader);
                    return DataReaderSpecialization<SqlDataReader>.ReadByConstructorImpl<T>.LoadRows(reader, out _);
                }

                public static readonly TRowArrayReader<T> LoadRows;

                [NotNull]
                static Type type => typeof(T);

                static string FriendlyName => type.ToCSharpFriendlyTypeName();
                static readonly ConstructorInfo constructor;

                [NotNull]
                static ParameterInfo[] ConstructorParameters => constructor.GetParameters();

                static ReadByConstructorImpl()
                {
                    constructor = VerifyTypeValidityAndGetConstructor();
                    LoadRows = CreateLoadRowsMethod<T>(
                        (readerParamExpr, lastReadExpr) => Expression.New(
                            constructor,
                            ConstructorParameters.Select(
                                (ci, i) =>
                                    Expression.Block(Expression.Assign(lastReadExpr, Expression.Constant(i)), GetColValueExpr(readerParamExpr, i, ci.ParameterType)))));
                }

                [NotNull]
                static ConstructorInfo VerifyTypeValidityAndGetConstructor()
                {
                    if (!type.IsSealed || !type.IsPublic) {
                        throw new ArgumentException(FriendlyName + " : ILoadFromDbByConstructor must be a public, sealed type.");
                    }

                    var constructors = type.GetConstructors().Where(ci => ci.GetParameters().Any()).ToArray();
                    if (constructors.Length != 1) {
                        throw new ArgumentException(
                            FriendlyName + " : ILoadFromDbByConstructor must have a single public constructor (not counting a structs implicit constructor), not "
                                + constructors.Length);
                    }
                    var retval = constructors.Single();

                    if (!retval.GetParameters().All(pi => SupportsType(pi.ParameterType))) {
                        throw new ArgumentException(
                            FriendlyName + " : ILoadFromDbByConstructor's constructor must have only simple types: cannot support "
                                + retval.GetParameters()
                                    .Where(pi => !SupportsType(pi.ParameterType))
                                    .Select(pi => pi.ParameterType.ToCSharpFriendlyTypeName() + " " + pi.Name)
                                    .JoinStrings(", "));
                    }
                    return retval;
                }

                public static void VerifyDataReaderShape([NotNull] TReader reader)
                {
                    if (reader.FieldCount != ConstructorParameters.Length) {
                        throw new InvalidOperationException(
                            "Cannot unpack DbDataReader into type " + FriendlyName + "; column count = " + reader.FieldCount + "; constructr parameter count = "
                                + ConstructorParameters.Length);
                    }
                    if (!Enumerable.Range(0, reader.FieldCount).Select(reader.GetName)
                        .SequenceEqual(ConstructorParameters.Select(ci => ci.Name), StringComparer.OrdinalIgnoreCase)
                        ||
                        !Enumerable.Range(0, reader.FieldCount).Select(reader.GetFieldType)
                            .SequenceEqual(ConstructorParameters.Select(ci => ci.ParameterType.GetNonNullableUnderlyingType()))) {
                        throw new InvalidOperationException(
                            "Cannot unpack DbDataReader:\n"
                                + Enumerable.Range(0, reader.FieldCount)
                                    .Select(i => reader.GetName(i) + " : " + reader.GetFieldType(i).ToCSharpFriendlyTypeName())
                                    .JoinStrings(", ") + "\n\t into type " + FriendlyName + ":\n"
                                + ConstructorParameters.Select(ci => ci.Name + " : " + ci.ParameterType.ToCSharpFriendlyTypeName()).JoinStrings(", "));
                    }
                }
            }

            public static class PlainImpl<T>
            {
                static string FriendlyName => type.ToCSharpFriendlyTypeName();
                public static readonly TRowArrayReader<T> LoadRows;

                [NotNull]
                static Type type => typeof(T);

                static PlainImpl()
                {
                    VerifyTypeValidity();
                    LoadRows = CreateLoadRowsMethod<T>((readerParamExpr, lastReadColumnExpr) => GetColValueExpr(readerParamExpr, 0, type));
                }

                static void VerifyTypeValidity()
                {
                    if (!SupportsType(type)) {
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
