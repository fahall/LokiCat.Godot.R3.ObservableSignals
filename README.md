# LokiCat.Godot.R3.ObservableSignals

**R3-compatible source generator for turning ********`[Signal]`******** delegates in Godot C# into cached ********`Observable<T>`******** properties.**

This package eliminates boilerplate when wiring up Godot's `[Signal]` system to R3's reactive observables. It generates cached, lazily-connected observables for every `[Signal]` delegate in your partial Godot classes.

---

## ✨ Features

- Automatically detects `[Signal]`-annotated delegate declarations in Godot C# scripts
- Generates type-safe `Observable<T>` or `Observable<(T1, T2, ...)>` properties
- Supports 0 to 5 parameters
- Emits clean diagnostic warnings for `[Signal]` delegates with more than 5 parameters
- Requires no manual wiring or `Observable.Create` logic
- Complies with `.editorconfig` and follows your R3 usage patterns
- Output is fully compatible with `AddTo(this)` disposal and R3 pipelines

---

## 📦 Installation

1. Add this NuGet package to your project:

```sh
dotnet add package LokiCat.Godot.R3.ObservableSignals
```

2. Ensure your Godot node classes are marked `partial`, and your `[Signal]` delegates are defined inside the class:

```csharp
public partial class VisionCone2D : Node2D {
  [Signal]
  public delegate void BodyEnteredEventHandler(Node body);
}
```

3. Build your project. The generator will automatically emit:

```csharp
private Observable<Node> _onBodyEntered;
public Observable<Node> OnBodyEntered =>
  this.Signal(nameof(BodyEntered), ref _onBodyEntered);
```

You can now subscribe:

```csharp
visionCone.OnBodyEntered
  .Subscribe(body => GD.Print($"Entered: {body.Name}"))
  .AddTo(this);
```

---

## ✅ Supported Signal Forms

| Signal form                          | Generated Observable type       |
| ------------------------------------ | ------------------------------- |
| `delegate void Something()`          | `Observable<Unit>`              |
| `delegate void Something(Node)`      | `Observable<Node>`              |
| `delegate void Something(Node, int)` | `Observable<(Node, int)>`       |
| ... up to 5 args                     | `Observable<(T1, T2, ..., T5)>` |

> Signals with **6+ arguments** will trigger a diagnostic warning and be skipped.

---

## 🧠 How It Works

- The generator looks for `[Signal]` delegate declarations inside partial classes.
- For each signal, it emits a `private Observable<T>?` field and a `public Observable<T>` property.
- It uses the `Signal(...)` extension method (also emitted by the generator) to wrap `Godot.Connect(...)` with `Observable.Create(...)`.
- The generated observables are cached and lazily initialized on first access.

---

## 🛠 Example: Full Class

```csharp
public partial class ButtonGroup : Node {
  [Signal]
  public delegate void PressedEventHandler(BaseButton button);
}
```

⬇️ Generates:

```csharp
private Observable<BaseButton> _onPressed;
public Observable<BaseButton> OnPressed =>
  this.Signal(nameof(Pressed), ref _onPressed);
```

---

## 🧪 Troubleshooting

- Make sure your classes are marked `partial`
- Your `[Signal]` delegates **must be declared inside** the class
- Check `.g.cs` output in `obj/Debug/.../generated/`
- Use `#nullable enable` if you use nullable types in your `[Signal]` delegate
- You must rebuild your project to trigger generation

---

## 📄 License

MIT License

---

## 💡 Bonus Tip

Pair this generator with Chickensoft, R3, Godot, and other LokiCat Godot/.NET packages for fully reactive, event-driven game logic in idiomatic C#.

