# Hello world

A first script, to check that Z# is installed and runs.

## Writing it

A script is a file of statements, which `zs` runs in order. Save this as `hello.zs`:

```zs
import { print } from "std:io";

print("Hello, World!");
```

## Running it

From the folder `hello.zs` is in:

```text
zs hello.zs
```

It prints:

```output
Hello, World!
```

If it does, Z# is installed. If the terminal says it has no `zs`, open a new one, since
a terminal sees what the installer put on your PATH only once it starts. With
`--no-path`, run `zs` by its folder, as `<folder>/bin/zs hello.zs`.

## What happened

- `import { print } from "std:io";` brought `print` in from `std:io`, a library of Z#'s
  standard library. A script has only the names it imports.
- `print("Hello, World!");` called it.
- Before the first statement, `zs` imported the [prelude](../advanced/prelude.md), which
  installs Z#'s standard definitions: what `fun`, `def` and `module` mean.

## What's next

[The basics](../basics/basics.md) go through the language from the start. The `examples`
folder of an install has more scripts, and a project that builds a .NET assembly.
