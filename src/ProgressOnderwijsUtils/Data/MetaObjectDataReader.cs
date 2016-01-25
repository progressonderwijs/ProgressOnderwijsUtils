using System.Data;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils
{
    public sealed class MetaObjectDataReader<T> : DbDataReaderBase
        where T : IMetaObject
    {
        readonly IEnumerator<T> metaObjects;
        public override DataTable GetSchemaTable() => schemaTable;

        public override void Close()
        {
            metaObjects.Dispose();
            isClosed = true;
            current = default(T);
        }

        T current;

        public MetaObjectDataReader(IEnumerable<T> objects)
        {
            metaObjects = objects.GetEnumerator();
        }

        protected override bool ReadImpl()
        {
            bool hasnext = metaObjects.MoveNext();
            if (hasnext) {
                current = metaObjects.Current;
            }
            return hasnext;
        }

        public override int FieldCount => cols.Length;
        public override Type GetFieldType(int ordinal) => cols[ordinal].DataType;
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
            if (getter != null) {
                return getter(current);
            } else {
                throw new InvalidOperationException($"Tried to access field {cols[ordinal].Name} of type {ObjectToCode.GetCSharpFriendlyTypeName(cols[ordinal].DataType)} as type {ObjectToCode.GetCSharpFriendlyTypeName(typeof(TCol))}.");
            }
        }

        struct ColumnInfo
        {
            public string Name;
            public Type DataType;
            public Func<T, object> GetAsObject;
            public Func<T, bool> IsFieldNull;
            public Delegate TypedGetter;
        }

        static readonly ColumnInfo[] cols;
        static readonly Dictionary<string, int> colIndexByName;
        static readonly DataTable schemaTable;

        static MetaObjectDataReader()
        {
            var metaProperties = MetaObject.GetMetaProperties<T>();
            var colsBuilder = new List<ColumnInfo>(metaProperties.Count);
            colIndexByName = new Dictionary<string, int>(metaProperties.Count, StringComparer.OrdinalIgnoreCase);
            schemaTable = MetaObjectDataReaderHelpers.CreateEmptySchemaTable();
            var i = 0;
            foreach (var mp in metaProperties) {
                if (mp.CanRead) {
                    var name = mp.Name;
                    var type = mp.DataType;
                    var isKey = mp.IsKey;
                    var fieldIsNullDelegate = FieldIsNullDelegate(mp);
                    var getter = mp.Getter;//safe for enum to int conversion?
                    var typedFieldGetter = TypedFieldGetter(mp);
                    var allowDbNull = fieldIsNullDelegate != null;
                    var isUnique = isKey && !metaProperties.Any(other => other != mp && other.IsKey);

                    colIndexByName.Add(mp.Name, i);
                    schemaTable.Rows.Add(name, i, -1, null, null, type, null, false, allowDbNull, true, false, isUnique, isKey, false, null, null, null, "val");
                    colsBuilder.Add(new ColumnInfo {
                        Name = name,
                        DataType = type,
                        GetAsObject = getter,
                        IsFieldNull = fieldIsNullDelegate,
                        TypedGetter = typedFieldGetter,
                    });
                    i++;
                }
            }

            cols = colsBuilder.ToArray();
        }

        static Delegate TypedFieldGetter(IMetaProperty<T> mp)
        {
            var typeForDb = mp.DataType.GetNonNullableUnderlyingType();
            var rowParExpr = Expression.Parameter(typeof(T));
            var memberExpr = mp.GetterExpression(rowParExpr);
            var convertedMemberExpr = Expression.Convert(memberExpr, typeForDb);
            var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), typeForDb);
            var typedGetter = Expression.Lambda(delegateType, convertedMemberExpr, rowParExpr);
            return typedGetter.Compile();
        }

        static Func<T, bool> FieldIsNullDelegate(IMetaProperty<T> mp)
        {
            var dataType = mp.DataType;

            if (dataType.IsValueType && dataType.IfNullableGetNonNullableType() == null) {
                return null;
            }

            var rowParExpr = Expression.Parameter(typeof(T));
            var memberExpr = mp.GetterExpression(rowParExpr);
            var typedIsNullChecker = Expression.Lambda<Func<T, bool>>(Expression.Equal(Expression.Default(dataType), memberExpr), rowParExpr);
            return typedIsNullChecker.Compile();
        }
    }

    static class MetaObjectDataReaderHelpers
    {
        public static DataTable CreateEmptySchemaTable()
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
