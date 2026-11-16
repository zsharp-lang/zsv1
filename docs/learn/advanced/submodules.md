# Submodules

A [project](../basics/projects.md) compiles its sources as one module. That is right
until one project holds things that shouldn't share their names, or that are built
differently: an application and the build-time utilities it uses, or a library and its
tests. Submodules let one project hold several modules.

A folder in a project with a module file of its own is a submodule: its sources are a
module of their own, and its module file says what building it gives.

## What each one sees

Each module file runs as a document of its own, and its sources are compiled in its
scope. So a submodule's sources see what its own module file imports, and nothing of the
module file above it: `hello.zs` has `greetings/.zs`'s imports, not `src/.zs`'s.

The sources of one submodule are one module, so they reach each other's definitions by
name, with no imports between them. Two submodules are two modules: the project
compiler gives no way for one to name the other's definitions yet, so code that uses
both, as a build script, uses each one by the module it emitted.

## In what order

`compile()` compiles its module file's sources, then runs the module file of each of its
submodules, in the order of their folders' names. So a submodule emits before the module
file that compiles it gets to its own `emit`:

| Emitted | By |
|---|---|
| `greetings` | `greetings/.zs`, run as `src/.zs`'s `compile("Shop")` finishes |
| `Shop` | `src/.zs`, as `compile` returns |

`Compiler(project).compile()` gives them in that order.

## What a module file emits

`emit` takes any node, not only a module, and a module file needn't emit what it
compiles. A module file is a document, so it can compile its sources, change the module
it got, as [programmatic code generation](code-generation/programmatic.md) does, emit
several things, or none, and what a build does with each is the build script's to decide.
