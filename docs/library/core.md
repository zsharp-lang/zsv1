# The core libraries

The core libraries are the toolchain's own, in C#: what Z# reaches of .NET, of the
toolchain and of the compiler.

## core:runtime

.NET's types Z# uses most, under the names .NET gives them:

| Kind | Types |
|---|---|
| Values | `Object`, `String`, `Boolean`, `Char`, `Byte`, `SByte`, `Int16`, `UInt16`, `Int32`, `UInt32`, `Int64`, `UInt64`, `Single`, `Double` |
| Types and generics | `Type`, `Nullable`, `IDisposable` |
| Collections | `List`, `Dictionary`, `HashSet`, `IEnumerable`, `Enumerable` |
| Delegates | `Func`, `Action` |
| References | `Ref`, `Out`, for parameters passed by reference |

It also has the running platform, `runtime`, and `explicitThis`, the decorator that makes
a CLR method's first parameter its object.

The rest of .NET is imported from its assemblies, as [Using .NET](../language/dotnet.md)
shows: `Path`, `File`, `Directory` and `Environment` are in `Object`'s, as in
`import { Path } from Object.Assembly;`. What defines CLR classes in Z# is the CLR's HIR,
in [`pkg:clr/hir`](#the-toolchains-packages).

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
[Projects](../toolchain/projects.md) shows them.

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

Definitions with no state of their own, under `pkg:`. The toolchain gives them, so they
are there before the package manager is:

| Package | What it is |
|---|---|
| `pkg:zsharp/core` | What every layer shares, as `Error` and `Result`. |
| `pkg:zsharp/hir`, `pkg:zsharp/hir/code` | HIR, and its code. |
| `pkg:zsharp/mir` | MIR: the compiler objects. |
| `pkg:zsharp/lir` | LIR. |
| `pkg:zsharp/lang/hir`, `/ast`, `/parser`, `/syntax`, `/tokenizer` | The language: its HIR, its AST, and what reads source. |
| `pkg:clr/hir` | The CLR's HIR: `Class`, `Module`, `Interface` and the rest, and their decorators, as `public`, `static`, `virtual` and `override`. |
| `pkg:clr/lir` | The CLR's LIR, as `ClrPlatform`. |
| `pkg:clr/platform` | What a build of a .NET assembly needs, as `bundle`. |

A name of the CLR's HIR that is also one of Z#'s is renamed on import, as
`import { Class as DotNet_Class } from "pkg:clr/hir";`.
