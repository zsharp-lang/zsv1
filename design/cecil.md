# The Cecil platform

`packages/cecil` is a CLR platform written in Z#, as a project: it writes LIR as a .NET
assembly through Mono.Cecil, which it imports as `pkg:Mono.Cecil@0.11.6`. A build script
imports it as `pkg:cecil`, a package that is a project, which compiles it and loads it
into the running platform, and gives it LIR:
`Cecil(name, version)` is an assembly being written, `add(module)` adds an LIR module,
`lower(module, target)` from `std:modules` gives a Z# module's LIR for it, and
`write(path)` writes the assembly.

- **Versions.** Given a version, as `net8.0`, it reads the newest reference pack of it
  installed with the running .NET, and finds each CLR handle there by its names: a type
  by its full name, a member by its name and declared signature, as keys LIR and Cecil
  give alike. What the pack doesn't have, as the toolchain's own assemblies, it reads
  from the files of the running process's, and the types their signatures name are the
  version's. Given null, it targets the .NET that runs it, and finds each handle in the
  running process, by its token.
- **CLR classes defined in Z#** are written as the runtime loads them: a class or an
  interface, its base and interfaces, its fields, constructors that make an object of
  its base first, methods marked as what they are, properties, and type parameters. A
  generic class's own code refers to its members through its instance over its own type
  parameters, as C# does.
- **What it writes.** A module is a static class of its functions and globals, whose
  type initializer is the module's initializer, marked as a module scope, so importing
  the assembly gives the module: `import { describe } from "Shop.dll"`. Its entry point,
  if it has one, is marked `[EntryPoint]`. A function with
  no module is in a class of its own. A structure is a value type. A polymorphic one, as
  a Z# class or interface lowers to, is a CLR class or interface: its fields are its
  own, its objects start with no code of their own, as the code that makes one sets its
  fields, and its methods are the functions of its module that are its methods, virtual
  so a method implements an interface's of its name and signature, and abstract with no
  code. A reference to a variable is a box the assembly defines for itself. Code is IL,
  as the runtime emits it, and the CLR handles are what they describe.
- **Live values** can't be written: a constant that is an object only the running
  process has fails, as does a type the running process made, which has no file.
- **It writes itself.** `packages/cecil/build.zs` builds it into
  `packages/cecil/build/cecil.dll`, with the platform `pkg:cecil` gives: the project,
  compiled, the first time, and the assembly after.
- **Builds are deterministic.** A module's identity is of its content and an assembly
  has no time of its own, so the same LIR is written as the same bytes, and a platform
  that builds itself builds what it was.
- **What a build needs goes with it.** `dependencies()` gives the files of the
  assemblies the written one needs that neither .NET nor the running toolchain has, as
  Mono.Cecil, with what they need in turn, and `std:dotnet`'s `bundle` copies them into
  the build's folder, leaving one already there the same, as it may be in use.
- **Not yet:**
  - A CLR method with a local function, whose captures only the running process has.
  - A Z# class's constructor is a function of its module, not a CLR constructor, and a
    class method is a method of its metaclass, so code outside Z# makes an object by
    calling the module's function. A capability for constructors could make them the
    class's own, as the platform realizes them.
