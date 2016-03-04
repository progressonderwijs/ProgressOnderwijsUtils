using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
            dt.Columns.AddRange(properties.Select(mp => new DataColumn(mp.Name, mp.DataType.GetNonNullableType()) { AllowDBNull = mp.DataType.CanBeNull() }).ToArray());

            foreach (var obj in objs) {
                dt.Rows.Add(properties.Select(mp => mp.Getter(obj) ?? DBNull.Value).ToArray());
            }

            if (optionalPrimaryKey != null) {
                dt.PrimaryKey = optionalPrimaryKey.Select(name => dt.Columns[name]).ToArray();
            }
            return dt;
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
