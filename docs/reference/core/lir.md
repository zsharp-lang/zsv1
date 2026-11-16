# LIR

`pkg:zsharp/lir`: core LIR, the language every platform reads. [LIR](../../learn/custom-platforms/lir.md)
in the platforms guide explains each concept.

## Definitions

| Type | Is |
|---|---|
| `Module` | The root of a graph: `Functions`, `Globals`, `Structures`, and its initializer. |
| `Function` | A `Signature` and a `Body`, `Code`; with no body, one a platform provides. |
| `Signature` | `Parameters`, and a `ReturnType` if it returns something. |
| `Parameter` | A parameter of a signature. |
| `Global` | A module's variable. |
| `Structure` | A record of `Fields`. |
| `Field` | A field of a structure. |
| `Initialized` | The capability of a definition with code that runs once before it is used. |
| `EntryPoint` | Metadata of a module's entry point. |
| `Metadata` | What a definition carries for a platform to interpret, or not. |
| `KeptAnnotation` | An annotation kept onto what a definition lowers to. |

## Types

| Type | Is |
|---|---|
| `Type` | What goes in a type slot. |
| `Reference` | A reference to a type, whose values are held by reference. |
| `FunctionType` | The type of a function of a signature. |
| `GenericInstance` | A generic definition given its type arguments, which the platform makes. |
| handles | A type or a function the platform has, with no LIR definition. |

## Code

| Kind | Nodes |
|---|---|
| Containers | `Code`, `Block`, `Local`, `Variable` |
| Statements | `Evaluate`, `If`, `Loop`, `Break`, `Continue`, `Return`, `SetLocal`, `SetGlobal`, `SetField`, `Store` |
| Expressions | `Constant`, `Call`, `CallIndirect`, `GetLocal`, `GetGlobal`, `GetField`, `GetFunction`, `New`, `Allocate`, `Load`, `Sequence` |
| Callables | `ICallable`: a `Function`, a function handle, a `MethodInstance`, an `InstanceMember` |

## Rules

- Core LIR refers only to core LIR. A dialect's LIR refers to its own dialect or to core
  LIR.
- LIR has no type system and names no type: every type that isn't a structure is a handle.
- Arithmetic, comparison and casts are calls of function handles.
- Control flow is structured.
