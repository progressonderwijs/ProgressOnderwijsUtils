namespace ProgressOnderwijsUtils;

public static class TypeExtension
{
    /// <summary>
    /// If type is Nullable&lt;T&gt;, returns typeof(T).  For non-Nullable&lt;&gt; types, returns null;
    /// </summary>
    [Pure]
    public static Type? IfNullableGetNonNullableType(this Type type)
        => type.IsNullableValueType() ? type.GetGenericArguments()[0] : null;

    /// <summary>
    /// If type is Nullable&lt;T&gt;, returns typeof(T).  For non-Nullable&lt;&gt; types, returns the type itself - this might also be a reference type, so the resulting type may still permit the value null.
    /// </summary>
    [Pure]
    public static Type GetNonNullableType(this Type type)
        => type.IsNullableValueType() ? type.GetGenericArguments()[0] : type;

    /// <summary>
    /// For enums, nullable types and nullable enums, return non-nullable underlying type;
    /// otherwise return original type.
    /// e.g. typeof(MyEnum?) => typeof(int)
    /// e.g. typeof(string) => typeof(string)
    /// 
    /// </summary>
    [Pure]
    public static Type GetNonNullableUnderlyingType(this Type type)
    {
        var nonNullableType = type.GetNonNullableType();
        if (nonNullableType.IsEnum) {
            return nonNullableType.GetEnumUnderlyingType();
        }
        return nonNullableType;
    }

    /// <summary>
    /// Find (nullable) underlying type corresponding to a (nullable) enum.
    /// Nullability is unaltered; Non-enum types are unaltered.
    /// </summary>
    [Pure]
    public static Type GetUnderlyingType(this Type type)
    {
        var maybeNonNullable = type.IfNullableGetNonNullableType();
        if (!(maybeNonNullable ?? type).IsEnum) {
            return type;
        } else if (maybeNonNullable == null) {
            return type.GetEnumUnderlyingType();
        } else {
            return typeof(Nullable<>).MakeGenericType(maybeNonNullable.GetEnumUnderlyingType());
        }
    }

    [Pure]
    public static bool CanBeNull(this Type type)
        => !type.IsValueType || IsNullableValueType(type);

    [Pure]
    public static bool IsNullableValueType(this Type type)
        => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

    /// <summary>
    /// If type is non-Nullable value type T, returns typeof(Nullable&lt;T&gt;).  For Nullable&lt;&gt; or reference types, returns null;
    /// </summary>
    [Pure]
    public static Type? MakeNullableType(this Type type)
        => type.CanBeNull() ? null : typeof(Nullable<>).MakeGenericType(type);

    [Pure]
    public static IEnumerable<Type> BaseTypes(this Type? type)
    {
        if (null == type) {
            yield break;
        }
        var baseType = type.BaseType;
        while (baseType != null) {
            yield return baseType;
            baseType = baseType.BaseType;
        }
    }

    [Pure]
    public static string GetNonGenericName(this Type type)
    {
        var typename = type.FullName ?? throw new ArgumentException("Only types with full names allowed.");
        var backtickIdx = typename.IndexOf('`');
        return backtickIdx == -1 ? typename : typename[..backtickIdx];
    }

    [Pure]
    public static T? Attr<T>(this MemberInfo mi)
        where T : Attribute
    {
        var customAttributes = mi.GetCustomAttributes(typeof(T), true);
        if (customAttributes.Length == 0) {
            return null;
        } else if (customAttributes.Length > 1) {
            throw new InvalidOperationException($"Expected zero or one {typeof(T)}, found {customAttributes.Length}");
        } else {
            return (T)customAttributes[0];
        }
    }
}
