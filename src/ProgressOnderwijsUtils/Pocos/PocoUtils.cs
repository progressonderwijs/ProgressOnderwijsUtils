using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionToCodeLib;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace ProgressOnderwijsUtils
{
    public static class PocoUtils
    {
        [Pure]
        public static IPocoProperties<IPocoProperty> GetProperties(this IPoco poco)
            => GetCache(poco.GetType());

        // ReSharper disable once UnusedParameter.Global
        [Pure]
        public static PocoProperties<T> GetProperties<T>()
            where T : IPoco
            => PocoProperties<T>.Instance;

        [Pure]
        public static IPocoProperty<TPoco> GetByExpression<TPoco, T>(Expression<Func<TPoco, T>> propertyExpression)
            where TPoco : IPoco
            => PocoProperties<TPoco>.Instance.GetByExpression(propertyExpression);

        public static class GetByInheritedExpression<TPoco>
            where TPoco : IPoco
        {
            [UsefulToKeep("library method for getting base-class poco-property")]
            [Pure]
            public static IReadonlyPocoProperty<TPoco> Get<TParent, T>(Expression<Func<TParent, T>> propertyExpression)
            {
                var memberInfo = GetMemberInfo(propertyExpression);
                if (typeof(TParent).IsClass || typeof(TParent) == typeof(TPoco)) {
                    var retval = PocoProperties<TPoco>.Instance.SingleOrNull(pocoProperty => pocoProperty.PropertyInfo == memberInfo);
                    if (retval == null) {
                        throw new ArgumentException(
                            "To configure a poco-property, must pass a lambda such as o=>o.MyPropertyName\n" +
                            "The argument lambda refers to a property " + memberInfo.Name + " that is not a poco-property");
                    }
                    return retval;
                } else if (typeof(TParent).IsInterface && typeof(TParent).IsAssignableFrom(typeof(TPoco))) {
                    var pi = (PropertyInfo)memberInfo;
                    var getter = pi.GetGetMethod();
                    var interfacemap = typeof(TPoco).GetInterfaceMap(typeof(TParent));
                    var getterIdx = Array.IndexOf(interfacemap.InterfaceMethods, getter);
                    if (getterIdx == -1) {
                        throw new InvalidOperationException("The poco " + typeof(TPoco) + " does not implement method " + (getter?.Name ?? "<<NULL>>"));
                    }
                    var mpGetter = interfacemap.TargetMethods[getterIdx];
                    return PocoProperties<TPoco>.Instance.Single(pocoProperty => pocoProperty.PropertyInfo.GetGetMethod() == mpGetter);
                } else {
                    throw new InvalidOperationException(
                        "Impossible: parent " + typeof(TParent) + " is neither the poco type " + typeof(TPoco)
                        + " itself, nor a (base) class, nor a base interface.");
                }
            }
        }

        [Pure]
        public static MemberInfo GetMemberInfo<TObject, TProperty>(Expression<Func<TObject, TProperty>> property)
        {
            var bodyExpr = property.Body;

            var innerExpr = UnwrapCast(bodyExpr);

            if (innerExpr is MemberExpression membExpr) {
                var memberInfo = membExpr.Member;
                AssertMemberMightMatchAProperty(property, memberInfo, membExpr);
                return memberInfo;
            }

            throw new ArgumentException(
                "To configure a poco-property, you must pass a lambda such as o=>o.MyPropertyName\n" +
                "The passed lambda isn't a simple MemberExpression, but a " + innerExpr.NodeType + ":  " + ExpressionToCode.ToCode(property));
        }

        static void AssertMemberMightMatchAProperty<TObject, TProperty>(Expression<Func<TObject, TProperty>> property, MemberInfo memberInfo, MemberExpression membExpr)
        {
            if (!memberInfo.DeclaringType!.IsAssignableFrom(typeof(TObject))) {
                throw new ArgumentException(
                    "To configure a poco-property, you must pass a lambda such as o=>o.MyPropertyName\n" +
                    "Actual input: " + ExpressionToCode.ToCode(property) + "\n" +
                    "(The type of " + ExpressionToCode.ToCode(membExpr.Expression) + " should be " + typeof(TObject).ToCSharpFriendlyTypeName() + " or a base type.)");
            }

            if (!(memberInfo is PropertyInfo) && !(memberInfo is FieldInfo)) {
                throw new ArgumentException(
                    "To configure a poco-property, must pass a lambda such as o=>o.MyPropertyName\n" +
                    "The argument lambda refers to a member " + membExpr.Member.Name + " that is not a property or field");
            }
        }

        [Pure]
        static Expression UnwrapCast(Expression bodyExpr)
            => bodyExpr is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert ? unaryExpression.Operand : bodyExpr;

        [Pure]
        public static IReadOnlyList<IPocoProperty> GetProperties(Type t)
        {
            if (!typeof(IPoco).IsAssignableFrom(t)) {
                throw new InvalidOperationException("Can't get poco-properties from type " + t + ", it's not a " + typeof(IPoco));
            }
            while (t.BaseType != null && !t.BaseType.IsAbstract && typeof(IPoco).IsAssignableFrom(t.BaseType)) {
                t = t.BaseType;
            }
            return GetCache(t);
        }

        static readonly MethodInfo genGetCache = Utils.F(GetProperties<IPoco>).Method.GetGenericMethodDefinition();

        [Pure]
        static IPocoProperties<IPocoProperty> GetCache(Type t)
            => (IPocoProperties<IPocoProperty>)genGetCache.MakeGenericMethod(t).Invoke(null, null)!;
    }
}
