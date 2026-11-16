# Accessing arguments

What follows a script on the command line is the script's own:

```text
zs greet.zs Ada Grace
```

The script gets it as `arguments`, of `core:zsharp`, a list of strings:

```zs fragment
import { print } from "std:io";
import { arguments } from "core:zsharp";

let index = 0;
while (index < arguments.Count) {
    print("Hello, " + arguments[index] + "!");
    index = index + 1;
}
```

```text
Hello, Ada!
Hello, Grace!
```

`zs`'s own options come before the script, so everything after it is the script's, even
what looks like an option of `zs`: in `zs build.zs --no-prelude`, `--no-prelude` is the
script's. [The CLI's arguments](../../cli/arguments.md) lists `zs`'s own.

`arguments` is `core:zsharp`'s because the CLI puts it there: what `core:` holds is the
toolchain's that runs the script, as [the `core:` namespace](../core-namespace.md) says.
