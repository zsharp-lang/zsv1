# Reflection

Reflection gives the node a definition was built from, as code runs: its HIR, the same
structure the compiler worked with. It is opt-in: what a library carries for it, it
carries only if it was built to.

## Building a library that can be reflected on

`std:encoding`'s `encode(module)` writes a module's HIR as a blob before the module is
lowered, giving each definition an id, which the platform writes on what the definition
becomes, as an attribute on the CLR. The platform embeds the blob in what it builds:

```zs fragment
let blob = encode(module);
let target = Cecil("Library", null);
target.add(lower(module, target));
target.embed("ZSharp.Reflection", blob, "std:encoding");
target.write("Library.dll");
```

The standard library is built this way, which is also how importing a built library
gives the definitions it was built from.

## Reflecting

`std:reflection`'s `reflect(value)` takes what carries a definition's id, as a type of a
built library, and gives its node:

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

What reflection gives is a node of the library that defines it, so a program that
reflects loads the libraries its nodes are of: only their structure, not what compiles
them. A value whose definition carries no id gives what .NET describes of its type, or
nothing.
