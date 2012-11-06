using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Data;

namespace ProgressOnderwijsUtils
{
	[UsedImplicitly(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.WithMembers)]
	public interface IMetaObject { }
	public interface IReadByConstructor { }
	[UsedImplicitly(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.WithMembers)]
	public interface IReadByFields { }


	public static class MetaObject
	{
		public static IEnumerable<IMetaProperty> GetMetaProperties(this IMetaObject metaobj) { return GetCache(metaobj.GetType()).Properties; }
		//public static object DynamicGet(this IMetaObject metaobj, string propertyName) { return GetCache(metaobj.GetType()).DynGet(metaobj, propertyName); }
		public static IEnumerable<IMetaProperty<T>> GetMetaProperties<T>() where T : IMetaObject { return MetaPropCache<T>.properties; }

		public static IMetaProperty<TMetaObject> GetByExpression<TMetaObject, T>(Expression<Func<TMetaObject, T>> property)
		{
			return GetByInheritedExpression<TMetaObject>.Get(property);
		}

		public static class GetByInheritedExpression<TMetaObject>
		{
			public static IMetaProperty<TMetaObject> Get<TParent, T>(Expression<Func<TParent, T>> property)
			{
				var propertyInfo = GetPropertyInfo(property);

				var mp = MetaPropCache<TMetaObject>.propertiesByInheritedInfo.GetOrDefault(propertyInfo);
				if (mp == null)
					throw new ArgumentException("To configure a metaproperty, must pass a lambda such as o=>o.MyPropertyName\n" +
						"The argument lambda refers to a property " + propertyInfo.Name + " that is not a MetaProperty");
				return mp;
			}

			public static PropertyInfo GetPropertyInfo<TParent, T>(Expression<Func<TParent, T>> property)
			{
				var paramExpr = property.Parameters.Single();
				var bodyExpr = property.Body;

				var innerExpr = UnwrapCast(bodyExpr);

				if (!(innerExpr is MemberExpression))
					throw new ArgumentException("To configure a metaproperty, you must pass a lambda such as o=>o.MyPropertyName\n" +
						"The passed lambda isn't a simple MemberExpression, but a " + innerExpr.NodeType + ":  " + ExpressionToCode.ToCode(property));
				var membExpr = ((MemberExpression)innerExpr);

				var targetExpr = UnwrapCast(membExpr.Expression);

				if (targetExpr != paramExpr)
					throw new ArgumentException("To configure a metaproperty, you must pass a lambda such as o=>o.MyPropertyName\n" +
						"A member is accessed, but not on the parameter " + paramExpr.Name + ": " + ExpressionToCode.ToCode(property));
				var propertyInfo = membExpr.Member as PropertyInfo;
				if (propertyInfo == null)
					throw new ArgumentException("To configure a metaproperty, must pass a lambda such as o=>o.MyPropertyName\n" +
						"The argument lambda refers to a member " + membExpr.Member.Name + " that is not a property");
				return propertyInfo;
			}
		}

		static Expression UnwrapCast(Expression bodyExpr) { return bodyExpr is UnaryExpression && bodyExpr.NodeType == ExpressionType.Convert ? ((UnaryExpression)bodyExpr).Operand : bodyExpr; }

		public static DataTable ToDataTable<T>(IEnumerable<T> objs, string[] primaryKey) where T : IMetaObject
		{
			var dt = new DataTable();
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

				var mapping = FieldMapping.VerifyAndCreate(clrColumns, ObjectToCode.GetCSharpFriendlyTypeName(typeof(T)), dataColumns, "table " + tableName, FieldMappingMode.IgnoreExtraDestinationFields);

				bulkCopy.BulkCopyTimeout = 3600;
				bulkCopy.DestinationTableName = tableName;
				foreach (var mapEntry in mapping)
					bulkCopy.ColumnMappings.Add(mapEntry.SrcIndex, mapEntry.DstIndex);
				bulkCopy.WriteToServer(objectReader);
			}
		}

		public static IReadOnlyList<IMetaProperty> GetMetaProperties(Type t)
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
			IReadOnlyList<IMetaProperty> Properties { get; }
		}

		sealed class MetaPropCache<T> : IMetaPropCache
		{
			public readonly static IMetaProperty<T>[] properties;
			public readonly static ReadOnlyDictionary<PropertyInfo, IMetaProperty<T>> propertiesByInheritedInfo;


			struct MetaAndInfo
			{
				readonly PropertyInfo propertyInfo;
				readonly IMetaProperty<T> metaProperty;
				public PropertyInfo PropertyInfo { get { return propertyInfo; } }
				public IMetaProperty<T> MetaProperty { get { return metaProperty; } }
				public MetaAndInfo(PropertyInfo propertyInfo, IMetaProperty<T> metaProperty)
				{
					this.propertyInfo = propertyInfo;
					this.metaProperty = metaProperty;
				}
			}


			static MetaPropCache()
			{
				if (typeof(T) == typeof(IMetaObject))
					throw new ArgumentException("Cannot determine metaproperties on IMetaObject itself");
				else if (typeof(T).IsInterface)
					throw new ArgumentException("Cannot determine metaproperties on interface type " + typeof(T));
				else if (typeof(T).IsAbstract)
					throw new ArgumentException("Cannot determine metaproperties on abstract type " + typeof(T));
				else if (typeof(T).BaseTypes().Any(bt => !bt.IsAbstract && typeof(IMetaObject).IsAssignableFrom(bt)))
					throw new ArgumentException("Cannot determine metaproperties on type with non-abstract base type(s)");
				else if (!typeof(T).GetProperties().Any())
					throw new ArgumentException("Cannot determine metaproperties on type without properties");

				properties = GetMetaPropertiesImpl().ToArray();

				Dictionary<MethodInfo, IMetaProperty<T>> propertiesByGetMethod = properties.Where(mp => mp.CanRead).ToDictionary(mp => mp.PropertyInfo.GetGetMethod());
				Dictionary<MethodInfo, IMetaProperty<T>> propertiesBySetMethod = properties.Where(mp => mp.Setter != null).ToDictionary(mp => mp.PropertyInfo.GetSetMethod());
				propertiesByInheritedInfo = typeof(T).GetInterfaces().SelectMany(ifaceType => {
					var map = typeof(T).GetInterfaceMap(ifaceType);
					return ifaceType.GetProperties()
						.Select(iProp => new MetaAndInfo(iProp,
								GetMP(map, iProp.GetGetMethod(), propertiesByGetMethod)
								?? GetMP(map, iProp.GetSetMethod(), propertiesBySetMethod)))
						.Where(entry => entry.MetaProperty != null);
				}).Concat(
					properties.Select(mp => new MetaAndInfo(mp.PropertyInfo, mp))
				).ToDictionary(entry => entry.PropertyInfo, entry => entry.MetaProperty).AsReadOnly();
			}

			static IMetaProperty<T> GetMP(InterfaceMapping map, MethodInfo ifaceMethod, Dictionary<MethodInfo, IMetaProperty<T>> propertiesByMethod)
			{
				var setMethodIdx = map.InterfaceMethods.IndexOf(ifaceMethod);
				if (setMethodIdx >= 0)
					return propertiesByMethod.GetOrDefault(map.TargetMethods[setMethodIdx]);
				return null;
			}

			public IReadOnlyList<IMetaProperty> Properties { get { return properties; } }

			static IEnumerable<IMetaProperty<T>> GetMetaPropertiesImpl() { return typeof(T).GetProperties().OrderBy(pi => pi.MetadataToken).Select(LoadIfMetaProperty).Where(mp => mp != null); }
			static IMetaProperty<T> LoadIfMetaProperty(PropertyInfo pi, int implicitOrder)
			{
				return pi.GetCustomAttributes(typeof(MpNotMappedAttribute), true).Any() ? null : new MetaProperty.Impl<T>(pi, implicitOrder);
			}
		}
		#endregion
	}
}