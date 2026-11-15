# CLR packages

The CLR's own extensions of Z#'s languages, and what its platforms share. [Using
.NET](../../learn/basics/dotnet.md) shows the CLR's HIR in use.

## pkg:clr/hir

The CLR's HIR: definitions of the CLR, written in Z#, and how .NET's are described.

| Kind | Members |
|---|---|
| Meta-types | `Class`, `Interface`, `Struct`, `Module`, and their members' nodes |
| Generics | `GenericInstance`, and a definition's type parameters |
| Decorators | `public`, `static`, `virtual`, `override`, `explicitThis`, which makes a method's first parameter its object |
| Parameters by reference | `Ref`, `Out` |

Its names are the CLR's, so code that has a `Class` of its own, as a Z# class, renames the
CLR's on import: `import { Class as DotNet_Class } from "pkg:clr/hir";`.

## pkg:clr/lir

The CLR's dialect of LIR, which a `ClrPlatform` realizes.

| Member | Is |
|---|---|
| `ClrPlatform` | A platform that realizes the CLR's handles and dialect: the runtime, or an assembly written for a version of .NET. What only a CLR platform can lower, lowers for a target that is one. |
| handles, `ClrMethod`, `Reflected` | .NET's types and methods, each described once. |
| `ClrCode` | IL a function holds as it is. |
| `AddressOf`, `ByRefType` | References by address, as `ref` parameters take. |
| `ClassDefinition` | A CLR class a module holds. |
| `Accessibility` | What a CLR definition's accessibility is. |
| `Polymorphic`, `MethodOf`, `CastDown`, `TypeTest` | The metadata `std:zsharp/clr`'s `Polymorphism` marks LIR with: a structure is a class or an interface, a function its method, a cast down to it, a test for it. |

## pkg:clr/platform

What a platform that writes assemblies shares with a build, in Z#:

| Member | Is |
|---|---|
| `bundle(assemblies, folder)` | Copies assembly files into a folder, as a build's dependencies, leaving one already there the same, as it may be in use. |
