# Common REIs

A CO says what it does through its capabilities. What asks it is the compiler's
operations, which code that works with COs calls in turn: a call, a member, a cast, a
lowering. They are grouped in REIs, sets of replaceable entrypoints, each reached from Z#
through `core:compiler`. These are the ones a CO most often meets.

## Who asked the number

When the code of [compiler objects](compiler-objects.md) asked for `doubled`, nothing
called `NumberObject.Member` directly. Loading the member's code called `cg.Member(number,
"doubled")`, an operation of CG, and CG's installed dispatcher asked the number's
`ICTGetMember`. Running the result called `ir.CompileExpression`, an operation of IR,
whose dispatcher asked its `ICompileIRExpression`. Every capability is answered for an
operation like that, and code that works with COs, as a meta-compiler's or a loader's,
calls the operations, never the capabilities.

## CG: code generation

`cg` is what code does with objects:

| Operation | What it does |
|---|---|
| `cg.Call(callee, arguments)` | calls an object |
| `cg.Get(object)`, `cg.Set(object, value)` | reads and assigns it |
| `cg.Member(object, name)`, and setting one | reaches a member |
| `cg.Index(object, arguments)`, and setting one | indexes it |
| `cg.Cast(object, type)`, `cg.ImplicitCast(object, type)` | converts it to a type, explicitly or implicitly |
| `cg.Declare(definition)` | declares what code defines |

Each gives the object it made, which is the code of what it did, or an error.

## TS: types

`ts` is what types are:

- `ts.TypeOf(object)` gives an object's type.
- `ts.IsAssignableTo(target, source)` says whether a value of `source` goes where
  `target` is expected. The source can say where it goes, `IAssignableTo`, and the target
  what it takes, `IAssignableFrom`.

TS names no types: what `Int32` or a class is, is the CO of whoever defines it.

## Matching and overloading

- `matching.Bind(parameter, arguments)` gives the value a parameter takes from a call's
  arguments, and `matching.Match(value, pattern)` the condition of `value is pattern`.
- `overloading.Group(candidates)` makes one object of several candidates, as the
  functions of one name, and choosing between them is `Apply`, which tries an operation
  on each and keeps the most specific that applies.

## IR: lowering

`ir` is MIR to LIR. Each entrypoint takes a target, the platform being lowered for, and
asks a different question:

| Entrypoint | Asks |
|---|---|
| `ir.CompileExpression(object, target)`, `ir.CompileStatement` | what code this is, where it is used |
| `ir.CompileType(object, target)` | what goes in a type slot: a class can define a structure and have a reference to it as its type |
| `ir.CompileDefinition(object, owner, target)` | to place this in an owner, as a function in a module |
| `ir.CompileReference(object, target)` | what code uses to refer to this, which may be something it doesn't define, as a handle |

## Loading

`hirLoader` is HIR to MIR: `LoadDefinition`, `LoadInstance`, `LoadType` and
`LoadIdentity`, one for each concept, and `hirCodeLoader` loads code. `mappedLoader`
is how a loader is registered for a kind of node, by its type, as
`mappedLoader.Instances.Register[Number](load)` did in [From HIR to LIR](hir-to-lir.md).

## Replacing how an operation is done

All of these are REIs: an operation's signature is fixed, and how it is done is whatever
is installed. The trait dispatchers of `std:traits` replace `CG.ImplicitCast` and
`CG.Cast`, so a value goes to a trait its type implements, without any CO knowing about
traits. That is the subject of [custom dispatchers](../custom-dispatchers/reis.md).
