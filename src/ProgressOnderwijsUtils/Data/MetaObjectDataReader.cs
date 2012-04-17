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

			//ReadValues = arrFiller.Compile();
		}

		public override void Close() { metaObjects.Dispose(); }
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
		public override int GetOrdinal(string name) { return indexLookup[name]; }
		public override object GetValue(int ordinal) { return cache[ordinal] ?? DBNull.Value; }

	}
}