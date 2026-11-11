# Composition and execution

## Composition

There is no pipeline and no platform as a concept.

```bash
zs build.zs
```

The CLI bootstraps the parser, the Z# source language extension and the runtime,
then runs the script. The compiler is simply a function: `build.zs` imports
compiler utilities and decides what to compile, from selecting files to running
optimisations and generating code.

The toolchain objects are reached from Z# through the `core:` prefix:

- `core:runtime`: what is specific to the runtime, such as the platform, its types and
  the CTS HIR.
- `core:compiler`: the compiler, such as CG and TS.
- `core:zsharp`: the language, such as the parser, the binder and the import system.
  Today it has the compiler, `import` and `@export`.

A project compiler, meant to be written in Z#, would add `core:project`, which gives
insight into the project being compiled.

```text
import { cg } from "core:compiler";
```

`CG` is an instance, and the pointer to the current one is static. That pointer is the
compiler's, `ZSharp.Compiler.Current`, in the compiler layer above the REIs: the
compiler in use, its operations and its loaders, which the host makes once. What
compiles reaches it there, and `core:compiler` is how Z# reaches it: its members are
`Current`'s, so the library is defined in terms of the compiler and not the other way
around. There is no scoping: a compilation never needs a different setup, because a
library is compiled when it is installed (not our responsibility) and only loaded on
import. A library can export compile-time utilities, which can be imported and used at
compile time.

That the compiler is reached as one, and not passed to what uses it, is a known form to
remove: a CO's operations don't take the compiler, and the standard library's Z# code
implements them, so passing it is a change of every protocol, which the seed takes in
steps.

## Execution

Parsing and execution both go statement by statement, so an earlier statement
can extend the syntax of later ones.

Compile-time evaluation is an operation, not an API of its own: the Evaluator
service is not carried over. `HIRRunner` is the REI for running HIR immediately,
and it knows nothing about where the HIR came from. `MappedRunner` implements it
with registered statement runners, which is how control flow runs and how an
extension adds statements that act at compile time. Anything else is loaded and
handed to functions the CLI supplies, which lower it and run it.
