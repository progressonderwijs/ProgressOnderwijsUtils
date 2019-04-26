using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using ExpressionToCodeLib;
using FastExpressionCompiler;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ProgressOnderwijsUtils
{
    public interface IOptionalObjectListForDebugging
    {
        IReadOnlyList<object> ContentsForDebuggingOrNull();
    }

    public interface IOptionalObjectProjectionForDebugging
    {
        object ProjectionForDebuggingOrNull();
    }

    public sealed class MetaObjectDataReader<T> : DbDataReaderBase, IOptionalObjectListForDebugging
        where T : IMetaObject
    {
        readonly CancellationToken _cancellationToken;
        readonly IEnumerator<T> metaObjects;
        readonly IReadOnlyList<T> objectsOrNull_ForDebugging;
        T current;
        int rowsProcessed;

        public int RowsProcessed
            => rowsProcessed;

        public MetaObjectDataReader([NotNull] IEnumerable<T> objects, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            metaObjects = objects.GetEnumerator();
            objectsOrNull_ForDebugging = objects as IReadOnlyList<T>;
        }

        public override void Close()
        {
            metaObjects.Dispose();
            isClosed = true;
            current = default(T);
        }

        protected override bool ReadImpl()
        {
            _cancellationToken.ThrowIfCancellationRequested();
            var hasnext = metaObjects.MoveNext();
            if (hasnext) {
                current = metaObjects.Current;
                rowsProcessed++;
            }
            return hasnext;
        }

        public override DataTable GetSchemaTable()
            => schemaTable;

        public override int FieldCount
            => columnInfos.Length;

        [NotNull]
        public override Type GetFieldType(int ordinal)
            => columnInfos[ordinal].ColumnType;

        public override string GetName(int ordinal)
            => columnInfos[ordinal].Name;

        public override int GetOrdinal(string name)
            => columnIndexByName[name];

        public override int GetInt32(int ordinal)
            => ((Func<T, int>)columnInfos[ordinal].TypedNonNullableGetter)(current);

        [NotNull]
        public override object GetValue(int ordinal)
            => columnInfos[ordinal].GetUntypedColumnValue(current) ?? DBNull.Value;

        public override bool IsDBNull(int ordinal)
        {
            var func = columnInfos[ordinal].WhenNullable_IsColumnDBNull;
            return func != null && func(current);
        }

        public override TColumn GetFieldValue<TColumn>(int ordinal)
        {
            if (!(columnInfos[ordinal].TypedNonNullableGetter is Func<T, TColumn> getter)) {
                throw new InvalidOperationException($"Tried to access field {columnInfos[ordinal].Name} of type {columnInfos[ordinal].ColumnType.ToCSharpFriendlyTypeName()} as type {typeof(TColumn).ToCSharpFriendlyTypeName()}.");
            }
            return getter(current);
        }

        static ValueConverter CreateValueConverter<TModel, TProvider, [UsedImplicitly] TConverterSource>()
            where TConverterSource : struct, IConverterSource<TModel, TProvider>
            where TModel : struct, IMetaObjectPropertyConvertible<TModel, TProvider, TConverterSource>
            => new TConverterSource().GetValueConverter();

        static readonly MethodInfo CreateValueConverter_OpenGenericMethod = ((Func<ValueConverter>)CreateValueConverter<UnusedTypeTemplate1, int, UnusedTypeTemplate2>).Method.GetGenericMethodDefinition();

        struct UnusedTypeTemplate1 : IMetaObjectPropertyConvertible<UnusedTypeTemplate1, int, UnusedTypeTemplate2> { }

        struct UnusedTypeTemplate2 : IConverterSource<UnusedTypeTemplate1, int>
        {
            public ValueConverter<UnusedTypeTemplate1, int> GetValueConverter()
                => throw new NotImplementedException();
        }

        struct ColumnInfo
        {
            public readonly string Name;

            //ColumnType is non nullable with enum types replaced by their underlying type
            [NotNull]
            public readonly Type ColumnType;

            public readonly Func<T, object> GetUntypedColumnValue;

            //WhenNullable_IsColumnDBNull is itself null if column non-nullable
            public readonly Func<T, bool> WhenNullable_IsColumnDBNull;

            //TypedNonNullableGetter is of type Func<T, _> such that typeof(_) == ColumnType - therefore cannot return nulls!
            public readonly Delegate TypedNonNullableGetter;

            public ColumnInfo([NotNull] IReadonlyMetaProperty<T> mp)
            {
                var propertyType = mp.DataType;
                var metaObjectParameter = Expression.Parameter(typeof(T));
                var propertyValue = mp.PropertyAccessExpression(metaObjectParameter);
                Name = mp.Name;
                var nonNullableUnderlyingType = propertyType.GetNonNullableUnderlyingType();
                var converterDefinition =
                    nonNullableUnderlyingType.GetInterfaces().SingleOrDefault(i =>
                        i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IMetaObjectPropertyConvertible<,,>));
                var isNonNullable = propertyType.IsValueType && propertyType.IfNullableGetNonNullableType() == null;

                if (converterDefinition != null) {
                    var modelType = converterDefinition.GenericTypeArguments[0];
                    var dbType = converterDefinition.GenericTypeArguments[1];
                    var converterType = converterDefinition.GenericTypeArguments[2];
                    var converter = ((Func<ValueConverter>)CreateValueConverter_OpenGenericMethod.MakeGenericMethod(modelType, dbType, converterType).CreateDelegate(typeof(Func<ValueConverter>)))();
                    var compiledConverter = converter.ConvertToProviderExpression.CompileFast();

                    // Fix hieronder
                    ColumnType = dbType;
                    var propertyValueAsNoNullable = Expression.Convert(propertyValue, modelType);
                    var columnValueAsNonNullable = Expression.Invoke(Expression.Constant(compiledConverter), propertyValueAsNoNullable);
                    var columnBoxedAsObject = Expression.Convert(columnValueAsNonNullable, typeof(object));
                    Expression columnBoxedAsColumnType;
                    if (isNonNullable) {
                        columnBoxedAsColumnType = columnBoxedAsObject;
                        WhenNullable_IsColumnDBNull = null;
                    } else {
                        var propertyIsNotNull = IsExpressionNonNull(propertyValue);
                        columnBoxedAsColumnType = Expression.Condition(propertyIsNotNull, columnBoxedAsObject, Expression.Constant(DBNull.Value, typeof(object)));
                        WhenNullable_IsColumnDBNull = Expression.Lambda<Func<T, bool>>(Expression.Not(propertyIsNotNull), metaObjectParameter).Compile();
                    }

                    TypedNonNullableGetter = Expression.Lambda(typeof(Func<,>).MakeGenericType(typeof(T), dbType), columnValueAsNonNullable, metaObjectParameter).Compile();
                    GetUntypedColumnValue = Expression.Lambda<Func<T, object>>(columnBoxedAsColumnType, metaObjectParameter).Compile();
                } else {
                    ColumnType = nonNullableUnderlyingType;
                    var propertyValueAsNoNullable = Expression.Convert(propertyValue, ColumnType);
                    var columnValueAsNonNullable = Expression.Convert(propertyValue, propertyType.GetUnderlyingType());
                    var columnBoxedAsObject = Expression.Convert(columnValueAsNonNullable, typeof(object));
                    Expression columnBoxedAsColumnType;
                    if (isNonNullable) {
                        columnBoxedAsColumnType = columnBoxedAsObject;
                        WhenNullable_IsColumnDBNull = null;
                    } else {
                        var propertyIsNotNull = IsExpressionNonNull(propertyValue);
                        columnBoxedAsColumnType = Expression.Coalesce(columnValueAsNonNullable, Expression.Constant(DBNull.Value, typeof(object)));
                        WhenNullable_IsColumnDBNull = Expression.Lambda<Func<T, bool>>(Expression.Not(propertyIsNotNull), metaObjectParameter).Compile();
                    }
                    TypedNonNullableGetter = Expression.Lambda(typeof(Func<,>).MakeGenericType(typeof(T), ColumnType), propertyValueAsNoNullable, metaObjectParameter).Compile();
                    GetUntypedColumnValue = Expression.Lambda<Func<T, object>>(columnBoxedAsColumnType, metaObjectParameter).Compile();
                }
            }

            static Expression IsExpressionNonNull(Expression propertyValue)
                => propertyValue.Type.IsNullableValueType() ? Expression.Property(propertyValue, nameof(Nullable<int>.HasValue))
                    : propertyValue.Type.IsValueType ? Expression.Constant(false)
                    : (Expression)Expression.NotEqual(Expression.Default(typeof(object)), Expression.Convert(propertyValue, typeof(object)));
        }

        static readonly ColumnInfo[] columnInfos;
        static readonly Dictionary<string, int> columnIndexByName;
        static readonly DataTable schemaTable;

        static MetaObjectDataReader()
        {
            var metaProperties = MetaObject.GetMetaProperties<T>();
            var columnInfosBuilder = new List<ColumnInfo>(metaProperties.Count);
            columnIndexByName = new Dictionary<string, int>(metaProperties.Count, StringComparer.OrdinalIgnoreCase);
            schemaTable = CreateEmptySchemaTable();
            var i = 0;
            foreach (var metaProperty in metaProperties) {
                if (metaProperty.CanRead) {
                    var columnInfo = new ColumnInfo(metaProperty);
                    var isKey = metaProperty.IsKey;
                    var allowDbNull = columnInfo.WhenNullable_IsColumnDBNull != null;
                    var isUnique = isKey && !metaProperties.Any(other => other != metaProperty && other.IsKey);
                    columnIndexByName.Add(columnInfo.Name, i);
                    schemaTable.Rows.Add(columnInfo.Name, i, -1, null, null, columnInfo.ColumnType, null, false, allowDbNull, true, false, isUnique, isKey, false, null, null, null, "val");
                    columnInfosBuilder.Add(columnInfo);
                    i++;
                }
            }
            columnInfos = columnInfosBuilder.ToArray();
        }

        [NotNull]
        static DataTable CreateEmptySchemaTable()
        {
            var dt = new DataTable();
            dt.Columns.AddRange(new[] {
                new DataColumn("ColumnName", typeof(string)),
                new DataColumn("ColumnOrdinal", typeof(int)),
                new DataColumn("ColumnSize", typeof(int)),
                new DataColumn("NumericPrecision", typeof(short)),
                new DataColumn("NumericScale", typeof(short)),
                new DataColumn("DataType", typeof(Type)),
                new DataColumn("ProviderType", typeof(int)),
                new DataColumn("IsLong", typeof(bool)),
                new DataColumn("AllowDBNull", typeof(bool)),
                new DataColumn("IsReadOnly", typeof(bool)),
                new DataColumn("IsRowVersion", typeof(bool)),
                new DataColumn("IsUnique", typeof(bool)),
                new DataColumn("IsKey", typeof(bool)),
                new DataColumn("IsAutoIncrement", typeof(bool)),
                new DataColumn("BaseCatalogName", typeof(string)),
                new DataColumn("BaseSchemaName", typeof(string)),
                new DataColumn("BaseTableName", typeof(string)),
                new DataColumn("BaseColumnName", typeof(string)),
            });
            return dt;
        }

        [CanBeNull]
        IReadOnlyList<object> IOptionalObjectListForDebugging.ContentsForDebuggingOrNull()
            => objectsOrNull_ForDebugging?.SelectIndexable(o => (o as IOptionalObjectProjectionForDebugging)?.ProjectionForDebuggingOrNull() ?? o);
    }
}
