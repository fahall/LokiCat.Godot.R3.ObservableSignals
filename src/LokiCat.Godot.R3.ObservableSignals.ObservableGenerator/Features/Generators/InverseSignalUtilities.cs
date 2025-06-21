using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.SyntaxHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.Generators;

internal static partial class InverseSignalUtilities
{
    internal static void AppendInverseSignalWiring(this StringBuilder body, string? inverseName,
        ParameterDefinition parameters, SemanticModel model)
    {
        if (inverseName is not null && BoolSignalCheck.IsSingleBoolParameter(parameters, model))
        {
            body.AppendLine($"EmitSignal(nameof({inverseName}), !value);");
        }
    }

    internal static Dictionary<string, string> GetInverseSignalMap(
        GeneratorExecutionContext context,
        IEnumerable<DelegateDeclarationSyntax> allDelegates)
    {
        var inverseMap = new Dictionary<string, string>();
        var inverseSignalDelegates = allDelegates.Where(d => d.HasAttribute(Attributes.INVERSE_SIGNAL));

        foreach (var delegateDecl in inverseSignalDelegates)
        {
            var inverseName = GodotSignalUtilities.GetSignalBaseName(delegateDecl.Identifier.Text);

            var targetName = GetTargetName(context, delegateDecl, inverseName);

            if (string.IsNullOrEmpty(targetName))
            {
                continue;
            }

            if (inverseMap.TryGetValue(targetName, out var existing))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(Diagnostics.MultipleInverseSignals, delegateDecl.GetLocation()));

                continue;
            }

            inverseMap[targetName] = inverseName;
        }

        return inverseMap;
    }

    private static string GetTargetName(GeneratorExecutionContext context, DelegateDeclarationSyntax? delegateDecl,
        string inverseName)
    {
        var inverseAttr = delegateDecl.AttributeLists
                                      .SelectMany(a => a.Attributes)
                                      .FirstOrDefault(attr => attr.Name.ToString().Equals(Attributes.INVERSE_SIGNAL));

        var targetName = string.Empty;

        if (inverseAttr?.ArgumentList?.Arguments.FirstOrDefault()?.Expression is InvocationExpressionSyntax
            {
                Expression: IdentifierNameSyntax { Identifier.Text: "nameof" }
            } invocation)
        {
            var expr = invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression;

            if (expr is not null)
            {
                var model = context.Compilation.GetSemanticModel(delegateDecl.SyntaxTree);
                var symbol = model.GetSymbolInfo(expr).Symbol;

                if (symbol is INamedTypeSymbol or IMethodSymbol or IFieldSymbol or IPropertySymbol)
                {
                    targetName = symbol.Name;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(targetName))
        {
            targetName = InferTargetNameFromInverse(inverseName);
        }

        return targetName;
    }

    private static string InferTargetNameFromInverse(string inverseName)
    {
        // Removes all occurrences of the word "Not" or "Inverse" when they appear as full words
        return $"{inverseName.WithoutNotOrInverse()}{GodotSignalUtilities.GODOT_SIGNAL_SUFFIX}";
    }
    
    /// <summary>
    /// Removes all occurrences of the word "Not" or "Inverse" when they appear as full words
    /// </summary>
    /// <param name="inverseName"></param>
    /// <returns></returns>
    private static string WithoutNotOrInverse(this string inverseName)
    {
        return MyRegex().Replace(inverseName, "");
    }

    [GeneratedRegex(@"\b(Not|Inverse)\b")]
    private static partial Regex MyRegex();
}