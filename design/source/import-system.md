# Import System

Z# does not define how imports are implemented. Instead, it
defines import statements as invocations into the import system.

Basically, we have an `import` function. When we import, the 
compiler compiles the source of the import, then constructs a
call and evaluates it to an ORS object. From there we bind names
to the imported members and bind them to the imported object.

To see a more concrete example:
```
import { print } from "std:io";
```

When the compiler sees this, it first needs to evaluate the
source. To do this, it needs to call the import function and
pass it the source (the `"std:io"` string). The import function
then proceeds and gives us either an error or an ORS `Reference`.

In case of success, we bind each imported name to an `ImportedName`
object, passing in the name.
Then we construct an `Import` object that holds the reference and
the imported names.

All this should happen in the ORS layer.

This gives decorators a chance to inspect and modify the import object.
If a decorator is indeed applied, it gets the `Import` object and it
can modify, inspect and even return a completely different ORS object.

The source in the import statement is an expression.

