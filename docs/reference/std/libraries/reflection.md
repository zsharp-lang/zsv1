# std:reflection

`reflect(value)` gives the node a built definition was built from: what describes it, as
its HIR. It takes what carries the definition's metadata, as a .NET type of a built
library:

```zs
import { print } from "std:io";
import { reflect } from "std:reflection";
import { Class } from "std:oop";
import { Named } from "pkg:zsharp/hir";
import { Type } from "core:runtime";

let built: Type = Class;
let node = reflect(built) as Named;
print(node.Name);
```

```output
Class
```

Reflection is opt-in: a library carries what reflection reads only if it was built to,
as the standard library is. [Reflection](../../../learn/advanced/reflection.md) says more.
