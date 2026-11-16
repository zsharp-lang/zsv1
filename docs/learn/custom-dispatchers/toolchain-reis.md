# The toolchain's REIs

Writing a dispatcher starts with knowing which operation to replace. This page lists every
REI the toolchain has, with what each of its entrypoints asks, as a map to come back to. Each is reached from Z# through
`core:compiler`, and its delegates' types through the `core:zsharp/...` library of its
layer.

## CG: what code does with objects

`cg`, of `core:zsharp/mir/cg`.

| Entrypoint | Asks |
|---|---|
| `Call(callee, arguments)` | What is this called with these arguments? |
| `Get(object)`, `Set(object, value)` | What is this read as, and assigned? |
| `GetMember`, `SetMember` | What is this member, by name or by position, and assigned? |
| `GetIndex`, `SetIndex` | What is this indexed with these arguments, and assigned? |
| `Cast(object, type)` | What is this converted to the type explicitly? |
| `ImplicitCast(object, type)` | What is this where the type is expected? |
| `Declare(definition)` | What is code's declaration of this? |

Installed: `CG.Direct`, which asks the objects' `ICT` capabilities, of the value first
and then of the type; `CG.Typed` on it, which asks a value's type, its runtime
descriptor, through `IRT` capabilities, and makes a cast a no-op when the value's type is
assignable to the type; then the trait dispatchers.

## TS: types

`ts`, of `core:zsharp/mir/ts`.

| Entrypoint | Asks |
|---|---|
| `TypeOf(object)` | What is this object's type? |
| `IsAssignableTo(target, source)` | Does a value of `source` go where `target` is expected? |

Installed: `TS.Static`, which asks the source's `IAssignableTo`, then the target's
`IAssignableFrom`, and an object's type of its `ITyped`.

## Matching

`matching`, of `core:zsharp/mir/matching`.

| Entrypoint | Asks |
|---|---|
| `BindParameter(parameter, arguments)` | What value does a parameter take from a call's arguments? |
| `MatchPattern(value, pattern)` | What condition says the value matches the pattern? |

Installed: the static dispatcher, which asks a parameter's `ICTBind`, then a value's
`ICTMatch`, then its runtime descriptor's `IRTMatch`; and `std:traits/dynamic`, which
matches a trait as the code runs.

## Overloading

`overloading`, of `core:zsharp/mir/overloading`.

| Entrypoint | Asks |
|---|---|
| `Group(candidates)` | What one object stands for these candidates? |
| `Apply(candidates, operation)` | Which candidate's result is the result of applying an operation to them? |
| `IsMoreSpecific(candidate, other)` | Of two that both apply, is this one more specific? |

Installed: the static dispatcher, whose group is an `OverloadGroup`, and which asks a
candidate how specific it is through `ISpecific`.

## IR: lowering

`ir`, of `core:zsharp/m2l`. Every entrypoint takes the target being lowered for.

| Entrypoint | Asks |
|---|---|
| `CompileExpression`, `CompileStatement` | What code does this produce where it is used? |
| `CompileType` | What goes in a type slot? |
| `CompileDefinition(object, owner)` | Place this in that owner. |
| `CompileDefinition[T](object)` | What entity does this define? It places nothing. |
| `CompileReference[T]` | What does code use to refer to this? |

Installed: the static dispatcher, which asks the object's `ICompileIR` capabilities.

## Loading: HIR to MIR

`hirLoader` and `hirCodeLoader`, of `core:zsharp/h2m`.

| Entrypoint | Loads |
|---|---|
| `LoadDefinition` | a definition, once: one node, one CO |
| `LoadIdentity` | what has identity but isn't a definition, as a module or an import, once |
| `LoadInstance` | an instance, where code uses it |
| `LoadType` | a type, where a type is expected |
| `hirCodeLoader`'s `LoadExpression`, `LoadStatement` | code |

Installed: `mappedLoader` and `mappedCodeLoader`, which choose by the node's exact type
what is registered for it, as `mappedLoader.Instances.Register[T](load)`. Registering
replaces what was registered and gives it back, so the new loader decides whether to
delegate.

## Running and reification

- `HIRRunner` runs HIR as it is compiled, as a document's statements and `ct` do. Its
  mapped implementation runs a statement by what is registered for its type, which is how
  an extension adds a statement that acts as it is compiled.
- `hirReifier` turns code into code that builds it: what a cast definition in code uses.
  Its entrypoints reify an expression, a statement, a pattern and whole code, and encode a
  value.

## The frontend's

The AST compiler, `core:zsharp`'s `compiler`, has the same shape: three endpoints,
`Expression`, `Statement` and `Definition`, each replaceable, and a `Fork()` that changes
them for a part of the code only. [Custom syntax](../custom-syntax/syntax.md) goes through
it.
