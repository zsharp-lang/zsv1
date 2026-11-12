# std:functions

Z#'s own functions, defined in Z#. A Z# function is a function of its own, not a method of
anything: `fun` defines one wherever nothing else compiles it.

The library is in two halves, each its own library:

- **`std:functions`** is the structure: the HIR nodes of a function and its parameters,
  which is all that reflection needs.
- **`std:functions/compiler`** is what the compiler needs of them: their COs, the
  meta-compiler that builds a function's node from what is written, and what binds a built
  function.

Importing `std:functions` imports its compiler, through its entry point, so a program that
compiles gets both. A program that only reflects loads the structure's assembly for its
nodes without importing it, so it gets the structure alone.

## The structure: `std:functions`

The nodes are the platform's own definitions, written with one meta-type, `Node`, which is
`DotNet_Class` here. Another platform reimplements them by giving that name its own.

| Node | Capabilities | What it holds |
|---|---|---|
| `Function` | `Instance`, `Definition`, `Named`, `Annotated`, `Overloadable` | its name, parameters, locals, return type, code and annotations |
| `Positional` | `Instance`, `Definition`, `Variable` | a parameter a positional argument fills: its name, type and default value |
| `Named` | `Instance`, `Definition`, `Variable` | a parameter an argument with its name fills, the same way |

Functions of one name overload, because a function is `Overloadable`. What is said of a
function, as a decorator says it is an entry point, is its annotations.

## The compiler: `std:functions/compiler`

### COs

| CO | What it is | Protocols |
|---|---|---|
| `FunctionObject` | A function. Calling it matches the arguments to its parameters. It lowers once per target, to an LIR function of what holds it, if something does. | `ICTCallable`, `ICTCastTo`, `Held`, `ICompileIRDefinitionIn[LIRModule]`, `ICompileIRReference[ICallable]` |
| `FunctionCall` | A call of a function, with its matched values. | `ICompileIRExpression`, `ITyped` |
| `ParameterObject` | A parameter, both as a call matches an argument to it, through the matching REI, and as its function's code uses it. It owns the LIR parameter for each target. | `ICTBind`, `ICompileIRExpression`, `ICompileIRReference[LIRParameter]`, `ITyped` |
| `Known` | A value that exists as the code is compiled, as a definition's node does. | `ICompileIRExpression`, `ITyped` |

### How it plugs into the compiler

Each of these is a decorator from `std:framework`. The framework turns it into the
registration call the compiler needs.

| Hook | Function | What it does |
|---|---|---|
| `@metaType(Function)` | `compileFunction` | builds a `Function` from a `fun`, filled by `Filler` |
| `@definitionLoader` | `loadFunction`, `loadPositional`, `loadNamed` | a node as a definition gives its CO |
| `@instanceLoader` | `instanceOfFunction` | a function used in code is its one CO |
| `@instanceLoader` | `instanceOfPositional`, `instanceOfNamed` | a parameter used in code is what its function defined it as, in the code's scope (`CodeLoaders.Defined`) |
| `@binder` | `bindFunction` | a built function is premapped onto the method it was built as |
| `@entrypoint` | `install` | makes `Function` the default meta-type of a `fun` |

### What it gives other libraries

- **`premap(declared, to)`:** makes a declared function, one with no code of its own,
  lower as another definition wherever it's called. `std:prelude` uses it, to make
  `reflect` the IL loader's description of a type.

## What it depends on

- **`core:runtime`:** the CLR's types, and `DotNet_Class` as the nodes' meta-type.
- **`core:zsharp`:** `importers` for its entry point, and `metaTypes` and `bodies`.
- **`core:compiler`:** the REIs.
- **The `std:zsharp/...` modules:** HIR, MIR, LIR, and the compiler's protocols.
- **`std:framework`:** its decorators.
- **`std:zsharp/lowering`:** `Holder` and `Held`.

## Open points for review

- **Global state.** `premapped` is a module global that every function's CO reads. It is
  the compiler's state, kept in the library rather than passed.
- **`Known` is defined twice**, here and in `std:oop/compiler`, with the same meaning. It
  belongs in one shared place.
- **`ParameterObject` loads its own node's HIR while it matches**: its type and its
  default value, each time, as both mean what the call's context makes them. The C#
  `ParameterMatch` is given functions for them instead, so it knows no HIR. This CO is
  its node's own, so it may read it, but loading belongs to what loads it.
- **The parameter kinds are fixed**: a parameter is `Positional` or `Named`. A new kind is
  a new node and a new loader here, rather than something this library opens up.
