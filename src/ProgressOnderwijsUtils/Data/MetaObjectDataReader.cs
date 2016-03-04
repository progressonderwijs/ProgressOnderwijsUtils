﻿using System.Data;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils
{
    public sealed class MetaObjectDataReader<T> : DbDataReaderBase
        where T : IMetaObject
    {
        readonly IEnumerator<T> metaObjects;
        T current;

        public MetaObjectDataReader(IEnumerable<T> objects)
        {
            metaObjects = objects.GetEnumerator();
        }

        public override void Close()
        {
            metaObjects.Dispose();
            isClosed = true;
            current = default(T);
        }

        protected override bool ReadImpl()
        {
            var hasnext = metaObjects.MoveNext();
            if (hasnext) {
                current = metaObjects.Current;
            }
            return hasnext;
        }

        public override DataTable GetSchemaTable() => schemaTable;
        public override int FieldCount => columnInfos.Length;
        public override Type GetFieldType(int ordinal) => columnInfos[ordinal].ColumnType;
        public override string GetName(int ordinal) => columnInfos[ordinal].Name;
        public override int GetOrdinal(string name) => columnIndexByName[name];
        public override int GetInt32(int ordinal) => ((Func<T, int>)columnInfos[ordinal].TypedNonNullableGetter)(current);
        public override object GetValue(int ordinal) => columnInfos[ordinal].GetUntypedColumnValue(current) ?? DBNull.Value;

        public override bool IsDBNull(int ordinal)
        {
            var func = columnInfos[ordinal].WhenNullable_IsColumnDBNull;
            return func != null && func(current);
        }

        public override TColumn GetFieldValue<TColumn>(int ordinal)
        {
            var getter = columnInfos[ordinal].TypedNonNullableGetter as Func<T, TColumn>;
            if (getter == null) {
                throw new InvalidOperationException($"Tried to access field {columnInfos[ordinal].Name} of type {ObjectToCode.GetCSharpFriendlyTypeName(columnInfos[ordinal].ColumnType)} as type {ObjectToCode.GetCSharpFriendlyTypeName(typeof(TColumn))}.");
            }
            return getter(current);
        }

        struct ColumnInfo
        {
            public readonly string Name;
            //ColumnType is non nullable with enum types replaced by their underlying type
            public readonly Type ColumnType;
            public readonly Func<T, object> GetUntypedColumnValue;
            //WhenNullable_IsColumnDBNull is itself null if column non-nullable
            public readonly Func<T, bool> WhenNullable_IsColumnDBNull;
            //TypedNonNullableGetter is of type Func<T, _> such that typeof(_) == ColumnType - therefore cannot return nulls!
            public readonly Delegate TypedNonNullableGetter;

            public ColumnInfo(IReadonlyMetaProperty<T> mp)
            {
                var propertyType = mp.DataType;
                var metaObjectParameter = Expression.Parameter(typeof(T));
                var propertyValue = mp.PropertyAccessExpression(metaObjectParameter);
                Name = mp.Name;
                ColumnType = propertyType.GetNonNullableUnderlyingType();
                var propertyIsDefault = Expression.Equal(Expression.Default(propertyType), propertyValue);
                var nonNullableGetterType = typeof(Func<,>).MakeGenericType(typeof(T), ColumnType);
                var propertyConvertedToUnderlyingType = Expression.Convert(propertyValue, propertyType.GetUnderlyingType());
                var propertyConvertedToColumnType = Expression.Convert(propertyValue, ColumnType);
                var isNonNullable = propertyType.IsValueType && propertyType.IfNullableGetNonNullableType() == null;
                Expression columnBoxedAsColumnType;
                if (isNonNullable) {
                    columnBoxedAsColumnType = Expression.Convert(propertyConvertedToUnderlyingType, typeof(object));
                    WhenNullable_IsColumnDBNull = null;
                } else {
                    columnBoxedAsColumnType = Expression.Coalesce(propertyConvertedToUnderlyingType, Expression.Constant(DBNull.Value, typeof(object)));
                    WhenNullable_IsColumnDBNull = Expression.Lambda<Func<T, bool>>(propertyIsDefault, metaObjectParameter).Compile();
                }
                TypedNonNullableGetter = Expression.Lambda(nonNullableGetterType, propertyConvertedToColumnType, metaObjectParameter).Compile();
                GetUntypedColumnValue = Expression.Lambda<Func<T, object>>(columnBoxedAsColumnType, metaObjectParameter).Compile();
            }
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
    }
}
