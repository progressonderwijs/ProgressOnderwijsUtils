using System.Data;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using ProgressOnderwijsUtils;
using MoreLinq;

namespace ProgressOnderwijsUtils.Data
{
	public sealed class MetaObjectDataReader<T> : DbDataReaderBase where T : IMetaObject
	{
		readonly IEnumerator<T> metaObjects;
		static readonly MetaProperty.Impl<T>[] fields;
		static readonly Action<T, object[]> ReadValues;

		static readonly Dictionary<string, int> indexLookup;
		static readonly DataTable schemaTable;

		static MetaObjectDataReader()
		{
			fields = MetaObject.GetMetaProperties<T>().Where(mp => mp.CanRead).Cast<MetaProperty.Impl<T>>().ToArray();
			indexLookup = fields.Select((mp, i) => new { mp.Naam, i }).ToDictionary(x => x.Naam, x => x.i);

			var parExpr = Expression.Parameter(typeof(T));
			var arrExpr = Expression.Parameter(typeof(object[]));

			var arrFiller = Expression.Lambda<Action<T, object[]>>(Expression.Block(fields.Select((field, i) => Expression.Assign(Expression.ArrayAccess(arrExpr, Expression.Constant(i)), Expression.Convert(Expression.Property(parExpr, field.propertyInfo), typeof(object))))), parExpr, arrExpr);

			var ab = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("MetaObjectDataReader_Helper"), AssemblyBuilderAccess.Run);
			var mod = ab.DefineDynamicModule("MetaObjectDataReader_HelperModule");
			var tb = mod.DefineType("MetaObjectDataReader_HelperType", TypeAttributes.Public);
			var mb = tb.DefineMethod("FillArray", MethodAttributes.Public | MethodAttributes.Static);
			arrFiller.CompileToMethod(mb);
			var t = tb.CreateType();

			ReadValues = (Action<T, object[]>)Delegate.CreateDelegate(typeof(Action<T, object[]>), t.GetMethod("FillArray"));

			schemaTable = CreateSchemaTable();

			//ReadValues = arrFiller.Compile();
		}

		public override DataTable GetSchemaTable()
		{
			return schemaTable;
		}

		static DataTable CreateSchemaTable()
		{
			var dt = new DataTable();
			dt.Columns.Add("ColumnName", typeof(string));
			dt.Columns.Add("ColumnOrdinal", typeof(int));
			dt.Columns.Add("ColumnSize", typeof(int));
			dt.Columns.Add("NumericPrecision", typeof(short));
			dt.Columns.Add("NumericScale", typeof(short));
			dt.Columns.Add("DataType", typeof(Type));
			dt.Columns.Add("ProviderType", typeof(int));
			dt.Columns.Add("IsLong", typeof(bool));
			dt.Columns.Add("AllowDBNull", typeof(bool));
			dt.Columns.Add("IsReadOnly", typeof(bool));
			dt.Columns.Add("IsRowVersion", typeof(bool));
			dt.Columns.Add("IsUnique", typeof(bool));
			dt.Columns.Add("IsKey", typeof(bool));
			dt.Columns.Add("IsAutoIncrement", typeof(bool));
			dt.Columns.Add("BaseCatalogName", typeof(string));
			dt.Columns.Add("BaseSchemaName", typeof(string));
			dt.Columns.Add("BaseTableName", typeof(string));
			dt.Columns.Add("BaseColumnName", typeof(string));

			for (int i = 0; i < fields.Length; i++)
			{
				dt.Rows.Add(fields[i].Naam, i, -1, null, null, fields[i].DataType, null, false, fields[i].AllowNull, true, false, false, false, false, null, null, null, "val");
			}
			return dt;

		}

		public override void Close() { metaObjects.Dispose(); isClosed = true; }
		public MetaObjectDataReader(IEnumerable<T> objects) { metaObjects = objects.GetEnumerator(); }
		object[] cache;
		protected override bool ReadImpl()
		{
			bool hasnext = metaObjects.MoveNext();
			if (hasnext)
			{
				cache = cache ?? new object[fields.Length];
				ReadValues(metaObjects.Current, cache);
			}
			return hasnext;
		}
		public override int FieldCount { get { return fields.Length; } }
		public override Type GetFieldType(int ordinal) { return fields[ordinal].DataType; }
		public override string GetName(int ordinal) { return fields[ordinal].Naam; }
		public IEnumerable<string> FieldNames { get { return fields.Select(field => field.Naam); } }
		public override int GetOrdinal(string name) { return indexLookup[name]; }
		public override object GetValue(int ordinal) { return cache[ordinal] ?? DBNull.Value; }
	}
}