using System.Linq;

namespace LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.Generators;

internal class SignalEmitterGenerator
{
    internal static string GetEmitCall(string signalName, ParameterDefinition parameters)
    {
        var emitCall = parameters.Count switch
        {
            0 => $"EmitSignal(nameof({signalName}))",
            1 => $"EmitSignal(nameof({signalName}), value!)",
            _ => $"EmitSignal(nameof({signalName}), {string.Join(", ", Enumerable.Range(1, parameters.Count).Select(i => $"value.Item{i}"))})",
        };

        return emitCall;
    }

}