# REI: Replacable Entrypoints Interface

An REI is a set of replaceable operations. CG, TS, IR, HIR -> MIR and overloading are
all REIs.

- An REI specifies only the signature of its operations, never their behaviour.
  Behaviour can always be changed, overridden or extended by replacing an
  entrypoint.
- An extension replaces an entrypoint and decides for itself when to delegate.
  It may check its own concepts first and delegate upwards, or the reverse. No
  fallback order is enforced.
- Default entrypoints return a known error type, distinct from other errors, so
  "unsupported" can be matched against a real failure.
- A concrete implementation is used directly only by those who know it. For the
  HIR -> MIR loader, that is the CLI and the CLR extension. Everyone else uses
  the REI.

### CG and TS

- `ZSharp.Compiler.Core` holds what the REIs share, including the known
  `UnsupportedError`. A dispatcher falls through only on that error, so a real
  failure is never replaced by a later dispatcher.
- `CG` and `TS` are plain instance classes and know nothing about a current
  instance. Dispatchers receive the instances they use.
- `CG` and `TS` know nothing about LIR, so `MatchResult` carries no instructions.
- `TS` names no types. An extension or the runtime exposes the ones it wants.
- `CG.Typed` asks `TS.IsAssignableTo` whether a cast is a no-op, rather than
  comparing COs or asking a separate reflection service.
- An REI is called through `CG`'s methods, as `cg.Member(object, name)`, which can
  overload, not through the delegate behind them, as `cg.GetMemberByName`.
- **The order of an implicit cast.** Each dispatcher asks the dispatchers before it
  first, so what was applied earlier is asked first. Of a dispatcher's own, the value
  comes before the type it goes to:
  - `CG.Direct` asks the value's `ICTImplicitCastTo`, then the type's
    `ICTImplicitCastFrom`;
  - `CG.Typed` asks the value's runtime descriptor, then whether the value's type is
    assignable to the type, which makes the cast a no-op.

  So a value that knows how it converts wins over a type that knows how to take it.

### IR

`IR` is the MIR -> LIR REI. Every entrypoint takes an optional target. Its
entrypoints ask different questions, so they stay separate:

| Entrypoint | Question |
|------------|----------|
| `CompileExpression`, `CompileStatement` | What code does this produce where it is used? |
| `CompileType` | What goes in a type slot? It need not be the definition: a class may define a `Structure` and have a `Reference` to it as its type. |
| `CompileDefinition<T>(object)` | What entity does this define? It places nothing, so a reference can reach an entity before its owner places it. Imported objects have none. |
| `CompileDefinition(object, owner)` | Place this in that owner. The owner need not know what kinds its members are. It returns `null`, or why it was not placed. Being unsupported means there is nothing to place. |
| `CompileReference<T>` | What does code use to refer to this? It may be something the object does not define, such as a handle. |

`zsv1`'s `CompileTypeAs<T>` is not carried over. It existed to require a specific
class-like type, such as a base class, and LIR has no base classes.

### HIR -> MIR

`HIRLoader` is the REI, with three entrypoints, one per concept. Extensions add
their own REI through the same mechanism: `HIRCodeLoader` does this for
`Expression` and `Statement`.

`MappedLoader` and `MappedCodeLoader` are the concrete implementations.
Transformations are registered by exact type. Registering replaces any existing
loader and returns it, so the new one can decide whether to delegate.

The concrete loader may cache Definitions so that one Definition gives one CO,
which can then own its LIR entity. This is the concrete loader's behaviour, not
part of the REI, and an extension may skip it. Instance and Type simply delegate
to the registered loader.
