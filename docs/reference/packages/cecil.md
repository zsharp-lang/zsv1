# .NET platform

`pkg:cecil`, the Cecil platform: a CLR platform written in Z#, which writes LIR as a .NET
assembly through Mono.Cecil. It is a package that is a project, `packages/cecil`, which
builds itself.

## Cecil

| Member | Is |
|---|---|
| `Cecil(name, version)` | An assembly being written, named `name`, for a version of .NET, as `"net8.0"`, or for the .NET that runs it, given `null`. |
| `add(module)` | Adds an LIR module, as `lower(module, target)` of `std:modules/compiler` gives one. |
| `embed(resource, bytes, decoder)` | Embeds bytes as a resource, with `[ReflectionBlob(resource, decoder)]`, which names the library that decodes them: what makes a built library describe its definitions. |
| `write(path)` | Writes the assembly. |
| `dependencies()` | The files of the assemblies the written one needs that neither .NET nor the running toolchain has, with what they need in turn, for `pkg:clr/platform`'s `bundle`. |

## Versions

Given a version, it reads the newest reference pack of it installed with the running
.NET, and finds each CLR handle there by its names. What the pack doesn't have, as the
toolchain's own assemblies, it reads from the running process's files. Given `null`, it
targets the .NET that runs it, and finds each handle in the running process.

## What it writes

- A module is a static class of its functions and globals, whose type initializer is the
  module's initializer, marked as a module scope, so importing the assembly gives the
  module. Its entry point is marked `[EntryPoint]`.
- A structure is a value type. A polymorphic one, as a Z# class or interface lowers to, is
  a CLR class or interface, whose methods are the functions of its module that are its
  methods, virtual, and abstract with no code.
- A CLR class defined in Z# is written as the runtime loads it: its base and interfaces,
  fields, constructors, methods, properties and type parameters.
- A type of a dotted name, as `Shape.Class`, is in the namespace before its last dot.
- Code is IL, and its debugging information a portable PDB embedded in the assembly: a
  sequence point for each statement with a source map, each source's checksum, and each
  method's locals by name.
- Builds are deterministic: the same LIR is written as the same bytes.

## What it can't write

- A constant that is an object only the running process has: "A value only the running
  process has, a ..., can't be written in an assembly".
- A type the running process made, which has no file.
- A CLR method with a local function, whose captures only the running process has.

A Z# class's constructor is a function of its module, not a CLR constructor, and a class
method is a method of its metaclass, so code outside Z# makes an object by calling the
module's function.
