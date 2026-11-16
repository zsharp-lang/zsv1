# The core libraries

The core libraries, `core:`, are the toolchain's own, in C#: what Z# reaches of .NET, of
the toolchain and of the compiler. What `core:` holds is what the running toolchain puts
there, as [the `core:` namespace](../../learn/advanced/core-namespace.md) says; these are
what the `zs` command puts. The core *languages*, HIR, MIR and LIR, are on the pages after this one.

## core:runtime

The runtime the code runs on, and the .NET types Z# uses most: [Runtime](../runtime.md)
lists them.

## core:zsharp

The toolchain's own, as Z# code reaches it:

| Name | What it is |
|---|---|
| `arguments` | What follows the script on the command line. |
| `export` | The decorator that makes a document give a definition. |
| `operator` | The decorator that makes a method an operator, as `Operator.Index`. |
| `ct(code)` | Runs code, given as HIR, as it is compiled. |
| `load(definition)` | A definition's runtime object. |
| `import(source)` | Imports a source, as an `import` does. |
| `importers` | What imports each prefix, as `std:` and `pkg:`, which code can add to. |
| `packages` | `pkg:`'s library: the toolchain's packages, and what imports the rest, which the package manager sets. |
| `documents` | What runs a document. |
| `metaTypes`, `bodies`, `language`, `compiler` | The language's own parts, which a library that defines a kind of definition uses. |

## core:compiler

The compiler's operations, which a meta-type's code uses to work with what it compiles:

| Name | What it does |
|---|---|
| `cg` | Code generation: calls, members, casts, assignments. |
| `ts` | Types: the type of a value, and what goes where. |
| `ir` | Lowering to LIR, for a platform. |
| `hirLoader`, `hirCodeLoader` | What turns HIR nodes into what the compiler works with. |
| `overloading` | Choosing between overloads. |
| `bindings` | What binds built nodes to what they were built as. |

## core:project

What a project's module file runs with: `compile()`, `compile(name)` and `emit(value)`.
[Projects](../../learn/basics/projects.md) shows them.

## The compiler's layers

The compiler's layers, under `core:zsharp/`, as Z# code reaches their types, which a
library that extends Z# is written against:

| Module | What it is |
|---|---|
| `core:zsharp/h2m` | HIR to MIR: the loaders of HIR nodes and code. |
| `core:zsharp/mir/core` | The compiler's core: errors, and what any CO has. |
| `core:zsharp/mir/cg`, `/direct`, `/typed` | Code generation, and its protocols at compile time and as code runs. |
| `core:zsharp/mir/ts` | Types: the type of a value, and what goes where. |
| `core:zsharp/mir/overloading` | Choosing between overloads. |
| `core:zsharp/mir/matching`, `/static` | Matching values to patterns, and arguments to parameters. |
| `core:zsharp/m2l` | MIR to LIR: lowering, for a platform. |
| `core:zsharp/lang` | The language's compiler, from its AST. |

## The toolchain's packages

The toolchain's packages, under `pkg:`, are definitions with no state of their own:
[the Z# packages](../packages/zsharp.md) and [the CLR packages](../packages/clr.md).
