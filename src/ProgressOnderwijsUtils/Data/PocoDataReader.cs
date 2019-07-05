﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using ExpressionToCodeLib;
using JetBrains.Annotations;

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

    public sealed class PocoDataReader<T> : DbDataReaderBase, IOptionalObjectListForDebugging
        where T : IReadImplicitly
    {
        readonly CancellationToken _cancellationToken;
        readonly IEnumerator<T> pocos;
        readonly IReadOnlyList<T> objectsOrNull_ForDebugging;
        T current;
        int rowsProcessed;

        public int RowsProcessed
            => rowsProcessed;

        public PocoDataReader([NotNull] IEnumerable<T> objects, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            pocos = objects.GetEnumerator();
            objectsOrNull_ForDebugging = objects as IReadOnlyList<T>;
        }

        public override void Close()
        {
            pocos.Dispose();
            isClosed = true;
            current = default;
        }

        protected override bool ReadImpl()
        {
            _cancellationToken.ThrowIfCancellationRequested();
            var hasnext = pocos.MoveNext();
            if (hasnext) {
                current = pocos.Current;
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

            public ColumnInfo([NotNull] IReadonlyPocoProperty<T> mp)
            {
                var propertyType = mp.DataType;
                var pocoParameter = Expression.Parameter(typeof(T));
                var propertyValue = mp.PropertyAccessExpression(pocoParameter);
                Name = mp.Name;
                var propertyConverter = PocoPropertyConverter.GetOrNull(propertyType);
                var isNonNullable = propertyType.IsValueType && propertyType.IfNullableGetNonNullableType() == null;

                if (propertyConverter != null) {
                    ColumnType = propertyConverter.DbType;
                    var propertyValueAsNoNullable = Expression.Convert(propertyValue, propertyConverter.ModelType);
                    var columnValueAsNonNullable = Expression.Invoke(Expression.Constant(propertyConverter.CompiledConverterToDb), propertyValueAsNoNullable);
                    var columnBoxedAsObject = Expression.Convert(columnValueAsNonNullable, typeof(object));
                    Expression columnBoxedAsColumnType;
                    if (isNonNullable) {
                        columnBoxedAsColumnType = columnBoxedAsObject;
                        WhenNullable_IsColumnDBNull = null;
                    } else {
                        var propertyIsNotNull = IsExpressionNonNull(propertyValue);
                        columnBoxedAsColumnType = Expression.Condition(propertyIsNotNull, columnBoxedAsObject, Expression.Constant(DBNull.Value, typeof(object)));
                        WhenNullable_IsColumnDBNull = Expression.Lambda<Func<T, bool>>(Expression.Not(propertyIsNotNull), pocoParameter).Compile();
                    }

                    TypedNonNullableGetter = Expression.Lambda(typeof(Func<,>).MakeGenericType(typeof(T), propertyConverter.DbType), columnValueAsNonNullable, pocoParameter).Compile();
                    GetUntypedColumnValue = Expression.Lambda<Func<T, object>>(columnBoxedAsColumnType, pocoParameter).Compile();
                } else {
                    ColumnType = propertyType.GetNonNullableUnderlyingType();
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
                        WhenNullable_IsColumnDBNull = Expression.Lambda<Func<T, bool>>(Expression.Not(propertyIsNotNull), pocoParameter).Compile();
                    }
                    TypedNonNullableGetter = Expression.Lambda(typeof(Func<,>).MakeGenericType(typeof(T), ColumnType), propertyValueAsNoNullable, pocoParameter).Compile();
                    GetUntypedColumnValue = Expression.Lambda<Func<T, object>>(columnBoxedAsColumnType, pocoParameter).Compile();
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

        static PocoDataReader()
        {
            var properties = PocoUtils.GetProperties<T>();
            var columnInfosBuilder = new List<ColumnInfo>(properties.Count);
            columnIndexByName = new Dictionary<string, int>(properties.Count, StringComparer.OrdinalIgnoreCase);
            schemaTable = CreateEmptySchemaTable();
            var i = 0;
            foreach (var pocoProperty in properties) {
                if (pocoProperty.CanRead) {
                    var columnInfo = new ColumnInfo(pocoProperty);
                    var isKey = pocoProperty.IsKey;
                    var allowDbNull = columnInfo.WhenNullable_IsColumnDBNull != null;
                    var isUnique = isKey && !properties.Any(other => other != pocoProperty && other.IsKey);
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