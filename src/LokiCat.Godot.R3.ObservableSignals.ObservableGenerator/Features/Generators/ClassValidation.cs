using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.Generators;

internal static class ClassValidation
{
    internal static void ValidateDoesNotEmitSignalManually(
        ClassDeclarationSyntax classDecl,
        string signalName,
        GeneratorExecutionContext context)
    {
        if (!EmitsSignalManually(classDecl, signalName))
        {
            return;
        }



        context.ReportDiagnostic(Diagnostic.Create(Diagnostics.EmitsSignalManually, classDecl.Identifier.GetLocation(), signalName));
    }

    private static bool EmitsSignalManually(ClassDeclarationSyntax classDecl, string signalName)
    {
        return classDecl.DescendantNodes()
                        .OfType<InvocationExpressionSyntax>()
                        .Any(invocation =>
                                 invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                                 memberAccess.Name.Identifier.Text == "EmitSignal" &&
                                 invocation.ArgumentList.Arguments.Count > 0 &&
                                 invocation.ArgumentList.Arguments[0].ToString()
                                           .Contains(signalName, StringComparison.OrdinalIgnoreCase)
                        );
    }
}