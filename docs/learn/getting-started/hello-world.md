# Hello world

To check that Z# is installed and that your terminal can reach it, save this as
`hello.zs`:

```zs
import { print } from "std:io";

print("Hello, World!");
```

and run it, from the folder it is in, with the `zs` command:

```text
zs hello.zs
```

It prints:

```output
Hello, World!
```

If it does, Z# is ready. If the terminal says it has no `zs`, open a new one: a terminal
sees what the installer added to your PATH only once it starts.

:::note
If you installed with `--no-path`, `zs` isn't on your PATH. Run it by its folder instead,
as `<folder>/bin/zs hello.zs`.
:::

## What happened

Before the script's first line, `zs` imported the [prelude](../advanced/prelude.md),
which sets up the standard definitions every script relies on. Then the script ran, one
statement at a time:

- `import { print } from "std:io";` brought in `print`, from `std:io`, a library of Z#'s
  standard library.
- `print("Hello, World!");` called it.

## What's next

[The basics](../basics/basics.md) go through the language from the start. The `examples`
folder of an install has more scripts, and a project that builds a .NET assembly.
