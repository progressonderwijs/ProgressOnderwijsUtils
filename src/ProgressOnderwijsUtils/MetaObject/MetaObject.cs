using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JetBrains.Annotations;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils
{
	[UsedImplicitly(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.WithMembers)]
	public interface IMetaObject { }

	public static class MetaObject
	{
		public static IEnumerable<MetaProperty> GetMetaProperties(this IMetaObject metaobj) { return GetCache(metaobj.GetType()).Properties; }
		public static object DynamicGet(this IMetaObject metaobj, string propertyName) { return GetCache(metaobj.GetType()).DynGet(metaobj, propertyName); }
		public static IEnumerable<MetaProperty> GetMetaProperties<T>() where T : IMetaObject { return MetaPropCache<T>.properties; }

		public static DataTable ToDataTable<T>(IEnumerable<T> objs, string[] primaryKey) where T : IMetaObject
		{
			DataTable dt = new DataTable();
			var properties = GetMetaProperties<T>().Where(mp => mp.CanRead).ToArray();
			dt.Columns.AddRange(properties.Select(mp => new DataColumn(mp.Naam, mp.DataType.StripNullability()) { AllowDBNull = !mp.Verplicht && mp.DataType.CanBeNull() }).ToArray());

			foreach (var obj in objs)
				dt.Rows.Add(properties.Select(mp => mp.Accessors.Getter(obj) ?? DBNull.Value).ToArray());

			if (primaryKey != null)
				dt.PrimaryKey = primaryKey.Select(name => dt.Columns[name]).ToArray();
			return dt;
		}

		public static IEnumerable<MetaProperty> GetMetaProperties(Type t)
		{
			if (!typeof(IMetaObject).IsAssignableFrom(t))
				throw new InvalidOperationException("Can't get meta-properties from type " + t + ", it's not a " + typeof(IMetaObject));
			IMetaPropCache cache = GetCache(t);
			return cache.Properties;
		}

		#region Meta property cache
		static IMetaPropCache GetCache(Type t) { return (IMetaPropCache)typeof(MetaPropCache<>).MakeGenericType(t).GetConstructor(Type.EmptyTypes).Invoke(null); }

		interface IMetaPropCache
		{
			MetaProperty[] Properties { get; }
			object DynGet(IMetaObject obj, string propertyName);
			void DynSet(IMetaObject obj, string propertyName, object val);
		}

		sealed class MetaPropCache<T> : IMetaPropCache
		{
			public readonly static MetaProperty[] properties;
			static MetaPropCache() { properties = GetMetaPropertiesImpl(typeof(T)).ToArray(); }
			public MetaProperty[] Properties { get { return properties; } }
			public object DynGet(IMetaObject obj, string propertyName) { return properties.Single(prop => prop.Naam == propertyName).Accessors.Getter(obj); }
			public void DynSet(IMetaObject obj, string propertyName, object val) { properties.Single(prop => prop.Naam == propertyName).Accessors.Setter(obj, val); }
		}
		#endregion

		#region private Helpers
		static IEnumerable<MetaProperty> GetMetaPropertiesImpl(Type t) { return t.GetProperties().OrderBy(pi=>pi.MetadataToken).Select(MetaProperty.LoadIfMetaProperty).Where(mp => mp != null); }
		#endregion
	}
}