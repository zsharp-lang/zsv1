# Projects

A project is a folder of Z# sources that builds into modules: a library, or a .NET
assembly. The standard library's own libraries are projects.

## A project's folders

A folder with a module file, a file named `.zs`, is a submodule. Its other `.zs` files are
its sources, and so are those of a folder in it that has no module file of its own; a
folder that has one is a submodule of it.

```text
greetings/
  src/
    .zs          the module file
    hello.zs     a source
    text/
      words.zs   a source too: text/ has no module file
```

The sources of a submodule are one module: they use each other's definitions with no
imports between them, and each imports what it uses from elsewhere.

## The module file

Building a project runs its module file, a document, with `core:project`:

- `compile()` compiles the folder's sources as a module named after the folder, or
  `compile(name)` with a name, and runs the module file of each submodule.
- `emit(value)` adds a value, as a module, to what building the project gives.

```zs fragment
// src/.zs
import { compile, emit } from "core:project";

emit(compile("Greetings"));
```

What the module file emits, the build script decides what to do with.

## Building it

`std:project`'s `discover(folder)` finds a project, and `Compiler(project).compile()` runs
its module file, giving what it emitted. To make a .NET assembly of a module, the Cecil
platform, `pkg:cecil`, is the target `lower` lowers it for, and `write` writes it. The
assembly is then imported by its path, as any is.

This script writes a project into its folder, builds it, and calls what it built:

```zs
import { print } from "std:io";
import { Object } from "core:runtime";
import { Path, File, Directory } from Object.Assembly;
import { discover, Compiler } from "std:project";
import { Module } from "std:modules";
import { lower } from "std:modules/compiler";
import { Cecil } from "pkg:cecil";

// A project of one source, in greetings/src.
let source = Path.GetFullPath(Path.Combine("greetings", "src"));
Directory.CreateDirectory(source);
File.WriteAllText(Path.Combine(source, ".zs"), "import { compile, emit } from \"core:project\";\nemit(compile(\"Greetings\"));\n");
File.WriteAllText(Path.Combine(source, "hello.zs"), "import { String } from \"core:runtime\";\nfun hello(name: String): String { return \"hello, \" + name; }\n");

// Built, and lowered for the Cecil platform, for the .NET that runs this.
let greetings = Compiler(discover(source)).compile()[0] as Module;
let target = Cecil("Greetings", null);
target.add(lower(greetings, target));

let written = Path.GetFullPath("Greetings.dll");
target.write(written);

import { hello } from written;
print(hello("world"));
```

```output
hello, world
```

`Cecil(name, version)` writes an assembly for a .NET version, as `"net8.0"`, or for the
.NET that runs it, given `null`. The [`examples/project`](../../examples/project) of an install
is a project of several submodules, with a build script for each.
