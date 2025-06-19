using System.Text;
using LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.Generators;
using Microsoft.CodeAnalysis;

namespace LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.SyntaxHelpers;

internal static class GodotSignalUtilities
{
    internal const string GODOT_SIGNAL_SUFFIX = "EventHandler";

    internal static string GetSignalBaseName(string delegateName, string suffixToTrim = GODOT_SIGNAL_SUFFIX)
    {
        // Step 1: Remove "EventHandler" suffix if present
        var baseName = delegateName.EndsWith(suffixToTrim)
            ? delegateName[..^suffixToTrim.Length]
            : delegateName;

        return baseName;
    }
    
    
    /// <summary>
    /// Prefix name with Is unless it's already prefixed with Is.
    /// </summary>
    /// <example>IsotopeEventHandler -> IsIsotope</example>
    /// <example>IsIsotopeEventHandler -> IsIsotope</example>
    /// <example>IsDeadEventHandler -> IsDead</example>
    /// <example>DeadEventHandler -> IsDead</example>
    /// <param name="baseName"></param>
    /// <returns></returns>
    internal static string WithDedupedPrefix(this string text, string prefix)
    {
        var index = prefix.Length;
        // Step 2: Strip prefix only if it's a standalone prefix followed by an uppercase letter
        if (text.StartsWith(prefix) && text.Length >= index && char.IsUpper(text[index]))
        {
            text = text[index..];
        }

        // Step 3: Prefix with desired prefix
        return $"{prefix}{text}";
    }
    
    internal static void AppendSignalWiring(this StringBuilder text, string signalName, int parameterCount)
    {
        text.AppendLine(SignalEmitterGenerator.GetEmitCall(signalName, parameterCount));
    }
}