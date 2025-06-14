using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.SyntaxHelpers;

public static class Attributes
{
    public static bool HasAttribute(this DelegateDeclarationSyntax declaration, string attributeName)
    {
        return declaration.AttributeLists
                    .SelectMany(a => a.Attributes)
                    .Any(attr => attr.Name.ToString().Equals(attributeName));
    }
}