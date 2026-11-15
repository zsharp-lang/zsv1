# The standard library

The standard library is written in Z#, and its sources ship with every install, in `std`.
A library is imported as `std:name`; one with a part, as `std:project/core`.

| Library | What it is |
|---|---|
| [`std:prelude`](libraries/prelude.md) | What `zs` imports before a script: the standard definitions, installed. |
| [`std:io`](libraries/io.md) | `print`. |
| [`std:oop`](libraries/oop.md) | Z#'s classes and interfaces. |
| [`std:types`](libraries/types.md) | `Type[T]`, the type of a definition's value. |
| [`std:numbers`](libraries/numbers.md) | Z#'s own integers and floating-point numbers. |
| [`std:boolean`](libraries/boolean.md) | `Bool`, Z#'s own truth values. |
| [`std:reflection`](libraries/reflection.md) | `reflect`, the node a built definition was built from. |
| [`std:project`](../packages/project-compiler.md) | The project compiler. |
| [`std:modules`](libraries/modules.md) | Z#'s modules. |
| [`std:functions`](libraries/functions.md) | Z#'s functions. |
| [`std:packages`](../packages/package-manager.md) | The package manager. |
| [`std:encoding`](libraries/encoding.md) | What a built module carries, so it can be imported as what it was and reflected on. |
| [`std:framework`](libraries/framework.md) | What a library declares it is: `@entrypoint`, `@metaType`, loaders and binders. |
| [`std:traits`](libraries/traits.md) | Traits, and their implementations for types. |
| [`std:fs`](libraries/fs.md) | Files and folders, as Z# types. |

`std:oop`, `std:functions` and `std:modules` are their definitions' structure: their
nodes, which a program that reflects needs alone. What compiles them is in
[the Z# packages](../packages/zsharp.md#the-standard-definitions-compilers). Importing a
structure library installs its compiler, so a script imports only the structure.
