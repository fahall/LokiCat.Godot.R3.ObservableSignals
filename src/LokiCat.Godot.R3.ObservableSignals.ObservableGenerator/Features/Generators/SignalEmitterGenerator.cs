using System.Linq;

namespace LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.Generators;

internal class SignalEmitterGenerator
{
    internal static string GetEmitCall(string signalName, int parameterCount)
    {
        var emitCall = parameterCount switch
        {
            0 => GetCustomEmitCall(signalName),
            1 => GetCustomEmitCall(signalName, "value!"),
            _ => GetCustomEmitCall(signalName, string.Join(", ", Enumerable.Range(1, parameterCount).Select(i => $"value.Item{i}"))),
        };

        return $"{emitCall};";
    }

    internal static string GetCustomEmitCall(string signalName, string? valueCode = null)
    {
        return valueCode is not null ?
            $"EmitSignal(nameof({signalName}), {valueCode});" 
            : $"EmitSignal(nameof({signalName}));";
    }

}