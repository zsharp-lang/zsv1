# Compiling projects

A [project](../../basics/projects.md) doesn't build itself: a script builds it. That
script decides everything a build system would elsewhere: which projects to compile, for
which platform, what to check, and where the result goes. This page goes through what such
a build script does, with one script that does all of it.

## A build script

This builds the `examples/project` project, which has two modules, into one .NET assembly
for .NET 8, and puts what the assembly needs beside it:

```zs fragment
// build.zs: builds the project in src into build/Shop.dll.
import { print } from "std:io";

// .NET, for files and paths.
import { Object } from "core:runtime";
import { Path, Directory, Environment } from Object.Assembly;

// The project compiler: discovering the project, and compiling it.
import { discover, Compiler } from "std:project";

// What a module is, and lowering one for a platform.
import { Module } from "std:modules";
import { lower, failures } from "std:modules/compiler";

// What a built module carries, so importing it gives back what it was.
import { encode } from "std:encoding";

// The platform, which writes .NET assemblies, and what it needs beside them.
import { Cecil } from "pkg:cecil";
import { bundle } from "pkg:clr/platform";

// 1. Discover the project's structure, and compile it: one module per module file.
let emitted = Compiler(discover("src")).compile();

// 2. The assembly being written.
let target = Cecil("Shop", "net8.0");

// 3. Each module: described, then lowered into the assembly.
let index = 0;
while (index < emitted.Count) {
    let built = emitted[index] as Module;
    let described = encode(built);
    target.add(lower(built, target));
    target.embed("ZSharp.Reflection", described, "std:encoding");

    // What couldn't be lowered was left out: a library needs all of it.
    let failed = failures(built);
    if (failed.Count > 0) {
        print(built.Name + " can't be built: " + failed[0]);
        Environment.Exit(1);
    }
    index = index + 1;
}

// 4. Written, with what it needs beside it.
Directory.CreateDirectory("build");
target.write(Path.GetFullPath("build/Shop.dll"));
bundle(target.dependencies(), Path.GetFullPath("build"));
print("built build/Shop.dll");
```

Run from the project's folder, it is `zs build.zs`.

## The steps

### Discovering the project's structure

`discover(folder)`, of the project compiler, reads the project's structure from the
folder: its root, the folder whose module file is `folder/.zs`, with its sources and its
submodules. Nothing is compiled yet.

### Compiling it

`Compiler(project).compile()` runs the root's module file, and gives a list of what every
module file of the project emitted, in the order they emitted it. Each is HIR, a module's
node, which nothing has loaded or lowered: what happens to it is the build script's to
decide. A project with [submodules](../submodules.md) gives one module for each.

### Describing a module

`encode(module)`, from `std:encoding`, describes the module, and the platform's `embed`
puts the description in what it writes. It is what makes importing the assembly give back
the module's own definitions, Z# classes as Z# classes, and what
[reflection](../reflection.md) reads. It comes before `lower`, as it gives each definition
the id the platform writes on what the definition becomes.

### Lowering it

`lower(module, target)`, from `std:modules/compiler`, gives the module's LIR for the
target, and `add` adds it to the assembly. Lowering leaves out what can't be lowered for
the target, rather than failing the whole module, so `failures(module)` says what was left
out, by name and why. A build that needs all of it, as a library's does, checks.

### Writing it

- `Cecil(name, version)` is an assembly being written, for a version of .NET, as
  `"net8.0"`, or for the .NET that runs the script, given `null`.
- `write(path)` writes it.
- `dependencies()` lists the assemblies it needs that neither .NET nor the toolchain has,
  and `bundle` copies them beside it.

## Using what was compiled without writing it

A build doesn't have to write anything. What was compiled can be used in the same process,
as any code is: this builds the HIR of a call to one of its functions, and runs it with
`core:zsharp`'s `ct`:

```zs fragment
import { print } from "std:io";
import { discover, Compiler } from "std:project";
import { Module } from "std:modules";
import { Get, NamedMember, StringLiteral } from "pkg:zsharp/hir/code";
import { Bodies } from "pkg:zsharp/lang/hir";
import { ct } from "core:zsharp";

let shop = Compiler(discover("src")).compile()[1] as Module;

let member = NamedMember();
member.Target = Get(shop);
member.Name = "describe";
print(ct(Bodies.Call(member, StringLiteral("apple"))));
```

That is how a tool can check or exercise a project without building it.

:::note
The project compiler is `std:project` today, though it is a build-time tool. It is to move
to the packages, as `pkg:`, with the other build-time libraries.
:::
