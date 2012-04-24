using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils
{
	[UsedImplicitly(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.WithMembers)]
	public interface IMetaObject { }

	public static class MetaObject
	{
		public static IEnumerable<IMetaProperty> GetMetaProperties(this IMetaObject metaobj) { return GetCache(metaobj.GetType()).Properties; }
		//public static object DynamicGet(this IMetaObject metaobj, string propertyName) { return GetCache(metaobj.GetType()).DynGet(metaobj, propertyName); }
		public static IEnumerable<IMetaProperty<T>> GetMetaProperties<T>() where T : IMetaObject { return MetaPropCache<T>.properties; }

		public static DataTable ToDataTable<T>(IEnumerable<T> objs, string[] primaryKey) where T : IMetaObject
		{
			DataTable dt = new DataTable();
			var properties = GetMetaProperties<T>().Where(mp => mp.CanRead).ToArray();
			dt.Columns.AddRange(properties.Select(mp => new DataColumn(mp.Naam, mp.DataType.StripNullability()) { AllowDBNull = !mp.Verplicht && mp.DataType.CanBeNull() }).ToArray());

			foreach (var obj in objs)
				dt.Rows.Add(properties.Select(mp => mp.Getter(obj) ?? DBNull.Value).ToArray());

			if (primaryKey != null)
				dt.PrimaryKey = primaryKey.Select(name => dt.Columns[name]).ToArray();
			return dt;
		}

		public static IEnumerable<IMetaProperty> GetMetaProperties(Type t)
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
			IMetaProperty[] Properties { get; }
			//object DynGet(IMetaObject obj, string propertyName);
			//void DynSet(IMetaObject obj, string propertyName, object val);
		}

		sealed class MetaPropCache<T> : IMetaPropCache
		{
			public readonly static IMetaProperty<T>[] properties;
			static MetaPropCache() { properties = GetMetaPropertiesImpl().Cast<IMetaProperty<T>>().ToArray(); }
			public IMetaProperty[] Properties { get { return properties; } }
			//public object DynGet(IMetaObject obj, string propertyName) { return properties.Single(prop => prop.Naam == propertyName).Getter(obj); }
			//public void DynSet(IMetaObject obj, string propertyName, object val) { properties.Single(prop => prop.Naam == propertyName).Setter(obj, val); }

			static IEnumerable<IMetaProperty> GetMetaPropertiesImpl() { return typeof(T).GetProperties().OrderBy(pi => pi.MetadataToken).Select(LoadIfMetaProperty).Where(mp => mp != null); }
			static IMetaProperty LoadIfMetaProperty(PropertyInfo pi, int implicitOrder)
			{
				return pi.GetCustomAttributes(typeof(MpNotMappedAttribute), true).Any() ? null : new MetaProperty.Impl<T>(pi, implicitOrder);
			}
		}
		#endregion

	}
}