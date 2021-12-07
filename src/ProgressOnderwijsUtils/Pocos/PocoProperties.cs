// ReSharper disable once CheckNamespace

namespace ProgressOnderwijsUtils;

public interface IPocoProperties<out T> : IReadOnlyList<T>
    where T : IPocoProperty
{
    IReadOnlyDictionary<string, int> IndexByName { get; }
    public Type PocoType { get; }
}

public sealed class PocoProperties<T> : IPocoProperties<IPocoProperty<T>>
    where T : IPoco
{
    readonly IPocoProperty<T>[] Properties;
    readonly IReadOnlyDictionary<string, int> indexByName;

    public Type PocoType
        => typeof(T);

    public IReadOnlyDictionary<string, int> IndexByName
        => indexByName;

    public static readonly PocoProperties<T> Instance = new();

    PocoProperties()
    {
        if (typeof(T).IsInterface) {
            throw new ArgumentException("Cannot determine properties on interface type " + typeof(T).ToCSharpFriendlyTypeName());
        } else if (typeof(T).IsAbstract) {
            throw new ArgumentException("Cannot determine properties on abstract type " + typeof(T).ToCSharpFriendlyTypeName());
        }

        Properties = GetPropertiesImpl();
        var dictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in Properties) { //perf:avoid LINQ.
            dictionary.Add(property.Name, property.Index);
        }
        indexByName = dictionary;
    }

    public IPocoProperty<T> GetByName(string name)
        => Properties[indexByName[name]];

    public int Count
        => Properties.Length;

    static IPocoProperty<T>[] GetPropertiesImpl()
    {
        var propertyInfos = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var properties = new IPocoProperty<T>[propertyInfos.Length];
        for (var index = 0; index < propertyInfos.Length; index++) {
            var propertyInfo = propertyInfos[index];
            var customAttributes = propertyInfo.GetCustomAttributes(true);
            properties[index] = new PocoProperty.Impl<T>(propertyInfo, index, customAttributes);
        }
        return properties;
    }

    public IPocoProperty<T> GetByExpression<TProp>(Expression<Func<T, TProp>> propertyExpression)
    {
        var memberInfo = PocoUtils.GetMemberInfo(propertyExpression);
        if (indexByName.TryGetValue(memberInfo.Name, out var propIdx)) {
            var prop = Properties[propIdx];
            if (prop.PropertyInfo == memberInfo) {
                return prop;
            }
        }

        throw new ArgumentException(
            "To configure a poco-property, must pass a lambda such as o=>o.MyPropertyName\n" +
            "The argument lambda refers to a property " + memberInfo.Name + " that is not a poco-property"
        );
    }

    public IEnumerator<IPocoProperty<T>> GetEnumerator()
    {
        foreach (var pocoProperty in Properties) {
            yield return pocoProperty;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public IPocoProperty<T> this[int index]
        => Properties[index];
}
