using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.SyntaxHelpers;

public static class Attributes
{
    public static bool HasAttribute(this DelegateDeclarationSyntax decl, string attributeName)
    {
        return decl.AttributeLists
                   .SelectMany(a => a.Attributes)
                   .Any(attr =>
                   {
                       var rawName = attr.Name.ToString();
                       return rawName == attributeName ||
                              rawName == attributeName + "Attribute" ||
                              rawName.EndsWith("." + attributeName) ||
                              rawName.EndsWith("." + attributeName + "Attribute");
                   });
    }
}