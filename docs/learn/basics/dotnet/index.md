# Using .NET

Z# runs on .NET, and uses .NET's types and functions for its values. So everything .NET
has, Z# reaches directly: its types, their methods, and any assembly, .NET's own or a
library's.

## When to reach for it

- **In a small script**, .NET is simply what is there: `String`'s methods, `Path` and
  `File` for files, `List` and `Dictionary` for collections. There is nothing to set up.
- **For what .NET's ecosystem has**: JSON, HTTP, databases, and every library on NuGet. An
  assembly is imported like anything else, so its types are Z#'s to use.
- **When .NET has to see your code** as .NET code: a class that a .NET library will call,
  or an assembly other .NET code will reference. Then you define [classes of the
  CLR](clr-classes.md) rather than Z#'s own.

`core:runtime` has the types Z# uses most, as `String`, `Int32`, `List` and `Func`. The
rest of `System`'s are in the assembly of `Object`, which a script imports as
`import { Path, File } from Object.Assembly;`.

## The pages

- [Types and members](types.md): reaching a type's constructors, methods and properties.
- [Generics](generics.md): .NET's generic types and methods.
- [Arrays](arrays.md).
- [Casts and nulls](casts.md).
- [Classes of the CLR](clr-classes.md): classes .NET sees as its own, and their
  accessibility.
- [Importing assemblies](assemblies.md): .NET's own, by their path or as the assembly
  itself, and NuGet's.
