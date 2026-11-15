# std:prelude

What the `zs` command imports before a script's first statement, unless it's run with `--no-prelude`,
for what it installs: the standard definitions, so `fun` is
a function, `def` a class and `module` a module, the trait dispatchers, the CLR's
capabilities, reflection, `.dll` imports and `pkg:`. It gives no names, so a script
imports what it uses from each library itself:

```zs
import { print } from "std:io";
import { Interface, static } from "std:oop";
import { Type } from "std:types";
```
