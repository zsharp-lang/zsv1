# The core libraries

The core libraries are the toolchain's own, in C#: what Z# reaches of .NET, of the
toolchain and of the compiler.

## core:runtime

.NET's types Z# uses most, under the names .NET gives them:

| Kind | Types |
|---|---|
| Values | `Object`, `String`, `Boolean`, `Char`, `Byte`, `SByte`, `Int16`, `UInt16`, `Int32`, `UInt32`, `Int64`, `UInt64`, `Single`, `Double` |
| Types and generics | `Type`, `Nullable`, `IDisposable` |
| Collections | `List`, `Dictionary`, `HashSet`, `IEnumerable`, `Enumerable` |
| Delegates | `Func`, `Action` |
| Files and the process | `Path`, `File`, `Directory`, `Environment`, `AppDomain`, `Assembly` |
| References | `Ref`, `Out`, for parameters passed by reference |

The rest of .NET is imported from its assemblies, as [Using .NET](../language/dotnet.md)
shows. `core:runtime` also has what defines CLR classes in Z#, as `DotNet_Class` and
`DotNet_Module`, and their decorators, as `explicitThis`, `virtual` and `override`.

## core:zsharp

The toolchain's own, as Z# code reaches it:

| Name | What it is |
|---|---|
| `arguments` | What follows the script on the command line. |
| `export` | The decorator that makes a document give a definition. |
| `operator` | The decorator that makes a method an operator, as `Operator.Index`. |
| `ct(code)` | Runs code, given as HIR, as it is compiled. |
| `load(definition)` | A definition's runtime object. |
| `import(source)` | Imports a source, as an `import` does. |
| `importers` | What imports each prefix, as `std:` and `pkg:`, which code can add to. |
| `documents` | What runs a document. |
| `metaTypes`, `bodies`, `language`, `compiler` | The language's own parts, which a library that defines a kind of definition uses. |

## core:compiler

The compiler's operations, which a meta-type's code uses to work with what it compiles:

| Name | What it does |
|---|---|
| `cg` | Code generation: calls, members, casts, assignments. |
| `ts` | Types: the type of a value, and what goes where. |
| `ir` | Lowering to LIR, for a platform. |
| `hirLoader`, `hirCodeLoader` | What turns HIR nodes into what the compiler works with. |
| `overloading` | Choosing between overloads. |
| `bindings` | What binds built nodes to what they were built as. |

## core:project

What a project's module file runs with: `compile()`, `compile(name)` and `emit(value)`.
[Projects](../toolchain/projects.md) shows them.

## std:zsharp

The toolchain's own modules, as Z# code reaches them, named after what they are:
`std:zsharp/hir`, `std:zsharp/hir/code`, `std:zsharp/mir`, `std:zsharp/lir`,
`std:zsharp/lang`, `std:zsharp/lang/ast`, `std:zsharp/lang/parser`, `std:zsharp/lang/syntax`,
`std:zsharp/compiler`, `std:zsharp/compiler/cg`, `std:zsharp/compiler/ir`,
`std:zsharp/compiler/ts`, and more. They are what a library that extends Z# is written
against.
