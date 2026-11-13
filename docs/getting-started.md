# Getting started

## Installing

A release of Z# is an archive for your platform: `zsharp-win-x64.zip`,
`zsharp-linux-x64.tar.gz`, `zsharp-osx-arm64.tar.gz`, and so on. It needs nothing else
installed, not even .NET.

Extract it anywhere, and run its installer from the extracted folder:

```text
install.cmd            on Windows
./install.sh           on Linux and macOS
```

The installer copies Z# into your own programs, `%LOCALAPPDATA%\Programs\ZSharp` on
Windows and `~/.local/share/zsharp` elsewhere, and puts its `zs` on your PATH, so a new
terminal has it. To choose the folder, give it:

```text
install.cmd D:\Tools\ZSharp
./install.sh ~/tools/zsharp
```

- `--no-path` installs without changing your PATH. Run `<folder>/bin/zs` then.
- `--uninstall` removes Z#, and what installing it put on your PATH. Run it from the
  installed folder, or give the folder: `install.cmd --uninstall D:\Tools\ZSharp`.

The installer replaces or removes a folder only if it is a Z# toolchain, so a folder of
anything else is never written over. Installing into a folder that already has Z# replaces
it, which is how you upgrade.

On Windows, your PATH is your user's `Path` variable. Elsewhere, it is a few marked lines
in your shell's profiles (`~/.profile`, `~/.bashrc`, `~/.zshrc`), which uninstalling
removes.

## A first script

A script is a file of statements, which `zs` runs in order:

```zs
import { print } from "std:io";

print("Hello, World!");
```

```output
Hello, World!
```

Save it as `hello.zs`, and run it:

```text
zs hello.zs
```

## The prelude

Almost everything Z# has is defined in its standard library, not by the toolchain: even
what `fun` and `def` mean. A script gets the standard definitions from the prelude, which
`zs` imports before the script's first statement:

```zs
import { print } from "std:io";
import { Int32 } from "core:runtime";

fun twice(value: Int32): Int32 { return value * 2; }

print("twice 21 is " + twice(21));
```

```output
twice 21 is 42
```

Run with `zs --no-prelude`, a script has only what it imports itself, and
`import "std:prelude";` gives it the standard definitions again. The samples in these
pages are run with the prelude.

`core:runtime` has .NET's own types, as `Int32` and `String`, which Z# uses for its values.
[Using .NET](language/dotnet.md) says more.

## Arguments

What follows a script on the command line is the script's own:

```text
zs greet.zs Ada Grace
```

```zs fragment
import { print } from "std:io";
import { arguments } from "core:zsharp";

let index = 0;
while (index < arguments.Count) {
    print("Hello, " + arguments[index] + "!");
    index = index + 1;
}
```

## What's next

The [language](language/basics.md) pages go through Z# from the start. The
[`examples`](../examples) folder of an install has more scripts, and a project that
builds a .NET assembly.
