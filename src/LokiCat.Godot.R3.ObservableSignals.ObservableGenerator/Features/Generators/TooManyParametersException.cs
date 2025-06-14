#nullable disable
using System;

namespace LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.Generators;

internal class TooManyParametersException : Exception {
    public int Count { get; }
    public int Max { get; }

    public TooManyParametersException(int count, int max,  string message) : base(message)
    {
        Count = count;
        Max = max;
    }
}