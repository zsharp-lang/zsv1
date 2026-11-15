# The prelude

Almost everything Z# has is defined in its standard library, not by the toolchain: even
what `fun` and `def` mean. The prelude, `std:prelude`, is what installs it. `zs` imports
it before a script's first statement, as if `import "std:prelude";` were written there:

```zs
import { print } from "std:io";
import { Int32 } from "core:runtime";

fun twice(value: Int32): Int32 { return value * 2; }

print("twice 21 is " + twice(21));
```

```output
twice 21 is 42
```

`fun` is a Z# function here because the prelude imported `std:functions`, whose
importing makes it so.

## What it installs

The prelude gives no names. It imports libraries for what importing them does:

- the standard definitions: `std:functions` for `fun`, `std:modules` for `module`, and
  `std:oop` for `def`, Z#'s classes and interfaces;
- the trait dispatchers, `std:traits/static` and `std:traits/dynamic`;
- what the platform can do, `std:zsharp/capabilities`, and what the CLR runtime has,
  `std:zsharp/clr`;
- reflection: `std:encoding`, which gives again what a built library describes, and
  `std:reflection`'s `reflect`, mapped onto it;
- `pkg:`, the package manager's packages, through `std:packages`.

Since it gives no names, a script imports what it uses from each library itself.

## Running without it

`zs --no-prelude` runs a script without it, so the script has only what it imports
itself, and `fun`, `def` and `module` mean what the toolchain alone makes them mean. The
bootstrap runs that way: it removes what earlier builds left of the packages the prelude
imports, and only then imports the prelude itself.

```text
zs --no-prelude build.zs
```

A script that wants the standard definitions then imports the prelude itself, as its first
statement:

```zs fragment
import "std:prelude";
```

## Changing it

The prelude is a document, `std/prelude.zs`, in the toolchain's folder. Only `zs` imports
it, and only for the script it runs: a document a script imports, or a project's module
file, imports what it needs itself.

So to start a script some other way, run it with `--no-prelude` and import, as its first
statements, a document of your own that imports what you want installed: the standard
prelude and more, or only part of it.
