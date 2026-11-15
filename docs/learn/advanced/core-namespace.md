# The core: namespace

`core:` is where code reaches the toolchain that runs it. The standard library is
the same wherever it runs; `core:` is not: what it holds is what the running toolchain
put there, so a script run by `zs` and a project's module file run by the project
compiler see different libraries under it.

## What it is for

The compiler, the platform a script runs on, the language's parser: these are live
objects of the process that runs, not definitions a library could carry. `core:` is how
Z# code gets at them. A library that needs one imports it from `core:`, and works with
whatever toolchain gives it.

It is also how the pipeline is extended. The parts of the compiler under `core:` are its
extension points: registering a meta-type, a loader, an importer or a dispatcher is done
through what `core:` gives, from Z#.

## Who puts what there

`core:` is a list of libraries that anything running can add to. `core:zsharp`'s `core`
is that list, and `core.Add(name, library)` adds `core:name`.

- **The CLI** puts the toolchain's own: `core:runtime`, the runtime and the .NET types Z#
  uses most; `core:compiler`, the compiler and its REIs; `core:zsharp`, the Z# language;
  `core:diagnostics`; and the compiler's layers, as `core:zsharp/h2m` and
  `core:zsharp/mir/cg`. [The CLI pipeline](../cli/pipeline.md) lists them.
- **The standard library** adds more as it is imported: `std:packages` adds
  `core:packages`, the package manager.
- **The project compiler** adds `core:project` for the module files it runs, with
  `compile` and `emit`: [the project compiler](../../reference/packages/project-compiler.md).

A library of yours can add to it the same way, for what the code it runs needs from it.

## What to import from it

Import from `core:` what only the running toolchain can give, and from `std:` and `pkg:`
everything else. Code that imports from `core:` runs only where a toolchain gives what it
imports: a module file that uses `core:project` runs under the project compiler, and a
build script that uses `core:compiler` runs where there is a compiler, which is under
`zs`.

The [reference](../../reference/core/index.md) lists what each library of `core:` has.
