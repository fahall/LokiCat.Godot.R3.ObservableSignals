using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.Generators;

public static class Attributes
{
    public const string RX_OBSERVABLE = "RxObservable";
    public const string INVERSE_SIGNAL = "InverseSignal";
    public const string RX_PROP = "RxProp";
} 

public static class AttributeGenerator
{
    
    // Injects a delegate‐level InverseSignalAttribute so no separate DLL is required
    public static void EmitInverseSignalAttributeDefinition(GeneratorExecutionContext context)
    {
        const string CUSTOM_BODY = $$"""
{
    public string OriginalName { get; }
    public {{Attributes.INVERSE_SIGNAL}}Attribute(string originalName = null) => OriginalName = originalName;
}
""";
        AddSimpleAttribute(context, Attributes.INVERSE_SIGNAL, CUSTOM_BODY);
    }
    
    // Injects a delegate‐level InverseSignalAttribute so no separate DLL is required
    public static void EmitRxObservableAttributeDefinition(GeneratorExecutionContext context)
    {
        AddSimpleAttribute(context, Attributes.RX_OBSERVABLE);
    }
    
    // Injects a delegate‐level RxSignalProp so no separate DLL is required
    public static void EmitRxPropAttributeDefinition(GeneratorExecutionContext context)
    {
        AddSimpleAttribute(context, Attributes.RX_PROP);
    }

    private static void AddSimpleAttribute(GeneratorExecutionContext context, string attributeName, string customBody = "")
    {
        var attribute = $"{attributeName}Attribute";
        var code = CreateSimpleAttributeCode(context, attribute, customBody);
        CreateCodeFile(context, attribute, code);
    }

    private static string CreateSimpleAttributeCode(GeneratorExecutionContext context, string attribute, string customBody = "")
    {

        const string DEFAULT_BODY = "{}";
        var requestedBody = customBody.Trim();

        if (requestedBody.Length == 0)
        {
            requestedBody = DEFAULT_BODY;
        }

        if (!requestedBody.StartsWith("{") || !requestedBody.EndsWith("}")) {
            context.ReportDiagnostic(Diagnostic.Create(Diagnostics.InvalidAttributeBody, Location.None));
            requestedBody = $$"""
                              {/*
                               INVALID BODY REQUESTED
                               {{requestedBody}} 
                               */}";
                              """;
        }

        var body = requestedBody != DEFAULT_BODY ? $"\n{requestedBody}\n" : requestedBody;
        
        var code = $$"""
                     using System;

                     namespace LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.Generators
                     {
                         [AttributeUsage(AttributeTargets.Delegate)]
                         internal sealed class {{attribute}} : Attribute {{body}}
                     }
                     """;
        return code;
    }


    private static void CreateCodeFile(GeneratorExecutionContext context, string fileName, string code)
    {
        context.AddSource(fileName, SourceText.From(code, Encoding.UTF8));

    }
}