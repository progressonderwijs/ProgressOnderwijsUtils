using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using static ProgressOnderwijsUtils.SafeSql;

// ReSharper disable once CheckNamespace
namespace ProgressOnderwijsUtils
{
    public interface IMetaObject { }

    public static class MetaObject
    {
        [Pure]
        public static IMetaPropCache<IMetaProperty> GetMetaProperties([NotNull] this IMetaObject metaobj)
            => GetCache(metaobj.GetType());

        // ReSharper disable once UnusedParameter.Global
        [Pure]
        public static MetaInfo<T> GetMetaProperties<T>()
            where T : IMetaObject
            => MetaInfo<T>.Instance;

        [NotNull]
        [Pure]
        [CodeThatsOnlyUsedForTests]
        public static IMetaProperty<TMetaObject> GetByExpression<TMetaObject, T>([NotNull] Expression<Func<TMetaObject, T>> propertyExpression)
            where TMetaObject : IMetaObject
        {
            return MetaInfo<TMetaObject>.Instance.GetByExpression(propertyExpression);
        }

        public static class GetByInheritedExpression<TMetaObject>
            where TMetaObject : IMetaObject
        {
            [UsefulToKeep("library method for getting base-class metaproperty")]
            [Pure]
            public static IReadonlyMetaProperty<TMetaObject> Get<TParent, T>([NotNull] Expression<Func<TParent, T>> propertyExpression)
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

        [NotNull]
        [Pure]
        public static MemberInfo GetMemberInfo<TObject, TProperty>([NotNull] Expression<Func<TObject, TProperty>> property)
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
        static IMetaPropCache<IMetaProperty> GetCache(Type t)
            => (IMetaPropCache<IMetaProperty>)genGetCache.MakeGenericMethod(t).Invoke(null, null);

        [Pure]
        public static ParameterizedSql SqlColumnName([NotNull] this IMetaProperty mp)
        {
            return SQL($@"[{ParameterizedSql.CreateDynamic(mp.Name)}]");
        }
    }
}
