# LokiCat.Godot.R3.ObservableSignals

**A source generator that turns `[Signal]`-annotated delegates into fully reactive R3 `Observable<T>` properties.**  
Supports zero to five signal arguments, automatic `EmitSignal(...)` wiring, and full Godot editor compatibility.

---

## ✨ What It Does

This generator lets you define a signal once using Godot's `[Signal]` attribute:

```csharp
[Signal]
public delegate void JumpEventHandler();
```

💥 And automatically provides:

- ✅ A backing `Subject<T>` (`_onJump`)
- ✅ A public observable property (`OnJump`)
- ✅ A lazy `.Subscribe(...)` that calls `EmitSignal(...)` with proper arguments

No need to write `Connect()`, `EmitSignal()`, or observable plumbing by hand.

---

## 🚀 Quick Start

### ✅ Define your signal:

```csharp
[Signal]
public delegate void DamageTakenEventHandler(int amount);
```

### ✅ Emit it in your code:

```csharp
_onDamageTaken.OnNext(42);
```

### ✅ Observe it reactively:

```csharp
OnDamageTaken.Subscribe(amount => GD.Print($"Took {amount} damage!"));
```

✅ The Godot editor will show a `DamageTaken` signal.  
✅ R3 observers will get updates when the signal fires.

---

## 🧠 How It Works

For this:

```csharp
[Signal]
public delegate void HitEventHandler(Vector3 point, Node target);
```

The generator emits:

```csharp
private readonly Subject<(Vector3, Node)> _onHit = new();
private bool _hitConnected;

public Observable<(Vector3, Node)> OnHit {
  get {
    if (!_hitConnected) {
      _hitConnected = true;
      _onHit.Subscribe(value =>
        EmitSignal(nameof(Hit), value.Item1, value.Item2)
      ).AddTo(this);
    }
    return _onHit;
  }
}
```

✅ One subject powers both `EmitSignal(...)` and the reactive pipeline.

---

## 📦 Installation

```sh
dotnet add package LokiCat.Godot.R3.ObservableSignals
```

Make sure your consumer project defines:

```csharp
public readonly struct Unit {} // in namespace R3
```

(Only needed for 0-arg signals.)

---

## ✅ Supported Signal Signatures

| Delegate Type                        | Observable Type                     | Emission |
|-------------------------------------|-------------------------------------|----------|
| `delegate void JumpEventHandler()`  | `Observable<Unit>`                  | `EmitSignal("Jump")` |
| `delegate void DamageEventHandler(int dmg)` | `Observable<int>`           | `EmitSignal("Damage", dmg)` |
| `delegate void HitEventHandler(Vector3, Node)` | `Observable<(Vector3, Node)>` | `EmitSignal("Hit", ...)` |

Up to **5 arguments** supported. More than 5 triggers a warning.

---

## 🚨 Generator Warnings

| ID        | Reason |
|-----------|--------|
| `SIGOBS001` | Signal delegate has more than 5 parameters |
| `SIGOBS002` | Missing `R3.Unit` type for 0-arg signals |
| `SIGOBS003` | `EmitSignal("X")` is called manually, but will not emit to OnX |

---

## ⚡ Best Practices

- Declare signals once with `[Signal]`
- Emit signals using `_onX.OnNext(...)` — not `EmitSignal(...)`
- Observe using `OnX.Subscribe(...)`
- Use `.Where`, `.Throttle`, `.TakeUntil(...)` for filtered pipelines
- Expose `Observable<T>`s in interfaces to keep logic clean

---

## 💡 Advanced

If you manually call `EmitSignal(...)`, observers on `OnX` **will not** receive anything.  
Only `_onX.OnNext(...)` propagates to both `EmitSignal(...)` and the observable.

---

## 📜 License

MIT License