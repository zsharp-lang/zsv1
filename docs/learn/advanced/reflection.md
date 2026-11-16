# Reflection

A program sometimes needs to know what it is made of as it runs: the fields of a class,
to save its objects; the functions of a module, to expose them; the definitions of a
library, to document them. Reflection gives that: the node a definition was built from,
its HIR, the same structure the compiler worked with and code generation builds.

## Reflecting on a definition

`std:reflection`'s `reflect(value)` takes a value that stands for a definition, as a
type, and gives the definition's node. The standard library's definitions can be
reflected on, so this asks `Module`, of `std:modules`, for its node:

```zs
import { print } from "std:io";
import { reflect } from "std:reflection";
import { Module } from "std:modules";
import { Named } from "pkg:zsharp/hir";
import { Type } from "core:runtime";

let built: Type = Module;
print((reflect(built) as Named).Name);
```

```output
Module
```

What comes back is the very node `Module` was compiled from: a class of `std:modules`,
with its fields, methods and everything a [meta-type](../custom-syntax/meta-types.md)
says a class has. It is the same kind of node a cast gives in [code
generation](code-generation/declarative.md), so code that generates definitions and code
that inspects them read the same structure.

A program that reflects loads the libraries its nodes are of, only their structure, not
what compiles them. A value whose definition was never described gives what .NET says of
its type, or nothing.

## Why it works there

A definition only exists as HIR while it is being compiled. Once it is built into an
assembly, what is left is .NET's: classes, methods, fields. Reflection works on the
standard library because each of its libraries was built to carry its description with
it, and that is opt-in: a library carries it only if it was built to.

## Building a library that can be reflected on

A build describes a module before lowering it. `std:encoding`'s `encode(module)` writes
the module's HIR as a blob, and gives each definition an id, which the platform writes on
what the definition becomes. Then the platform embeds the blob in what it writes:

```zs fragment
let blob = encode(module);
let target = Cecil("Library", null);
target.add(lower(module, target));
target.embed("ZSharp.Reflection", blob, "std:encoding");
target.write("Library.dll");
```

`reflect` reads the id of what it is given, finds the blob in its assembly, and decodes
the node. The same description is what importing the assembly gives back, which is why a
built library's Z# classes are imported as Z# classes, not as plain .NET ones:
[compiling projects](build-scripts/compiling-projects.md) builds every library this way.
