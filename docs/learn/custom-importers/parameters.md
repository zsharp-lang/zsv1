# Import parameters

An import can have arguments of its own, written after `import`:

```zs fragment
import(arguments...) { names } from source;
```

It calls the `import` overload with the source first and then these arguments, so they
are the parameters of the overload: positional, named, with default values, as any
function's.

## An import with options

This overload of `import` takes a number to count to, and a named `step`, which defaults
to 1:

```zs
import { print } from "std:io";
import { Module } from "std:modules";
import { operator } from "core:zsharp";
import { SyntaxFunctions } from "pkg:zsharp/lang/hir";
import { Instance } from "pkg:zsharp/hir";
import { Int32 } from "core:runtime";

module Counting {
    @operator(SyntaxFunctions("import"))
    fun count(to: Int32, {step: Int32 = 1}): Instance {
        return module Counted { let last = to; let by = step; } as Module;
    }
}

import(step: 5) { last, by } from 10;
print(last);
print(by);
```

```output
10
5
```

## What they are for

Arguments make an import fine-grained without changing its source: which version of a
package, which parts of a schema, which platform's view of a library, whether to load
lazily. They are evaluated with the source, as part of the same import, so a module's
import with arguments is still run once, when it is loaded.

An import has no alias of its own: only the names it brings in can be renamed, as in
`{ a as b }`.
