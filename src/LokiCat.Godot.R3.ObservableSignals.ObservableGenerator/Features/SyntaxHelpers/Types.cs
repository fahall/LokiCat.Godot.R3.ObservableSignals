using Microsoft.CodeAnalysis;

namespace LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.SyntaxHelpers;

public static class Types
{
    public static string GetFullTypeName(this ITypeSymbol? symbol) =>
        symbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "object";
    
    public static bool IsBoolean(this ITypeSymbol? symbol)
    {
        return symbol?.SpecialType == SpecialType.System_Boolean;
    }

}