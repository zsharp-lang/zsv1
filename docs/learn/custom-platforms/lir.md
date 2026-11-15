# LIR

A platform has to read whatever lowering gives it. If that were Z#'s classes and
functions, every platform would need to know all of them, and every new kind of
definition. Instead, lowering gives LIR: a small language of what runs, which every
definition lowers to, so a platform reads only that.

Core LIR is resolved and closed, so a platform can handle all of it. It is minimal on
purpose: higher-level concepts, as classes, generics with constraints or named parameters,
stay in HIR, and are lowered to LIR's few concepts by their COs.

## Definitions

| Concept | What it is |
|---|---|
| `Module` | The root of every graph: functions, globals and structures, and an initializer. Modules hold no modules. |
| `Function` | A signature and code. A function with no code is one a platform provides. |
| `Signature` | Ordered parameters, and a return type if it returns something. |
| `Global` | A module's variable. |
| `Structure` | A record of fields, laid out by the platform, in field order. |
| `Initialized` | A definition with code that runs once before any of it is used, as a module's initializer, which what is lowered into it adds to. |

An entity may refer directly to one another module owns, as a function of another module.

## Types

LIR has no type system of its own. A type is what goes in a type slot:

- a `Structure`;
- a `Reference` to a type, which says the value is held by reference, not inline;
- a `FunctionType`, of a signature;
- a `GenericInstance`, a generic definition given its type arguments, which the platform
  makes, with `InstanceMember` and `MethodInstance` for its members and generic methods;
- a **handle**: a type the platform has, as `Int32`, which LIR names nothing of.

## Code

Code is a tree of expressions, evaluated in order, and structured control flow.

| Statements | Expressions |
|---|---|
| `Block`, which declares its locals | `Constant`, of a type |
| `If`, `Loop`, `Break`, `Continue`, `Return` | `Call` of anything callable, and `CallIndirect` of a function value |
| `Evaluate`, an expression as a statement | `GetLocal`, `GetGlobal`, `GetField`, `GetFunction` |
| `SetLocal`, `SetGlobal`, `SetField`, `Store` | `New`, `Allocate`, `Load`, `Sequence` |

`Break` names what it leaves and `Continue` the loop it repeats, so named loops keep their
structure. A target with only innermost breaks lowers them with flags; one that jumps,
as the CLR, jumps.

There is no arithmetic, comparison or cast in LIR: each is a call of a function handle
the platform maps, as `Int32`'s addition. Virtual dispatch isn't a concept either: it is a
structure of function references and `CallIndirect`.

## Handles

A handle stands for what has no LIR definition: a type or a function the platform has. A
type handle is a `Type`, and a function handle is an `ICallable`, with a signature, so
each goes only where its kind goes. Each platform maps a handle its own way, and a
platform should be able to map every one the standard library uses.

## Metadata

A definition carries metadata: what a platform may interpret, as a function being a
method, but needn't. A kept annotation of a definition, as one that is to be read at run
time, is carried as metadata too, `KeptAnnotation`, for the platform to write its own way.
LIR stays correct for a platform that ignores all of it.
