using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Data;

namespace ProgressOnderwijsUtils
{
	//[UsedImplicitly(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.WithMembers)]
	public interface IMetaObject { }
	public interface ILoadFromDbByConstructor { }
	public interface ILoadFromDbByFields { }


	public static class MetaObject
	{
		public static IEnumerable<IMetaProperty> GetMetaProperties(this IMetaObject metaobj) { return GetCache(metaobj.GetType()).Properties; }
		//public static object DynamicGet(this IMetaObject metaobj, string propertyName) { return GetCache(metaobj.GetType()).DynGet(metaobj, propertyName); }
		public static IEnumerable<IMetaProperty<T>> GetMetaProperties<T>() where T : IMetaObject { return MetaPropCache<T>.properties; }

		public static DataTable ToDataTable<T>(IEnumerable<T> objs, string[] primaryKey) where T : IMetaObject
		{
			DataTable dt = new DataTable();
			var properties = GetMetaProperties<T>().Where(mp => mp.CanRead).ToArray();
			dt.Columns.AddRange(properties.Select(mp => new DataColumn(mp.Naam, mp.DataType.GetNonNullableType()) { AllowDBNull = !mp.Verplicht && mp.DataType.CanBeNull() }).ToArray());

			foreach (var obj in objs)
				dt.Rows.Add(properties.Select(mp => mp.TypedGetter(obj) ?? DBNull.Value).ToArray());

			if (primaryKey != null)
				dt.PrimaryKey = primaryKey.Select(name => dt.Columns[name]).ToArray();
			return dt;
		}

		public static MetaObjectDataReader<T> CreateDataReader<T>(IEnumerable<T> entities) where T : IMetaObject { return new MetaObjectDataReader<T>(entities); }

		/// <summary>
		/// Performs a bulk insert.  Maps columns based on name, not order (unlike SqlBulkCopy by default); uses a 1 hour timeout.
		/// Default SqlBulkCopyOptions are SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.UseInternalTransaction
		/// </summary>
		/// <typeparam name="T">The type of metaobject to be inserted</typeparam>
		/// <param name="metaObjects">The list of entities to insert</param>
		/// <param name="sqlconn">The Sql connection to write to</param>
		/// <param name="tableName">The name of the table to import into; must be a valid sql identifier (i.e. you must escape special characters if any).</param>
		/// <param name="options">The SqlBulkCopyOptions to use.  If unspecified, uses SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.UseInternalTransaction which is NOT SqlBulkCopyOptions.Default</param>
		public static void SqlBulkCopy<T>(IEnumerable<T> metaObjects, SqlConnection sqlconn, string tableName, SqlBulkCopyOptions? options = null) where T : IMetaObject
		{
			if (metaObjects == null)
				throw new ArgumentNullException("metaObjects");
			if (tableName.Contains('[') || tableName.Contains(']'))
				throw new ArgumentException("Tablename may not contain '[' or ']': " + tableName, "tableName");
			if (sqlconn == null)
				throw new ArgumentNullException("sqlconn");
			if (sqlconn.State != ConnectionState.Open)
				throw new InvalidOperationException("Cannot bulk copy into " + tableName + ": connection isn't open but " + sqlconn.State);

			SqlBulkCopyOptions effectiveOptions = options ?? (SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.UseInternalTransaction);
			DbColumnDefinition[] dataColumns = DbColumnDefinition.GetFromTable(sqlconn, tableName);

			using (var objectReader = CreateDataReader(metaObjects))
			using (var bulkCopy = new SqlBulkCopy(sqlconn, effectiveOptions, null))
			{
				DbColumnDefinition[] clrColumns = DbColumnDefinition.GetFromReader(objectReader);

				var mapping = FieldMapping.VerifyAndCreate(clrColumns, ObjectToCode.GetCSharpFriendlyTypeName(typeof(T)), dataColumns, "table " + tableName, FieldMappingMode.RequireExactColumnMatches);

				bulkCopy.BulkCopyTimeout = 3600;
				bulkCopy.DestinationTableName = tableName;
				foreach (var mapEntry in mapping)
					bulkCopy.ColumnMappings.Add(mapEntry.SrcIndex, mapEntry.DstIndex);
				bulkCopy.WriteToServer(objectReader);
			}
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
			static MetaPropCache()
			{
				if (typeof(T) == typeof(IMetaObject))
					throw new ArgumentException("Cannot determine metaproperties on IMetaObject itself");
				else if (typeof(T).IsAbstract)
					throw new ArgumentException("Cannot determine metaproperties on abstract type " + typeof(T));
				else if (typeof(T).IsInterface)
					throw new ArgumentException("Cannot determine metaproperties on interface type " + typeof(T));
				else if (!typeof(T).IsSealed)
					throw new ArgumentException("IMetaObjects must be sealed! The type " + typeof(T) + " is not sealed");

				properties = GetMetaPropertiesImpl().Cast<IMetaProperty<T>>().ToArray();
			}
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