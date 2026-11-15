# std:modules

Z#'s own modules, defined in Z#. A module is structure: its globals and its definitions.
`module Name { ... }` defines one, and so does `def Name of Module { ... }`.

The library is in two halves, each its own library:

- **`std:modules`** is the structure: the HIR nodes of a module and its globals, which is
  all that reflection needs.
- **`std:modules/compiler`** is what the compiler needs of them: the COs that lower a
  module and its globals, and the meta-compiler that builds a module's node.

Importing `std:modules` imports its compiler, through its entry point. Importing any
library imports `std:modules` too, since a library's nodes are modules.

## The structure: `std:modules`

The nodes are written with one meta-type, `Node`, which is the CLR's `Class`, of
`pkg:clr/hir`, here, so another platform reimplements them by giving that name its own.

| Node | Capabilities | What it holds |
|---|---|---|
| `Module` | `Instance`, `Named`, `Annotated` | its name, globals, definitions, entry point and annotations |
| `Global` | `Instance`, `Definition`, `Variable`, `Named`, `Annotated` | its name, type, initializer and annotations |

A module is an instance and not a definition, so nothing that holds definitions can hold
it. Its entry point, if it has one, is one of its definitions, called with nothing when
the module is imported. That is what loads it into the environment that imports it, as a
plugin is loaded. Building a module, as compiling its project does, doesn't call it.

## The compiler: `std:modules/compiler`

### COs

| CO | What it is | Protocols |
|---|---|---|
| `ModuleObject` | A module. It finds its members by the names the `Named` capability gives them, and holds them. It lowers, per target, to an LIR module of its own, its globals first and in their order. | `Holder`, `ICTGetMember[String]` |
| `GlobalObject` | A global, held by its module. What holds it lowers it into its LIR module, and gives it its initial value in that module's initializer. | `ICompileIRExpression`, `ICTSet`, `ITyped`, `Held`, `ICompileIRDefinitionIn[LIRModule]` |
| `GlobalSet` | Setting a global. | `ICompileIRStatement` |

LIR has no submodules, so a module inside a module lowers to an LIR module of its own. The
target realizes each LIR module; the CLR realizes it as a class of statics.

### How it plugs into the compiler

| Hook | Function | What it does |
|---|---|---|
| `@metaType(Module)` | `compileModule` | builds a `Module` from any definition with a body, using `Members` and `Filler` |
| `@instanceLoader` | `loadModule` | a module is its one CO, kept per node |
| `@definitionLoader` | `loadGlobal` | a global as a definition gives its CO |
| `@instanceLoader` | `instanceOfGlobal` | a global used in code is its one CO |
| `@binder` | `bindGlobal` | a built global is the static member of the class its module was built as |
| `@entrypoint` | `install` | makes `Module` the default meta-type of a `module` |

In a module, a `let` is a global bound by its name, an import runs as the module is built,
and a definition is compiled by its own meta-type.

### What it gives other libraries

| Function | What it does | Used by |
|---|---|---|
| `lower(node, target)` | the LIR module a module lowers to for a target | `bootstrap.zs`, the Cecil and package-manager builds |
| `failures(node)` | what of a module failed to lower for any target, and why, which a library's build refuses | `bootstrap.zs` |
| `enter(node)` | calls a module's entry point the first time, as importing a built one does | `std:packages` |

## What it depends on

- **`core:runtime`**, **`core:zsharp`** and **`core:compiler`**.
- **`pkg:clr/hir`:** `Class`, as the nodes' meta-type.
- **The toolchain's packages and layers:** HIR, MIR and LIR, under `pkg:zsharp/`, and the
  compiler's protocols, overloading and lowering, under `core:zsharp/`.
- **`std:framework`:** its decorators.

## Open points for review

- **Global state.** `loaded` (each module's CO), `entered` (the modules entered in the
  running process) and `bound` (each built global) are module globals: the compiler's and
  the process's state, kept in the library.
- **Lowering is reached by name.** `lower`, `failures` and `enter` are plain functions that
  builds import directly, rather than operations of the module's CO or an REI. A build of
  another kind of module can't take part without importing this library.
- **`std:packages` enters modules through the compiler half.** A package manager that
  only loads built modules still depends on what compiles them.
