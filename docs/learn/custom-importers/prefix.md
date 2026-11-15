# Custom prefix importers

Some libraries aren't files: they live in a store, behind a URL, or are made on demand.
A prefix gives such a source a kind of its own, as `std:` and `pkg:` do, so the rest of
the string is whatever that kind needs.

A string source with a prefix is routed by `core:zsharp`'s `importers` to what is
registered for the prefix, which is given the rest of the string.
`importers.Register(prefix, importer)` adds a prefix.

## A prefix of greetings

```zs
import { print } from "std:io";
import { importers } from "core:zsharp";
import { Module } from "std:modules";
import { Instance } from "pkg:zsharp/hir";
import { Error, Result, IResult } from "pkg:zsharp/core";
import { String } from "core:runtime";

module Greetings {
    fun load(name: String): IResult[Instance, Error] {
        return Result[Instance].Ok(module Greeting { let text = "hello, " + name; } as Module);
    }
}

importers.Register("greet", Greetings.load);

import { text } from "greet:world";
print(text);
```

```output
hello, world
```

The importer is given `world`, what follows `greet:`.

## What a prefix is for

A prefix says what kind of library a source names, so the rest of the string can be
whatever that kind needs: a package's name and version, a path in a store, a URL, a
query. The toolchain's own prefixes are of that kind:

- `std:` names the standard library, its common APIs.
- `core:` names what the running toolchain gives, as [the `core:`
  namespace](../advanced/core-namespace.md) says.
- `pkg:` names a package: the toolchain's own packages, then whatever the package manager
  finds. The package manager is itself an importer, written in Z#, which `std:packages`
  registers as what `pkg:` falls back to.

A prefix that only routes to a library of named modules, as `core:` does, can use what
`core:zsharp` has for one instead of writing an importer: `core`, `packages` and the
standard library are each a library importer, with `Add(name, module)`.
