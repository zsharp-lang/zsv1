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

- `ZSharp.Compiler.Core` holds what the REIs share, including `Unsupported`, the
  capability of an error that says an entrypoint doesn't support an operation, which
  `UnsupportedError` has. A dispatcher falls through only on an error with it, so a
  real failure is never replaced by a later dispatcher.
- **An error's capabilities.** What an error can say beyond why it failed is a
  capability, found with `Is<T>` as a CO's are. An error that wraps another, as one
  with where it happened would, has the capabilities of the one it wraps, so wrapping
  an unsupported error keeps it unsupported.
- `CG` and `TS` are plain instance classes and know nothing about a current
  instance. Dispatchers receive the instances they use.
- `CG` and `TS` know nothing about LIR.
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
  There is no typed cast *from*: a runtime descriptor speaks for a value, as its type,
  and on the receiving side that is the target type itself, which `CG.Direct` already
  asks through `ICTImplicitCastFrom`.

### Matching

`Matching` is an REI of its own, since matching isn't part of the general operations
of code: a CO can use it when it is called or set, but doesn't have to. HIR has no
node for matching arguments, which is why it isn't CG's.

| Entrypoint | Question |
|------------|----------|
| `Pattern`, through `Match(value, pattern)` | What condition, as a CO, says whether a value matches a pattern, as `value is of T`? |
| `Parameter`, through `Match(parameter, arguments)` | What value does a parameter take for a call, from its arguments? |

Its static dispatcher asks what the COs say of themselves: a parameter's
`ICTParameter`, then a value's `ICTMatch`, then its runtime descriptor's `IRTMatch`.
A match gives the condition as a CO, with nothing around it: what else a match does,
as running other code when it fails, is an operation of that CO. `std:traits/dynamic`
replaces `Pattern` to match a trait as the code runs.

`ArgumentStream` is what a parameter takes from: the next positional argument, the
one with its name, or `Rest()`, every argument left, which a parameter that takes the
rest uses.

### Overloading

`Overloading` chooses between the candidates one object stands for. `Group` makes that
object from the candidates, as the members of one name are, and a single candidate
stands for itself; `Apply` applies an operation to each candidate and chooses whose
result is the result; `IsMoreSpecific` decides between two that both apply. Nothing
names the group object itself: the static dispatcher's `Group` gives an
`OverloadGroup`, and without it more than one candidate is unsupported. A candidate
says how specific it is through `ISpecific`, which `Parameters.AreAsSpecificAs`, a
comparison of what each accepts, helps a candidate with parameters answer.

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
