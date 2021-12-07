namespace ProgressOnderwijsUtils;

public static class ReflectionLoadableClassHelper
{
    static bool EffectivelyAssemblyInternal(Type type)
    {
        if (!type.IsPublic) {
            return true;
        }
        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (constructors.Any() && constructors.All(ci => ci.IsAssembly || ci.IsPrivate || ci.IsFamilyAndAssembly)) {
            return true;
        }
        return false;
    }

    static readonly Func<Assembly, Type[]> typesIn = Utils.F((Assembly assembly) => assembly.GetTypes()).ThreadSafeMemoize();

    public static Type[] CachedTypes(this Assembly assembly)
        => typesIn(assembly);

    public static Maybe<ConstructorInfo[], string[]> AllSubclassConstructorsForType(Type baseType)
    {
        var instantiableSubTypes = new List<ConstructorInfo>();
        var problems = new List<string>();

        if (baseType.IsGenericTypeDefinition) {
            problems.Add($"{baseType.ToCSharpFriendlyTypeName()} can have an arbitrary number of subclasses and thus cannot be enumerated because it is an open generic type definition.");
        }

        if (!EffectivelyAssemblyInternal(baseType)) {
            problems.Add($"{baseType.ToCSharpFriendlyTypeName()} must be assembly-internal or have only assembly-internal constructors to ensure that all subclasses are enumerable via the assembly reference");
        }

        if (!baseType.IsAbstract) {
            problems.Add($"{baseType.ToCSharpFriendlyTypeName()} must be abstract so each type has a single unambiguous constructor.");
        }

        foreach (var subType in baseType.Assembly.CachedTypes()) {
            if (!baseType.IsAssignableFrom(subType) || subType.IsAbstract) {
                continue;
            }
            if (!subType.IsSealed) {
                problems.Add($"{subType.ToCSharpFriendlyTypeName()} is a non-abstract subclass of {baseType.ToCSharpFriendlyTypeName()}; it must be sealed so an unambiguous instance can be constructed.");
            } else if (subType.IsGenericType) {
                problems.Add($"{subType.ToCSharpFriendlyTypeName()} is a non-abstract subclass of {baseType.ToCSharpFriendlyTypeName()}: it must be non-generic to allow constructing an instance without context.");
            } else if (subType.GetConstructor(Type.EmptyTypes) is not { } constructor) {
                problems.Add($"{subType.ToCSharpFriendlyTypeName()} is a non-abstract subclass of {baseType.ToCSharpFriendlyTypeName()}: it must have a public parameterless constructor.");
            } else {
                instantiableSubTypes.Add(constructor);
            }
        }

        if (problems.Count == 0) {
            return Maybe.Ok(instantiableSubTypes.ToArray());
        } else {
            return Maybe.Error(problems.ToArray());
        }
    }

    public static Maybe<Func<TBaseType>[], string[]> AllSubclassConstructorsForType<TBaseType>()
    {
        var safelyReflectionLoadableTypes = AllSubclassConstructorsForType(typeof(TBaseType));
        if (safelyReflectionLoadableTypes.IsError) {
            return Maybe.Error(safelyReflectionLoadableTypes.AssertError());
        }
        var constructors = safelyReflectionLoadableTypes.AssertOk();
        var factories = new List<Func<TBaseType>>();
        foreach (var constructor in constructors) {
            var constructionExpression = Expression.Lambda<Func<TBaseType>>(Expression.New(constructor), Array.Empty<ParameterExpression>());
            var constructorDelegate = constructionExpression.CompileFast(true);
            if (constructorDelegate == null) {
                return Maybe.Error(new[] { "Failed to create factory for " + typeof(TBaseType).ToCSharpFriendlyTypeName() });
            }
            factories.Add(constructorDelegate);
        }
        return Maybe.Ok(factories.ToArray());
    }

    public static Maybe<TBaseType[], string[]> AllInstancesOfType<TBaseType>()
        => AllSubclassConstructorsForType<TBaseType>()
            .WhenOkTry(
                factories =>
                    factories
                        .Select(
                            f =>
                                Maybe.Try(f).Catch<Exception>()
                                    .WhenError(ex => "Could not execute factory " + f.GetType().ToCSharpFriendlyTypeName() + ":\r\n" + ex.ToString())
                        )
                        .WhenAllOk()
                        .WhenError(errors => errors.ToArray())
            );
}
