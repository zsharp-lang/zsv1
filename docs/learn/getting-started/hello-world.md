# Hello world

A first script, which checks that Z# is installed and runs.

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
[Using .NET](../basics/dotnet.md) says more.

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

The [language](../basics/basics.md) pages go through Z# from the start. The
[`examples`](../../../examples) folder of an install has more scripts, and a project that
builds a .NET assembly.
