# LokiCat.Godot.R3.ObservableSignals

**R3-compatible source generator for turning `[RxSignal]`-annotated observables and `[Signal]`-annotated delegates in Godot C# into fully reactive Godot signals and cached `Observable<T>` properties.**

This package eliminates boilerplate when exposing Godot signals through R3 observables.  
It provides two distinct but complementary features:
- **[RxSignal]** — Turn your private observable fields into real Godot signals and clean public properties.
- **[Signal]** — Automatically wrap built-in and custom Godot signals into Observables.

> 📢 **Important:**
> - `[RxSignal]`: `_onJump` ➔ `OnJump` observable ➔ emits `Jump` Godot signal.
> - `[Signal]`: `JumpEventHandler` delegate ➔ `Jump` Godot signal ➔ exposes `OnJump` observable.

| Attribute | Field | Godot Signal | Observable |
|:---|:---|:---|:---|
| `[RxSignal]` | `_onJump` | `Jump` | `OnJump` |
| `[Signal]` | `JumpEventHandler` | `Jump` | `OnJump` |

---

## 🚀 Quick Start

### To expose a custom signal:

```csharp
[RxSignal]
private Subject<Unit> _onJump = new();

// Access OnJump and subscribe
OnJump.Subscribe(_ => GD.Print("Jumped!"));

// Fire the signal manually
_onJump.OnNext(Unit.Default);
```

✅ The Godot editor shows a `Jump` signal, auto-wired.

### To wrap a built-in or manual signal:

```csharp
[Signal]
public delegate void JumpEventHandler();

// Automatically exposes OnJump observable
OnJump.Subscribe(_ => GD.Print("Built-in Jumped!"));
```

✅ The `Jump` Godot signal emits and is observed reactively.

---

## ✨ Features

- **[RxSignal]**
  - Detects `[RxSignal]`-annotated observable fields.
  - Generates matching `[Signal]` Godot delegates (like `JumpEventHandler`).
  - Exposes a lazily connected public `Observable<T>` property (`OnJump`).

- **[Signal]**
  - Detects `[Signal]`-annotated delegates.
  - Wraps Godot signals as reactive `Observable<T>` properties (`OnJump`).

- Full R3 compatibility.
- Full Godot Editor and visual signal connection support.
- Fast incremental source generation.

---

## 📦 Installation

```sh
dotnet add package LokiCat.Godot.R3.ObservableSignals
```

---

## ✅ Supported Signal Forms

| Observable Type                     | Generated Signal and Emission |
|:------------------------------------ |:-------------------------------|
| `Observable<Unit>`                  | `EmitSignal("SignalName")`     |
| `Observable<T>`                     | `EmitSignal("SignalName", T)`  |
| `Observable<(T1, T2)>`              | `EmitSignal("SignalName", T1, T2)` |
| ... up to 5 arguments               | `EmitSignal("SignalName", T1, T2, ..., T5)` |

> Signals with more than 5 parameters are not supported and will trigger a generator warning.

---

## ⚡ Best Practices

- Use `[RxSignal]` for custom gameplay-driven events you control.
- Use `[Signal]` for observing existing Godot signals.
- Always prefix `[RxSignal]` fields with `_on`.
- Compose filtered observables manually (`.Where`, `.Throttle`, etc.).

---

## 🚨 Warnings

- `[RxSignal]` fields must be `Observable<T>` — otherwise, warning `RXSG0002`.
- `[Signal]` delegates over 5 parameters trigger warning `SIGOBS001`.
- Signal names are sanitized to valid C# identifiers.

---

# 📜 License

MIT License.

---

# 💡 Bonus Tip

Pair this generator with Chickensoft, R3, Godot, and other LokiCat Godot/.NET packages to build fully reactive, signal-driven gameplay systems that are easy to extend, test, and maintain.
