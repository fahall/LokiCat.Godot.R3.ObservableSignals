# LokiCat.Godot.R3.ObservableSignals

**A source generator that turns `[Signal]`-annotated delegates into fully reactive R3 `Observable<T>` properties.**
Supports zero to five signal arguments, `[RxProperty]` bindings, `[InverseSignal]` auto-generation, and full Godot editor compatibility.

---

## ✨ What It Does

This generator lets you define signals with `[Signal]` **in combination with** `[RxObservable]` or `[RxProperty]`:

```csharp
[Signal]
[RxObservable]
public delegate void JumpEventHandler();
```

💥 And automatically provides:

* ✅ A backing `Subject<T>` or `RxVar<T>`
* ✅ A public observable or reactive property (`OnJump`, `IsDead`, etc.)
* ✅ Lazy `.Subscribe(...)` wiring that calls `EmitSignal(...)` automatically

No need to write `Connect()`, `EmitSignal()`, or observable plumbing by hand.

---

## 🚀 Quick Start

### ✅ Define a signal:

```csharp
[Signal]
[RxObservable]
public delegate void DamageTakenEventHandler(int amount);
```

### ✅ Emit it:

```csharp
_onDamageTaken.OnNext(42);
```

### ✅ Observe it:

```csharp
OnDamageTaken.Subscribe(amount => GD.Print($"Took {amount} damage!"));
```

🚫 `[Signal]` alone will not generate code. You must pair it with `[RxObservable]` or `[RxProperty]`.

---

## 🔄 RxProperty Support

Use `[RxProperty]` to generate a reactive `IRxProp<T>` that syncs with a signal:

```csharp
[Signal]
[RxProperty]
public delegate void IsDeadEventHandler(bool isDead);
```

Generates:

```csharp
public IRxProp<bool> IsDead => ...;
```

### 🧠 Naming Rules

The generated property name:

* Always starts with `Is`
* Strips a leading `Is` from the delegate name if it exists as a standalone prefix
* Strips the `EventHandler` suffix

Examples:

| Delegate Name              | Generated Property Name |
| -------------------------- | ----------------------- |
| `IsDeadEventHandler`       | `IsDead`                |
| `DeadEventHandler`         | `IsDead`                |
| `IslandEventHandler`       | `IsIsland`              |
| `IsIslandEventHandler`     | `IsIsland`              |
| `IslandTimeEventHandler`   | `IsIslandTime`          |
| `IsIslandTimeEventHandler` | `IsIslandTime`          |

---

## 🔁 Inverse Signal Support

Generate inverse signals from a single source:

```csharp
[Signal]
[RxObservable]
public delegate void IsVisibleEventHandler(bool value);

// inferred from name:
[Signal]
[InverseSignal]
public delegate void IsInvisibleEventHandler(bool value);

// explicit source:
[Signal]
[InverseSignal(nameof(IsVisibleEventHandler))]
public delegate void HiddenEventHandler(bool value);
```

✅ Emits `EmitSignal("Hidden", !value)` when `OnIsVisible` is triggered.
🚫 Only valid for `bool` signals
🚫 Cannot be used with `[RxProperty]`

---

## ✅ Supported Signal Signatures

| Delegate Type                                  | Observable Type                  | Emission                      |
| ---------------------------------------------- | -------------------------------- | ----------------------------- |
| `delegate void JumpEventHandler()`             | `Observable<Unit>`               | `EmitSignal("Jump")`          |
| `delegate void DamageEventHandler(int)`        | `Observable<int>`                | `EmitSignal("Damage", dmg)`   |
| `delegate void HitEventHandler(Vector3, Node)` | `Observable<(Vector3, Node)>`    | `EmitSignal("Hit", ...)`      |
| `delegate void IsDeadEventHandler(bool)`       | `IRxProp<bool>` (`[RxProperty]`) | `EmitSignal("IsDead", value)` |

Up to **5 parameters** are supported.

---

## 🚨 Generator Diagnostics

| ID          | Reason                                                         |
| ----------- | -------------------------------------------------------------- |
| `SIGOBS001` | Missing `R3.Unit` for zero-arg signals                         |
| `SIGOBS002` | Signal delegate has more than 5 parameters                     |
| `SIGOBS003` | Manual `EmitSignal("X")` bypasses observable                   |
| `SIGOBS004` | `[InverseSignal]` used with `[RxProperty]` or `[RxObservable]` |
| `SIGOBS005` | More than one `[InverseSignal]` targets a single signal        |
| `SIGOBS009` | `[RxProperty]` and `[RxObservable]` used together              |
| `SIGOBS010` | Could not resolve `nameof(...)` in `[InverseSignal]`           |
| `SIGOBS011` | Malformed attribute body in generated code                     |
| `SOEG0001`  | Generator is running (informational)                           |

---

## 💡 Best Practices

* ✅ Use `_onX.OnNext(...)` to emit signals
* ✅ Subscribe using `OnX.Subscribe(...)`
* ⚠️ Do not call `EmitSignal(...)` manually — it will not notify observers
* 🧠 Prefer `[RxProperty]` for stateful values
* 🔁 Use `[InverseSignal]` for UI toggles, state flips, or binding inverses

---

## 📦 Installation

```sh
dotnet add package LokiCat.Godot.R3.ObservableSignals
```

Also define:

```csharp
namespace R3 {
  public readonly struct Unit {} // required for 0-arg signals
}
```

---

## 📜 License

MIT License
