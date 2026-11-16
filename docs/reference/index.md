# Reference

What each library and package gives, by name. The [Learn](../learn/getting-started/installation.md)
pages explain; these list.

## The libraries

Z# has three kinds of libraries. The **standard library**, `std:`, is written in Z#, and
defines almost everything the language has, even what `def`, `fun` and `module` mean. The
**core libraries**, `core:`, are the toolchain's own, written in C#, and live: what Z#
reaches of the running platform and of the compiler. **Packages**, `pkg:`, are
definitions with no state of their own: the toolchain's, as its HIR and LIR, and the
package manager's.

### What a script uses

| Library | What it gives |
|---|---|
| [`std:prelude`](std/libraries/prelude.md) | The standard definitions, installed: what the `zs` command imports into every script. |
| [`std:io`](std/libraries/io.md) | `print`. |
| [`std:oop`](std/libraries/oop.md) | Classes and interfaces: `Class`, `Interface`, `static`, `class`. |
| [`std:types`](std/libraries/types.md) | `Type[T]`, the type of a definition's value. |
| [`std:numbers`](std/libraries/numbers.md) | Z#'s own integers and floating-point numbers, apart from the CLR's. |
| [`std:boolean`](std/libraries/boolean.md) | `Bool`, Z#'s own truth values. |
| [`std:reflection`](std/libraries/reflection.md) | `reflect`, the node a built definition was built from. |
| [`core:runtime`](core/index.md#coreruntime) | .NET's common types, as `String`, `Int32`, `List` and `Func`. |
| [`core:zsharp`](core/index.md#corezsharp) | The toolchain's own: `arguments`, `export`, `operator`, `ct`. |
| `pkg:name` | A package: [the toolchain's](packages/zsharp.md), or else through [the package manager](packages/package-manager.md). |

### What a build uses

| Library | What it gives |
|---|---|
| [`std:project`](packages/project-compiler.md) | Projects: `discover` and `Compiler`. Its module files get `core:project`. |
| [`std:modules/compiler`](std/libraries/modules.md) | `lower`, which lowers a module for a platform. |
| [`std:encoding`](std/libraries/encoding.md) | What a built module carries, so it can be reflected on and imported. |
| [`pkg:clr/platform`](packages/clr.md#pkgclrplatform) | `bundle`, for what a built assembly needs. |
| [`pkg:cecil`](packages/cecil.md) | The Cecil platform, which writes .NET assemblies. |

### What extending Z# uses

| Library | What it gives |
|---|---|
| [`std:framework`](std/libraries/framework.md) | What a library declares it is: `@entrypoint`, `@metaType`, loaders and binders. |
| [`std:traits`](std/libraries/traits.md) | Traits, and their implementations for types. |
| [`std:oop`, `std:functions`, `std:modules`](packages/zsharp.md#the-standard-definitions-compilers) | The standard definitions' structure, and with `/compiler`, what compiles them. |
| [`pkg:zsharp/...`](packages/zsharp.md), [`pkg:clr/...`](packages/clr.md) | The toolchain's definitions: HIR, MIR, LIR, the language's AST and parser, and the CLR's HIR and LIR. |
| [`core:zsharp/...`](core/index.md#the-compilers-layers) | The compiler's layers: `h2m`, `mir/cg`, `mir/ts`, `m2l`, and the language's compiler. |
| [`core:compiler`](core/index.md#corecompiler) | The compiler's operations, as `cg`, `ir` and `ts`. |

## The languages

- [Z#'s syntax](language/syntax.md) and [its semantics](language/semantics.md), as
  specifications.
- The core languages: [HIR](core/hir.md), [MIR](core/mir.md) and [LIR](core/lir.md).
- [The runtime](runtime.md) and `core:runtime`.
