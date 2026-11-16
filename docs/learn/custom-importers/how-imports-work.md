# How imports work

Everything a script uses, it imports: a library, a document, a .NET assembly. Each of
those is a different kind of thing, but an import treats them alike, because what an
import gives is always a description of what it imported, as HIR. That is what lets you
import kinds of things Z# knows nothing about: give an importer for them, and they are as
importable as an assembly.

An importer is a frontend: it takes a source and gives HIR. This page goes through what an
import is, so the pages after it can say where an importer plugs in.

## Following one import

Take the first line of almost every script:

```zs fragment
import { print } from "std:io";
```

1. **It is a call.** An import is a call of the function the syntax `import` stands for,
   with the source as its argument: here, `import("std:io")`. `import` is an overloaded
   function of the language, so what a source can be is whatever an overload takes;
   `core:zsharp`'s `import(String)` takes a string.
2. **The string is routed by its prefix.** `import(String)` gives the string to
   `core:zsharp`'s `importers`, which looks at what comes before its first `:`, here
   `std`, and gives the rest, `io`, to what is registered for it: the standard library.
3. **The importer gives HIR.** The standard library gives the node of its `io` library, a
   module.
4. **The names are its members.** `print` is the member `print` of what the call gave.

Every import goes through the same steps, and each step is a place to plug in:

| Step | Plug in with |
|---|---|
| What a source can be | another overload of `import`: [custom source importers](source.md) |
| What an import's own arguments are, as `import(x) { a } from source` calling `import(source, x)` | the overload's parameters: [import parameters](parameters.md) |
| What a prefix means | `importers.Register`: [custom prefix importers](prefix.md) |
| What a file of an extension means | `files.Register`: [custom file suffix importers](file-suffix.md) |
| What is imported is described as | [nodes of your own](nodes-and-loaders.md) |

## Strings: prefixes and files

The prefixes there are, and who registers them:

| Prefix | Importer | Registered by |
|---|---|---|
| `std:` | the standard library | the toolchain |
| `core:` | the running toolchain's own libraries | the toolchain, and what adds to `core:` |
| `pkg:` | the toolchain's packages, then the package manager | the toolchain, and `std:packages` |
| none | a file, by its extension | `core:zsharp`'s `files` |

A file is imported by what is registered for its extension: `.zs` is a document, and
`.dll` a .NET assembly. A path is relative to the document that imports it.

## When an import runs

An import means what the definition it is in makes it mean, so when it runs depends on
where it is.

- **In a document**, an import runs as it is compiled, as every statement of a document
  does. Its names are bound to what it gave, so the next statement can use them as real
  values: in a decorator, in `of`, or as another import's source. An unknown name is an
  error of the import, right there.
- **In a module or a body**, nothing runs while compiling. The import compiles to two HIR
  nodes: an `Import`, holding its source as code, and an `ImportedMember` of it for each
  name it brings in, to which the name is bound. Loading an `Import` evaluates its source
  and runs the call; loading an `ImportedMember` loads its `Import` and reads its name off
  what it gave. Each is loaded once and kept, so an import is run once however many names
  or uses it has.

So a module's import runs when something of the module that uses it is loaded, and an
unknown name is an error at that point, not while compiling. A name nothing uses is
never checked.

A body's import is evaluated on its own, as it is loaded, not as part of the code it is
in: its source can't use a variable of that code. Inside code that builds code, it can,
since the import is built along with the code it is in, from what the building code holds
when it runs. [Declarative code generation](../advanced/code-generation/declarative.md)
builds code that way.

## What an importer gives

An importer gives HIR, and only that: an instance whose members are what can be imported
from it. It does nothing with the compiler: what it gives crosses into the compiler when
it is loaded, the same way any HIR does. So an importer is free in how it describes what
it imports:

- with HIR the toolchain already has, as a module: the samples of these pages build one
  with a [cast](../advanced/code-generation/declarative.md);
- with what describes a .NET assembly, as the assembly importer does;
- or with [nodes of its own](nodes-and-loaders.md), in the terms of what it imports, which
  a loader of its own then gives meaning to.
