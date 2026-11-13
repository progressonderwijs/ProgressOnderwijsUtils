namespace ProgressOnderwijsUtils;

public static class ReflectionLoadableClassHelper
{
    // Constructor accessibility rather than type accessibility decides who can derive, so nesting doesn't matter.
    // The constructor alternative exists for non-generic roots with generic subclasses, which cannot be closed (CS9383).
    static bool OnlyDerivableWithinItsAssembly(Type type)
        => type.IsDefined(typeof(IsClosedTypeAttribute), false)
            || type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .All(ci => ci.IsPrivate || ci.IsAssembly || ci.IsFamilyAndAssembly);

    static readonly Func<Assembly, Type[]> typesIn = Utils.F((Assembly assembly) => assembly.GetTypes()).ThreadSafeMemoize();

    public static Type[] CachedTypes(this Assembly assembly)
        => typesIn(assembly);

    public static Maybe<ConstructorInfo[], string[]> AllSubclassConstructorsForType(Type baseType)
    {
        var instantiableSubTypes = new List<ConstructorInfo>();
        var problems = new List<string>();

        if (baseType.IsGenericType) {
            problems.Add(
                $"{baseType.ToCSharpFriendlyTypeName()} must be non-generic, because subclasses of generic intermediates cannot be reliably enumerated "
                + "and an open generic type definition can have an arbitrary number of subclasses."
            );
        }

        if (!baseType.IsClass) {
            problems.Add($"{baseType.ToCSharpFriendlyTypeName()} must be a class, because implementations of an interface cannot be restricted to its assembly.");
        }

        if (!OnlyDerivableWithinItsAssembly(baseType)) {
            problems.Add(
                $"{baseType.ToCSharpFriendlyTypeName()} must be closed or have only private, internal or private protected constructors "
                + "to ensure that all subclasses are enumerable via the assembly reference."
            );
        }

        if (!baseType.IsAbstract) {
            problems.Add($"{baseType.ToCSharpFriendlyTypeName()} must be abstract so each type has a single unambiguous constructor.");
        }

        foreach (var subType in baseType.Assembly.CachedTypes()) {
            if (subType == baseType || !baseType.IsAssignableFrom(subType)) {
                continue;
            }
            if (subType.IsAbstract) {
                if (!OnlyDerivableWithinItsAssembly(subType)) {
                    problems.Add(
                        $"{subType.ToCSharpFriendlyTypeName()} is an abstract subclass of {baseType.ToCSharpFriendlyTypeName()} that is not closed and has constructors other assemblies may call; "
                        + "mark it closed to ensure that all subclasses are enumerable via the assembly reference."
                    );
                }
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
        if (!safelyReflectionLoadableTypes.TryGet(out var constructors, out var error)) {
            return Maybe.Error(error);
        }
        var factories = new List<Func<TBaseType>>();
        foreach (var constructor in constructors) {
            var constructionExpression = Expression.Lambda<Func<TBaseType>>(Expression.New(constructor));
            var constructorDelegate = constructionExpression.CompileFast(true);
            if (constructorDelegate == null) {
                return Maybe.Error(new[] { $"Failed to create factory for {typeof(TBaseType).ToCSharpFriendlyTypeName()}", });
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
                                    .WhenError(ex => $"Could not execute factory {f.GetType().ToCSharpFriendlyTypeName()}:\n{ex}")
                        )
                        .WhenAllOk()
                        .WhenError(errors => errors.ToArray())
            );
}
