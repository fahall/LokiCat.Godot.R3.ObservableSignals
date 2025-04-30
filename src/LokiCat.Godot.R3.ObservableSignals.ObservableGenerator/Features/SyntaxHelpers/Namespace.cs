using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.SyntaxHelpers;

public static class Namespace
{
    public static string GetNamespace(ClassDeclarationSyntax classDecl)
    {
        var ns = classDecl.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();

        return ns?.Name.ToString() ?? "GlobalNamespace";
    }
}