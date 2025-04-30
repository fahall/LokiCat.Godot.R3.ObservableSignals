using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.SyntaxHelpers;

public static class Namespace
{
    public static string GetNamespace(SyntaxNode node)
    {
        var current = node;
        while (current != null)
        {
            if (current is BaseNamespaceDeclarationSyntax ns)
            {
                return ns.Name.ToString();
            }

            if (current is CompilationUnitSyntax unit)
            {
                var fileScoped = unit.Members.OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault();
                return fileScoped?.Name.ToString() ?? "Global";
            }

            current = current.Parent;
        }

        return "Global";
    }
}