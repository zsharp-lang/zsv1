# Overview

So far, Z# has been a language you write and the `zs` command runs. This part looks at
what happens in between, because in Z# that is something your own code can take part in:
a build is a script, definitions are built by code, and every stage of compiling can be
extended.

## The layers

Between the text you write and the code that runs, a program passes through layers. Each
layer describes the program at its own level, and moving from one to the next is a
transformation:

```text
source  →  AST  →  HIR  →  MIR  →  LIR  →  a platform's code
```

| Layer | What it describes |
|---|---|
| **AST** | What was written, as the parser read it. |
| **HIR** | What is defined: the structure of each definition and of its code, in its own terms. It is also what reflection gives back. |
| **MIR** | How things behave. Loading an HIR node gives its compiler object, its CO, which knows how it is called, what its members are, how it converts, and how it lowers. |
| **LIR** | What runs, at a low level: modules, globals, functions, structures and code, which a platform can realize. |

A layer never refers to another. Only a transformation goes between two, and it only
translates.

Three of these layers, HIR, MIR and LIR, are Z#'s **core**, and so are the two
transformations between them: **loading**, HIR to MIR, and **lowering**, MIR to LIR.
Everything at either end is an extension of the core:

- **Frontends** give HIR. The Z# language is one: its parser reads text into an AST, and
  its AST compiler builds HIR from the AST. So the AST is Z#'s, but not the core's: the
  core only ever sees the HIR it gave. The importer of .NET assemblies is another frontend,
  which gives HIR from an assembly's metadata, and so is the project compiler, which gives
  it from a folder. Any importer is a frontend, which is why importing gives HIR.
- **Backends** take LIR. A platform realizes it: the runtime, which runs it in the
  process, or the Cecil platform, which writes it as a .NET assembly.

## Where your code comes in

The `zs` command, the Z# CLI, is what puts the layers together for a script: it sets up
the Z# frontend, so the script is Z#; the standard definitions, through [the
prelude](prelude.md); and the runtime as its platform, so each statement runs as soon as
it is compiled. That is why a statement can use what the one before it defined: by then,
it has already run.

From there, the script decides the rest:

- **A build is a script.** It imports the compiler's parts and decides what to compile,
  for which platform, and where it goes. There is no build system to configure: [build
  scripts](build-scripts/arguments.md) are programs.
- **A platform is whatever a build gives lowering.** Lowering takes a target as an
  object and asks it what it can do. The CLI uses the runtime; a build can use the Cecil
  platform, another, or none.
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

The Z# CLI itself is such a combination: [the Z# CLI](../cli/pipeline.md) shows how it is
put together.
