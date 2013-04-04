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

namespace ProgressOnderwijsUtils
{
	[UsedImplicitly(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.WithMembers)]
	public interface IReadByFields { }
	[UsedImplicitly(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.WithMembers)]
	public interface IMetaObject { }
	public interface IReadByConstructor { }


	public static class MetaObject
	{
		public static IEnumerable<IMetaProperty> GetMetaProperties(this IMetaObject metaobj) { return GetCache(metaobj.GetType()).Properties; }
		//public static object DynamicGet(this IMetaObject metaobj, string propertyName) { return GetCache(metaobj.GetType()).DynGet(metaobj, propertyName); }
		public static IEnumerable<IMetaProperty<T>> GetMetaProperties<T>() where T : IMetaObject { return MetaPropCache<T>.MetaProperties; }

		public static IMetaProperty<TMetaObject> GetByExpression<TMetaObject, T>(Expression<Func<TMetaObject, T>> propertyExpression)
		{
			var memberInfo = GetMemberInfo(propertyExpression);
			var retval = MetaPropCache<TMetaObject>.MetaProperties.SingleOrDefault(mp => mp.PropertyInfo == memberInfo);
			if (retval == null)
				throw new ArgumentException("To configure a metaproperty, must pass a lambda such as o=>o.MyPropertyName\n" +
						"The argument lambda refers to a property " + memberInfo.Name + " that is not a MetaProperty");
			return retval;
		}

		public static class GetByInheritedExpression<TMetaObject>
		{
			public static IMetaProperty<TMetaObject> Get<TParent, T>(Expression<Func<TParent, T>> propertyExpression)
			{
				var memberInfo = GetMemberInfo(propertyExpression);
				if (typeof(TParent).IsClass || typeof(TParent) == typeof(TMetaObject))
				{
					var retval = MetaPropCache<TMetaObject>.MetaProperties.SingleOrDefault(mp => mp.PropertyInfo == memberInfo);
					if (retval == null)
						throw new ArgumentException("To configure a metaproperty, must pass a lambda such as o=>o.MyPropertyName\n" +
								"The argument lambda refers to a property " + memberInfo.Name + " that is not a MetaProperty");
					return retval;
				}
				else if (typeof(TParent).IsInterface && typeof(TParent).IsAssignableFrom(typeof(TMetaObject)))
				{
					var pi = (PropertyInfo)memberInfo;
					var getter = pi.GetGetMethod();
					var interfacemap = typeof(TMetaObject).GetInterfaceMap(typeof(TParent));
					var getterIdx = Array.IndexOf(interfacemap.InterfaceMethods, getter);
					if (getterIdx == -1)
						throw new InvalidOperationException("The metaobject " + typeof(TMetaObject) + " does not implement method " + getter.Name);
					var mpGetter = interfacemap.TargetMethods[getterIdx];
					return MetaPropCache<TMetaObject>.MetaProperties.Single(mp => mp.PropertyInfo is PropertyInfo && ((PropertyInfo)mp.PropertyInfo).GetGetMethod() == mpGetter);
				}
				else throw new InvalidOperationException("Impossible: parent " + typeof(TParent) + " is neither the metaobject type " + typeof(TMetaObject) + " itself, nor a (base) class, nor a base interface.");
			}

		}

		public static MemberInfo GetMemberInfo<TObject, TProperty>(Expression<Func<TObject, TProperty>> property)
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
			var memberInfo = membExpr.Member;
			if (memberInfo is PropertyInfo || memberInfo is FieldInfo)
				return memberInfo;
			throw new ArgumentException("To configure a metaproperty, must pass a lambda such as o=>o.MyPropertyName\n" +
				"The argument lambda refers to a member " + membExpr.Member.Name + " that is not a property or field");
		}

		static Expression UnwrapCast(Expression bodyExpr) { return bodyExpr is UnaryExpression && bodyExpr.NodeType == ExpressionType.Convert ? ((UnaryExpression)bodyExpr).Operand : bodyExpr; }

		public static DataTable ToDataTable<T>(IEnumerable<T> objs, string[] primaryKey) where T : IMetaObject
		{
			var dt = new DataTable();
			var properties = GetMetaProperties<T>().Where(mp => mp.CanRead).ToArray();
			dt.Columns.AddRange(properties.Select(mp => new DataColumn(mp.Name, mp.DataType.GetNonNullableType()) { AllowDBNull = !mp.Required && mp.DataType.CanBeNull() }).ToArray());

			foreach (var obj in objs)
				dt.Rows.Add(properties.Select(mp => mp.Getter(obj) ?? DBNull.Value).ToArray());

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
			ColumnDefinition[] dataColumns = ColumnDefinition.GetFromTable(sqlconn, tableName);

			using (var objectReader = CreateDataReader(metaObjects))
			using (var bulkCopy = new SqlBulkCopy(sqlconn, effectiveOptions, null))
			{
				ColumnDefinition[] clrColumns = ColumnDefinition.GetFromReader(objectReader);

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
			while (t.BaseType != null && !t.BaseType.IsAbstract && typeof(IMetaObject).IsAssignableFrom(t.BaseType))
				t = t.BaseType;
			IMetaPropCache cache = GetCache(t);
			return cache.Properties;
		}

		#region Meta property cache
		static IMetaPropCache GetCache(Type t) { return (IMetaPropCache)typeof(MetaPropCache<>).MakeGenericType(t).GetConstructor(Type.EmptyTypes).Invoke(null); }

		#endregion
	}


}