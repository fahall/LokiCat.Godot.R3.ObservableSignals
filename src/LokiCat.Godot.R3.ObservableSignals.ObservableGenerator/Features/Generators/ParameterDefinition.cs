#nullable disable
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.Generators;

internal record ParameterDefinition
{
    public string AggregateType { get; }
    public int Count { get; }
    public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }
    public ParameterDefinition(DelegateDeclarationSyntax delegateDeclaration)
    {
        Parameters = delegateDeclaration.ParameterList.Parameters;
        Count = Parameters.Count;
            
            
        AggregateType = Count switch
        {
            0 => "Unit",
            1 => $"{Parameters[0].Type}",
            2 => $"({Parameters[0].Type}, {Parameters[1].Type})",
            3 => $"({Parameters[0].Type}, {Parameters[1].Type}, {Parameters[2].Type})",
            4 => $"({Parameters[0].Type}, {Parameters[1].Type}, {Parameters[2].Type}, {Parameters[3].Type})",
            5 => $"({Parameters[0].Type}, {Parameters[1].Type}, {Parameters[2].Type}, {Parameters[3].Type}, {Parameters[4].Type})",
            _ => throw new TooManyParametersException(Parameters.Count, 5, "Too many parameters"),
        };
    }
}