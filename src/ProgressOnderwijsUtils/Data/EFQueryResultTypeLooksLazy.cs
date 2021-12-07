using System;
using System.Collections;
using ExpressionToCodeLib;
using System.Collections.Generic;

namespace ProgressOnderwijsUtils;

public static class EFQueryResultTypeLooksLazy<TQueryResult>
{
    public static readonly string? TypeIsLazyErrorMessage;

    public static bool IsLazyQueryResultType
        => TypeIsLazyErrorMessage != null;

    static EFQueryResultTypeLooksLazy()
        => TypeIsLazyErrorMessage = //what's lazy?
            typeof(IEnumerable).IsAssignableFrom(typeof(TQueryResult)) //anything enumerable;
            && !typeof(Array).IsAssignableFrom(typeof(TQueryResult)) //...except arrays,
            && !typeof(IDictionary).IsAssignableFrom(typeof(TQueryResult)) //...except dictionaries
            && !typeof(IEquatable<TQueryResult>).IsAssignableFrom(typeof(TQueryResult)) //...except things with value-like semantics, in particular strings
            && typeof(TQueryResult).GetInterfaces().None(implementedInterface => implementedInterface.IsConstructedGenericType && implementedInterface.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
            && !typeof(IWrittenImplicitly).IsAssignableFrom(typeof(TQueryResult)) //...except POCOs (likely very rare)
                ? "Avoid lazily enumerated return types such as " + typeof(TQueryResult).ToCSharpFriendlyTypeName() + " because they can exhibit unexpected behavior when enumerated multiple times or when the db connection is used during enumeration"
                : null;
}