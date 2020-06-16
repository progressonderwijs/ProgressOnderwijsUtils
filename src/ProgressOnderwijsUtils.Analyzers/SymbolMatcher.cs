using System;
using Microsoft.CodeAnalysis;

namespace ProgressOnderwijsUtils.Analyzers
{
    public sealed class SymbolMatcher
    {
        readonly TypeKind kind;
        readonly string name;
        readonly string[] reverseNamespaces;

        public SymbolMatcher(TypeKind kind, string fullyQualifiedName)
        {
            this.kind = kind;
            var splitName = fullyQualifiedName.Split('.');
            // ReSharper disable once UseIndexFromEndExpression //impossible or really messing in netstandard 2
            name = splitName[splitName.Length - 1];
            reverseNamespaces = splitName.AsSpan(0, splitName.Length - 1).ToArray();
            Array.Reverse(reverseNamespaces);
        }

        public bool IsSymbolOfType(ITypeSymbol? symbol)
        {
            if (symbol?.TypeKind != kind) {
                return false;
            } else if (symbol.Name != name) {
                return false;
            } else {
                var containingNamespace = symbol.ContainingNamespace;
                foreach (var namespacePart in reverseNamespaces) {
                    if (namespacePart != containingNamespace?.Name) {
                        return false;
                    }
                    containingNamespace = containingNamespace.ContainingNamespace;
                }

                return true;
            }
        }

        public static readonly SymbolMatcher IsActionDelegate = new SymbolMatcher(TypeKind.Delegate,"System.Action");
        public static readonly SymbolMatcher IsUnit = new SymbolMatcher(TypeKind.Struct,"ProgressOnderwijsUtils.Collections.Unit");
        public static readonly SymbolMatcher IsMaybe = new SymbolMatcher(TypeKind.Struct, "ProgressOnderwijsUtils.Collections.Maybe");
    }
}
