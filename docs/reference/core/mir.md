# MIR

`pkg:zsharp/mir`: the core of MIR, the language of interactions. It is one interface.

## CompilerObject

```csharp
public interface CompilerObject
{
    public sealed T? As<T>() where T : class
        => Is<T>(out var result) ? result : null;

    public bool Is<T>([NotNullWhen(true)] out T? result) where T : class
        => (result = this as T) is not null;
}
```

A CO is opaque. What it can do, it does by implementing capabilities, interfaces of the
operations it takes part in, which are reached through `Is<T>`. A CO that stands for
another overrides `Is<T>` to forward them.

## Rules

- Code never inspects a CO: it applies an operation and handles the result. Only a
  dispatcher may ask what a CO implements.
- `Is<T>` is only ever used with an interface.
- COs are never compared by identity.
- A CO may hold anything, as long as how it interacts with other COs doesn't depend on
  another layer, unless the CO is scoped to that layer.

## Where the operations are

MIR's operations are extensions, each an REI of its own, reached through `core:compiler`
and typed by the libraries of `core:zsharp/mir/...`:

| Library | Has |
|---|---|
| `core:zsharp/mir/core` | What the REIs share: `UnsupportedError`, and errors' capabilities |
| `core:zsharp/mir/cg` | `CG`, its delegates, and `Argument` |
| `core:zsharp/mir/cg/direct` | The `ICT` capabilities, decided as code is compiled |
| `core:zsharp/mir/cg/typed` | The `IRT` capabilities, decided by a value's type |
| `core:zsharp/mir/ts` | `TS`, `ITyped`, `IAssignableTo`, `IAssignableFrom` |
| `core:zsharp/mir/overloading` | `Overloading`, `OverloadGroup`, `ISpecific` |
| `core:zsharp/mir/matching`, `/static` | `Matching`, `ArgumentStream`, `ICTBind`, `ICTMatch`, `IRTMatch` |
| `core:zsharp/m2l` | `IR`, and the `ICompileIR` capabilities |

[The toolchain's REIs](../../learn/custom-dispatchers/toolchain-reis.md) lists their
entrypoints.
