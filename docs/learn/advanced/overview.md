# Overview

The basics treat Z# as a language you write and `zs` runs. This part looks at what
happens in between, because in Z# that is something your code can take part in: a
build is a script, definitions are built by code, and every stage can be extended.

## Languages and transformations

Z# is a chain of languages, each describing a program at another level, and of
transformations from one to the next:

```text
source  →  AST  →  HIR  →  MIR  →  LIR  →  a platform's code
```

| Language | What it describes |
|---|---|
| **HIR** | What is defined, in its own terms: the structure of each definition and of its code. It is what reflection gives back. |
| **MIR** | How things interact. Its one concept is the compiler object, the CO: loading an HIR node gives its CO, which knows how it is called, what its members are, how it converts, and how it lowers. |
| **LIR** | What runs, at a low level: modules, globals, functions, structures and code, which a platform can realize. |

A language never refers to another. Only a transformation goes between two, and it only
translates.

The core of Z# is the two transformations in the middle: **HIR → MIR**, loading, and
**MIR → LIR**, lowering. Everything at either end is an extension.

- **Frontends** give HIR. The Z# language is one: its parser gives an AST, and its AST
  compiler builds HIR from it. The importer of .NET assemblies is another, which reads IL
  metadata. The project compiler is a third, which builds a module from a folder. Any
  importer is a frontend, which is why importing gives HIR.
- **Backends** take LIR. A platform realizes it: the runtime, which runs it in the
  process, or the Cecil platform, which writes it as a .NET assembly.

AST and IL aren't Z#'s: to the core, there is only HIR coming in and LIR going out.

## Where your code comes in

`zs` puts a chain together and runs a script with it: the Z# frontend, so the script is
Z#; the standard definitions, through [the prelude](prelude.md); and the runtime as its
platform, so each statement runs as soon as it is compiled. The script runs statement by
statement, so what a statement defines is there for the next one, and code that builds a
definition runs as it is compiled.

From there, the script decides the rest:

- **A build is a script.** It imports the compiler's parts and decides what to compile,
  for which platform, and where it goes. There is no build system: [build
  scripts](build-scripts/arguments.md) are programs.
- **A platform is whatever a build gives lowering.** Z# doesn't define what a platform
  is: lowering takes a target as an object, and what it can do is asked of it. The CLI
  uses the runtime; a build can use the Cecil platform, another, or none.
- **Code can build code.** A definition is built by running code, so a program can
  generate definitions, and change them as they are built: [code
  generation](code-generation/declarative.md) and [decorators](decorators.md).
- **Code can read what was built.** The structure HIR describes is what
  [reflection](reflection.md) gives back, as the program runs.

## Extending a stage

Each stage is extensible, and the guides after this part go through them:

| To change | Extend | Guide |
|---|---|---|
| What can be written | the parser and the AST compiler | [Custom syntax and definitions](../custom-syntax/syntax.md) |
| What a kind of definition is | a meta-type and its meta-compiler | [Meta-types](../custom-syntax/meta-types.md) |
| What can be imported | the import system | [Custom importers](../custom-importers/how-imports-work.md) |
| What a definition means | its CO and its loader | [Custom semantics](../custom-semantics/hir-to-lir.md) |
| How an operation is done | the REIs, by a dispatcher | [Custom dispatchers](../custom-dispatchers/reis.md) |
| What code runs on | a platform | [Custom platforms](../custom-platforms/overview.md) |

The CLI itself is such a combination: [the Z# CLI](../cli/pipeline.md) shows how it is
put together.
