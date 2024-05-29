// ReSharper disable once CheckNamespace

namespace ProgressOnderwijsUtils;

public static class PocoUtils
{
    [Pure]
    public static IPocoProperties<IPocoProperty> GetProperties(this IPoco poco)
        => GetProperties(poco.GetType());

    [Pure]
    public static PocoProperties<T> GetProperties<T>()
        => PocoProperties<T>.Instance;

    [Pure]
    public static IPocoProperty<TPoco> GetByExpression<TPoco, T>(Expression<Func<TPoco, T>> propertyExpression)
        => PocoProperties<TPoco>.Instance.GetByExpression(propertyExpression);

    public static class GetByInheritedExpression<TPoco>
    {
        [UsefulToKeep("library method for getting base-class poco-property")]
        [Pure]
        public static IPocoProperty<TPoco> Get<TParent, T>(Expression<Func<TParent, T>> propertyExpression)
        {
            var memberInfo = GetMemberInfo(propertyExpression);
            if (typeof(TParent).IsClass || typeof(TParent) == typeof(TPoco)) {
                return PocoProperties<TPoco>.Instance.SingleOrNull(pocoProperty => pocoProperty.PropertyInfo == memberInfo)
                    ?? throw new ArgumentException(
                        "To configure a poco-property, must pass a lambda such as o=>o.MyPropertyName\n"
                        + $"The argument lambda refers to a property {memberInfo.Name} that is not a poco-property"
                    );
            } else if (typeof(TParent).IsInterface && typeof(TParent).IsAssignableFrom(typeof(TPoco))) {
                var pi = (PropertyInfo)memberInfo;
                var getter = pi.GetGetMethod();
                var interfacemap = typeof(TPoco).GetInterfaceMap(typeof(TParent));
                var getterIdx = Array.IndexOf(interfacemap.InterfaceMethods, getter);
                if (getterIdx == -1) {
                    throw new InvalidOperationException($"The poco {typeof(TPoco)} does not implement method {getter?.Name ?? "<<NULL>>"}");
                }
                var mpGetter = interfacemap.TargetMethods[getterIdx];
                return PocoProperties<TPoco>.Instance.Single(pocoProperty => pocoProperty.PropertyInfo.GetGetMethod() == mpGetter);
            } else {
                throw new InvalidOperationException($"Impossible: parent {typeof(TParent)} is neither the poco type {typeof(TPoco)} itself, nor a (base) class, nor a base interface.");
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
            "To configure a poco-property, you must pass a lambda such as o=>o.MyPropertyName\n"
            + $"The passed lambda isn't a simple MemberExpression, but a {innerExpr.NodeType}:  {ExpressionToCode.ToCode(property)}"
        );
    }

    static void AssertMemberMightMatchAProperty<TObject, TProperty>(Expression<Func<TObject, TProperty>> property, MemberInfo memberInfo, MemberExpression membExpr)
    {
        if (memberInfo.DeclaringType is null || !memberInfo.DeclaringType.IsAssignableFrom(typeof(TObject))) {
            throw new ArgumentException(
                "To configure a poco-property, you must pass a lambda such as o=>o.MyPropertyName\n"
                + $"Actual input: {ExpressionToCode.ToCode(property)}\n(The type of {ExpressionToCode.ToCode(membExpr.Expression.AssertNotNull())} should be {typeof(TObject).ToCSharpFriendlyTypeName()} or a base type.)"
            );
        }

        if (memberInfo is not PropertyInfo && memberInfo is not FieldInfo) {
            throw new ArgumentException(
                "To configure a poco-property, must pass a lambda such as o=>o.MyPropertyName\n"
                + $"The argument lambda refers to a member {membExpr.Member.Name} that is not a property or field"
            );
        }
    }

    [Pure]
    static Expression UnwrapCast(Expression bodyExpr)
        => bodyExpr is UnaryExpression { NodeType: ExpressionType.Convert, } unaryExpression ? unaryExpression.Operand : bodyExpr;

    [Pure]
    public static IPocoProperties<IPocoProperty> GetProperties(Type t)
        => memoizedGetProperties(t);

    static readonly MethodInfo genericMethodInfo_GetProperties = Utils.F(GetProperties<object>).Method.GetGenericMethodDefinition();
    static readonly Func<Type, IPocoProperties<IPocoProperty>> memoizedGetProperties = Utils.F((Type t) => (IPocoProperties<IPocoProperty>)genericMethodInfo_GetProperties.MakeGenericMethod(t).Invoke(null, null).AssertNotNull()).ThreadSafeMemoize();

    [Pure]
    public static string PropertiesDiffLog<TPoco>(TPoco a, TPoco b)
        => (
            from prop in GetProperties<TPoco>()
            let getter = prop.Getter
            where getter is not null
            let aVal = getter(a)
            let bVal = getter(b)
            where !Equals(aVal, bVal)
            select $"{prop.Name}={aVal}»»{bVal}"
        ).JoinStrings("; ");
}
