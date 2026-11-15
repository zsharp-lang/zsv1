# Compiling projects

[Projects and packages](../../basics/projects.md) builds a project of one module. This
page goes through what a build script does with a project in general: finding it,
compiling it, and doing something with what it gives, whether that is running it,
writing it as an assembly, or both.

## Finding and compiling

`std:project` is the project compiler:

- `discover(folder)` finds the project in a folder: its root submodule, the folder whose
  module file is `folder/.zs`, with its sources and its submodules.
- `Compiler(project).compile()` runs the root's module file, and gives a list of what
  every module file of the project emitted, in the order they emitted it.

A folder in a project with a module file of its own is a submodule. `compile()` in a module
file compiles that file's own sources, then runs the module file of each of its
submodules, so they emit before it does:

```text
src/
  .zs                emit(compile("Shop"));
  items.zs
  prices.zs
  text/labels.zs     a source of Shop: text/ has no module file
  greetings/
    .zs              emit(compile());
    hello.zs
```

Compiling this gives two modules, `greetings` first and then `Shop`.

What a module file emits is HIR, a module's node, which nothing has loaded or lowered.
The project compiler doesn't decide what happens to it. The build script does.

## Running what was compiled

A module that was compiled can be used in the same process without writing anything:
code that reaches it is compiled and run as any other code is. This builds the HIR of a
call to one of its functions and runs it, with `core:zsharp`'s `ct`:

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

## Writing an assembly

To use a module in another process, a build lowers it for a platform that writes it. The
.NET platform is the Cecil platform, `pkg:cecil`:

```zs fragment
import { Module } from "std:modules";
import { lower, failures } from "std:modules/compiler";
import { encode } from "std:encoding";
import { bundle } from "pkg:clr/platform";
import { Cecil } from "pkg:cecil";

let target = Cecil("Shop", "net8.0");

let described = encode(shop);
target.add(lower(shop, target));
target.embed("ZSharp.Reflection", described, "std:encoding");

let failed = failures(shop);
if (failed.Count > 0) print("Shop can't be built: " + failed[0]);

target.write("build/Shop.dll");
bundle(target.dependencies(), "build");
```

Each step:

- `Cecil(name, version)` is an assembly being written, for a .NET version, as `"net8.0"`,
  or for the .NET that runs the script, given `null`.
- `encode(module)`, from `std:encoding`, describes the module, and `embed` puts the
  description in the assembly. It is what makes importing the assembly give the module's
  own definitions back, Z# classes as Z# classes, and what [reflection](../reflection.md)
  reads. It runs before `lower`, as it gives each definition the id the platform writes on
  what the definition becomes.
- `lower(module, target)`, from `std:modules/compiler`, gives the module's LIR for the
  target, and `add` adds it to the assembly.
- `failures(module)` lists what lowering left out, by name and why. Lowering a module
  leaves out what can't be lowered for the target, rather than failing the whole module,
  so a build that needs all of it checks.
- `write(path)` writes the assembly.
- `dependencies()` lists the assemblies the written one needs that neither .NET nor the
  toolchain has, as Mono.Cecil for a library that uses it, and `pkg:clr/platform`'s
  `bundle` copies them beside it.

A project of several modules is lowered module by module, into one assembly or into one
each, as the build chooses.

## Building a package

A package is used built when its `build` folder has its assembly, named for it:
`packages/greetings/build/greetings.dll` for `pkg:greetings`. A package's build script
writes it there, and the package manager imports that assembly from then on, so the
package isn't compiled again in every process that imports it.

A built package has to be built again when its sources change, as nothing compares them.
The toolchain's own packages each have a `build.zs` that does this, as
`packages/lsp/build.zs` does for `pkg:lsp`.
