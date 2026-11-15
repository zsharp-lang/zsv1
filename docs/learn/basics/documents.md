# Documents

Every file of Z# is a document, and the script the `zs` command runs is one too. Another document is
imported by its path, relative to the one importing it, which is how a script is split
across files.

## Exporting

A document gives the importer only what it marks with `@export`, from `core:zsharp`:

```zs fragment
// shapes.zs
import { export } from "core:zsharp";
import { Int32 } from "core:runtime";

@export
def Square {
    let side: Int32;
    new(this, side: Int32) { this.side = side; }
    fun area(): Int32 { return side * side; }
}
```

```zs fragment
// main.zs
import { print } from "std:io";
import { Square } from "shapes.zs";

print(Square(4).area());
```

`@export("Name")` exports a definition under another name. A document runs once, however
often it is imported, and two documents that import each other are an error.

## Documents and projects

A document runs as it is imported, statement by statement, as a script does. For a few
files that is all you need. For more, with definitions that use each other freely across
files, a [project](projects.md) compiles them as one module.
