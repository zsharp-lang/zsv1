# Projects

Up to now, everything has been a script, perhaps with a few documents it imports. As a
program grows, that stops scaling: each file imports the others it uses, in the right
order, and a definition in one can only use what was defined before it. A project
removes that bookkeeping. It is a folder of sources compiled together as one module, so
every source uses every other's definitions by name, with no imports between them.

## Making one

A project is a folder with a module file, a file named just `.zs`. The folder's other
`.zs` files are its sources:

```text
greetings/
  src/
    .zs          the module file
    hello.zs     a source
```

A source holds definitions, as a module does:

```zs fragment
// src/hello.zs
fun hello(name: String): String { return "hello, " + name; }
```

The module file says what building the project gives. Most say only this:

```zs fragment
// src/.zs
import { compile, emit } from "core:project";
import { String } from "core:runtime";

emit(compile("Greetings"));
```

`compile(name)` compiles the folder's sources as one module, here named `Greetings`, and
`emit` makes it what building the project gives.

Notice that `hello.zs` doesn't import `String`. The sources are compiled in the module
file's scope, so what the module file imports, every source has: imports used across the
project go in one place. A source can still import more of its own.

A folder inside `src` with no module file of its own just holds more sources of the same
module, so the sources can be arranged in folders however you like.

## Building it

A project is built by a script, which is what makes a project different from a script:
building is something your code does. This one writes a small project, builds it, writes
it as a .NET assembly, and calls what it built:

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
File.WriteAllText(Path.Combine(source, ".zs"), "import { compile, emit } from \"core:project\";\nimport { String } from \"core:runtime\";\nemit(compile(\"Greetings\"));\n");
File.WriteAllText(Path.Combine(source, "hello.zs"), "fun hello(name: String): String { return \"hello, \" + name; }\n");

// Compiled: what its module file emitted, the Greetings module.
let greetings = Compiler(discover(source)).compile()[0] as Module;

// Written as an assembly, for the .NET that runs this.
let target = Cecil("Greetings", null);
let described = encode(greetings);
target.add(lower(greetings, target));
target.embed("ZSharp.Reflection", described, "std:encoding");

let written = Path.GetFullPath("Greetings.dll");
target.write(written);

// And used, as any assembly is.
import { hello } from written;
print(hello("world"));
```

```output
hello, world
```

The [overview](../advanced/overview.md) of the advanced pages explains what each step is,
and [compiling projects](../advanced/build-scripts/compiling-projects.md) goes through a
build script in full.

## Where to go from here

- A project can hold more than one module: [submodules](../advanced/submodules.md).
- A project can be shared and reused by name, as a [package](../advanced/packages.md).
- The `examples/project` folder of an install is a project of several submodules, with a
  build script for each.
