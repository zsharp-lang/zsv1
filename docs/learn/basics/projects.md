# Projects and packages

A script is one file. A project is a folder of Z# sources that builds into a module, which
other code imports: as a package, by its name, or as a .NET assembly the project was
built into. The standard library's own libraries are projects.

## Making one

A project is a folder with a module file, a file named `.zs`. The folder's other `.zs`
files are its sources:

```text
greetings/
  src/
    .zs          the module file
    hello.zs     a source
```

A source holds definitions, as a module does:

```zs fragment
// src/hello.zs
import { String } from "core:runtime";

fun hello(name: String): String { return "hello, " + name; }
```

The module file is a document that says what building the project gives. Most say only
this:

```zs fragment
// src/.zs
import { compile, emit } from "core:project";

emit(compile("Greetings"));
```

`compile(name)` compiles the folder's sources as one module, named `Greetings`, and
`emit(module)` adds it to what building the project gives. With no name, `compile()`
names the module after the folder.

The sources of a project are one module: each uses the others' definitions with no
imports between them. They are compiled in the module file's scope, so what the module
file imports, every source has, and a source can import more of its own. A folder in
`src` with no module file of its own has more sources of the same module, so a project
can be arranged in folders freely.

## Using it as a package

A package is imported by its name, with `pkg:`. The package manager looks for it in its
roots: first `packages` in the folder a script runs from, then the toolchain's own. A
project in a root is a package by its folder's name, so with `greetings` in `packages`:

```text
packages/
  greetings/
    src/
      .zs
      hello.zs
main.zs
```

`main.zs` imports it as any library:

```zs fragment
// main.zs
import { print } from "std:io";
import { hello } from "pkg:greetings";

print(hello("world"));
```

A package that is a project is compiled from its sources the first time a process
imports it, and the same module is given to every import after that. A root of your own
is added with `core:packages`'s `add(folder)`, before the imports that need it:

```zs fragment
import { add } from "core:packages";
add("libraries");

import { hello } from "pkg:greetings";
```

## Building it

Compiling a project each time a process imports it takes time. A project built as a .NET
assembly, into its `build` folder, is imported as that assembly instead: for the
`greetings` package, `packages/greetings/build/greetings.dll`.

A build is a script. `std:project`'s `discover(folder)` finds a project and
`Compiler(project).compile()` runs its module file, giving what it emitted. The Cecil
platform, `pkg:cecil`, is the target `lower` lowers a module for, and `write` writes the
assembly. Before the module is lowered, `std:encoding`'s `encode` describes it, and the
assembly carries that description, so importing it gives back Z#'s own definitions and
not only the .NET types they were built as.

This script writes a project, builds it, and calls what it built:

```zs
import { print } from "std:io";
import { Object } from "core:runtime";
import { Path, File, Directory } from Object.Assembly;
import { discover, Compiler } from "std:project";
import { Module } from "std:modules";
import { lower } from "std:modules/compiler";
import { encode } from "std:encoding";
import { Cecil } from "pkg:cecil";

// A project of one source, in greetings/src.
let source = Path.GetFullPath(Path.Combine("greetings", "src"));
Directory.CreateDirectory(source);
File.WriteAllText(Path.Combine(source, ".zs"), "import { compile, emit } from \"core:project\";\nemit(compile(\"Greetings\"));\n");
File.WriteAllText(Path.Combine(source, "hello.zs"), "import { String } from \"core:runtime\";\nfun hello(name: String): String { return \"hello, \" + name; }\n");

// Built, described, and lowered for the Cecil platform, for the .NET that runs this.
let greetings = Compiler(discover(source)).compile()[0] as Module;
let target = Cecil("Greetings", null);
let described = encode(greetings);
target.add(lower(greetings, target));
target.embed("ZSharp.Reflection", described, "std:encoding");

let written = Path.GetFullPath("Greetings.dll");
target.write(written);

import { hello } from written;
print(hello("world"));
```

```output
hello, world
```

`Cecil(name, version)` writes an assembly for a .NET version, as `"net8.0"`, or for the
.NET that runs it, given `null`. An assembly is imported by its path, as here, or, written
into a package's `build` folder, by the package's name.

[Compiling projects](../advanced/build-scripts/compiling-projects.md) goes further: a
project of several modules, what `lower` leaves out, and what an assembly needs beside it.
The `examples/project` folder of an install is a project of several submodules, with a
build script for each.
