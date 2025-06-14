using Microsoft.CodeAnalysis;

namespace LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.Generators;

internal static class EnvironmentValidation
{
    internal static void ValidateEnvironment(GeneratorExecutionContext context, string diagnosticId)
    {
        if (!UnitTypeIsMissing(context.Compilation))
        {
            return;
        }

    

        context.ReportDiagnostic(Diagnostic.Create(Diagnostics.MissingUnitType, Location.None));
    }
    
    private static bool UnitTypeIsMissing(Compilation compilation)
    {
        return compilation.GetTypeByMetadataName("R3.Unit") is null;
    }
}