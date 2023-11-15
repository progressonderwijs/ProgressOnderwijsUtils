namespace ProgressOnderwijsUtils;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.Itself)]
public sealed class ConstructedImplicitlyAttribute : Attribute;

[ConstructedImplicitly]
public abstract class ReflectionLoadedBaseClass<T>
    where T : ReflectionLoadedBaseClass<T>
{
    public static TSub GetInstance<TSub>()
        where TSub : T, new() //new() constraint prevents usage of T itself.
        => SingletonHelper<TSub>.StaticInstance ?? (SingletonHelper<TSub>.StaticInstance = (TSub)instancesBySubType[typeof(TSub)]);

    static class SingletonHelper<TSub>
        where TSub : T, new()
    {
        public static TSub? StaticInstance;
    }

    public static T[] AllInstances()
        => instances.AssertNotNull();

    static readonly string[] initializationError; //crashes in type constructors are annoying to debug, so instead set aside any initialization errors and throw those on usage.
    static readonly Dictionary<Type, T>? uncheckedInstancesBySubType;

    static Dictionary<Type, T> instancesBySubType
        => uncheckedInstancesBySubType ?? throw new(initializationError.JoinStrings("\n\n"));

    static readonly T[]? uncheckedInstances;

    static T[] instances
        => uncheckedInstances ?? throw new(initializationError.JoinStrings("\n\n"));

    static ReflectionLoadedBaseClass()
    {
        var maybeInstances = ReflectionLoadableClassHelper.AllInstancesOfType<T>();

        if (!maybeInstances.TryGet(out var okInstances, out var error)) {
            initializationError = error;
            return;
        }

        var instantiableSubTypes = new Dictionary<Type, T>();
        var problems = new List<string>();
        foreach (var instance in okInstances) {
            if (instance is { } typedInstance) {
                instantiableSubTypes.Add(typedInstance.GetType(), typedInstance);
            } else {
                problems.Add($"All instantiable subclasses of {typeof(ReflectionLoadedBaseClass<T>).ToCSharpFriendlyTypeName()} must be subclasses of {typeof(T).ToCSharpFriendlyTypeName()}; {instance.GetType().ToCSharpFriendlyTypeName()} is not.");
            }
        }

        if (problems.Count == 0) {
            uncheckedInstances = instantiableSubTypes.Values.ToArray();
            uncheckedInstancesBySubType = instantiableSubTypes;
            initializationError = Array.Empty<string>();
        } else {
            uncheckedInstances = null;
            uncheckedInstancesBySubType = null;
            initializationError = problems.ToArray();
        }
    }
}
