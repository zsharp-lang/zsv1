# The CLI pipeline

Every script so far ran with the `zs` command, which set everything up before the
script's first line. The guides before this page each described one part that can be
replaced: a frontend, dispatchers, loaders, importers, a platform. The Z# CLI is those
parts put together, once, in C#, before a script runs, and what it sets up is what the
script then reaches through `core:`.

This page goes through what the CLI sets up, in the order it does.

## The compiler

- **The loaders.** The HIR loader and the code loader are the mapped ones, which choose
  by a node's type what is registered for it. The code loaders of `HIR.Code` are
  registered, and the IL loader installs what loads the CLR's nodes.
- **The dispatchers**, each asking those applied before it first:
  - `CG.Direct`, which asks what objects say of themselves;
  - `CG.Typed`, which asks a value's type;
  - the static matching dispatcher;
  - `CG.External`, which finds members from outside an object's own, as .NET's extension
    methods, after its own;
  - the static dispatchers of TS, IR and overloading.
- **The reifier**, with the reifiers of `HIR.Code`.

## Running code

The platform is the runtime. HIR that runs as it is compiled, as a document's statement or
`ct`'s code, is lowered for the runtime and run in the process: the runner's statements
are compiled with `ir` for the runtime, and the runtime executes the LIR.

## Libraries

- **`std:`** has `std:io`, which is C#, and falls back to the standard library's folder,
  `std` above where the `zs` command is: a library there is its build, `std/name/build/name.dll`, or
  else its document.
- **`core:`** has `core:compiler`, `core:runtime`, `core:diagnostics` and `core:zsharp`,
  and the compiler's layers, `core:zsharp/h2m`, `core:zsharp/mir/...`, `core:zsharp/m2l`
  and `core:zsharp/lang`.
- **`pkg:`** has the toolchain's own packages, as `pkg:zsharp/hir` and `pkg:clr/lir`, and
  falls back to the package manager once `std:packages` installs it.

## The language

The Z# frontend: the binder of names, the language, with the operators of the CLR's
types and of the libraries as functions of their syntax, and `core:zsharp`'s
`import(String)` as what `import` calls. Then the CLR's meta-types, which are C#: what
`def Name of Class` with `pkg:clr/hir`'s `Class`, `module` with `pkg:clr/hir`'s `Module`,
and what a definition is before the standard library makes `fun`, `def` and `module` Z#'s
own.

## Documents and files

- `.zs` files are documents, which [the document compiler](document-compiler.md) runs.
- `.dll` files are assemblies, imported as their module, or as what their reflection blob
  describes.
- A string with no prefix is a file.

## Then the script

The `zs` command runs the script as a document, with `import "std:prelude";` before its first
statement unless it is given `--no-prelude`, and what follows the script on the command
line as `core:zsharp`'s `arguments`. From there the script decides the rest: what to
import, what to compile and where it goes.

So the CLI is not special. A host of its own could set up another frontend, other
dispatchers or another platform, and run scripts with that instead.
