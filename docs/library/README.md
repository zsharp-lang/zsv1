# The libraries

Z# has three kinds of libraries. The **standard library**, `std:`, is written in Z#, and
defines almost everything the language has, even what `def`, `fun` and `module` mean. The
**core libraries**, `core:`, are the toolchain's own, written in C#, and live: what Z#
reaches of the running platform and of the compiler. **Packages**, `pkg:`, are
definitions with no state of their own: the toolchain's, as its HIR and LIR, and the
package manager's.

## What a script uses

| Library | What it gives |
|---|---|
| [`std:prelude`](standard.md#stdprelude) | The standard definitions, installed: what `zs` imports into every script. |
| [`std:io`](standard.md#stdio) | `print`. |
| [`std:oop`](standard.md#stdoop) | Classes and interfaces: `Class`, `Interface`, `static`, `class`. |
| [`std:types`](standard.md#stdtypes) | `Type[T]`, the type of a definition's value. |
| [`std:numbers`](standard.md#stdnumbers) | Z#'s own integers and floating-point numbers, apart from the CLR's. |
| [`std:reflection`](standard.md#stdreflection) | `reflect`, the node a built definition was built from. |
| [`core:runtime`](core.md#coreruntime) | .NET's common types, as `String`, `Int32`, `List` and `Func`. |
| [`core:zsharp`](core.md#corezsharp) | The toolchain's own: `arguments`, `export`, `operator`, `ct`. |
| `pkg:name` | A package: [the toolchain's](core.md#the-toolchains-packages), or else through [the package manager](standard.md#stdpackages). |

## What a build uses

| Library | What it gives |
|---|---|
| [`std:project`](standard.md#stdproject) | Projects: `discover` and `Compiler`. Its module files get `core:project`. |
| [`std:modules/compiler`](standard.md#stdmodules) | `lower`, which lowers a module for a platform. |
| [`std:encoding`](standard.md#stdencoding) | What a built module carries, so it can be reflected on and imported. |
| [`pkg:clr/platform`](core.md#the-toolchains-packages) | `bundle`, for what a built assembly needs. |
| `pkg:cecil` | The Cecil platform, which writes .NET assemblies. |

## What extending Z# uses

| Library | What it gives |
|---|---|
| [`std:framework`](standard.md#stdframework) | What a library declares it is: `@entrypoint`, `@metaType`, loaders and binders. |
| [`std:traits`](standard.md#stdtraits) | Traits, and their implementations for types. |
| [`std:oop`, `std:functions`, `std:modules`](standard.md#structures-and-their-compilers) | The standard definitions' structure, and with `/compiler`, what compiles them. |
| [`pkg:zsharp/...`, `pkg:clr/...`](core.md#the-toolchains-packages) | The toolchain's definitions: HIR, MIR, LIR, the language's AST and parser, and the CLR's HIR and LIR. |
| [`core:zsharp/...`](core.md#the-compilers-layers) | The compiler's layers: `h2m`, `mir/cg`, `mir/ts`, `m2l`, and the language's compiler. |
| [`core:compiler`](core.md#corecompiler) | The compiler's operations, as `cg`, `ir` and `ts`. |
