#nullable disable
using System;
using Microsoft.CodeAnalysis;

namespace LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.Generators;

internal static class Diagnostics
{
    internal const string CATEGORY = "SignalObservables";

    public static readonly DiagnosticDescriptor MissingUnitType = new(
        id: "SIGOBS001",
        title: "Missing R3.Unit type",
        messageFormat:
        "The R3.Unit type must be defined for zero-parameter signals. Define 'public readonly struct Unit' in namespace 'R3'.",
        category: CATEGORY,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor TooManyParameters = new(
        id: "SIGOBS002",
        title: "Signal has too many parameters",
        messageFormat: "[Signal] delegate '{0}' has more than {1} parameters — observable not generated",
        category: CATEGORY,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor MultipleInverseSignals = new(
        id: "SIGOBS005",
        title: "Multiple inverse signals",
        messageFormat: "Multiple [InverseSignal] declarations target the same signal '{0}'. Only one is allowed.",
        category: CATEGORY,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor EmitsSignalManually = new(
        id: "SIGOBS003",
        title: "Manual EmitSignal bypasses observable",
        messageFormat:
        "EmitSignal(\"{0}\") is called directly, but will not emit to On{0} unless you also call _on{0}.OnNext(...)",
        category: CATEGORY,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor InverseOnRxSignal = new(
        id: "SIGOBS004",
        title: "Signal has conflicting attributes",
        messageFormat:
        "[Signal] delegate '{0}' cannot have [InverseSignal] if it already has [RxProperty] or [RxObservable]",
        category: CATEGORY,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor ConflictingRxAttributes = new(
        id: "SIGOBS009",
        title: "Conflicting signal attributes",
        messageFormat: "[Signal] delegate '{0}' cannot have both [RxProperty] and [RxObservable]. Use only one.",
        category: CATEGORY,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor InvalidAttributeBody = new(
        id: "SIGOBS011",
        title: "Invalid Attribute Body",
        messageFormat: "The attribute body must be a valid C# class body",
        category: "SignalObservablesAttributes",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor GeneratorRunning = new(
        id: "SOEG0001",
        title: $"{nameof(SignalObservableExtensionGenerator)} Running",
        messageFormat: $"{nameof(SignalObservableExtensionGenerator)} is active in this compilation",
        category: nameof(SignalObservableExtensionGenerator),
        DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor InvalidDelegateName = new(
        "SIGOBS998",
        "Malformed delegate name",
        "Delegate '{0}' does not end with 'EventHandler'",
        Diagnostics.CATEGORY,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor GeneratorException(Exception ex)
    {
        return new DiagnosticDescriptor(
            "SIGOBS999",
            "Generator Exception",
            $"Unhandled exception: {ex.GetType().Name} - {ex.Message}",
            Diagnostics.CATEGORY,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
    }
}