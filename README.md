# LokiCat.Godot.R3.ObservableSignals

**R3-compatible source generator for turning `[RxSignal]`-annotated observables in Godot C# into fully reactive Godot signals and cached `Observable<T>` properties.**

This package eliminates boilerplate when exposing Godot signals through R3 observables.  
It generates real `[Signal]` Godot events, connects them to your observable streams, and automatically emits signals when your code pushes data.

---

## ✨ Features

- Automatically detects `[RxSignal]`-annotated observable fields (e.g., `Subject<T>`) in Godot C# partial classes
- Generates matching `[Signal]` Godot delegate declarations automatically
- Generates a `ConnectGodotSignals()` method for wiring Observables to Godot signals
- Supports 0 to 5 parameters in emitted signals
- No runtime `Connect` needed
- Full R3 compatibility for reactive pipelines and disposal
- Clean, manual control over when signals are fired via `.OnNext()`
- Fully supports Godot Editor and visual signal connections
- Supports `Subject<T>`, `ReplaySubject<T>`, `BehaviorSubject<T>`, `ReactiveProperty<T>`, and any other `Observable<T>`-derived types

---

## 👍 Why This is Useful

Before, exposing Godot signals in a reactive way required manual wiring, manual `Connect(...)` calls, and custom observable creation:

### Old Way (Manual)

```csharp
[Signal]
public delegate void PressedEventHandler(BaseButton button);

private Observable<BaseButton> _onPressed;
public Observable<BaseButton> OnPressed => _onPressed ??= Observable.Create<BaseButton>(observer => {
    var callable = Callable.From((BaseButton button) => observer.OnNext(button));
    Connect(nameof(Pressed), callable);
    return Disposable.Empty;
});
```

### New Way (With This Package)

```csharp
[RxSignal]
private readonly Subject<BaseButton> _onButtonPressed = new();

public Observable<BaseButton> OnButtonPressed => _onButtonPressed;

public override void _Ready()
{
    ConnectGodotSignals();
}

public void PressButton(BaseButton button)
{
    _onButtonPressed.OnNext(button);
}
```

✅ Much less code.
✅ No manual `Connect()` needed.
✅ Full control when you fire signals with `.OnNext()`.
✅ Safer, reactive, and easier to test.

---

## 📦 Installation

1. Add this NuGet package to your project:

```sh
dotnet add package LokiCat.Godot.R3.ObservableSignals
```

2. Define your interfaces and classes:

### Interface

```csharp
public partial interface IPauseMenu
{
    Observable<Unit> OnMainMenuSelected { get; }
}
```

### Class Implementation

```csharp
public partial class PauseMenu : Control, IPauseMenu
{
    [RxSignal]
    private readonly Subject<Unit> _onMainMenuSelected = new();

    public Observable<Unit> OnMainMenuSelected => _onMainMenuSelected;

    public override void _Ready()
    {
        ConnectGodotSignals();
    }

    public void SelectMainMenu()
    {
        _onMainMenuSelected.OnNext(Unit.Default);
    }
}
```

✅ That's it. No manual Connect, no manual EmitSignal needed.

---

## ✅ Supported Signal Forms

| Observable Type                     | Generated Signal and Emission |
|:------------------------------------ |:-------------------------------|
| `Observable<Unit>`                  | `EmitSignal("SignalName")`     |
| `Observable<T>`                     | `EmitSignal("SignalName", T)`  |
| `Observable<(T1, T2)>`              | `EmitSignal("SignalName", T1, T2)` |
| ... up to 5 arguments               | `EmitSignal("SignalName", T1, T2, ..., T5)` |

> Signals with more than 5 parameters are not supported and will trigger a generator warning.

### Supported Field Types

You can annotate fields of type:
- `Subject<T>`
- `ReplaySubject<T>`
- `BehaviorSubject<T>`
- `ReactiveProperty<T>`
- Any custom type inheriting `Observable<T>` that supports emitting values

---

## 🧠 How It Works

- You define `[RxSignal]` observable fields (`Subject<T>`, `ReplaySubject<T>`, etc.)
- You expose public `Observable<T>` properties
- The generator emits:
  - A `[Signal]` Godot delegate for each signal
  - A `ConnectGodotSignals()` method that wires the Observables to `EmitSignal` calls
- When you call `.OnNext()`, it automatically fires the Godot signal and notifies any R3 subscribers.

---

## 🛠 Example: Full Class

```csharp
public partial class ButtonGroup : Control
{
    [RxSignal]
    private readonly Subject<BaseButton> _onButtonPressed = new();

    public Observable<BaseButton> OnButtonPressed => _onButtonPressed;

    public override void _Ready()
    {
        ConnectGodotSignals();
    }

    public void PressButton(BaseButton button)
    {
        _onButtonPressed.OnNext(button);
    }
}
```

⬇️ The generator automatically emits:

```csharp
[Signal]
public delegate void ButtonPressedEventHandler();

private void ConnectGodotSignals()
{
    OnButtonPressed.Subscribe(button => EmitSignal(nameof(ButtonPressed), button)).AddTo(this);
}
```

---

## 🧪 Troubleshooting

- Make sure your classes are marked `partial`
- Annotate observable fields with `[RxSignal]`
- Expose observables with public `Observable<T>` properties
- Call `ConnectGodotSignals()` in `_Ready()`
- Use `#nullable enable` if you are using nullable types in your fields or properties

---

## 📄 License

MIT License

---

## 💡 Bonus Tip

Pair this generator with Chickensoft, R3, Godot, and other LokiCat Godot/.NET packages to create fully reactive, signal-driven gameplay systems that are easy to extend, test, and maintain.

