namespace ProgressOnderwijsUtils;

public interface ICopyable<T>
    where T : ICopyable<T>, IEquatable<T> { }

public static class CopyableExtensions
{
    static readonly MethodInfo memberwiseCloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance).AssertNotNull();
    static readonly Func<object, object> memberwiseCloneDelegate = (Func<object, object>)Delegate.CreateDelegate(typeof(Func<object, object>), memberwiseCloneMethod);
    static readonly ConcurrentDictionary<Type, Func<object, object>> copiers = new();

    static readonly Func<Type, Func<object, object>> copyDelegateFactory = type => type switch {
        _ when type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrNull(ci => ci.GetParameters() is { Length: 1, } parms && parms[0].ParameterType == type)
                is { } copyConstructor
            && Expression.Parameter(typeof(object)) is { } copyExpressionParameter
            => Expression.Lambda<Func<object, object>>(Expression.New(copyConstructor, Expression.Convert(copyExpressionParameter, type)), copyExpressionParameter).Compile(),
        _ => memberwiseCloneDelegate,
    };

    public static T Copy<T>(this T obj)
        where T : class, ICopyable<T>, IEquatable<T>
        => (T)copiers.GetOrAdd(obj.GetType(), copyDelegateFactory)(obj);
}
