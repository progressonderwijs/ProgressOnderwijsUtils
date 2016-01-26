using System.Data;
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
            bool hasnext = metaObjects.MoveNext();
            if (hasnext) {
                current = metaObjects.Current;
            }
            return hasnext;
        }

        public override DataTable GetSchemaTable() => schemaTable;
        public override int FieldCount => cols.Length;
        public override Type GetFieldType(int ordinal) => cols[ordinal].ColumnType;
        public override string GetName(int ordinal) => cols[ordinal].Name;
        public override int GetOrdinal(string name) => colIndexByName[name];
        public override int GetInt32(int ordinal) => ((Func<T, int>)cols[ordinal].TypedGetter)(current);
        public override object GetValue(int ordinal) => cols[ordinal].GetAsObject(current) ?? DBNull.Value;

        public override bool IsDBNull(int ordinal)
        {
            var func = cols[ordinal].IsFieldNull;
            return func != null && func(current);
        }

        public override TCol GetFieldValue<TCol>(int ordinal)
        {
            var getter = cols[ordinal].TypedGetter as Func<T, TCol>;
            if (getter == null) {
                throw new InvalidOperationException($"Tried to access field {cols[ordinal].Name} of type {ObjectToCode.GetCSharpFriendlyTypeName(cols[ordinal].ColumnType)} as type {ObjectToCode.GetCSharpFriendlyTypeName(typeof(TCol))}.");
            }
            return getter(current);
        }

        struct ColumnInfo
        {
            public string Name;
            public Type ColumnType;
            public Func<T, object> GetAsObject;
            public Func<T, bool> IsFieldNull;
            public Delegate TypedGetter;

            public static ColumnInfo MkColumnInfo(IMetaProperty<T> mp) 
            {
                var rowParExpr = Expression.Parameter(typeof(T));
                var memberExpr = mp.GetterExpression(rowParExpr);
                var columnInfo = new ColumnInfo {
                    Name = mp.Name,
                    ColumnType = mp.DataType.GetNonNullableUnderlyingType(),
                    GetAsObject = mp.Getter, //but what about enums then?
                    IsFieldNull = FieldIsNullDelegate(mp.DataType, rowParExpr, memberExpr),
                    TypedGetter = TypedFieldGetter(mp.DataType, rowParExpr, memberExpr),
                };
                return columnInfo;
            }
        }

        static readonly ColumnInfo[] cols;
        static readonly Dictionary<string, int> colIndexByName;
        static readonly DataTable schemaTable;

        static MetaObjectDataReader()
        {
            var metaProperties = MetaObject.GetMetaProperties<T>();
            var colsBuilder = new List<ColumnInfo>(metaProperties.Count);
            colIndexByName = new Dictionary<string, int>(metaProperties.Count, StringComparer.OrdinalIgnoreCase);
            schemaTable = CreateEmptySchemaTable();
            var i = 0;
            foreach (var mp in metaProperties) {
                if (mp.CanRead) {
                    var columnInfo = ColumnInfo.MkColumnInfo(mp);
                    var isKey = mp.IsKey;
                    var allowDbNull = columnInfo.IsFieldNull != null;
                    var isUnique = isKey && !metaProperties.Any(other => other != mp && other.IsKey);
                    colIndexByName.Add(columnInfo.Name, i);
                    schemaTable.Rows.Add(columnInfo.Name, i, -1, null, null, columnInfo.ColumnType, null, false, allowDbNull, true, false, isUnique, isKey, false, null, null, null, "val");
                    colsBuilder.Add(columnInfo);
                    i++;
                }
            }
            cols = colsBuilder.ToArray();
        }

        static Delegate TypedFieldGetter(Type propType, ParameterExpression rowParExpr, Expression memberExpr)
        {
            var typeForDb = propType.GetNonNullableUnderlyingType();
            var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), typeForDb);
            return Expression.Lambda(delegateType, Expression.Convert(memberExpr, typeForDb), rowParExpr).Compile();
        }

        static Func<T, bool> FieldIsNullDelegate(Type propType, ParameterExpression rowParExpr, Expression memberExpr)
        {
            if (propType.IsValueType && propType.IfNullableGetNonNullableType() == null) {
                return null;
            }
            var memberIsDefault = Expression.Equal(Expression.Default(propType), memberExpr);
            return Expression.Lambda<Func<T, bool>>(memberIsDefault, rowParExpr).Compile();
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
