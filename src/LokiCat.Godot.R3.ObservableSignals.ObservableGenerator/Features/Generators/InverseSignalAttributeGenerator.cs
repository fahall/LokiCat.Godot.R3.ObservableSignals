using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.Generators;

public class InverseSignalAttributeGenerator
{
    // Injects a delegate‐level InverseSignalAttribute so no separate DLL is required
    public static void EmitInverseSignalAttributeDefinition(GeneratorExecutionContext context)
    {
        const string code = @"
using System;

namespace LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.Generators
{
    [AttributeUsage(AttributeTargets.Delegate)]
    internal sealed class InverseSignalAttribute : Attribute
    {
        public string OriginalName { get; }
        public InverseSignalAttribute(string originalName) => OriginalName = originalName;
    }
}
";
        context.AddSource("InverseSignalAttribute.g.cs", SourceText.From(code, Encoding.UTF8));
    }
}