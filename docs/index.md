# Z#

Z# is a language whose definitions are its own to define. A class, a function, a module,
even what `def` and `fun` mean, are written in Z# itself, in its standard library, and a
library can add new kinds of definitions the same way. The toolchain runs on .NET, and Z#
reaches everything .NET has.

Every sample in these pages is run by the toolchain's tests, so what a page shows is what
Z# does.

## Contents

- [Getting started](learn/getting-started/installation.md): installing Z#, a first script, the prelude, and
  running a script with arguments.
- **The language**
  - [Basics](learn/basics/basics.md): documents, statements, `let`, values and operators.
  - [Control flow](learn/basics/control-flow.md): `if`, `while`, `break` and `continue`, and
    loops that give a value.
  - [Functions](learn/basics/functions.md): `fun`, parameters, named parameters and default
    values, and overloads.
  - [Classes](learn/basics/classes.md): fields, constructors, methods, properties, static and
    class members, and indexing.
  - [Interfaces](learn/basics/interfaces.md): interfaces, implementing them, casts and type
    tests, and an interface's class methods.
  - [Modules and documents](learn/basics/modules.md): modules, their globals, and what a
    document exports to another.
  - [Using .NET](learn/basics/dotnet.md): .NET's types, generics, arrays, and importing
    assemblies.
  - [What isn't there yet](learn/basics/not-yet.md): what a reader of other languages might
    reach for, and what to write instead.
- **The libraries**
  - [Overview](reference/index.md): which library gives what.
  - [The standard library](reference/std/index.md): `std:`, written in Z#.
  - [The core libraries](reference/core/index.md): `core:`, the toolchain's own.
- **The toolchain**
  - [The zs command](learn/cli/arguments.md): running scripts, and where the standard library is.
  - [Projects](learn/basics/projects.md): module files, building a module, and building a
    .NET assembly.
  - [Reflection](learn/advanced/reflection.md): building what can be reflected on, and
    reflecting.
  - [The bootstrap and releases](internals/bootstrap.md): the seed, building it again, and
    building a release.

- **The design**
  - [Principles](internals/principles.md): the ideas Z# is built on.
  - [How Z# compiles](learn/advanced/overview.md): from text to running code, the REIs, and
    lowering by capability.
- **Extending Z#**
  - [Decorators and compile time](learn/advanced/decorators.md): code that takes part in
    compiling.
  - [New kinds of definitions](learn/custom-syntax/meta-compilers.md): a meta-type's node,
    meta-compiler, loader and CO.

The record of every design decision is [`design/`](../design/README.md),
and the [`examples`](../examples) show more.
