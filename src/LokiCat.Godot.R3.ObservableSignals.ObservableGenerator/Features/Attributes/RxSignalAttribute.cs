using System;

// ReSharper disable once CheckNamespace
namespace LokiCat.Godot.R3.ObservableSignals
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class RxSignalAttribute : Attribute
    {
        public RxSignalAttribute() { }
    }
}