using LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.Generators;
using Microsoft.CodeAnalysis;

namespace LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.SyntaxHelpers;

internal static class BoolSignalCheck
{
    public static bool IsSingleBoolParameter(ParameterDefinition parameters, SemanticModel model)
    {
        if (parameters.Count != 1)
        {
            return false;
        }

        var paramSyntax = parameters.Parameters[0];
        var type = model.GetTypeInfo(paramSyntax.Type!).Type;

        return type?.SpecialType == SpecialType.System_Boolean;
    }
}