using System;
using Microsoft.CodeAnalysis;

namespace ProgressOnderwijsUtils.Analyzers;

public sealed class SymbolMatcher
{
    readonly TypeKind kind;
    readonly string[] namePartsInReverse;

    public SymbolMatcher(TypeKind kind, string fullyQualifiedName)
    {
        this.kind = kind;
        namePartsInReverse = fullyQualifiedName.Split('.');
        Array.Reverse(namePartsInReverse);
    }

    public bool IsSymbolOfType(ITypeSymbol? symbol)
    {
        if (symbol?.TypeKind != kind) {
            return false;
        } else {
            var currentSymbol = (ISymbol)symbol;
            foreach (var namePart in namePartsInReverse) {
                if (namePart != currentSymbol?.Name) {
                    return false;
                }
                currentSymbol = currentSymbol.ContainingNamespace;
            }

            return currentSymbol is INamespaceSymbol { IsGlobalNamespace: true };
        }
    }

    public static readonly SymbolMatcher IsActionDelegate = new(TypeKind.Delegate, "System.Action");
    public static readonly SymbolMatcher IsUnit = new(TypeKind.Struct, "ProgressOnderwijsUtils.Collections.Unit");
    public static readonly SymbolMatcher IsMaybe = new(TypeKind.Struct, "ProgressOnderwijsUtils.Collections.Maybe");
}
