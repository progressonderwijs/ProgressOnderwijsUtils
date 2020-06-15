using Microsoft.CodeAnalysis;

namespace ProgressOnderwijsUtils.Analyzers
{
    static class AnalyzerUtils
    {
        public static bool IsSymbolOfType(this ITypeSymbol? symbol, TypeKind kind, string name, params string[] ns)
        {
            if (symbol?.TypeKind != kind) {
                return false;
            } else if (symbol.Name != name) {
                return false;
            } else {
                var cns = symbol.ContainingNamespace;
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < ns.Length; i++) {
                    if (ns[i] != cns?.Name) {
                        return false;
                    } else {
                        cns = cns.ContainingNamespace;
                    }
                }

                return true;
            }
        }
    }
}
