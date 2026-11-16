# The core: namespace

The standard library is the same wherever it runs. But some of what code needs isn't a
definition a library could carry: the compiler compiling it, the platform it runs on, the
arguments it was given. Those are live objects of the toolchain that runs the code, and
`core:` is where code reaches them.

So what `core:` holds depends on the toolchain running: a script run by the `zs` command
and a project's module file run by the project compiler see different libraries under it.

## Reaching the toolchain

You have already used `core:` a few times: `core:runtime` for .NET's types,
`core:zsharp`'s `arguments` for what a script is given, and `core:project` in a module
file. Each is something only the running toolchain has. A library that needs one imports
it from `core:`, and works with whatever toolchain gives it.

That also makes `core:` how the pipeline is extended. The compiler's parts are under it,
so registering a meta-type, a loader, an importer or a dispatcher is done through what
`core:` gives, from Z#.

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
build script that uses `core:compiler` runs where there is a compiler, which is under the
`zs` command.

The [reference](../../reference/core/index.md) lists what each library of `core:` has.
