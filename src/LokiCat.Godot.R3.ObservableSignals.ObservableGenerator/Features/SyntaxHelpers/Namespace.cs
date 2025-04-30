using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.SyntaxHelpers;

public static class Namespace
{
    public static string GetNamespace(SyntaxNode node)
    {
        return GetBlockStyleNamespace(node) ?? GetFileScopedNamespace(node) ?? "Global";
    }

    private static string? GetBlockStyleNamespace(SyntaxNode node)
    {
        return node.Ancestors()
                   .OfType<BaseNamespaceDeclarationSyntax>()
                   .Select(n => n.Name.ToString())
                   .FirstOrDefault(n => n.HasGlyphs());
    }
    
    private static string? GetFileScopedNamespace(SyntaxNode node)
    {
        var unit = node.SyntaxTree.GetRoot() as CompilationUnitSyntax;
        return unit?.Members.OfType<FileScopedNamespaceDeclarationSyntax>()
                             .Select(n => n.Name.ToString())
                             .FirstOrDefault(n => n.HasGlyphs());
    }

    private static bool HasGlyphs(this string? text)
    {
        return text is not null && text.Trim().Length > 0;
    }
}