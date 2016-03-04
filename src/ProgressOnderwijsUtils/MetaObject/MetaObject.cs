using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils
{
    [UsedImplicitly(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.WithMembers)]
    public interface IMetaObject { }

    public static class MetaObject
    {
        [Pure]
        public static IMetaPropCache<IMetaProperty> GetMetaProperties(this IMetaObject metaobj) => GetCache(metaobj.GetType());

        public static MetaInfo<T> GetMetaInfo<T>(this T metaobj) where T : IMetaObject => MetaInfo<T>.Instance;

        //public static object DynamicGet(this IMetaObject metaobj, string propertyName) => GetCache(metaobj.GetType()).DynGet(metaobj, propertyName);
        [Pure]
        public static MetaInfo<T> GetMetaProperties<T>() where T : IMetaObject
        {
            return MetaInfo<T>.Instance;
        }

        [Pure]
        [CodeDieAlleenWordtGebruiktInTests]
        public static IMetaProperty<TMetaObject> GetByExpression<TMetaObject, T>(Expression<Func<TMetaObject, T>> propertyExpression)
            where TMetaObject : IMetaObject
        {
            return MetaInfo<TMetaObject>.Instance.GetByExpression(propertyExpression);
        }

        public static class GetByInheritedExpression<TMetaObject>
            where TMetaObject : IMetaObject
        {
            [UsefulToKeep("library method for getting base-class metaproperty")]
            [Pure]
            public static IReadonlyMetaProperty<TMetaObject> Get<TParent, T>(Expression<Func<TParent, T>> propertyExpression)
            {
                var memberInfo = GetMemberInfo(propertyExpression);
                if (typeof(TParent).IsClass || typeof(TParent) == typeof(TMetaObject)) {
                    var retval = MetaInfo<TMetaObject>.Instance.SingleOrDefault(mp => mp.PropertyInfo == memberInfo);
                    if (retval == null) {
                        throw new ArgumentException(
                            "To configure a metaproperty, must pass a lambda such as o=>o.MyPropertyName\n" +
                                "The argument lambda refers to a property " + memberInfo.Name + " that is not a MetaProperty");
                    }
                    return retval;
                } else if (typeof(TParent).IsInterface && typeof(TParent).IsAssignableFrom(typeof(TMetaObject))) {
                    var pi = (PropertyInfo)memberInfo;
                    var getter = pi.GetGetMethod();
                    var interfacemap = typeof(TMetaObject).GetInterfaceMap(typeof(TParent));
                    var getterIdx = Array.IndexOf(interfacemap.InterfaceMethods, getter);
                    if (getterIdx == -1) {
                        throw new InvalidOperationException("The metaobject " + typeof(TMetaObject) + " does not implement method " + getter.Name);
                    }
                    var mpGetter = interfacemap.TargetMethods[getterIdx];
                    return MetaInfo<TMetaObject>.Instance.Single(mp => mp.PropertyInfo.GetGetMethod() == mpGetter);
                } else {
                    throw new InvalidOperationException(
                        "Impossible: parent " + typeof(TParent) + " is neither the metaobject type " + typeof(TMetaObject)
                            + " itself, nor a (base) class, nor a base interface.");
                }
            }
        }

        [Pure]
        public static MemberInfo GetMemberInfo<TObject, TProperty>(Expression<Func<TObject, TProperty>> property)
        {
            var bodyExpr = property.Body;

            var innerExpr = UnwrapCast(bodyExpr);

            if (!(innerExpr is MemberExpression)) {
                throw new ArgumentException(
                    "To configure a metaproperty, you must pass a lambda such as o=>o.MyPropertyName\n" +
                        "The passed lambda isn't a simple MemberExpression, but a " + innerExpr.NodeType + ":  " + ExpressionToCode.ToCode(property));
            }
            var membExpr = (MemberExpression)innerExpr;

            //*
            var targetExpr = UnwrapCast(membExpr.Expression);

            //expensive:
            var paramExpr = property.Parameters[0];
            if (targetExpr != paramExpr) {
                throw new ArgumentException(
                    "To configure a metaproperty, you must pass a lambda such as o=>o.MyPropertyName\n" +
                        "A member is accessed, but not on the parameter " + paramExpr.Name + ": " + ExpressionToCode.ToCode(property));
            }
            //*/

            var memberInfo = membExpr.Member;
            if (memberInfo is PropertyInfo || memberInfo is FieldInfo) {
                return memberInfo;
            }
            throw new ArgumentException(
                "To configure a metaproperty, must pass a lambda such as o=>o.MyPropertyName\n" +
                    "The argument lambda refers to a member " + membExpr.Member.Name + " that is not a property or field");
        }

        [Pure]
        static Expression UnwrapCast(Expression bodyExpr)
        {
            return bodyExpr is UnaryExpression && bodyExpr.NodeType == ExpressionType.Convert ? ((UnaryExpression)bodyExpr).Operand : bodyExpr;
        }

        [Pure]
        public static DataTable ToDataTable<T>(IEnumerable<T> objs, string[] optionalPrimaryKey) where T : IMetaObject
        {
            var dt = new DataTable();
            var properties = GetMetaProperties<T>().Where(mp => mp.CanRead).ToArray();
            dt.Columns.AddRange(
                properties.Select(mp => new DataColumn(mp.Name, mp.DataType.GetNonNullableType()) { AllowDBNull = !mp.Required && mp.DataType.CanBeNull() }).ToArray());

            foreach (var obj in objs) {
                dt.Rows.Add(properties.Select(mp => mp.Getter(obj) ?? DBNull.Value).ToArray());
            }

            if (optionalPrimaryKey != null) {
                dt.PrimaryKey = optionalPrimaryKey.Select(name => dt.Columns[name]).ToArray();
            }
            return dt;
        }

        [Pure]
        public static MetaObjectDataReader<T> CreateDataReader<T>(IEnumerable<T> entities) where T : IMetaObject
        {
            return new MetaObjectDataReader<T>(entities);
        }

        /// <summary>
        /// Performs a bulk insert.  Maps columns based on name, not order (unlike SqlBulkCopy by default); uses a 1 hour timeout.
        /// Default SqlBulkCopyOptions are SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.UseInternalTransaction
        /// </summary>
        /// <typeparam name="T">The type of metaobject to be inserted</typeparam>
        /// <param name="metaObjects">The list of entities to insert</param>
        /// <param name="sqlconn">The Sql connection to write to</param>
        /// <param name="tableName">The name of the table to import into; must be a valid sql identifier (i.e. you must escape special characters if any).</param>
        /// <param name="options">The SqlBulkCopyOptions to use.  If unspecified, uses SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.UseInternalTransaction which is NOT SqlBulkCopyOptions.Default</param>
        /// <param name="rowsCopiedEventHandler">An event handler to call periodically for progress reporting.</param>
        public static void SqlBulkCopy<T>(IEnumerable<T> metaObjects, SqlConnection sqlconn, string tableName, SqlBulkCopyOptions? options = null, SqlRowsCopiedEventHandler rowsCopiedEventHandler = null) where T : IMetaObject
        {
            if (metaObjects == null) {
                throw new ArgumentNullException(nameof(metaObjects));
            }
            if (tableName.Contains('[') || tableName.Contains(']')) {
                throw new ArgumentException("Tablename may not contain '[' or ']': " + tableName, nameof(tableName));
            }
            if (sqlconn == null) {
                throw new ArgumentNullException(nameof(sqlconn));
            }
            if (sqlconn.State != ConnectionState.Open) {
                throw new InvalidOperationException("Cannot bulk copy into " + tableName + ": connection isn't open but " + sqlconn.State);
            }
            if (!typeof(T).IsVisible) {
                throw new ArgumentException("MetaObject " + ObjectToCode.GetCSharpFriendlyTypeName(typeof(T)) + " must be public (accessable to other assemblies)");
            }

            var effectiveOptions = options ?? SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.UseInternalTransaction;


            using (var bulkCopy = new SqlBulkCopy(sqlconn, effectiveOptions, null)) {
                if (rowsCopiedEventHandler != null) {
                    bulkCopy.SqlRowsCopied += rowsCopiedEventHandler;
                }
                bulkCopy.BulkCopyTimeout = 3600;
                bulkCopy.DestinationTableName = tableName;

                using (var objectReader = CreateDataReader(metaObjects)) {
                    var mapping = ApplyMetaObjectColumnMapping(sqlconn, tableName, objectReader, bulkCopy);

                    try {
                        bulkCopy.WriteToServer(objectReader);
                    } catch (SqlException ex) {
                        var colid_message = new Regex(@"Received an invalid column length from the bcp client for colid ([0-9]+).", RegexOptions.Compiled);
                        var match = colid_message.Match(ex.Message);
                        if (match.Success) {
                            var oneBasedColId = int.Parse(match.Groups[1].Value);
                            var destinationColumnIndex = oneBasedColId - 1;
                            throw new Exception(
                                $"Received an invalid column length from the bcp client for column name {mapping.Single(m => m.DstIndex == destinationColumnIndex).SourceColumnDefinition.Name}",
                                ex);
                        }
                        throw;
                    }
                }
            }
        }

        static FieldMapping[] ApplyMetaObjectColumnMapping<T>(SqlConnection sqlconn, string tableName, MetaObjectDataReader<T> objectReader, SqlBulkCopy bulkCopy) where T : IMetaObject
        {
            var dataColumns = ColumnDefinition.GetFromTable(sqlconn, tableName);
            var clrColumns = ColumnDefinition.GetFromReader(objectReader);
            var mapping = FieldMapping.VerifyAndCreate(
                clrColumns,
                ObjectToCode.GetCSharpFriendlyTypeName(typeof(T)),
                dataColumns,
                "table " + tableName,
                FieldMappingMode.IgnoreExtraDestinationFields);

            foreach (var mapEntry in mapping) {
                bulkCopy.ColumnMappings.Add(mapEntry.SrcIndex, mapEntry.DstIndex);
            }
            return mapping;
        }

        [Pure]
        public static IReadOnlyList<IMetaProperty> GetMetaProperties(Type t)
        {
            if (!typeof(IMetaObject).IsAssignableFrom(t)) {
                throw new InvalidOperationException("Can't get meta-properties from type " + t + ", it's not a " + typeof(IMetaObject));
            }
            while (t.BaseType != null && !t.BaseType.IsAbstract && typeof(IMetaObject).IsAssignableFrom(t.BaseType)) {
                t = t.BaseType;
            }
            return GetCache(t);
        }

        static readonly MethodInfo genGetCache = Utils.F(GetMetaProperties<IMetaObject>).Method.GetGenericMethodDefinition();

        [Pure]
        static IMetaPropCache<IMetaProperty> GetCache(Type t) => (IMetaPropCache<IMetaProperty>)genGetCache.MakeGenericMethod(t).Invoke(null, null);
    }
}
