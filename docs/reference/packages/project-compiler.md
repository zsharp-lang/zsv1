# Project compiler

`std:project`: a project compiler, written in Z#. [Projects and
packages](../../learn/basics/projects.md) and [compiling
projects](../../learn/advanced/build-scripts/compiling-projects.md) show it in use.

## Projects

A project is a tree of submodules, found from a folder. A submodule is a folder with a
module file, `.zs`. Its other `.zs` files are its sources, and so are those of a folder in
it with no module file of its own; a folder in it with one is a submodule of it. Sources
and submodules are taken in the order of their names.

## std:project

| Member | Is |
|---|---|
| `discover(folder)` | The `Project` in a folder, whose root submodule is the folder. |
| `Compiler(project)` | What compiles a project. |
| `Compiler.compile()` | Runs the root's module file, and gives a `List[Instance]` of what the project's module files emitted, in the order they did. |

## core:project

What a module file runs with, as `core:project`, which the project compiler adds to
`core:` when it is imported:

| Member | Is |
|---|---|
| `compile()` | Compiles the running submodule's sources as a module named after its folder, then runs the module file of each of its submodules, and gives the module. |
| `compile(name)` | The same, with the module named `name`. |
| `emit(instance)` | Adds a node to what compiling the project gives. |
| `emit(type)` | Adds a type, as what describes it. |

## Module files

- A module file is a document of its own each time the project is compiled, run with
  `core:project`. It imports what it needs, and isn't given the prelude.
- A submodule's sources are compiled as one module, in the module file's scope: what the
  module file imports, the sources have.
- What the project gives is what was emitted, in order: a submodule emits before the
  module file whose `compile` ran it reaches its own `emit`.

## std:project/core

What `core:project` is: `Submodule`, with its `Name`, `Location`, `ModuleFile`, `Sources`
and `Submodules`; `Project`, with its `Root`; and `compileProject(project)`, which
`Compiler` calls.
