# Frameworks and projects

## Frameworks

Code that builds a definition runs as it is built, at every level: a document, a
module, a definition. So a library that installs itself into the environment that
imports it, as one that registers a meta-type does, can't say so with code that runs as
it is built, which would install it where it is built. It declares it instead, and a
framework, a library a build uses, makes what is declared into what the module does
when it is imported. `std:framework` is one.

- **Declarations are annotations.** `Annotated`, an HIR capability, is a definition's
  list of what has been said of it. A Z# function has one. A decorator of
  `std:framework` adds an annotation and gives the definition, so a declaration is data,
  for whatever reads the definition.
- **What it declares.** `@entrypoint` says a function is called when its module is
  imported. `@definitionLoader`, `@instanceLoader` and `@typeLoader` say a function is
  what loads the nodes of the type of its parameter, in the registry of that name.
  `@metaType(M)` says a function is the meta-compiler of `M`. Making `M` a default stays
  code, in an `@entrypoint`.
- **`build(module)`** reads the annotations of a module's functions and gives the module
  an entry point, a function named `<entry>`, whose code does what each declares, in
  their order, as `mappedLoader.Definitions.Register[T](f)` and `metaTypes.Register(M as
  Instance, f)`, then calls the explicit entry points. A project's module file builds
  what it emits: `emit(build(compile("Plugin")))`.
- **A module's entry point** is `std:modules`' `Module.EntryPoint`: a function of it that
  is called, with nothing, when the module is imported, and not when it is built. It
  lowers as LIR `EntryPoint` metadata of the module, which the Cecil platform writes as
  `[EntryPoint]` on the method. Importing an assembly calls each method marked so, the
  first time it is imported. A module compiled from its sources is
  imported with `enter(module)`, which calls it the first time, in the running process,
  as `std:packages` does for a package that is a project. So a module does the same
  built or from its sources.
- **What its declarations name is described as it runs.** `@metaType(Thing)` registers
  `Thing as Instance`, a description cast, so a built module registers the description
  its process gives its own `Thing`, the same one code that imports it gets.
- **Not yet:**
  - A decorator that takes arguments gives a function of a bound method, as
    `@metaType(M)` does, since a Z# function captures nothing. The argument is in
    parentheses, not `@metaType[M]`, as a Z# function has no type parameters to index.
  - A trait implementation is made known by loading its definition, which a built
    module doesn't have, so a module that defines one makes it known in an
    `@entrypoint`, with `std:traits`' `implement(trait, type, adapter)`, as
    `std:zsharp/clr` does.

## Projects

`std:project` is a minimal project compiler, written in Z#, to bootstrap the real one.

- **Discovery.** `discover(directory)` gives a `Project`, which holds its root
  `Submodule`. A submodule is a directory with a module file, `.zs`. Its other `.zs`
  files are its sources, and so are those of a directory in it without a module file;
  a directory in it with one is a submodule of it. Files and directories are taken in the
  order of their names.
- **Compiling.** `Compiler(project).compile()` runs the root's module file as a
  document, through the import system, and gives the `Instance`s that were emitted, in
  the order they were.
- **`core:project`.** A module file imports `compile` and `emit` from it. They are of
  `std:project/core`, what a module file runs in: the submodule running, what was
  emitted, and `compile` and `emit`, each two overloads. `std:project`'s entry point
  adds that module to `core:`, as `core:project`, so a module file can reach the rest of
  what it runs in too.
- **`emit(x)`** adds an instance to what compiling gives. A type is given as what
  describes it, through an overload for `HIR.Type`, so `emit(Int32)` emits the
  description, as a type is where a type is expected.
- **`compile()`** is a convenience, not a requirement:
  - It is a plain call, so it runs as the module file runs, in the document's scope:
    the sources see what the module file imported.
  - It parses the running submodule's sources as one AST module, named after the
    submodule's directory, or by `compile(name)`, and compiles it with the module
    meta-compiler on a fork of the compiler.
  - It builds the result and returns the `Module` HIR node, then runs the module file of
    each of its submodules, which emit for themselves.
  - Sources don't import one another, since they are one module.
- **The build script** decides what to do with what was emitted, as loading it and
  calling into it, or lowering it for a platform.
- **Not yet:**
  - Only the AST compiler is forked, and the HIR loader is shared. A module file runs as
    a document of its own each time its project is compiled, through `core:zsharp`'s
    `documents`, though importing it still gives what it gave when first imported.
  - A path is relative to where the toolchain runs.
  - A definition of a module that fails to load is left out of what it holds, so the
    failure shows only where the definition is used, and less clearly.
  - A syntax error in a source stops the build with the parser's exception.
  - There are no rules for names across submodules.
  - Writing an LIR file, and building the platform as a submodule, come next.
